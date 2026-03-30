using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Attributes;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Core;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Enumerables;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Utilities;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Controllers
{
    [Authorize2]
    [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.IT })]
    public class ITAvailabilityController : BaseController
    {
        // GET: ITAvailability
        public ActionResult Manage()
        {
            var currentUser = GetUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            var currentUserRegistrationDate = _db.Registrations
                .Where(r => r.Id == currentUser.Id)
                .Select(r => r.RegistrationDate)
                .FirstOrDefault();

            var blockedDates = _db.ITAvailabilities
               .Where(b => b.RegistrationId == currentUser.Id)
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
                UserRegistrationDate = currentUserRegistrationDate ?? DateTime.Now,
                SelectedStringDates = joinedStringBlockedDates
            });
        }

        #region API

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                        throw new Exception("An error occured.");
                    }

                    var currentUser = GetUserSession();
                    if (currentUser == null)
                    {
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

                                var hasScheduledService = _db.TechnicalServiceRequestHistories
                                   .Any(h => h.ActionTakenByRegistrationId == currentUser.Id &&
                                             DbFunctions.TruncateTime(h.DateAction.Value) == blockDate.Date);
                                // Check if there is a scheduled service on the block date
                                if (hasScheduledService)
                                {
                                    throw new Exception($"Cannot block {blockDate:yyyy-MM-dd} as there is a scheduled service on that date.");
                                }

                                var existingEntry = _db.ITAvailabilities
                                    .FirstOrDefault(b => b.RegistrationId == currentUser.Id && b.BlockDate == blockDate);
                                // Only add if it doesn't already exist
                                if (existingEntry == null)
                                {
                                    _db.ITAvailabilities.Add(new ITAvailability
                                    {
                                        RegistrationId = currentUser.Id,
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
                                    throw new Exception($"Cannot unblock {removeDate:yyyy-MM-dd} as it is outside the allowed date range.");
                                }

                                var existingEntry = _db.ITAvailabilities
                                    .FirstOrDefault(b => b.RegistrationId == currentUser.Id && b.BlockDate == removeDate);
                                if (existingEntry != null)
                                {
                                    _db.ITAvailabilities.Remove(existingEntry);
                                }
                            }
                        }
                    }

                    _db.SaveChanges();
                    transaction.Commit();

                    return Json(new
                    {
                        success = true,
                        message = "Your schedule has been updated successfully.",
                        redirectLink = Url.Action("Index", "Home")
                    });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["alertModal"] = new AlertModalUtility()
                    {
                        Title = "Error",
                        Message = "An error occurred while making a request. Please try again.",
                        Status = AlertModalStatus.Error
                    };
                    ModelState.AddModelError("", "An error occurred while making a request: " + ex.Message);
                }

                return Json(new
                {
                    success = false,
                    message = "Your schedule cannot be modified.",
                });
            }
        }

        [HttpGet]
        public ActionResult GetBlockedDates(int month, int year)
        {
            try
            {
                var currentUser = GetUserSession();
                if (currentUser == null)
                {
                    throw new Exception("User not found.");
                }
                var currentUserRegistratioDate = _db.Registrations
                    .Where(r => r.Id == currentUser.Id)
                    .Select(r => r.RegistrationDate)
                    .FirstOrDefault();

                // Validate month
                if (month < 1 || month > 12)
                {
                    throw new Exception("Invalid month value.");
                }

                // Validate year (must be between user's registration year and current year)
                if (year < currentUserRegistratioDate.Value.Year || year > DateTime.Now.Year)
                {
                    throw new Exception("Invalid year value.");
                }

                var blockedDates = _db.ITAvailabilities
                    .Where(b => b.RegistrationId == currentUser.Id &&
                                b.BlockDate.Month == month &&
                                b.BlockDate.Year == year)
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