using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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
                throw new HttpException(403, "Forbidden");
            }

            ViewBag.CurrentUser = currentUser;
            return View();
        }

        // GET: TechnicalServiceRequests/Details/5
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD, AccountTypeEnum.IT, AccountTypeEnum.ADMIN })]
        public ActionResult Details(int id)
        {
            if (id < 1)
            {
                throw new HttpException(403, "Forbidden");
            }

            var currentUser = GetUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            var technicalServiceRequest = _db.TechnicalServiceRequests
                .Include(t => t.ClientRegistration)
                .Include(t => t.TechnicalServiceType)
                .Include(t => t.TechnicalServiceRequestSeverity)
                .Include(t => t.TechnicalServiceRequestStatus)
                .Include(t => t.TechnicalServiceRequestEquipment)
                .Include(t => t.ScheduledControlProcessDetail)
                .Include(t => t.TechnicalServiceRequestHistories
                    .Select(h => h.ActionTakenByRegistration))
                .Include(t => t.TechnicalServiceRequestHistories
                    .Select(h => h.TechnicalServiceRequestStatus))
                .FirstOrDefault(t => t.Id == id);
            if (technicalServiceRequest == null)
            {
                throw new HttpException(404, "Not found");
            }

            var isQueued = false;
            if (technicalServiceRequest.TechnicalServiceRequestStatusId == (int)TechnicalServiceRequestStatusEnum.PENDING)
            {
                isQueued = _db.TechnicalServiceRequestQueues
                    .Any(q => q.TechnicalServiceRequestId == id && !q.IsProcessed);
            }

            if (!AccountTypeEnum.IsAdmin(currentUser.PrivilegeIds))
            {
                var involvedTechnicianIds = technicalServiceRequest.TechnicalServiceRequestHistories
                    .Select(t => t.ActionTakenByRegistrationId)
                    .ToList();

                var isAssistedRequest = technicalServiceRequest.TechnicalServiceTypeId.HasValue &&
                    !TechnicalServiceTypeEnum.IsNonAssistedRequest(technicalServiceRequest.TechnicalServiceTypeId.Value);

                var isRequestClient = AccountTypeEnum.IsStandard(currentUser.PrivilegeIds) &&
                    currentUser.Id == technicalServiceRequest.ClientRegistrationId;

                // IT can view if: (non-assisted) OR (assisted AND involved)
                var isIT = AccountTypeEnum.IsIT(currentUser.PrivilegeIds);
                var isInvolvedTechnician = isAssistedRequest && isIT && involvedTechnicianIds.Contains(currentUser.Id);
                var isNonAssistedAndIT = !isAssistedRequest && isIT;

                if (!isRequestClient && !isInvolvedTechnician && !isNonAssistedAndIT)
                {
                    return RedirectToAction("Index");
                }
            }

            // Cast technical service request to details view model 
            var casted = TechnicalServiceRequestTypeCaster
                .ToTechnicalServiceRequestDetailsViewModel(technicalServiceRequest);

            // Get the latest history with a completed status to determine if the form can be generated
            var completedStatusIds = TechnicalServiceRequestStatusEnum.GetCompletedStatusIds();
            if (technicalServiceRequest.TechnicalServiceRequestStatusId.HasValue &&
                completedStatusIds.Contains(technicalServiceRequest.TechnicalServiceRequestStatusId.Value))
            {
                var lastFormGeneratableHistory = technicalServiceRequest.TechnicalServiceRequestHistories
                    .LastOrDefault(h =>
                        h.TechnicalServiceRequestStatusId.HasValue &&
                        completedStatusIds.Contains(h.TechnicalServiceRequestStatusId.Value)
                    );
                if (lastFormGeneratableHistory != null)
                {
                    casted.TechnicalServiceRequestFormGeneratableHistoryId = lastFormGeneratableHistory.Id;
                }
            }

            ViewBag.CurrentUser = currentUser;
            ViewBag.IsQueued = isQueued;
            return View(casted);
        }

        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD, AccountTypeEnum.IT, AccountTypeEnum.ADMIN })]
        public ActionResult Form(int id) // <- The Id here is the TechnicalServiceRequestHistory Id
        {
            if (id < 1)
            {
                throw new HttpException(403, "Forbidden");
            }

            var currentUser = GetUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            var technicalServiceRequestHistory = _db.TechnicalServiceRequestHistories
                .Include(h => h.TechnicalServiceRequest)
                .Include(h => h.TechnicalServiceRequest.ScheduledControlProcessDetail)
                .Include(h => h.TechnicalServiceRequest.ClientRegistration)
                .FirstOrDefault(h => h.Id == id);
            var technicalServiceRequest = technicalServiceRequestHistory?.TechnicalServiceRequest;
            if (technicalServiceRequest == null || technicalServiceRequestHistory == null)
            {
                throw new HttpException(404, "Not found");
            }

            if (!AccountTypeEnum.IsAdmin(currentUser.PrivilegeIds))
            {
                var isAssistedRequest = technicalServiceRequest.TechnicalServiceTypeId.HasValue &&
                  !TechnicalServiceTypeEnum.IsNonAssistedRequest(technicalServiceRequest.TechnicalServiceTypeId.Value);

                var isRequestClient = AccountTypeEnum.IsStandard(currentUser.PrivilegeIds) &&
                    currentUser.Id == technicalServiceRequest.ClientRegistrationId;

                // IT can view if: (non-assisted) OR (assisted AND involved)
                var isIT = AccountTypeEnum.IsIT(currentUser.PrivilegeIds);
                var isInvolvedTechnician = isAssistedRequest && isIT && technicalServiceRequestHistory.ActionTakenByRegistrationId == currentUser.Id;
                var isNonAssistedAndIT = !isAssistedRequest && isIT;

                if (!isRequestClient && !isInvolvedTechnician && !isNonAssistedAndIT)
                {
                    return RedirectToAction("Index");
                }
            }

            return View(new TechnicalServiceRequestFormViewModel
            {
                TechnicalServiceRequest = technicalServiceRequest,
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
                throw new HttpException(404, "Not found");
            }

            ViewBag.CurrentUser = currentUser;
            return View(new TechnicalServiceRequestCreateViewModel()
            {
                ClientRegistrationId = currentUser.Id,
                ClientFirstName = currentUser.FirstName,
                ClientLastName = currentUser.LastName,
                ClientMiddleName = currentUser.MiddleName,
                ClientExtensionName = currentUser.ExtensionName,
                ClientEmailAddress = currentUser.Email,
                ClientContactNumber = currentUser.ContactNumber,
                ClientOffice = currentUser.Office,
                ClientPosition = currentUser.Position
            });
        }

        // POST: TechnicalServiceRequests/Create
        [Authorize2]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD })]
        public ActionResult Create(TechnicalServiceRequestCreateViewModel technicalServiceRequestCreateViewModel)
        {
            var currentUser = GetUserSession();
            if (currentUser == null)
            {
                throw new HttpException(404, "Not found");
            }
            ViewBag.CurrentUser = currentUser;

            if (!ModelState.IsValid)
            {
                var errors = GetModelStateErrors();
                Log.Warning($"Model state is invalid: {errors}");
                return View(technicalServiceRequestCreateViewModel);
            }

            // Use a transaction to ensure data integrity
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var isQueued = false;
                    Equipment updatedEquipment = null;

                    var clientInfo = _db.Registrations
                        .Where(r => r.Id == technicalServiceRequestCreateViewModel.ClientRegistrationId)
                        .FirstOrDefault();
                    if (clientInfo == null)
                    {
                        throw new Exception("Client not found.");
                    }

                    var technicalServiceRequest = TechnicalServiceRequestTypeCaster
                        .ToTechnicalServiceRequest(technicalServiceRequestCreateViewModel);
                    // Invalidate severity if user selected "Not Applicable" (option value -1)
                    if (technicalServiceRequest.TechnicalServiceRequestSeverityId < 1)
                    {
                        technicalServiceRequest.TechnicalServiceRequestSeverityId = null;
                    }
                    technicalServiceRequest.ReferenceCode = GenerateReferenceCode();
                    technicalServiceRequest.DateRequest = DateTime.Now;

                    // Trim and uppercase information for consistency
                    technicalServiceRequest.Others = !string.IsNullOrWhiteSpace(technicalServiceRequest.Others)
                        ? technicalServiceRequest.Others.Trim().ToUpperInvariant()
                        : string.Empty;

                    var selectedTechnicalServiceType = technicalServiceRequest.TechnicalServiceTypeId;

                    if (selectedTechnicalServiceType.HasValue)
                    {
                        // Invalidate "Others" if service type is specified
                        technicalServiceRequest.Others = string.Empty;

                        // Implement different logic based on the selected technical service type
                        if (TechnicalServiceTypeEnum.IsRepairTroubleshootingRequest(selectedTechnicalServiceType.Value))
                        {
                            try { CreateEquipmentRepairTroubleshootingRequest(ref technicalServiceRequest, ref isQueued, out updatedEquipment); }
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
                        // Invalidate service type
                        technicalServiceRequest.TechnicalServiceTypeId = null;
                        technicalServiceRequest.TechnicalServiceType = null;

                        // Default to Equipment Repair Troubleshooting logic
                        CreateEquipmentRepairTroubleshootingRequest(ref technicalServiceRequest, ref isQueued, out updatedEquipment);
                    }
                    else
                    {
                        TempData["alertModal"] = new AlertModalUtility()
                        {
                            Title = "Invalid Service Type",
                            Message = "Please select a valid service type or specify the details in the 'Others' field.",
                            Status = AlertModalStatus.Error
                        };
                        return View(technicalServiceRequestCreateViewModel);
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

                    // Refresh the equipment hub when the repair request changes an equipment's status/count
                    if (updatedEquipment != null)
                    {
                        EquipmentHub.RefreshEquipmentStatus(
                            updatedEquipment.Id,
                            EquipmentStatusEnum.DisplayName(updatedEquipment.EquipmentStatusId ?? 0)
                        );
                        EquipmentHub.RefreshEquipmentRepairCount(updatedEquipment.Id, updatedEquipment.RepairCount);
                    }

                    // Refresh the equipment list so any new/updated equipment is reflected immediately
                    EquipmentHub.RefreshEquipmentList();

                    if (!isQueued)
                    {
                        var notificationService = new NotificationService();
                        if (
                            selectedTechnicalServiceType.HasValue &&
                            TechnicalServiceTypeEnum.IsNonAssistedRequest(
                                selectedTechnicalServiceType.Value
                            )
                        )
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
                        Log.Information($"Request with reference code {technicalServiceRequest.ReferenceCode} has been created and assigned to a technician.");
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
                        Log.Information($"Request with reference code {technicalServiceRequest.ReferenceCode} has been queued.");
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
                    Log.Error(ex, $"An error occured while user with ID {currentUser.Id} was creating a request.");
                    ModelState.AddModelError("", "An error occurred while making a request: " + ex.Message);
                }
            }
            return View(technicalServiceRequestCreateViewModel);
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
                IQueryable<TechnicalServiceRequest> query = _db.TechnicalServiceRequests
                    .Include(r => r.ClientRegistration)
                    .AsQueryable();
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

                    /**
                     * IT can see requests:
                     * - They have taken action on (based on TechnicalServiceRequestHistories)
                     * - Non-Assisted requests
                     */
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
                        .Where(t => t.ClientRegistrationId == associatedUser.Id);
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
                        var nonAssistedServices = TechnicalServiceTypeEnum.GetNonAssistedServiceIds();
                        query = query.Where(i =>
                            i.TechnicalServiceTypeId.HasValue &&
                            !nonAssistedServices.Contains(i.TechnicalServiceTypeId.Value) &&
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
                    else if (typeFilter == "queued")
                    {
                        var queuedRequestIds = _db.TechnicalServiceRequestQueues
                            .Include(q => q.TechnicalServiceRequest)
                            .Where(q =>
                                !q.IsProcessed &&
                                 q.TechnicalServiceRequest.TechnicalServiceRequestStatusId.HasValue &&
                                 q.TechnicalServiceRequest.TechnicalServiceRequestStatusId.Value == (int)TechnicalServiceRequestStatusEnum.PENDING)
                            .Select(q => q.TechnicalServiceRequestId)
                            .ToList();

                        // Reset base query to fetch only queued requests
                        query = _db.TechnicalServiceRequests
                            .Include(t => t.TechnicalServiceType)
                            .Include(t => t.TechnicalServiceRequestStatus)
                            .Where(i =>
                                queuedRequestIds.Contains(i.Id) &&
                                i.TechnicalServiceRequestStatusId.HasValue &&
                                i.TechnicalServiceRequestStatusId.Value == (int)TechnicalServiceRequestStatusEnum.PENDING
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
                        t.ClientRegistration.FirstName.Contains(searchValue) ||
                        t.ClientRegistration.LastName.Contains(searchValue) ||
                        t.ClientRegistration.MiddleName.Contains(searchValue) ||
                        t.ClientRegistration.ExtensionName.Contains(searchValue) ||
                        t.ClientRegistration.Email.Contains(searchValue) ||
                        t.ClientRegistration.ContactNumber.Contains(searchValue) ||
                        t.ClientRegistration.Office.Contains(searchValue) ||
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
                        case 1: // Reference Code
                            query = sortDirection == "asc"
                                ? query.OrderBy(t => t.ReferenceCode)
                                : query.OrderByDescending(t => t.ReferenceCode);
                            break;
                        case 2: // Client Name
                            query = sortDirection == "asc"
                                ? query.OrderBy(t => t.ClientRegistration.LastName)
                                : query.OrderByDescending(t => t.ClientRegistration.LastName);
                            break;
                        case 3: // Service Type
                            query = sortDirection == "asc"
                                ? query.OrderBy(t => t.TechnicalServiceType.TechnicalServiceTypeName)
                                : query.OrderByDescending(t => t.TechnicalServiceType.TechnicalServiceTypeName);
                            break;
                        case 4: // Status
                            query = sortDirection == "asc"
                                ? query.OrderBy(t => t.TechnicalServiceRequestStatus.TechnicalServiceRequestStatusName)
                                : query.OrderByDescending(t => t.TechnicalServiceRequestStatus.TechnicalServiceRequestStatusName);
                            break;
                        case 5: // Date Requested
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
                        t.ClientRegistration.FirstName,
                        t.ClientRegistration.MiddleName,
                        t.ClientRegistration.LastName,
                        t.ClientRegistration.ExtensionName,
                        t.ClientRegistration.Email,
                        t.ClientRegistration.ContactNumber,
                        t.ClientRegistration.Office,
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
                Log.Error(ex, $"An error occurred while user was fetching technical service requests with ID {GetUserSession()?.Id.ToString() ?? "Unknown"}.");
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize2]
        [HttpGet]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD, AccountTypeEnum.IT, AccountTypeEnum.ADMIN })]
        public JsonResult GetFormGenerationState(int id)
        {
            try
            {
                if (id < 1)
                {
                    throw new HttpException(404, "Not found");
                }

                var currentUser = GetUserSession();
                if (currentUser == null)
                {
                    throw new HttpException(403, "Forbidden");
                }

                var technicalServiceRequest = _db.TechnicalServiceRequests
                    .Include(t => t.TechnicalServiceRequestHistories)
                    .FirstOrDefault(t => t.Id == id);
                if (technicalServiceRequest == null)
                {
                    throw new HttpException(404, "Not found");
                }

                if (!AccountTypeEnum.IsAdmin(currentUser.PrivilegeIds))
                {
                    var involvedTechnicianIds = technicalServiceRequest.TechnicalServiceRequestHistories
                        .Where(h => h.ActionTakenByRegistrationId.HasValue)
                        .Select(h => h.ActionTakenByRegistrationId.Value)
                        .ToList();

                    var isAssistedRequest = technicalServiceRequest.TechnicalServiceTypeId.HasValue &&
                        !TechnicalServiceTypeEnum.IsNonAssistedRequest(technicalServiceRequest.TechnicalServiceTypeId.Value);

                    var isRequestClient = AccountTypeEnum.IsStandard(currentUser.PrivilegeIds) &&
                        currentUser.Id == technicalServiceRequest.ClientRegistrationId;

                    var isIT = AccountTypeEnum.IsIT(currentUser.PrivilegeIds);
                    var isInvolvedTechnician = isAssistedRequest && isIT && involvedTechnicianIds.Contains(currentUser.Id);
                    var isNonAssistedAndIT = !isAssistedRequest && isIT;

                    if (!isRequestClient && !isInvolvedTechnician && !isNonAssistedAndIT)
                    {
                        throw new HttpException(403, "Forbidden");
                    }
                }

                var completedStatusIds = TechnicalServiceRequestStatusEnum.GetCompletedStatusIds();
                var isFormGeneratable = technicalServiceRequest.TechnicalServiceRequestStatusId.HasValue &&
                    completedStatusIds.Contains(technicalServiceRequest.TechnicalServiceRequestStatusId.Value);

                var formGeneratableHistoryId = technicalServiceRequest.TechnicalServiceRequestHistories
                    .Where(h => h.TechnicalServiceRequestStatusId.HasValue &&
                        completedStatusIds.Contains(h.TechnicalServiceRequestStatusId.Value))
                    .OrderByDescending(h => h.UpdatedAt)
                    .Select(h => (int?)h.Id)
                    .FirstOrDefault();

                return Json(new
                {
                    success = true,
                    isFormGeneratable = isFormGeneratable && formGeneratableHistoryId.HasValue,
                    formLink = formGeneratableHistoryId.HasValue
                        ? Url.Action("Form", "TechnicalServiceRequests", new { id = formGeneratableHistoryId.Value })
                        : "#"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occurred while checking form generation state for request ID {id}.");
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize2]
        [HttpGet]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD, AccountTypeEnum.IT, AccountTypeEnum.ADMIN })]
        public JsonResult GetEquipmentDetailByAssetTag(string assetTag)
        {
            try
            {
                var equipmentService = new EquipmentService();

                var normalizedAssetTag = equipmentService.NormalizeAssetTag(assetTag);
                if (string.IsNullOrEmpty(normalizedAssetTag))
                {
                    throw new Exception("Invalid equipment asset tag.");
                }

                var activeStatusIds = EquipmentStatusEnum.GetActiveIds();

                var equipments = equipmentService
                    .GetListEquipmentByAssetTag(normalizedAssetTag, exactMatch: false)
                    .Where(e =>
                        e.IsActive &&
                        e.EquipmentStatusId.HasValue &&
                        activeStatusIds.Contains(e.EquipmentStatusId.Value)
                    )
                    .Select(e => new
                    {
                        e.Id,
                        e.EquipmentModel,
                        e.AssetTag,
                        e.EquipmentTypeId
                    })
                    .ToList();

                Log.Information($"Successfully retrieved equipment details for asset tag {normalizedAssetTag}. Retrieved {equipments.Count} equipment(s).");
                return Json(new
                {
                    success = true,
                    data = equipments
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occurred while retrieving equipment details for asset tag {assetTag}.");
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize2]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD })]
        public ActionResult EditDescription(TechnicalServiceRequest technicalServiceRequestParam, int id)
        {
            try
            {
                if (id < 1)
                {
                    throw new Exception("Invalid request ID.");
                }

                if (!ModelState.IsValid)
                {
                    var errors = GetModelStateErrors();
                    Log.Warning($"Model state is invalid: {errors}");
                    throw new Exception("An error occured.");
                }

                var currentUser = GetUserSession();
                if (currentUser == null)
                {
                    throw new Exception("User not found.");
                }

                var technicalServiceRequest = _db.TechnicalServiceRequests
                    .Include(r => r.ClientRegistration)
                    .Include(r => r.TechnicalServiceRequestHistories)
                    .FirstOrDefault(r => r.Id == id);
                if (technicalServiceRequest == null)
                {
                    throw new Exception("Technical service request not found.");
                }

                if (currentUser.Id != technicalServiceRequest.ClientRegistrationId)
                {
                    throw new Exception("You are not allowed to perform this action.");
                }

                technicalServiceRequest.TechnicalServiceRequestDescription = technicalServiceRequestParam.TechnicalServiceRequestDescription?.Trim();
                _db.Entry(technicalServiceRequest).State = EntityState.Modified;
                _db.SaveChanges();

                TechnicalServiceRequestHub.RefreshTechnicalServiceRequestDescription(
                    technicalServiceRequest.Id,
                    technicalServiceRequest.TechnicalServiceRequestDescription
                );

                var latestTechnicianId = technicalServiceRequest.TechnicalServiceRequestHistories
                    .OrderByDescending(h => h.UpdatedAt)
                    .Select(h => h.ActionTakenByRegistrationId)
                    .FirstOrDefault();
                var serviceType = technicalServiceRequest.TechnicalServiceTypeId.HasValue
                    ? technicalServiceRequest.TechnicalServiceTypeId.Value
                    : 0;
                if (latestTechnicianId.HasValue && serviceType > 0)
                {
                    /**
                     * Notify the assigned technician about the updated description if the request is an assisted service, otherwise,
                     * notify all technicians about the updated description for non-assisted service
                     */
                    var isNonAssisted = TechnicalServiceTypeEnum.IsNonAssistedRequest(serviceType);
                    var firstParam = !isNonAssisted
                        ? latestTechnicianId
                        : null;

                    var notificationService = new NotificationService();
                    notificationService.NotifyTechnicianDescriptionUpdate(firstParam, technicalServiceRequest.ReferenceCode);
                }

                TempData["alertModal"] = new AlertModalUtility()
                {
                    Title = "Success",
                    Message = "Description updated successfully.",
                    Status = AlertModalStatus.Success
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occurred while user with ID {GetUserSession()?.Id.ToString() ?? "Unknown"} was editing description of request with ID {id}.");
                ModelState.AddModelError("", "An error occurred while making a request: " + ex.Message);
                TempData["alertModal"] = new AlertModalUtility()
                {
                    Title = "Error",
                    Message = "An error occurred while updating description. Please try again.",
                    Status = AlertModalStatus.Error
                };
            }

            return RedirectToAction("Details", new { id = id });
        }

        [Authorize2]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD })]
        public ActionResult CancelRequest(int id)
        {
            try
            {
                if (id < 1)
                {
                    throw new Exception("Invalid request ID.");
                }

                if (!ModelState.IsValid)
                {
                    var errors = GetModelStateErrors();
                    Log.Warning($"Model state is invalid: {errors}");
                    throw new Exception("An error occured.");
                }

                var technicalServiceRequest = _db.TechnicalServiceRequests.Find(id);
                if (technicalServiceRequest == null)
                {
                    throw new Exception("Technical service request not found.");
                }

                var client = _db.Registrations
                    .Where(i => i.Id == technicalServiceRequest.ClientRegistrationId)
                    .Select(i => new { i.Id, i.Email })
                    .FirstOrDefault();
                if (client == null)
                {
                    throw new Exception("Client not found.");
                }

                /**
                 * Throw an error if the ID of the logged in user does not match the ID 
                 * associated with the request, to prevent unauthorized cancellation of requests
                 */
                if (client.Id != technicalServiceRequest.ClientRegistrationId)
                {
                    throw new Exception("Your are not allowed to perform this action.");
                }

                var currentStatus = technicalServiceRequest.TechnicalServiceRequestStatusId;
                if (!currentStatus.HasValue)
                {
                    throw new Exception("Status not found.");
                }

                var currentStatusName = technicalServiceRequest.TechnicalServiceRequestStatus.TechnicalServiceRequestStatusName;

                // Check is current status can still be cancelled
                var cancellableStatus = TechnicalServiceRequestStatusEnum.GetCancellableStatusIds();
                if (!cancellableStatus.Contains(currentStatus.Value))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Your request cannot be cancelled as it is already in " + currentStatusName + " status.",
                    });
                }

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

                // Only status can be editted
                var newStatus = (int)TechnicalServiceRequestStatusEnum.CANCELLED;
                technicalServiceRequest.TechnicalServiceRequestStatusId = newStatus;
                _db.Entry(technicalServiceRequest).State = EntityState.Modified;
                _db.SaveChanges();

                // Notify all connected clients about the cancelled request
                TechnicalServiceRequestHub.RefreshTechnicalServiceRequestList();
                TechnicalServiceRequestHub.RefreshTechnicalServiceRequestStatus(
                    technicalServiceRequest.Id,
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

                Log.Information($"Request with reference code {technicalServiceRequest.ReferenceCode} has been cancelled by client with email {client.Email}.");
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
                Log.Error(ex, $"An error occurred while user with ID {GetUserSession()?.Id.ToString() ?? "Unknown"} was cancelling request with ID {id}.");
                ModelState.AddModelError("", "An error occurred while making a request: " + ex.Message);
                return Json(new
                {
                    success = false,
                    message = "An error occurred while making a request. Please try again.",
                });
            }
        }

        [Authorize2]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.IT })]
        public ActionResult UpdateSeverity(int id, int severityId)
        {
            try
            {
                if (severityId < TechnicalServicRequestSeverityEnum.LOW ||
                    severityId > TechnicalServicRequestSeverityEnum.CRITICAL)
                {
                    throw new Exception("Invalid severity level.");
                }

                var currentUser = GetUserSession();
                if (currentUser == null)
                {
                    throw new Exception("User not found.");
                }

                var technicalServiceRequest = _db.TechnicalServiceRequests
                    .Include(t => t.TechnicalServiceRequestHistories)
                    .FirstOrDefault(t => t.Id == id);
                if (technicalServiceRequest == null)
                {
                    throw new Exception("Technical service request not found.");
                }

                /**
                 * Check if the current user has acted on the technical service request,
                 * if not, throw an error to prevent unauthorized severity update.
                 */
                var involvedTechnicianIds = technicalServiceRequest.TechnicalServiceRequestHistories
                    .Select(h => h.ActionTakenByRegistrationId)
                    .ToList();
                if (!involvedTechnicianIds.Contains(currentUser.Id))
                {
                    throw new Exception("You are not allowed to perform this action.");
                }

                var currentSeverityId = technicalServiceRequest.TechnicalServiceRequestSeverityId;
                if (severityId != currentSeverityId)
                {
                    if (technicalServiceRequest.TechnicalServiceRequestStatusId.HasValue &&
                        technicalServiceRequest.TechnicalServiceRequestStatusId.Value == (int)TechnicalServiceRequestStatusEnum.CANCELLED ||
                        technicalServiceRequest.TechnicalServiceRequestStatusId.Value == (int)TechnicalServiceRequestStatusEnum.CLOSED)
                    {
                        throw new Exception("You cannot update severity when the status is already cancelled / closed.");
                    }

                    technicalServiceRequest.TechnicalServiceRequestSeverityId = severityId;
                    _db.Entry(technicalServiceRequest).State = EntityState.Modified;

                    // Notify all connected clients about the updated severity
                    TechnicalServiceRequestHub.RefreshTechnicalServiceRequestSeverity(
                        technicalServiceRequest.Id,
                        technicalServiceRequest.TechnicalServiceRequestSeverity.SeverityName
                    );

                    // Notify client about the updated severity
                    var notificationService = new NotificationService();
                    _db.Notifications.Add(new Notification()
                    {
                        RecipientRegistrationId = technicalServiceRequest.ClientRegistrationId,
                        Title = "Severity Updated: " + technicalServiceRequest.ReferenceCode,
                        Message = notificationService.BuildRecipientMessageFromRequestSeverity(
                                severityId, technicalServiceRequest.ReferenceCode, currentSeverityId
                            ),
                        IsRead = false,
                        CreatedAt = DateTime.Now,
                    });
                    notificationService.RefreshUserUi(technicalServiceRequest.ClientRegistrationId);
                }

                _db.SaveChanges();

                Log.Information($"Severity level of request with reference code {technicalServiceRequest.ReferenceCode} has been updated to {TechnicalServicRequestSeverityEnum.DisplayName(severityId)} by user with ID {currentUser.Id}.");
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
                Log.Error(ex, $"An error occurred while user with ID {GetUserSession()?.Id.ToString() ?? "Unknown"} was updating severity of request with ID {id}.");
                ModelState.AddModelError("", "An error occurred while making a request: " + ex.Message);

                return Json(new
                {
                    success = false,
                    message = "An error occurred while making a request. Please try again.",
                });
            }
        }

        [Authorize2]
        [HttpGet]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD })]
        public ActionResult GetFullyBookedDayByLimit(int scheduleServiceTypeId)
        {
            try
            {
                var validScheduleServiceTypeIds = new int[]
                {
                    TechnicalServiceTypeEnum.AUDIO_VISUAL_SETUP,
                    TechnicalServiceTypeEnum.LIVESTREAM_SETUP,
                    TechnicalServiceTypeEnum.ZOOM_WEBEX_LINK
                };
                if (!validScheduleServiceTypeIds.Contains(scheduleServiceTypeId))
                {
                    throw new Exception("Invalid schedule service proccess Id.");
                }

                var startDate = DateTime.Today;
                var endDate = startDate.AddDays(30);     // Check for the next 30 days

                // Check per-day-limit
                var selectedTypeId = 0;
                var selectedTypeLimit = 0;
                switch (scheduleServiceTypeId)
                {
                    case TechnicalServiceTypeEnum.AUDIO_VISUAL_SETUP:
                        selectedTypeId = (int)TechnicalServiceTypeEnum.AUDIO_VISUAL_SETUP;
                        selectedTypeLimit = (int)TechnicalServiceRequestScheduleLimitEnum.AUDIO_VISUAL_SETUP;
                        break;
                    case TechnicalServiceTypeEnum.LIVESTREAM_SETUP:
                        selectedTypeId = (int)TechnicalServiceTypeEnum.LIVESTREAM_SETUP;
                        selectedTypeLimit = (int)TechnicalServiceRequestScheduleLimitEnum.LIVESTREAM_SETUP;
                        break;
                    case TechnicalServiceTypeEnum.ZOOM_WEBEX_LINK:
                        selectedTypeId = (int)TechnicalServiceTypeEnum.ZOOM_WEBEX_LINK;
                        selectedTypeLimit = (int)TechnicalServiceRequestScheduleLimitEnum.ZOOM_WEBEX_LINK;
                        break;
                    default:
                        throw new Exception("Invalid service type.");
                }

                var inactiveStatusIds = new int[]
                {
                    (int)TechnicalServiceRequestStatusEnum.RESOLVED,
                    (int)TechnicalServiceRequestStatusEnum.CANCELLED,
                    (int)TechnicalServiceRequestStatusEnum.CLOSED
                };

                /**
                 * Get fully booked days based on the count of scheduled requests for the selected
                 * service type, within the next 30 days, that have reached the per-day limit
                 */
                var fullyBookedStringDates = _db.TechnicalServiceRequests
                    .Where(r =>
                        r.TechnicalServiceRequestStatusId.HasValue &&
                        !inactiveStatusIds.Contains(r.TechnicalServiceRequestStatusId.Value) &&
                        r.TechnicalServiceTypeId == selectedTypeId &&
                        r.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledDate >= startDate &&
                        r.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledDate < endDate)
                    .GroupBy(r => DbFunctions.TruncateTime(r.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledDate))
                    .Where(g => g.Count() >= selectedTypeLimit)
                    .Select(g => g.Key)
                    .Where(date => date.HasValue)
                    .ToList()
                    .Select(date => date.Value.ToString("yyyy-MM-dd"))
                    .ToList();

                return Json(new
                {
                    success = true,
                    dates = fullyBookedStringDates
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occurred while user with ID {GetUserSession()?.Id.ToString() ?? "Unknown"} was fetching fully booked days by limit for service type ID {scheduleServiceTypeId}.");
                return Json(new
                {
                    success = false,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize2]
        [HttpGet]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD })]
        public ActionResult GetFullyBookedDayBySchedule()
        {
            try
            {
                var startDate = DateTime.Today;
                var endDate = startDate.AddDays(30);     // Check for the next 30 days
                var startTime = TimeSpan.FromHours(8);   // 8:00 AM
                var endTime = TimeSpan.FromHours(16);    // 4:00 PM

                var inactiveStatusIds = new int[]
                {
                     (int)TechnicalServiceRequestStatusEnum.RESOLVED,
                     (int)TechnicalServiceRequestStatusEnum.CANCELLED,
                     (int)TechnicalServiceRequestStatusEnum.CLOSED
                };

                var fullyBookedStringDates = new List<string>();

                /**
                 * Get fully booked days based on the schedule of requests for the selected service type, 
                 * within the next 30 days, that have no available time slots between 8:00 AM to 4:00 PM
                 */
                for (var date = startDate; date < endDate; date = date.AddDays(1))
                {
                    var thisDate = date;
                    var events = _db.TechnicalServiceRequests
                        .Where(r =>
                            // Fetch only requests that are active 
                            r.TechnicalServiceRequestStatusId.HasValue &&
                            !inactiveStatusIds.Contains(r.TechnicalServiceRequestStatusId.Value) &&
                            // Check if the request is scheduled on the same date as the new request
                            DbFunctions.TruncateTime(r.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledDate) ==
                            DbFunctions.TruncateTime(thisDate)
                        )
                        .Select(r => new
                        {
                            Start = r.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledStartTime,
                            End = r.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledEndTime
                        })
                        .OrderBy(e => e.Start)
                        .ToList();

                    var current = startTime;
                    bool hasGap = false;
                    foreach (var e in events)
                    {
                        /**
                         * If there is a gap of 1 hour or more between the current time and the start of the next event,
                         * consider the day as not fully booked and break the loop to check the next day
                         */
                        var totalGap = e.Start.Value.Subtract(current);
                        if (totalGap.TotalHours > 1)
                        {
                            hasGap = true;
                            break;
                        }

                        /**
                         * Move the current time to the end of the event
                         * if it is greater than the current time
                         */
                        if (e.End > current)
                        {
                            current = e.End.Value;
                        }

                    }
                    if (!hasGap && events.Any() && current >= endTime)
                    {
                        fullyBookedStringDates.Add(thisDate.ToString("yyyy-MM-dd"));
                    }
                }

                return Json(new
                {
                    success = true,
                    dates = fullyBookedStringDates
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occurred while user with ID {GetUserSession()?.Id.ToString() ?? "Unknown"} was fetching fully booked days by schedule.");
                return Json(new
                {
                    success = false,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
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

        public int? GetAvailableTechnician()
        {
            var itAccountTypeName = AccountTypeEnum.DisplayName(AccountTypeEnum.IT);

            var activeStatusIds = TechnicalServiceRequestStatusEnum.GetActiveStatusIds();
            var scheduledControlProcessRequestIds = TechnicalServiceTypeEnum.GetScheduledServiceIds();
            var nonAssistedRequestIds = TechnicalServiceTypeEnum.GetNonAssistedServiceIds();

            var today = DateTime.Now.Date;

            // Build a set of technicians currently busy with active requests
            var busyTechnicianIds = _db.TechnicalServiceRequests
                .Where(r =>
                    // Check only active requests
                    r.TechnicalServiceRequestStatusId.HasValue &&
                    activeStatusIds.Contains(r.TechnicalServiceRequestStatusId.Value) &&
                    // Check only assisted service requests
                    r.TechnicalServiceTypeId.HasValue &&
                    !nonAssistedRequestIds.Contains(r.TechnicalServiceTypeId.Value) &&
                    (
                        // Busy now
                        !scheduledControlProcessRequestIds.Contains(r.TechnicalServiceTypeId.Value)
                        ||
                        // Busy only on scheduled day
                        (r.ScheduledControlProcessDetail != null &&
                         r.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledDate.HasValue &&
                         DbFunctions.TruncateTime(r.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledDate.Value) == DbFunctions.TruncateTime(today))
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

            // Pick the least recently assigned, then by Id.
            var availableTechnicianId = _db.Registrations
               .Where(r =>
                   r.IsActive &&
                   r.AccountType == itAccountTypeName)
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

        public int? GetAvailableTechnicianOnSchedule(DateTime? scheduleDate, TimeSpan? startTime, TimeSpan? endTime)
        {
            if (scheduleDate == null || startTime == null || endTime == null)
            {
                throw new Exception("Schedule Date, Start Time, and End Time must be provided.");
            }

            var itAccountTypeName = AccountTypeEnum.DisplayName(AccountTypeEnum.IT);

            // Requests in these statuses are still actively consuming a technician
            var activeStatusIds = TechnicalServiceRequestStatusEnum.GetActiveStatusIds();
            var scheduledControlProcessRequestIds = TechnicalServiceTypeEnum.GetScheduledServiceIds();
            var nonAssistedRequestIds = TechnicalServiceTypeEnum.GetNonAssistedServiceIds();

            // Build a set of technicians currently busy with active requests
            var busyTechnicianIds = _db.TechnicalServiceRequests
                .Where(r =>
                    // Check only active requests
                    r.TechnicalServiceRequestStatusId.HasValue &&
                    activeStatusIds.Contains(r.TechnicalServiceRequestStatusId.Value) &&
                    // Check only scheduled control process requests
                    r.TechnicalServiceTypeId.HasValue &&
                    !nonAssistedRequestIds.Contains(r.TechnicalServiceTypeId.Value) &&
                    scheduledControlProcessRequestIds.Contains(r.TechnicalServiceTypeId.Value) &&
                    // Check if the request is scheduled on the same date as the new request
                    r.ScheduledControlProcessDetail != null &&
                    DbFunctions.TruncateTime(r.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledDate) == DbFunctions.TruncateTime(scheduleDate) &&
                    // Check if the scheduled time is overlapping with the new request's schedule
                    r.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledStartTime.HasValue &&
                    r.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledEndTime.HasValue &&
                    startTime.HasValue &&
                    endTime.HasValue &&
                    (
                        (startTime >= r.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledStartTime &&
                         startTime < r.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledEndTime) ||
                        (endTime > r.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledStartTime &&
                         endTime <= r.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledEndTime) ||
                        (startTime <= r.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledStartTime &&
                         endTime >= r.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledEndTime)
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
                .Where(r =>
                    r.IsActive &&
                    r.AccountType == itAccountTypeName
                )
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

        private void CreateEquipmentRepairTroubleshootingRequest(ref TechnicalServiceRequest technicalServiceRequest, ref bool isQueued, out Equipment updatedEquipment)
        {
            updatedEquipment = null;

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

            /**
            * If equipment details are provided, try to find the equipment  
            * in the database and associate it with the request. 
            */
            var serviceType = technicalServiceRequest.TechnicalServiceTypeId.HasValue
                ? technicalServiceRequest.TechnicalServiceTypeId.Value
                : 0;
            if (serviceType == (int)TechnicalServiceTypeEnum.EQUIPMENT_REPAIR_TROUBLESHOOTING)
            {
                var equipmentDetails = technicalServiceRequest.TechnicalServiceRequestEquipment;
                if (equipmentDetails != null && !string.IsNullOrWhiteSpace(equipmentDetails.AssetTag))
                {
                    var equipmentService = new EquipmentService();

                    // Normalize and search for existing equipment by Asset Tag
                    var normalizedAssetTag = equipmentService.NormalizeAssetTag(equipmentDetails.AssetTag);
                    equipmentDetails.AssetTag = normalizedAssetTag;

                    var existingEquipment = equipmentService.GetEquipmentByAssetTag(normalizedAssetTag);
                    if (existingEquipment != null)
                    {
                        // Check if equipment is active
                        var activeEquipmentIds = EquipmentStatusEnum.GetActiveIds();
                        var isActive =
                            existingEquipment.IsActive &&
                            existingEquipment.EquipmentStatusId.HasValue &&
                            activeEquipmentIds.Contains(existingEquipment.EquipmentStatusId.Value);
                        if (!isActive)
                        {
                            throw new Exception("You cannot select an inactive equipment.");
                        }

                        // Check if equipment is already uder repair
                        var isUnderRepair = existingEquipment.EquipmentStatusId == (int)EquipmentStatusEnum.UNDER_REPAIR;
                        if (isUnderRepair)
                        {
                            throw new Exception("Selected equipment is currently under repair.");
                        }

                        existingEquipment.EquipmentStatusId = EquipmentStatusEnum.UNDER_REPAIR;
                        existingEquipment.RepairCount++;
                        updatedEquipment = existingEquipment;

                        // Associate existing equipment with the request
                        technicalServiceRequest.TechnicalServiceRequestEquipmentId = existingEquipment.Id;
                        technicalServiceRequest.TechnicalServiceRequestEquipment = null; // Set to null to avoid creating a new record

                        _db.Entry(existingEquipment).State = EntityState.Modified;
                    }
                    else
                    {
                        // New equipment will be created
                        equipmentDetails.EquipmentModel = equipmentDetails.EquipmentModel.Trim().ToUpperInvariant();
                        equipmentDetails.EquipmentLocationId = null;
                        equipmentDetails.EquipmentStatusId = (int)EquipmentStatusEnum.UNDER_REPAIR;
                        equipmentDetails.RepairCount = 1;
                        equipmentDetails.CreatedByRegistrationId = GetUserSession()?.Id;
                        equipmentDetails.IsActive = true;
                        equipmentDetails.CreatedAt = DateTime.UtcNow;
                        equipmentDetails.UpdatedAt = DateTime.UtcNow;

                        _db.Equipments.Add(equipmentDetails);
                    }
                }
                else
                {
                    // No equipment details provided for repair request
                    technicalServiceRequest.TechnicalServiceRequestEquipmentId = null;
                    technicalServiceRequest.TechnicalServiceRequestEquipment = null;
                }
            }
            else
            {
                // Service type is not Equipment Repair/Troubleshooting, invalidate equipment
                technicalServiceRequest.TechnicalServiceRequestEquipmentId = null;
                technicalServiceRequest.TechnicalServiceRequestEquipment = null;
            }

            // Invalidate schedule fields
            technicalServiceRequest.ScheduledControlProcessDetailId = null;
            technicalServiceRequest.ScheduledControlProcessDetail = null;
        }

        private void CreateScheduleControlProcessRequest(ref TechnicalServiceRequest technicalServiceRequest)
        {
            var scheduledControlProcessDetail = technicalServiceRequest.ScheduledControlProcessDetail;
            var scheduledDate = scheduledControlProcessDetail.TechnicalServiceRequestScheduledDate;
            if (!scheduledDate.HasValue)
            {
                throw new Exception("Schedule is not defined.");
            }

            var serviceType = technicalServiceRequest.TechnicalServiceTypeId;
            if (!serviceType.HasValue)
            {
                throw new Exception("Please select a valid service type.");
            }

            var newScheduledDate = scheduledControlProcessDetail.TechnicalServiceRequestScheduledDate;
            var newStartTime = scheduledControlProcessDetail.TechnicalServiceRequestScheduledStartTime;
            var newEndTime = scheduledControlProcessDetail.TechnicalServiceRequestScheduledEndTime;
            var inactiveStatusIds = new int[]
            {
                (int)TechnicalServiceRequestStatusEnum.RESOLVED,
                (int)TechnicalServiceRequestStatusEnum.CANCELLED,
                (int)TechnicalServiceRequestStatusEnum.CLOSED
            };

            // Get all schedules on the same day
            var sameDaySchedules = _db.TechnicalServiceRequests
                .Where(i =>
                    DbFunctions.TruncateTime(i.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledDate) == DbFunctions.TruncateTime(newScheduledDate) &&
                    i.TechnicalServiceRequestStatusId.HasValue &&
                    !inactiveStatusIds.Contains(i.TechnicalServiceRequestStatusId.Value)
                )
                .ToList();

            // Check whether the schedule is not conflictiing with other requests
            var isConflicting = sameDaySchedules.Any(i =>
            (newStartTime >= i.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledStartTime && newStartTime < i.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledEndTime) ||
            (newEndTime > i.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledStartTime && newEndTime <= i.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledEndTime) ||
            (newStartTime <= i.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledStartTime && newEndTime >= i.ScheduledControlProcessDetail.TechnicalServiceRequestScheduledEndTime)
        );
            if (isConflicting == true)
            {
                throw new Exception("Selected Date and Time is already reserved. Please select another schedule.");
            }

            // Check for limits per day
            switch (serviceType)
            {
                case (int)TechnicalServiceTypeEnum.ZOOM_WEBEX_LINK:
                    if (sameDaySchedules.Count(i =>
                        i.TechnicalServiceTypeId == serviceType &&
                        i.TechnicalServiceRequestStatusId == (int)TechnicalServiceRequestStatusEnum.PENDING) >=
                        TechnicalServiceRequestScheduleLimitEnum.ZOOM_WEBEX_LINK)
                    {
                        throw new Exception("The maximum number of requests for Zoom/Webex Link on the selected date has been reached. Please select another schedule.");
                    }
                    break;
                case (int)TechnicalServiceTypeEnum.LIVESTREAM_SETUP:
                    if (sameDaySchedules.Count(i =>
                        i.TechnicalServiceTypeId == serviceType &&
                        i.TechnicalServiceRequestStatusId == (int)TechnicalServiceRequestStatusEnum.PENDING) >=
                        TechnicalServiceRequestScheduleLimitEnum.LIVESTREAM_SETUP)
                    {
                        throw new Exception("The maximum number of requests for Livestream Setup on the selected date has been reached. Please select another schedule.");
                    }
                    break;
                case (int)TechnicalServiceTypeEnum.AUDIO_VISUAL_SETUP:
                    if (sameDaySchedules.Count(i =>
                        i.TechnicalServiceTypeId == serviceType &&
                        i.TechnicalServiceRequestStatusId == (int)TechnicalServiceRequestStatusEnum.PENDING) >=
                        TechnicalServiceRequestScheduleLimitEnum.AUDIO_VISUAL_SETUP)
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
                    DateAction = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    TechnicalServiceRequestStatusId = (int)TechnicalServiceRequestStatusEnum.PENDING
                }
            };
            technicalServiceRequest.TechnicalServiceRequestStatusId =
                (int)TechnicalServiceRequestStatusEnum.PENDING;

            // Invalidate equipment fields
            technicalServiceRequest.TechnicalServiceRequestEquipmentId = null;
            technicalServiceRequest.TechnicalServiceRequestEquipment = null;
        }

        private void CreateNonAssistedRequest(ref TechnicalServiceRequest technicalServiceRequest)
        {
            // Assign an automatic status of "Ongoing" 
            technicalServiceRequest.TechnicalServiceRequestStatusId =
                (int)TechnicalServiceRequestStatusEnum.ONGOING;

            // Invalidate equipment fields
            technicalServiceRequest.TechnicalServiceRequestEquipmentId = null;
            technicalServiceRequest.TechnicalServiceRequestEquipment = null;

            // Invalidate schedule fields
            technicalServiceRequest.ScheduledControlProcessDetailId = null;
            technicalServiceRequest.ScheduledControlProcessDetail = null;
        }

        #endregion

    }
}
