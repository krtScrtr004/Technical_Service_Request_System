using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Attributes;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Core;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Enumerables;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Services;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Utilities;
using static TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.RegistrationRequestHub;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Controllers
{
    [Authorize2]
    public class TechnicalServiceRequestsController : BaseController
    {
        // GET: TechnicalServiceRequests
        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD, AccountTypeEnum.IT, AccountTypeEnum.ADMIN })]
        public ActionResult Index()
        {
            var currentUser = GetUserSession();
            if (currentUser == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            ViewBag.CurrentUser = currentUser;
            return View();
        }

        // GET: TechnicalServiceRequests/Details/5
        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD, AccountTypeEnum.IT, AccountTypeEnum.ADMIN })]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var currentUser = GetUserSession();
            if (currentUser == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            TechnicalServiceRequest technicalServiceRequest = _db.TechnicalServiceRequests
                .Include(t => t.TechnicalServiceType)
                .Include(t => t.TechnicalServiceRequestSeverity)
                .Include(t => t.TechnicalServiceRequestStatus)
                .Include(t => t.TechnicalServiceRequestHistories
                    .Select(h => h.ActionTakenByRegistration))
                .Include(t => t.TechnicalServiceRequestHistories
                    .Select(h => h.TechnicalServiceRequestStatus))
                .FirstOrDefault(t => t.Id == id);
            if (technicalServiceRequest == null)
            {
                return HttpNotFound();
            }

            // Cast technical service request to details view model 
            var casted = TechnicalServiceRequestTypeCaster
                .ToTechnicalServiceRequestDetailsViewModel(technicalServiceRequest);

            ViewBag.CurrentUser = currentUser;
            return View(casted);
        }

        public ActionResult Form(int id)
        {
            if (id < 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var technicalServiceRequestHistory = _db.TechnicalServiceRequestHistories
                .Include(h => h.TechnicalServiceRequest)
                .FirstOrDefault(h => h.Id == id);
            if (technicalServiceRequestHistory == null)
            {
                return HttpNotFound();
            }    

            return View(new TechnicalServiceRequestFormViewModel
            {
                TechnicalServiceRequest = technicalServiceRequestHistory.TechnicalServiceRequest,
                TechnicalServiceRequestHistory = technicalServiceRequestHistory
            });
        }

        // GET: TechnicalServiceRequests/Create
        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD })]
        public ActionResult Create()
        {
            var currentUser = GetUserSession();
            if (currentUser == null)
            {
                return HttpNotFound();
            }

            ViewBag.CurrentUser = currentUser;
            return View(new TechnicalServiceRequestCreateViewModel()
            {
                ClientFirstName = currentUser.FirstName,
                ClientLastName = currentUser.LastName,
                ClientMiddleName = currentUser.MiddleName,
                ClientEmailAddress = currentUser.Email,
                ClientContactNumber = currentUser.ContactNumber,
            });
        }

        // POST: TechnicalServiceRequests/Create
        [Authorize2]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD })]
        public ActionResult Create(TechnicalServiceRequestCreateViewModel technicalServiceRequestCreateViewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = GetModelStateErrors();
                return View(technicalServiceRequestCreateViewModel);
            }

            // Use a transaction to ensure data integrity
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var isQueued = false;

                    var clientId = _db.Registrations
                        .Where(r => r.Email == technicalServiceRequestCreateViewModel.ClientEmailAddress)
                        .Select(r => (int?)r.Id)
                        .FirstOrDefault();
                    if (!clientId.HasValue)
                    {
                        throw new Exception("Client not found.");
                    }

                    var technicalServiceRequest = TechnicalServiceRequestTypeCaster
                        .ToTechnicalServiceRequest(technicalServiceRequestCreateViewModel);
                    technicalServiceRequest.ReferenceCode = GenerateReferenceCode();
                    technicalServiceRequest.DateRequest = DateTime.Now;

                    // Trim and uppercase information for consistency
                    technicalServiceRequest.ClientFirstName = technicalServiceRequest.ClientFirstName?.Trim().ToUpper();
                    technicalServiceRequest.ClientLastName = technicalServiceRequest.ClientLastName?.Trim().ToUpper();
                    technicalServiceRequest.ClientMiddleName = technicalServiceRequest.ClientMiddleName?.Trim().ToUpper();
                    technicalServiceRequest.ClientExtensionName = technicalServiceRequest.ClientExtensionName?.Trim().ToUpper();
                    technicalServiceRequest.ClientOffice = technicalServiceRequest.ClientOffice?.Trim().ToUpper();
                    technicalServiceRequest.ClientPosition = technicalServiceRequest.ClientPosition?.Trim().ToUpper();
                    technicalServiceRequest.ClientOffice = technicalServiceRequest.ClientOffice.Trim().ToUpper();
                    technicalServiceRequest.ClientPosition = technicalServiceRequest.ClientPosition.Trim().ToUpper();
                    technicalServiceRequest.Others = !string.IsNullOrWhiteSpace(technicalServiceRequest.Others) 
                            ? technicalServiceRequest.Others.Trim() 
                            : string.Empty;

                    var selectedTechnicalServiceType = technicalServiceRequest.TechnicalServiceTypeId;

                    if (selectedTechnicalServiceType.HasValue)
                    {
                        // Implement different logic based on the selected technical service type
                        if (TechnicalServiceTypeEnum.IsRepairTroubleshootingRequest(selectedTechnicalServiceType.Value))
                        {
                            CreateEquipmentRepairTroubleshootingRequest(ref technicalServiceRequest, ref isQueued);
                        }
                        else if (TechnicalServiceTypeEnum.IsScheduleControlProcessRequest(selectedTechnicalServiceType.Value))
                        {
                            try { CreateScheduleControlProcessRequest(ref technicalServiceRequest); }
                            catch (Exception ex)
                            {
                                TempData["alertModal"] = new AlertModalUtility()
                                {
                                    Title = "Error",
                                    Message = ex.Message,
                                    Status = AlertModalStatus.Error
                                };
                                ModelState.AddModelError("", "An error occurred while making a request: " + ex.Message);
                                return View(technicalServiceRequestCreateViewModel);
                            }
                        }
                        else if (TechnicalServiceTypeEnum.IsNonAssistedRequest(selectedTechnicalServiceType.Value))
                        {
                            CreateNonAssistedRequest(ref technicalServiceRequest);
                        }
                    }
                    else if (!string.IsNullOrEmpty(technicalServiceRequestCreateViewModel.Others))
                    {
                        // If "Others" is not specified, default to Equipment Repair Troubleshooting logic
                        CreateEquipmentRepairTroubleshootingRequest(ref technicalServiceRequest, ref isQueued);
                    }
                    else
                    {
                        throw new Exception("Please select a valid service type or specify the details in the 'Others' field.");
                    }

                    if (!isQueued)
                    {
                        // Set Date Receive when a technicacian is assigned
                        technicalServiceRequest.DateReceived = DateTime.Now;
                    }

                    _db.TechnicalServiceRequests.Add(technicalServiceRequest);

                    _db.SaveChanges();
                    transaction.Commit();

                    // Notify all connected clients about the new request
                    TechnicalServiceRequestHub.RefreshTechnicalServiceRequestList();

                    if (!isQueued)
                    {
                        var notificationService = new NotificationService();
                        if (TechnicalServiceTypeEnum.IsNonAssistedRequest(selectedTechnicalServiceType.Value))
                        {
                            notificationService.NotifyTechnicianNonAssistedService(
                                technicalServiceRequest.ReferenceCode
                            );
                        }
                        else
                        {
                            // Notify the assigned technician about the new assignment
                            var assignedTechnicianId = _db.TechnicalServiceRequestHistories
                                .Where(r => r.TechnicalServiceRequestId == technicalServiceRequest.Id)
                                .OrderByDescending(h => h.UpdatedAt)
                                .Select(h => h.ActionTakenByRegistrationId)
                                .FirstOrDefault();
                            if (assignedTechnicianId.HasValue)
                            {
                                notificationService.NotifyTechnicianAssignment(
                                    assignedTechnicianId.Value,
                                    technicalServiceRequest.ReferenceCode
                                );
                            }
                        }

                        TempData["alertModal"] = new AlertModalUtility()
                        {
                            Title = "Success",
                            Message = "Your request is now being processed.",
                            Status = AlertModalStatus.Success
                        };
                    }
                    else
                    {
                        // Push to queue
                        (new TechnicalServiceRequestQueueService())
                            .Push(technicalServiceRequest.Id);

                        TempData["alertModal"] = new AlertModalUtility()
                        {
                            Title = "Info",
                            Message = "Your request is currently pending. All technicians are assisting other actions. You will be notified once a technician becomes available.",
                            Status = AlertModalStatus.Info
                        };
                    }

                    return RedirectToAction("Index");
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
            }
            return View(technicalServiceRequestCreateViewModel);
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

        // GET
        [Authorize2]
        [HttpGet]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD, AccountTypeEnum.IT, AccountTypeEnum.ADMIN })]
        public JsonResult GetTechnicalRequest()
        {
            try
            {
                // Authenticate user
                int.TryParse(Request["userId"], out int Id);
                var associatedUser = _db.Registrations
                    .Where(i => i.Id == Id)
                    .FirstOrDefault();
                if (associatedUser == null)
                {
                    throw new Exception("User not found.");
                }

                // Get user's privilege
                var associatedUserPrivilege = _db.UserPrivileges
                    .Where(j => j.RegistrationId == associatedUser.Id)
                    .Select(i => i.PrivilegeId)
                    .First();

                // Get DataTables parameters from request
                var draw = Request["draw"];
                var start = Request["start"];
                var length = Request["length"];
                var searchValue = Request["search[value]"];
                var sortColumn = Request["order[0][column]"];
                var sortDirection = Request["order[0][dir]"];

                var typeFilter = Request["typeFilter"];
                var statusFilter = Request["statusFilter"];
                var dateRequestFilter = Request["dateRequestFilter"];

                // Parse parameters
                int pageSize = length != null ? Convert.ToInt32(length) : 10;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                // Base query
                IQueryable<TechnicalServiceRequest> query = null;
                if (associatedUserPrivilege == AccountTypeEnum.ADMIN)
                {
                    // Admin can see all requests
                    query = _db.TechnicalServiceRequests
                        .Include(t => t.TechnicalServiceType)
                        .Include(t => t.TechnicalServiceRequestStatus);
                }
                else if (associatedUserPrivilege == AccountTypeEnum.IT)
                {
                    var nonAssistedServiceIds = TechnicalServiceTypeEnum.GetNonAssistedServiceIds();
                    // IT can see requests that they have taken action on (based on TechnicalServiceRequestHistories)
                    query = _db.TechnicalServiceRequests
                        .Include(t => t.TechnicalServiceType)
                        .Include(t => t.TechnicalServiceRequestStatus)
                        .Where(t => t.TechnicalServiceRequestHistories
                            .Any(h => h.ActionTakenByRegistrationId == associatedUser.Id) ||
                            (t.TechnicalServiceTypeId.HasValue &&
                            nonAssistedServiceIds.Contains(t.TechnicalServiceTypeId.Value)
                        ));
                }
                else if (associatedUserPrivilege == AccountTypeEnum.STANDARD)
                {
                    // Standard users can only see their own requests
                    query = _db.TechnicalServiceRequests
                        .Include(t => t.TechnicalServiceType)
                        .Include(t => t.TechnicalServiceRequestStatus)
                        .Where(t => t.ClientEmailAddress == associatedUser.Email);
                }
                else
                {
                    throw new Exception("Invalid account type.");
                }

                // Apply Type filter
                if (!string.IsNullOrEmpty(typeFilter))
                {
                    if (int.TryParse(typeFilter, out int typeIntValue) && typeIntValue > 0)
                    {
                        // Filter specific type
                        if (typeIntValue == 100)
                        {
                            query = query.Where(i => !string.IsNullOrEmpty(i.Others));
                        }
                        else
                        {
                            query = query.Where(i => i.TechnicalServiceTypeId == typeIntValue);
                        }
                    }
                    else if (typeFilter == "assigned")
                    {
                        query = query.Where(i =>
                            i.TechnicalServiceRequestHistories
                            .Any(h => h.ActionTakenByRegistrationId == associatedUser.Id)
                        );
                    }
                    else if (typeFilter == "non-assisted")
                    {
                        var nonAssistedServices = TechnicalServiceTypeEnum.GetNonAssistedServiceIds();
                        query = query.Where(i =>
                            i.TechnicalServiceTypeId.HasValue &&
                            nonAssistedServices.Contains(i.TechnicalServiceTypeId.Value)
                        );
                    }
                }

                // Apply Status filter
                if (!string.IsNullOrEmpty(statusFilter) && int.TryParse(statusFilter, out int statusIntValue) && statusIntValue > 0)
                {
                    query = query.Where(i => i.TechnicalServiceRequestStatusId == statusIntValue);
                }

                // Apply Date Request filter
                if (!string.IsNullOrEmpty(dateRequestFilter) &&
                    int.TryParse(dateRequestFilter, out var dateRequestIntValue) &&
                    dateRequestIntValue > 0)
                {
                    var now = DateTime.Now;
                    var today = now.Date;

                    switch (dateRequestIntValue)
                    {
                        case 1: // Today
                            query = query.Where(i =>
                                i.DateRequest.HasValue &&
                                DbFunctions.TruncateTime(i.DateRequest.Value) == DbFunctions.TruncateTime(today)
                            );
                            break;

                        case 2: // This Week
                            var weekStart = GeneralUtilities.GetStartOfWeek(today, DayOfWeek.Monday); // or Sunday
                            var nextWeekStart = weekStart.AddDays(7);

                            query = query.Where(i =>
                                i.DateRequest.HasValue &&
                                i.DateRequest.Value >= weekStart &&
                                i.DateRequest.Value < nextWeekStart);
                            break;


                        case 3: // This Month
                            query = query.Where(i =>
                                i.DateRequest.HasValue &&
                                i.DateRequest.Value.Year == now.Year &&
                                i.DateRequest.Value.Month == now.Month);
                            break;

                        case 4: // This Year
                            query = query.Where(i =>
                                i.DateRequest.HasValue &&
                                i.DateRequest.Value.Year == now.Year);
                            break;

                    }
                }

                // Get total records before filtering
                recordsTotal = query.Count();

                // Apply search filter
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(t =>
                        t.ReferenceCode.Contains(searchValue) ||
                        t.ClientFirstName.Contains(searchValue) ||
                        t.ClientLastName.Contains(searchValue) ||
                        t.ClientMiddleName.Contains(searchValue) ||
                        t.ClientEmailAddress.Contains(searchValue) ||
                        t.ClientContactNumber.Contains(searchValue) ||
                        t.ClientOffice.Contains(searchValue) ||
                        t.TechnicalServiceType.TechnicalServiceTypeName.Contains(searchValue) ||
                        t.TechnicalServiceRequestStatus.TechnicalServiceRequestStatusName.Contains(searchValue) ||
                        t.Others.Contains(searchValue)
                    );
                }

                // Get filtered count
                int recordsFiltered = query.Count();

                // Apply sorting
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
                {
                    int columnIndex = Convert.ToInt32(sortColumn);
                    switch (columnIndex)
                    {
                        case 0: // Reference Code
                            query = sortDirection == "asc"
                                ? query.OrderBy(t => t.ReferenceCode)
                                : query.OrderByDescending(t => t.ReferenceCode);
                            break;
                        case 1: // Client Name
                            query = sortDirection == "asc"
                                ? query.OrderBy(t => t.ClientFirstName)
                                : query.OrderByDescending(t => t.ClientFirstName);
                            break;
                        case 2: // Service Type
                            query = sortDirection == "asc"
                                ? query.OrderBy(t => t.TechnicalServiceType.TechnicalServiceTypeName)
                                : query.OrderByDescending(t => t.TechnicalServiceType.TechnicalServiceTypeName);
                            break;
                        case 3: // Status
                            query = sortDirection == "asc"
                                ? query.OrderBy(t => t.TechnicalServiceRequestStatus.TechnicalServiceRequestStatusName)
                                : query.OrderByDescending(t => t.TechnicalServiceRequestStatus.TechnicalServiceRequestStatusName);
                            break;
                        case 4: // Date Requested
                            query = sortDirection == "asc"
                                ? query.OrderBy(t => t.DateRequest)
                                : query.OrderByDescending(t => t.DateRequest);
                            break;
                        default:
                            query = query.OrderByDescending(t => t.DateRequest);
                            break;
                    }
                }
                else
                {
                    query = query.OrderByDescending(t => t.DateRequest);
                }

                // Apply pagination
                var data = query
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList()
                    .Select(t => new
                    {
                        t.Id,
                        t.ReferenceCode,
                        t.ClientFirstName,
                        t.ClientMiddleName,
                        t.ClientLastName,
                        t.ClientEmailAddress,
                        t.ClientContactNumber,
                        t.ClientOffice,
                        t.TechnicalServiceType?.TechnicalServiceTypeName,
                        t.Others,
                        t.TechnicalServiceRequestStatus.TechnicalServiceRequestStatusName,
                        DateRequest = t.DateRequest.HasValue
                            ? t.DateRequest.Value.ToString("yyyy-MM-ddTHH:mm:ss")
                            : null
                    })
                    .ToList();

                // Return data in DataTables format
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
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize2]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD })]
        public ActionResult CancelRequest(int id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = GetModelStateErrors();
                    throw new Exception("An error occured.");
                }

                var technicalServiceRequest = _db.TechnicalServiceRequests.Find(id);
                if (technicalServiceRequest == null)
                {
                    throw new Exception("Technical service request not found.");
                }

                var clientId = _db.Registrations
                    .Where(i => i.Email == technicalServiceRequest.ClientEmailAddress)
                    .Select(i => i.Id)
                    .FirstOrDefault();
                if (clientId == 0)
                {
                    throw new Exception("Client not found.");
                }

                var currentStatus = technicalServiceRequest.TechnicalServiceRequestStatusId;
                if (!currentStatus.HasValue)
                {
                    throw new Exception("Status not found.");
                }

                var currentStatusName = technicalServiceRequest.TechnicalServiceRequestStatus.TechnicalServiceRequestStatusName;

                // Check is current status can still be cancelled
                var cancellableStatus = TechnicalServiceRequestStatusEnum.GetCancellableStatusIds();
                if (cancellableStatus.Contains(currentStatus.Value))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Your request cannot be cancelled as it is already in " + currentStatusName + " status.",
                    });
                }

                // If the current status is "Open", notify the assigned technician about the cancellation
                if (currentStatus == (int)TechnicalServiceRequestStatusEnum.OPEN)
                {
                    var history = _db.TechnicalServiceRequestHistories
                        .Where(h => h.TechnicalServiceRequestId == technicalServiceRequest.Id)
                        .OrderByDescending(h => h.UpdatedAt)
                        .FirstOrDefault();
                    if (history != null)
                    {
                        var technicianId = history.ActionTakenByRegistrationId;
                        _db.Notifications.Add(new Notification()
                        {
                            RecipientRegistrationId = technicianId.Value,
                            Title = "Request Cancelled: " + technicalServiceRequest.ReferenceCode,
                            Message = "The request with reference code " + technicalServiceRequest.ReferenceCode + " has been cancelled by the client. Please check the request details for more information.",
                            IsRead = false,
                            CreatedAt = DateTime.Now,
                        });
                        (new NotificationService()).RefreshUserUi(technicianId.Value);
                    }
                }

                // Only status can be editted
                var newStatus = (int)TechnicalServiceRequestStatusEnum.CANCELLED;
                technicalServiceRequest.TechnicalServiceRequestStatusId = newStatus;
                _db.Entry(technicalServiceRequest).State = EntityState.Modified;
                _db.SaveChanges();

                // Notify all connected clients about the cancelled request
                TechnicalServiceRequestHub.RefreshTechnicalServiceRequestList();
                TechnicalServiceRequestHub.RefreshTechnicalServiceRequestStatus(
                    TechnicalServiceRequestStatusEnum.DisplayName(newStatus)
                );

                var severityName = technicalServiceRequest.TechnicalServiceRequestSeverity != null
                    ? technicalServiceRequest
                        .TechnicalServiceRequestSeverity
                        .SeverityName
                    : (technicalServiceRequest.TechnicalServiceRequestSeverityId.HasValue
                        ? TechnicalServicRequestSeverityEnum.DisplayName(
                            technicalServiceRequest
                            .TechnicalServiceRequestSeverityId.Value
                        ) : "Unknown severity");

                return Json(new
                {
                    success = true,
                    message = "Your request has been cancelled.",
                    redirectUrl = Url.Action(
                        "Details",
                        "TechnicalServiceRequests",
                        new { id = id }
                    ),
                    severityName = severityName
                });
            }
            catch (Exception ex)
            {
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
                message = "Your request cannot be cancelled.",
            });
        }

        [Authorize2]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.IT })]
        public ActionResult UpdateSeverity(int id, int severityId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = GetModelStateErrors();
                    throw new Exception("An error occured.");
                }

                if (severityId < TechnicalServicRequestSeverityEnum.LOW ||
                    severityId > TechnicalServicRequestSeverityEnum.CRITICAL)
                {
                    throw new Exception("Invalid severity level.");
                }

                var technicalServiceRequest = _db.TechnicalServiceRequests
                    .FirstOrDefault(i => i.Id == id);
                if (technicalServiceRequest == null)
                {
                    throw new Exception("Technical service request not found.");
                }

                var currentSeverityId = technicalServiceRequest.TechnicalServiceRequestSeverityId;
                if (severityId != currentSeverityId)
                {
                    technicalServiceRequest.TechnicalServiceRequestSeverityId = severityId;
                    _db.Entry(technicalServiceRequest).State = EntityState.Modified;

                    // Notify all connected clients about the updated severity
                    TechnicalServiceRequestHub.RefreshTechnicalServiceRequestSeverity(
                        technicalServiceRequest.TechnicalServiceRequestSeverity.SeverityName);

                    // Notify client about the updated severity
                    var clientId = _db.Registrations
                        .Where(i => i.Email == technicalServiceRequest.ClientEmailAddress)
                        .Select(i => (int?)i.Id)
                        .FirstOrDefault();
                    if (clientId != null)
                    {
                        var notificationService = new NotificationService();
                        _db.Notifications.Add(new Notification()
                        {
                            RecipientRegistrationId = clientId.Value,
                            Title = "Severity Updated: " + technicalServiceRequest.ReferenceCode,
                            Message = notificationService.BuildRecipientMessageFromRequestSeverity(
                                    severityId, technicalServiceRequest.ReferenceCode, currentSeverityId
                                ),
                            IsRead = false,
                            CreatedAt = DateTime.Now,
                        });
                        notificationService.RefreshUserUi(clientId.Value);
                    }
                }

                _db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Severity level has been updated.",
                    redirectUrl = Url.Action(
                        "Details",
                        "TechnicalServiceRequests",
                        new { id = id }
                    ),
                });
            }
            catch (Exception ex)
            {
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
                message = "Your request cannot be cancelled.",
            });
        }

        #endregion

        #region Helpers

        /**
         * Generate Reference Code in the format "YYYY-XXXXXX" where YYYY is the current year
         * and XXXXXX is a sequential number starting from 000001 for each year
         */
        private string GenerateReferenceCode()
        {
            string currentYear = DateTime.Now.Year.ToString();

            // Get the last code for the current year
            var lastCode = _db.TechnicalServiceRequests
                .Where(r => r.ReferenceCode.StartsWith(currentYear))
                .OrderByDescending(r => r.ReferenceCode)
                .Select(r => (string)r.ReferenceCode)
                .FirstOrDefault();

            int nextNumber = 1; // default if no records yet

            if (!string.IsNullOrEmpty(lastCode))
            {
                // Extract the numeric part after "YYYY-"
                string numericPart = lastCode.Split('-')[1];
                nextNumber = int.Parse(numericPart) + 1;
            }

            // Format: 2026-000001
            return $"{currentYear}-{nextNumber:D6}";
        }

        private int? GetAvailableTechnician()
        {
            var itAccountTypeName = AccountTypeEnum.DisplayName(AccountTypeEnum.IT);

            var activeStatusIds = TechnicalServiceRequestStatusEnum.GetActiveStatusIds();

            // Build a set of technicians currently busy with active requests
            var busyTechnicianIds = _db.TechnicalServiceRequests
                .Where(r =>
                    r.TechnicalServiceRequestStatusId.HasValue &&
                    activeStatusIds.Contains(r.TechnicalServiceRequestStatusId.Value))
                .Select(r => r.TechnicalServiceRequestHistories
                    .OrderByDescending(h => h.UpdatedAt)
                    .Select(h => h.ActionTakenByRegistrationId)
                    .FirstOrDefault())
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Distinct()
                .ToList();

            var today = DateTime.Now.Date;

            // Pick the least recently assigned (fairness), then by Id.
            var availableTechnicianId = _db.Registrations
                .Where(r => r.AccountType == itAccountTypeName)
                .Where(r => !r.ITAvailabilities.Any(a =>
                    DbFunctions.TruncateTime(a.BlockDate) == today))
                .Where(r => !busyTechnicianIds.Contains(r.Id))
                .Select(r => new
                {
                    r.Id,
                    LastAssignedAt = _db.TechnicalServiceRequestHistories
                        .Where(h => h.ActionTakenByRegistrationId == r.Id)
                        .Select(h => (DateTime?)h.UpdatedAt)
                        .Max()
                })
                .OrderBy(r => r.LastAssignedAt ?? DateTime.MinValue)
                .ThenBy(r => r.Id)
                .Select(r => (int?)r.Id)
                .FirstOrDefault();

            return availableTechnicianId;
        }

        private int? GetAvailableTechnicianOnSchedule(DateTime? scheduleDate, TimeSpan? startTime, TimeSpan? endTime)
        {
            if (scheduleDate == null || startTime == null || endTime == null)
            {
                throw new Exception("Schedule Date, Start Time, and End Time must be provided.");
            }

            var itAccountTypeName = AccountTypeEnum.DisplayName(AccountTypeEnum.IT);

            // Requests in these statuses are still actively consumi ng a technician
            var activeStatusIds = new List<int>
            {
                TechnicalServiceRequestStatusEnum.PENDING,
                TechnicalServiceRequestStatusEnum.OPEN,
                TechnicalServiceRequestStatusEnum.ONGOING
            };


            // Build a set of technicians currently busy with active requests
            var busyTechnicianIds = _db.TechnicalServiceRequests
                .Where(r =>
                    r.TechnicalServiceRequestStatusId.HasValue &&
                    activeStatusIds.Contains(r.TechnicalServiceRequestStatusId.Value) &&
                    // Check if the request is scheduled on the same date as the new request
                    (DbFunctions.TruncateTime(r.TechnicalServiceRequestScheduledDate) == DbFunctions.TruncateTime(scheduleDate) &&
                        (
                            r.TechnicalServiceRequestScheduledStartTime.HasValue &&
                            r.TechnicalServiceRequestScheduledEndTime.HasValue &&
                            startTime.HasValue &&
                            endTime.HasValue
                        ) &&
                        (
                           // Check if the scheduled time overlaps with the new request's schedule
                           (startTime >= r.TechnicalServiceRequestScheduledStartTime && startTime < r.TechnicalServiceRequestScheduledEndTime) ||
                           (endTime > r.TechnicalServiceRequestScheduledStartTime && endTime <= r.TechnicalServiceRequestScheduledEndTime) ||
                           (startTime <= r.TechnicalServiceRequestScheduledStartTime && endTime >= r.TechnicalServiceRequestScheduledEndTime)
                        )
                    )
                )
                .Select(r => r.TechnicalServiceRequestHistories
                    .OrderByDescending(h => h.UpdatedAt)
                    .Select(h => h.ActionTakenByRegistrationId)
                    .FirstOrDefault())
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Distinct()
                .ToList();

            // Pick the least recently assigned (fairness), then by Id.
            var availableTechnicianId = _db.Registrations
                .Where(r => r.AccountType == itAccountTypeName)
                .Where(r => !r.ITAvailabilities.Any(a =>
                    DbFunctions.TruncateTime(a.BlockDate) == DbFunctions.TruncateTime(scheduleDate)))
                .Where(r => !busyTechnicianIds.Contains(r.Id))
                .Select(r => new
                {
                    r.Id,
                    LastAssignedAt = _db.TechnicalServiceRequestHistories
                        .Where(h => h.ActionTakenByRegistrationId == r.Id)
                        .Select(h => (DateTime?)h.UpdatedAt)
                        .Max()
                })
                .OrderBy(r => r.LastAssignedAt ?? DateTime.MinValue)
                .ThenBy(r => r.Id)
                .Select(r => (int?)r.Id)
                .FirstOrDefault();

            return availableTechnicianId;
        }

        private void CreateEquipmentRepairTroubleshootingRequest(ref TechnicalServiceRequest technicalServiceRequest, ref bool isQueued)
        {
            int? availableTechnicianId = GetAvailableTechnician();
            if (availableTechnicianId.HasValue)
            {
                // If there is available technician, create a new history record
                technicalServiceRequest.TechnicalServiceRequestHistories = new List<TechnicalServiceRequestHistory>
                {
                    new TechnicalServiceRequestHistory
                    {
                        ActionTakenByRegistrationId = availableTechnicianId,
                        TechnicalServiceRequestStatusId = (int)TechnicalServiceRequestStatusEnum.ONGOING,
                        DateAction = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }
                };
                technicalServiceRequest.TechnicalServiceRequestStatusId =
                    (int)TechnicalServiceRequestStatusEnum.ONGOING;
            }
            else
            {
                isQueued = true;
                technicalServiceRequest.TechnicalServiceRequestStatusId =
                    (int)TechnicalServiceRequestStatusEnum.PENDING;
            }

            // Invalidate schedule fields
            technicalServiceRequest.TechnicalServiceRequestScheduledDate = null;
            technicalServiceRequest.TechnicalServiceRequestScheduledStartTime = null;
            technicalServiceRequest.TechnicalServiceRequestScheduledEndTime = null;
        }

        private void CreateScheduleControlProcessRequest(ref TechnicalServiceRequest technicalServiceRequest)
        {
            var scheduledDate = technicalServiceRequest.TechnicalServiceRequestScheduledDate;
            if (!scheduledDate.HasValue)
            {
                throw new Exception("Schedule is not defined.");
            }

            var serviceType = technicalServiceRequest.TechnicalServiceTypeId;
            if (!serviceType.HasValue)
            {
                throw new Exception("Please select a valid service type.");
            }

            var newScheduledDate = technicalServiceRequest.TechnicalServiceRequestScheduledDate;
            var newStartTime = technicalServiceRequest.TechnicalServiceRequestScheduledStartTime;
            var newEndTime = technicalServiceRequest.TechnicalServiceRequestScheduledEndTime;

            // Get all schedules on the same day
            var sameDaySchedules = _db.TechnicalServiceRequests
                .Where(i => i.TechnicalServiceRequestScheduledDate == newScheduledDate)
                .ToList();

            // Check whether the schedule is not conflictiing with other requests
            var isConflicting = sameDaySchedules.Any(i =>
                (newStartTime >= i.TechnicalServiceRequestScheduledStartTime && newStartTime < i.TechnicalServiceRequestScheduledEndTime) ||
                (newEndTime > i.TechnicalServiceRequestScheduledStartTime && newEndTime <= i.TechnicalServiceRequestScheduledEndTime) ||
                (newStartTime <= i.TechnicalServiceRequestScheduledStartTime && newEndTime >= i.TechnicalServiceRequestScheduledEndTime)
            );
            if (isConflicting == true)
            {
                throw new Exception("Selected Date and Time is already reserved. Please select another schedule.");
            }

            // Check for limits per day
            switch (serviceType)
            {
                case (int)TechnicalServiceTypeEnum.ZOOM_WEBEX_LINK:
                    if (sameDaySchedules.Count(i => i.TechnicalServiceTypeId == serviceType) > 4)
                    {
                        throw new Exception("The maximum number of requests for Zoom/Webex Link on the selected date has been reached. Please select another schedule.");
                    }
                    break;
                case (int)TechnicalServiceTypeEnum.LIVESTREAM_SETUP:
                    if (sameDaySchedules.Count(i => i.TechnicalServiceTypeId == serviceType) >= 2)
                    {
                        throw new Exception("The maximum number of requests for Livestream Setup on the selected date has been reached. Please select another schedule.");
                    }
                    break;
                case (int)TechnicalServiceTypeEnum.AUDIO_VISUAL_SETUP:
                    if (sameDaySchedules.Count(i => i.TechnicalServiceTypeId == serviceType) >= 3)
                    {
                        throw new Exception("The maximum number of requests for Audio Visual Setup on the selected date has been reached. Please select another schedule.");
                    }
                    break;
            }

            // If schedule is valid, assign to technician and create history record
            var assignedTechnician = GetAvailableTechnicianOnSchedule(newScheduledDate, newStartTime, newEndTime);
            if (!assignedTechnician.HasValue)
            {
                throw new Exception("No available technicians on the selected schedule. Please select another schedule.");
            }

            technicalServiceRequest.TechnicalServiceRequestHistories = new List<TechnicalServiceRequestHistory>
            {
               // Create a history record for the assigned technician with a status of "Pending" since the action is not yet taken
                new TechnicalServiceRequestHistory
                {
                    ActionTakenByRegistrationId = assignedTechnician,
                    ActionTaken = "",
                    DateAction = technicalServiceRequest.TechnicalServiceRequestScheduledDate.Value
                        .Add(technicalServiceRequest.TechnicalServiceRequestScheduledStartTime.Value),
                    UpdatedAt = DateTime.Now,
                    TechnicalServiceRequestStatusId = (int)TechnicalServiceRequestStatusEnum.PENDING
                }
            };
            technicalServiceRequest.TechnicalServiceRequestStatusId =
                (int)TechnicalServiceRequestStatusEnum.PENDING;
        }

        private void CreateNonAssistedRequest(ref TechnicalServiceRequest technicalServiceRequest)
        {
            // Assign an automatic status of "Ongoing" 
            technicalServiceRequest.TechnicalServiceRequestStatusId =
                (int)TechnicalServiceRequestStatusEnum.ONGOING;

            // Invalidate schedule fields
            technicalServiceRequest.TechnicalServiceRequestScheduledDate = null;
            technicalServiceRequest.TechnicalServiceRequestScheduledStartTime = null;
            technicalServiceRequest.TechnicalServiceRequestScheduledEndTime = null;
        }

        #endregion

    }
}
