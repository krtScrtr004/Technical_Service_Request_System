using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TECHNICAL_SERVICE_REQUEST.Attributes;
using TECHNICAL_SERVICE_REQUEST.Core;
using TECHNICAL_SERVICE_REQUEST.Enumerables;
using TECHNICAL_SERVICE_REQUEST.Models;
using TECHNICAL_SERVICE_REQUEST.Services;
using TECHNICAL_SERVICE_REQUEST.Utilities;

namespace TECHNICAL_SERVICE_REQUEST.Controllers
{
    [RoutePrefix("Registration")]
    public class AppUserRegistrationController : BaseController
    {
        [Route("Index")]
        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.ADMIN })]
        public ActionResult Index()
        {
            var currentUser = GetAppUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            try
            {
                ViewBag.CurrentUser = currentUser;
                return View();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occured while loading account registrations list page: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }

        }

        [Route("Create")]
        [AllowAnonymous]
        public ActionResult Create()
        {
            try
            {
                return View(new AppUserRegistrationCreateViewModel());
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occured while loading account registration create page: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
        }

        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Create(AppUserRegistrationCreateViewModel appUserRegistrationCreateViewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = GetModelStateErrors();
                Log.Warning($"Model state is invalid: {errors}");
                return View(appUserRegistrationCreateViewModel);
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var isEmailTakenR = _db.AppUserRegistrations
                        .Count(i => i.Email == appUserRegistrationCreateViewModel.Email);
                    var isEmailTakenU = _db.AppUsers
                        .Count(i => i.Email == appUserRegistrationCreateViewModel.Email);
                    if (isEmailTakenR > 0 || isEmailTakenU > 0)
                    {
                        TempData["alertModal"] = new AlertModalUtility()
                        {
                            Title = "Error",
                            Message = "Email has already been taken.",
                            Status = AlertModalStatus.Error
                        };
                        return View();
                    }

                    var registration = new AppUserRegistration
                    {
                        ContactNumber = appUserRegistrationCreateViewModel.ContactNumber,
                        Email = appUserRegistrationCreateViewModel.Email,
                        FirstName = appUserRegistrationCreateViewModel.FirstName.Trim().ToUpper(),
                        LastName = appUserRegistrationCreateViewModel.LastName.Trim().ToUpper(),
                        MiddleName = appUserRegistrationCreateViewModel.MiddleName?.Trim().ToUpper(),
                        ExtensionName = appUserRegistrationCreateViewModel.ExtensionName?.Trim().ToUpper(),
                        Office = appUserRegistrationCreateViewModel.Office?.Trim().ToUpper(),
                        Position = appUserRegistrationCreateViewModel.Position?.Trim().ToUpper(),
                        RequestDate = DateTime.Now
                    };
                    _db.AppUserRegistrations.Add(registration);
                    _db.SaveChanges();

                    transaction.Commit();

                    // Refresh the registration list for all admin clients
                    var notificationService = new NotificationService();
                    notificationService.NotifyAdminNewRegistrationRequest();

                    var encryptedId = Custom.Controllers.EncryptionHelper.Encrypt(registration.Id.ToString());

                    Log.Information($"New registration request created with ID {registration.Id} and email {registration.Email}");
                    return RedirectToAction(
                        "Success",
                        "Registration",
                        new { userId = encryptedId }
                    );
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
                    Log.Error(ex, $"An error occurred while creating a registration request for email {appUserRegistrationCreateViewModel.Email}");
                    ModelState.AddModelError("", "An error occurred while making a request: " + ex.Message);
                    return View(appUserRegistrationCreateViewModel);
                }
            }
        }

        [Route("Success")]
        [AllowAnonymous]
        public ActionResult Success(string userId)
        {
            // Decrypt the id
            try
            {
                var dec = Custom.Controllers.EncryptionHelper.Decrypt(userId);
                int? id = Int32.Parse(dec);
                if (!id.HasValue)
                {
                    throw new HttpException(403, "Forbidden");
                }

                // Find the registration request by the decrypted id
                var registration = _db.AppUserRegistrations
                    .FirstOrDefault(i => i.Id == id);
                if (registration == null)
                {
                    throw new HttpException(404, "Not found");
                }

                return View(new AppUserRegistrationVerifyAccountModel
                {
                    Registration = registration
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occured while loading account creation success page: {ex.Message}");
                return RedirectToAction("NotFound", "Error");
            }
        }

        #region API

        [Route("GetRegistrationRequests/")]
        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.ADMIN })]
        public ActionResult GetRegistrationRequests()
        {
            try
            {
                // Authenticate user
                int.TryParse(Request["userId"], out int Id);
                var associatedUser = _db.AppUsers
                    .Where(i => i.Id == Id)
                    .Select(i => new
                    {
                        i.Id,
                        i.RoleId,
                        i.ContactNumber,
                        i.IsActive,
                    })
                    .FirstOrDefault();
                if (associatedUser == null || associatedUser?.IsActive == false)
                {
                    throw new Exception("User not found.");
                }

                if (associatedUser.RoleId != (int)AppUserRoleEnum.ADMIN)
                {
                    throw new Exception("You do not have permission to access this resource.");
                }

                // Get DataTables parameters from request
                var draw = Request["draw"];
                var start = Request["start"];
                var length = Request["length"];
                var searchValue = Request["search[value]"];
                var sortColumn = Request["order[0][column]"];
                var sortDirection = Request["order[0][dir]"];

                var statusFilter = Request["statusFilter"];
                var requestDateFilter = Request["requestDateFilter"];

                var query = _db.AppUserRegistrations.AsQueryable();

                // Filter by status
                if (!string.IsNullOrEmpty(statusFilter) &&
                    int.TryParse(statusFilter, out var statusIntValue) &&
                    statusIntValue > 0)
                {
                    switch (statusIntValue)
                    {
                        // Pending
                        case 1:
                            query = query.Where(i => !i.IsApproved && !i.IsDenied);
                            break;

                        // Approved
                        case 2:
                            query = query.Where(i => i.IsApproved);
                            break;

                        // Declined
                        case 3:
                            query = query.Where(i => i.IsDenied);
                            break;
                    }
                }

                // Filter by request date
                if (!string.IsNullOrEmpty(requestDateFilter) &&
                int.TryParse(requestDateFilter, out var requestDateIntValue) &&
                requestDateIntValue > 0)
                {
                    var now = DateTime.Now;
                    var today = now.Date;

                    switch (requestDateIntValue)
                    {
                        case 1: // Today
                            query = query.Where(i =>
                                i.RequestDate.HasValue &&
                                DbFunctions.TruncateTime(i.RequestDate.Value) == DbFunctions.TruncateTime(today)
                            );
                            break;

                        case 2: // This Week
                            var weekStart = GeneralUtilities.GetStartOfWeek(today, DayOfWeek.Monday); // or Sunday
                            var nextWeekStart = weekStart.AddDays(7);

                            query = query.Where(i =>
                                i.RequestDate.HasValue &&
                                i.RequestDate.Value >= weekStart &&
                                i.RequestDate.Value < nextWeekStart);
                            break;


                        case 3: // This Month
                            query = query.Where(i =>
                                i.RequestDate.HasValue &&
                                i.RequestDate.Value.Year == now.Year &&
                                i.RequestDate.Value.Month == now.Month);
                            break;

                        case 4: // This Year
                            query = query.Where(i =>
                                i.RequestDate.HasValue &&
                                i.RequestDate.Value.Year == now.Year);
                            break;

                    }
                }

                var recordsTotal = query.Count();

                // Apply search filter
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(i =>
                        i.FirstName.Contains(searchValue) ||
                        i.MiddleName.Contains(searchValue) ||
                        i.LastName.Contains(searchValue) ||
                        i.Email.Contains(searchValue)
                    );
                }
                var recordsFiltered = query.Count();

                // Apply sorting
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
                {
                    int columnIndex = int.Parse(sortColumn);
                    switch (columnIndex)
                    {
                        case 1:
                            query = sortDirection == "asc"
                                ? query.OrderBy(i => i.LastName)
                                : query.OrderByDescending(i => i.LastName);
                            break;
                        case 2:
                            query = sortDirection == "asc"
                                ? query.OrderBy(i => i.Email)
                                : query.OrderByDescending(i => i.Email);
                            break;
                        case 3:
                            query = sortDirection == "asc"
                                ? query.OrderBy(i => i.RequestDate)
                                : query.OrderByDescending(i => i.RequestDate);
                            break;
                        default:
                            query = query.OrderByDescending(i => i.RequestDate); // Default sorting
                            break;
                    }
                }
                else
                {
                    query = query.OrderByDescending(i => i.RequestDate); // Default sorting
                }

                var data = query
                    .Skip(int.Parse(start))
                    .Take(int.Parse(length))
                    .ToList()
                    .Select(i => new
                    {
                        i.Id,
                        i.FirstName,
                        i.MiddleName,
                        i.LastName,
                        i.Email,
                        i.IsApproved,
                        i.IsDenied,
                        RequestDate = i.RequestDate.HasValue
                            ? i.RequestDate.Value.ToString("yyyy-MM-ddTHH:mm:ss")
                            : null
                    })
                    .ToList();

                return Json(new
                {
                    draw,
                    recordsTotal,
                    recordsFiltered,
                    data
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occurred while fetching registration requests for user ID {GetAppUserSession()?.Id.ToString() ?? "Unknown"}");
                return Json(
                    new { success = false, message = ex.Message },
                    JsonRequestBehavior.AllowGet
                );
            }
        }

        #endregion

    }
}
