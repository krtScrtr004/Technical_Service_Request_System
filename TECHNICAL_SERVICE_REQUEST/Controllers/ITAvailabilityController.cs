using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TECHNICAL_SERVICE_REQUEST.Attributes;
using TECHNICAL_SERVICE_REQUEST.Core;
using TECHNICAL_SERVICE_REQUEST.Enumerables;
using TECHNICAL_SERVICE_REQUEST.Models;
using TECHNICAL_SERVICE_REQUEST.Utilities;

namespace TECHNICAL_SERVICE_REQUEST.Controllers
{
    [Authorize2]
    [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.IT, AppUserRoleEnum.ADMIN })]
    public class ITAvailabilityController : BaseController
    {
        // GET: ITAvailability
        public ActionResult Manage(int id)
        {
            if (id < 1)
            {
                throw new HttpException(404, "Not Found");
            }

            var currentUser = GetAppUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            // Only allow IT users to access their own schedule
            var isIt = AppUserRoleEnum.IsIT(currentUser.RoleId);
            if (isIt && currentUser.Id != id)
            {
                throw new HttpException(403, "Forbidden");
            }

            var user = _db.AppUsers
                .Where(r => r.Id == id)
                .Select(r => new
                {
                    r.Id,
                    r.RegistrationDate
                })
                .FirstOrDefault();
            if (user == null)
            {
                throw new HttpException(404, "Not Found");
            }

            try
            {
                var blockedDates = _db.ITAvailabilities
                   .Where(b => b.UserId == id)
                   .OrderBy(d => d.BlockDate)
                   .Select(d => d.BlockDate)
                   .ToList();

                var stringBlockedDates = blockedDates
                    .Select(d => d.ToString("yyyy-MM-dd"))
                    .ToList();

                var joinedStringBlockedDates = string.Join(",", stringBlockedDates);

                ViewBag.CurrentUser = currentUser;
                return View(new ITAvailabilityManageViewModel
                {
                    UserId = user.Id,
                    RegistrationDate = user.RegistrationDate ?? DateTime.Now,
                    SelectedStringDates = joinedStringBlockedDates
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occured while loading technician availability management page: {ex.Message}");
                return View("Error", "Error");
            }
        }

        #region API

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.IT })]
        public ActionResult Manage(int id, List<string> toAdd, List<string> toRemove)
        {
            DateTime TODAY = DateTime.Now;
            DateTime FUTURE = DateTime.Now.AddMonths(1);

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    if (!ModelState.IsValid)
                    {
                        var errors = GetModelStateErrors();
                        Log.Warning($"Model state is invalid: {errors}");
                        throw new Exception("An error occured.");
                    }

                    if (toAdd != null)
                    {
                        // Add new blocked dates
                        foreach (var date in toAdd)
                        {
                            if (DateTime.TryParse(date, out DateTime blockDate))
                            {
                                if (blockDate.Date < TODAY.Date || blockDate.Date > FUTURE.Date)
                                {
                                    throw new Exception($"Cannot block {blockDate:yyyy-MM-dd} as it is outside the allowed date range.");
                                }

                                var hasScheduledService = _db.RequestHistories
                                   .Any(h => h.ActionTakenById == id && DbFunctions.TruncateTime(h.DateAction.Value) == blockDate.Date);
                                // Check if there is a scheduled service on the block date
                                if (hasScheduledService)
                                {
                                    transaction.Rollback();
                                    return Json(new
                                    {
                                        success = false,
                                        message = $"Cannot block {blockDate:yyyy-MM-dd} as there is a scheduled service on that date.",
                                    });
                                }

                                var existingEntry = _db.ITAvailabilities
                                    .FirstOrDefault(b => b.UserId == id && b.BlockDate == blockDate);
                                // Only add if it doesn't already exist
                                if (existingEntry == null)
                                {
                                    _db.ITAvailabilities.Add(new ITAvailability
                                    {
                                        UserId = id,
                                        BlockDate = blockDate
                                    });
                                }
                            }
                        }
                    }

                    if (toRemove != null)
                    {
                        // Unblocking dates
                        foreach (var date in toRemove)
                        {
                            if (DateTime.TryParse(date, out DateTime removeDate))
                            {
                                if (removeDate.Date < TODAY.Date || removeDate.Date > FUTURE.Date)
                                {
                                    transaction.Rollback();
                                    return Json(new
                                    {
                                        success = false,
                                        message = $"Cannot unblock {removeDate:yyyy-MM-dd} as it is outside the allowed date range.",
                                    });
                                }

                                var existingEntry = _db.ITAvailabilities
                                    .FirstOrDefault(b => b.UserId == id && b.BlockDate == removeDate);
                                if (existingEntry != null)
                                {
                                    _db.ITAvailabilities.Remove(existingEntry);
                                }
                            }
                        }
                    }

                    _db.SaveChanges();
                    transaction.Commit();

                    // Notify clients about changes in IT availabilty
                    ITAvailabilityHub.RefreshITAvailabilityTable(id);

                    Log.Information($"User {id} updated their IT availability schedule.");
                    return Json(new
                    {
                        success = true,
                        message = "Your schedule has been updated successfully.",
                    });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    Log.Error(ex, "An error occurred while user was updating their IT availability schedule.");
                    ModelState.AddModelError("", "An error occurred while making a request: " + ex.Message);

                    return Json(new
                    {
                        success = false,
                        message = "Your schedule cannot be modified.",
                    });
                }
            }
        }

        [HttpGet]
        public ActionResult GetBlockedDates(int id, int month, int year)
        {
            try
            {
                if (id < 1)
                {
                    throw new Exception("User not found");
                }

                var userInfo = _db.AppUsers
                    .Where(r => r.Id == id)
                    .AsEnumerable()
                    .Select(r => new
                    {
                        r.RegistrationDate,
                        r.RoleId
                    })
                    .FirstOrDefault();
                if (userInfo == null)
                {
                    throw new Exception("User not found");
                }

                if (!AppUserRoleEnum.IsIT(userInfo.RoleId))
                {
                    throw new Exception("User is not an IT.");
                }

                // Validate month
                if (month < 1 || month > 12)
                {
                    throw new Exception("Invalid month value.");
                }

                // Validate year (must be between user's registration year and current year)
                if (year < userInfo.RegistrationDate.Value.Year || year > DateTime.Now.Year)
                {
                    throw new Exception("Invalid year value.");
                }

                var blockedDates = _db.ITAvailabilities
                    .Where(b => 
                        b.UserId == id &&
                        b.BlockDate.Month == month &&
                        b.BlockDate.Year == year
                    )
                    .Select(b => b.BlockDate)
                    .ToList()
                    .Select(d => d.ToString("yyyy-MM-dd"))
                    .ToList();

                return Json(new { 
                    success = true, 
                    dates = blockedDates 
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occurred while getting blocked dates for user ID {GetAppUserSession()?.Id.ToString() ?? "Unknown"}.");
                return Json(new
                {
                    success = false,
                    message = "An error occurred while making a request: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

    }
}