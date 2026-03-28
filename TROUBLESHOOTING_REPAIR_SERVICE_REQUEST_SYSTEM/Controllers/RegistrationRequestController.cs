using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Attributes;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Core;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Enumerables;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Services;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Utilities;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Controllers
{
    public class RegistrationRequestController : BaseController
    {

        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.ADMIN })]
        public ActionResult Index()
        {
            var currentUser = GetUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            ViewBag.CurrentUser = currentUser;
            return View();
        }

        [AllowAnonymous]
        public ActionResult Create()
        {
            // Redirect to Dashboard when logged in
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(new RegistrationRequestCreateViewModel());
        }

        // POST: /Registration/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Create(RegistrationRequestCreateViewModel registrationRequestCreateViewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = GetModelStateErrors();
                return View(registrationRequestCreateViewModel);
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var isEmailTaken = _db.Registrations
                        .Count(i => i.Email == registrationRequestCreateViewModel.Email);
                    if (isEmailTaken > 0)
                    {
                        TempData["alertModal"] = new AlertModalUtility()
                        {
                            Title = "Error",
                            Message = "Email has already been taken.",
                            Status = AlertModalStatus.Error
                        };
                        return View();
                    }

                    var registration = new RegistrationRequest
                    {
                        ContactNumber = registrationRequestCreateViewModel.ContactNumber,
                        Email = registrationRequestCreateViewModel.Email,
                        FirstName = registrationRequestCreateViewModel.FirstName.Trim().ToUpper(),
                        LastName = registrationRequestCreateViewModel.LastName.Trim().ToUpper(),
                        MiddleName = registrationRequestCreateViewModel.MiddleName?.Trim().ToUpper(),
                        RequestDate = DateTime.Now
                    };
                    _db.RegistrationRequests.Add(registration);
                    _db.SaveChanges();

                    transaction.Commit();

                    // Refresh the registration request list for all admin clients
                    var notificationService = new NotificationService();
                    notificationService.NotifyAdminNewRegistrationRequest();

                    var encryptedId = Custom.Controllers.EncryptionHelper.Encrypt(registration.Id.ToString());
                    return RedirectToAction(
                        "Success",
                        "RegistrationRequest",
                        new { registrationId = encryptedId }
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
                    ModelState.AddModelError("", "An error occurred while making a request: " + ex.Message);
                    return View(registrationRequestCreateViewModel);
                }
            }
        }

        [AllowAnonymous]
        public ActionResult Success(string registrationId)
        {
            // Decrypt the id
            try
            {
                var dec = Custom.Controllers.EncryptionHelper.Decrypt(registrationId);
                int? id = Int32.Parse(dec);
                if (!id.HasValue)
                {
                    throw new HttpException(403, "Forbidden");
                }

                // Find the registration request by the decrypted id
                var registrationRequest = _db.RegistrationRequests
                    .FirstOrDefault(i => i.Id == id);
                if (registrationRequest == null)
                {
                    throw new HttpException(404, "Not found");
                }

                return View(new RegistrationVerifyAccountModel
                {
                    RegistrationRequest = registrationRequest
                });
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

        #region API

        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.ADMIN })]
        public ActionResult GetRegistrationRequest()
        {
            try
            {
                // Authenticate user
                int.TryParse(Request["userId"], out int Id);
                var associatedUser = _db.Registrations
                    .Where(i => i.Id == Id)
                    .Select(i => new
                    {
                        i.Id,
                        i.AccountType,
                        i.ContactNumber,
                        i.IsActive,
                        UserPrivileges = i.UserPrivileges
                            .Select(j => j.PrivilegeId)
                    })
                    .FirstOrDefault();
                if (associatedUser == null || associatedUser?.IsActive == false)
                {
                    throw new Exception("User not found.");
                }

                if (!associatedUser.UserPrivileges.Contains(AccountTypeEnum.ADMIN))
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

                var query = _db.RegistrationRequests.AsQueryable();

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
                        case 0:
                            query = sortDirection == "asc"
                                ? query.OrderBy(i => i.LastName)
                                : query.OrderByDescending(i => i.LastName);
                            break;
                        case 1:
                            query = sortDirection == "asc"
                                ? query.OrderBy(i => i.Email)
                                : query.OrderByDescending(i => i.Email);
                            break;
                        case 2:
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
                return Json(
                    new { success = false, message = ex.Message },
                    JsonRequestBehavior.AllowGet
                );
            }
        }

        #endregion

    }
}
