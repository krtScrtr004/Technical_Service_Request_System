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
    public class RequestController : BaseController
    {
        // GET: TechnicalServiceRequests
        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.STANDARD, AppUserRoleEnum.IT, AppUserRoleEnum.ADMIN })]
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
                Log.Error(ex, $"An error occured while loading service requests list page: {ex.Message}");
                return View("Error", "Error");
            }
        }

        // GET: TechnicalServiceRequests/Details/5
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.STANDARD, AppUserRoleEnum.IT, AppUserRoleEnum.ADMIN })]
        public ActionResult Details(int id)
        {
            if (id < 1)
            {
                throw new HttpException(403, "Forbidden");
            }

            var currentUser = GetAppUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            var technicalServiceRequest = _db.Requests
                .Include(t => t.Client)
                .Include(t => t.Type)
                .Include(t => t.Severity)
                .Include(t => t.Status)
                .Include(t => t.Equipment)
                .Include(t => t.ScheduledControlProcessDetail)
                .Include(t => t.Histories
                    .Select(h => h.ActionTakenBy))
                .Include(t => t.Histories
                    .Select(h => h.Status))
                .FirstOrDefault(t => t.Id == id);
            if (technicalServiceRequest == null)
            {
                throw new HttpException(404, "Not found");
            }

            try
            {
                var isQueued = false;
                if (technicalServiceRequest.StatusId == (int)RequestStatusEnum.PENDING)
                {
                    isQueued = _db.RequestQueues
                        .Any(q => q.RequestId == id && !q.IsProcessed);
                }

                if (!AppUserRoleEnum.IsAdmin(currentUser.RoleId))
                {
                    var involvedTechnicianIds = technicalServiceRequest.Histories
                        .Select(t => t.ActionTakenById)
                        .ToList();

                    var isAssistedRequest = technicalServiceRequest.TypeId.HasValue &&
                        !RequestTypeEnum.IsNonAssistedRequest(technicalServiceRequest.TypeId.Value);

                    var isRequestClient = AppUserRoleEnum.IsStandard(currentUser.RoleId) &&
                        currentUser.Id == technicalServiceRequest.ClientId;

                    // IT can view if: (non-assisted) OR (assisted AND involved)
                    var isIT = AppUserRoleEnum.IsIT(currentUser.RoleId);
                    var isInvolvedTechnician = isAssistedRequest && isIT && involvedTechnicianIds.Contains(currentUser.Id);
                    var isNonAssistedAndIT = !isAssistedRequest && isIT;

                    if (!isRequestClient && !isInvolvedTechnician && !isNonAssistedAndIT)
                    {
                        return RedirectToAction("Index");
                    }
                }

                // Cast technical service request to details view model 
                var casted = (new RequestService())
                    .ToRequestDetailsViewModel(technicalServiceRequest);

                // Get the latest history with a completed status to determine if the form can be generated
                var completedStatusIds = RequestStatusEnum.GetCompletedStatusIds();
                if (technicalServiceRequest.StatusId.HasValue &&
                    completedStatusIds.Contains(technicalServiceRequest.StatusId.Value))
                {
                    var lastFormGeneratableHistory = technicalServiceRequest.Histories
                        .LastOrDefault(h =>
                            h.StatusId.HasValue &&
                            completedStatusIds.Contains(h.StatusId.Value)
                        );
                    if (lastFormGeneratableHistory != null)
                    {
                        casted.FormGeneratableHistoryId = lastFormGeneratableHistory.Id;
                    }
                }

                ViewBag.CurrentUser = currentUser;
                ViewBag.IsQueued = isQueued;
                return View(casted);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occured while loading service request details page: {ex.Message}");
                return View("Error", "Error");
            }
        }

        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.STANDARD, AppUserRoleEnum.IT, AppUserRoleEnum.ADMIN })]
        public ActionResult Form(int id) // <- The Id here is the TechnicalServiceRequestHistory Id
        {
            if (id < 1)
            {
                throw new HttpException(403, "Forbidden");
            }

            var currentUser = GetAppUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            var technicalServiceRequestHistory = _db.RequestHistories
                .Include(h => h.Request)
                .Include(h => h.Request.ScheduledControlProcessDetail)
                .Include(h => h.Request.Client)
                .FirstOrDefault(h => h.Id == id);
            var technicalServiceRequest = technicalServiceRequestHistory?.Request;
            if (technicalServiceRequest == null || technicalServiceRequestHistory == null)
            {
                throw new HttpException(404, "Not found");
            }

            try
            {
                if (!AppUserRoleEnum.IsAdmin(currentUser.RoleId))
                {
                    var isAssistedRequest = technicalServiceRequest.TypeId.HasValue &&
                      !RequestTypeEnum.IsNonAssistedRequest(technicalServiceRequest.TypeId.Value);

                    var isRequestClient = AppUserRoleEnum.IsStandard(currentUser.RoleId) &&
                        currentUser.Id == technicalServiceRequest.ClientId;

                    // IT can view if: (non-assisted) OR (assisted AND involved)
                    var isIT = AppUserRoleEnum.IsIT(currentUser.RoleId);
                    var isInvolvedTechnician = isAssistedRequest && isIT && technicalServiceRequestHistory.ActionTakenById == currentUser.Id;
                    var isNonAssistedAndIT = !isAssistedRequest && isIT;

                    if (!isRequestClient && !isInvolvedTechnician && !isNonAssistedAndIT)
                    {
                        return RedirectToAction("Index");
                    }
                }

                return View(new RequestFormViewModel
                {
                    Request = technicalServiceRequest,
                    History = technicalServiceRequestHistory
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occured while loading service request form page: {ex.Message}");
                return View("Error", "Error");
            }
        }

        // GET: TechnicalServiceRequests/Create
        [Authorize2]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.STANDARD })]
        public ActionResult Create()
        {
            var currentUser = GetAppUserSession();
            if (currentUser == null)
            {
                throw new HttpException(404, "Not found");
            }
            try
            {
                ViewBag.CurrentUser = currentUser;
                return View(new RequestCreateViewModel()
                {
                    ClientId = currentUser.Id,
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
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occured while loading service request creation page: {ex.Message}");
                return View("Error", "Error");
            }
        }

        // POST: TechnicalServiceRequests/Create
        [Authorize2]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.STANDARD })]
        public ActionResult Create(RequestCreateViewModel technicalServiceRequestCreateViewModel)
        {
            var currentUser = GetAppUserSession();
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

                    var clientInfo = _db.AppUsers
                        .Where(r => r.Id == technicalServiceRequestCreateViewModel.ClientId)
                        .FirstOrDefault();
                    if (clientInfo == null)
                    {
                        throw new Exception("Client not found.");
                    }

                    var technicalServiceRequest = (new RequestService())
                        .ToRequest(technicalServiceRequestCreateViewModel);
                    // Invalidate severity if user selected "Not Applicable" (option value -1)
                    if (technicalServiceRequest.SeverityId < 1)
                    {
                        technicalServiceRequest.SeverityId = null;
                    }
                    technicalServiceRequest.ReferenceCode = GenerateReferenceCode();
                    technicalServiceRequest.DateRequest = DateTime.Now;

                    // Trim and uppercase information for consistency
                    technicalServiceRequest.Others = !string.IsNullOrWhiteSpace(technicalServiceRequest.Others)
                        ? technicalServiceRequest.Others.Trim().ToUpperInvariant()
                        : string.Empty;

                    var selectedTechnicalServiceType = technicalServiceRequest.TypeId;

                    if (technicalServiceRequest.TypeId > 0 && selectedTechnicalServiceType.HasValue)
                    {
                        // Invalidate "Others" if service type is specified
                        technicalServiceRequest.Others = string.Empty;

                        // Implement different logic based on the selected technical service type
                        if (RequestTypeEnum.IsRepairTroubleshootingRequest(selectedTechnicalServiceType.Value))
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
                        else if (RequestTypeEnum.IsScheduleControlProcessRequest(selectedTechnicalServiceType.Value))
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
                        else if (RequestTypeEnum.IsNonAssistedRequest(selectedTechnicalServiceType.Value))
                        {
                            CreateNonAssistedRequest(ref technicalServiceRequest);
                        }
                    }
                    else if (!string.IsNullOrEmpty(technicalServiceRequestCreateViewModel.Others))
                    {
                        // Invalidate service type
                        technicalServiceRequest.TypeId = null;
                        technicalServiceRequest.Type = null;

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

                    _db.Requests.Add(technicalServiceRequest);

                    _db.SaveChanges();
                    transaction.Commit();

                    // Notify all connected clients about the new request
                    RequestHub.RefreshRequestList();

                    // Refresh the equipment hub when the repair request changes an equipment's status/count
                    if (updatedEquipment != null)
                    {
                        EquipmentHub.RefreshEquipmentStatus(
                            updatedEquipment.Id,
                            EquipmentStatusEnum.DisplayName(updatedEquipment.StatusId ?? 0)
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
                            RequestTypeEnum.IsNonAssistedRequest(
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
                            var assignedTechnicianId = _db.RequestHistories
                                .Where(r => r.RequestId == technicalServiceRequest.Id)
                                .OrderByDescending(h => h.UpdatedAt)
                                .Select(h => h.ActionTakenById)
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
                        (new RequestQueueService())
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
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.STANDARD, AppUserRoleEnum.IT, AppUserRoleEnum.ADMIN })]
        public JsonResult GetTechnicalRequest()
        {
            try
            {
                // Authenticate user
                int.TryParse(Request["userId"], out int Id);
                var associatedUser = _db.AppUsers
                    .Where(i => i.Id == Id)
                    .FirstOrDefault();
                if (associatedUser == null)
                {
                    throw new Exception("User not found.");
                }

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
                IQueryable<Request> query = _db.Requests
                    .Include(r => r.Client)
                    .AsQueryable();
                if (associatedUser.RoleId == AppUserRoleEnum.ADMIN)
                {
                    // Admin can see all requests
                    query = _db.Requests
                        .Include(t => t.Type)
                        .Include(t => t.Status);
                }
                else if (associatedUser.RoleId == AppUserRoleEnum.IT)
                {
                    var nonAssistedServiceIds = RequestTypeEnum.GetNonAssistedServiceIds();

                    /**
                     * IT can see requests:
                     * - They have taken action on (based on TechnicalServiceRequestHistories)
                     * - Non-Assisted requests
                     */
                    query = _db.Requests
                        .Include(t => t.Type)
                        .Include(t => t.Status)
                        .Where(t => t.Histories
                            .Any(h => h.ActionTakenById == associatedUser.Id) ||
                            (t.TypeId.HasValue &&
                            nonAssistedServiceIds.Contains(t.TypeId.Value)
                        ));
                }
                else if (associatedUser.RoleId == AppUserRoleEnum.STANDARD)
                {
                    // Standard users can only see their own requests
                    query = _db.Requests
                        .Include(t => t.Type)
                        .Include(t => t.Status)
                        .Where(t => t.ClientId == associatedUser.Id);
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
                            query = query.Where(i => i.TypeId == typeIntValue);
                        }
                    }
                    else if (typeFilter == "assigned")
                    {
                        var nonAssistedServices = RequestTypeEnum.GetNonAssistedServiceIds();
                        query = query.Where(i =>
                            i.TypeId.HasValue &&
                            !nonAssistedServices.Contains(i.TypeId.Value) &&
                            i.Histories
                            .Any(h => h.ActionTakenById == associatedUser.Id)
                        );
                    }
                    else if (typeFilter == "non-assisted")
                    {
                        var nonAssistedServices = RequestTypeEnum.GetNonAssistedServiceIds();
                        query = query.Where(i =>
                            i.TypeId.HasValue &&
                            nonAssistedServices.Contains(i.TypeId.Value)
                        );
                    }
                    else if (typeFilter == "queued")
                    {
                        var queuedRequestIds = _db.RequestQueues
                            .Include(q => q.Request)
                            .Where(q =>
                                !q.IsProcessed &&
                                 q.Request.StatusId.HasValue &&
                                 q.Request.StatusId.Value == (int)RequestStatusEnum.PENDING)
                            .Select(q => q.RequestId)
                            .ToList();

                        // Reset base query to fetch only queued requests
                        query = _db.Requests
                            .Include(t => t.Type)
                            .Include(t => t.Status)
                            .Where(i =>
                                queuedRequestIds.Contains(i.Id) &&
                                i.StatusId.HasValue &&
                                i.StatusId.Value == (int)RequestStatusEnum.PENDING
                            );
                    }
                }

                // Apply Status filter
                if (!string.IsNullOrEmpty(statusFilter) && int.TryParse(statusFilter, out int statusIntValue) && statusIntValue > 0)
                {
                    query = query.Where(i => i.StatusId == statusIntValue);
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
                        t.Client.FirstName.Contains(searchValue) ||
                        t.Client.LastName.Contains(searchValue) ||
                        t.Client.MiddleName.Contains(searchValue) ||
                        t.Client.ExtensionName.Contains(searchValue) ||
                        t.Client.Email.Contains(searchValue) ||
                        t.Client.ContactNumber.Contains(searchValue) ||
                        t.Client.Office.Contains(searchValue) ||
                        t.Type.Name.Contains(searchValue) ||
                        t.Status.Name.Contains(searchValue) ||
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
                                ? query.OrderBy(t => t.Client.LastName)
                                : query.OrderByDescending(t => t.Client.LastName);
                            break;
                        case 3: // Service Type
                            query = sortDirection == "asc"
                                ? query.OrderBy(t => t.Type.Name)
                                : query.OrderByDescending(t => t.Type.Name);
                            break;
                        case 4: // Status
                            query = sortDirection == "asc"
                                ? query.OrderBy(t => t.Status.Name)
                                : query.OrderByDescending(t => t.Status.Name);
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
                        t.Client.FirstName,
                        t.Client.MiddleName,
                        t.Client.LastName,
                        t.Client.ExtensionName,
                        t.Client.Email,
                        t.Client.ContactNumber,
                        t.Client.Office,
                        Type = t.Type?.Name,
                        t.Others,
                        Status = t.Status.Name,
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
                Log.Error(ex, $"An error occurred while user was fetching technical service requests with ID {GetAppUserSession()?.Id.ToString() ?? "Unknown"}.");
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize2]
        [HttpGet]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.STANDARD, AppUserRoleEnum.IT, AppUserRoleEnum.ADMIN })]
        public JsonResult GetFormGenerationState(int id)
        {
            try
            {
                if (id < 1)
                {
                    throw new HttpException(404, "Not found");
                }

                var currentUser = GetAppUserSession();
                if (currentUser == null)
                {
                    throw new HttpException(403, "Forbidden");
                }

                var technicalServiceRequest = _db.Requests
                    .Include(t => t.Histories)
                    .FirstOrDefault(t => t.Id == id);
                if (technicalServiceRequest == null)
                {
                    throw new HttpException(404, "Not found");
                }

                if (!AppUserRoleEnum.IsAdmin(currentUser.RoleId))
                {
                    var involvedTechnicianIds = technicalServiceRequest.Histories
                        .Where(h => h.ActionTakenById.HasValue)
                        .Select(h => h.ActionTakenById.Value)
                        .ToList();

                    var isAssistedRequest = technicalServiceRequest.TypeId.HasValue &&
                        !RequestTypeEnum.IsNonAssistedRequest(technicalServiceRequest.TypeId.Value);

                    var isRequestClient = AppUserRoleEnum.IsStandard(currentUser.RoleId) &&
                        currentUser.Id == technicalServiceRequest.ClientId;

                    var isIT = AppUserRoleEnum.IsIT(currentUser.RoleId);
                    var isInvolvedTechnician = isAssistedRequest && isIT && involvedTechnicianIds.Contains(currentUser.Id);
                    var isNonAssistedAndIT = !isAssistedRequest && isIT;

                    if (!isRequestClient && !isInvolvedTechnician && !isNonAssistedAndIT)
                    {
                        throw new HttpException(403, "Forbidden");
                    }
                }

                var completedStatusIds = RequestStatusEnum.GetCompletedStatusIds();
                var isFormGeneratable = technicalServiceRequest.StatusId.HasValue &&
                    completedStatusIds.Contains(technicalServiceRequest.StatusId.Value);

                var formGeneratableHistoryId = technicalServiceRequest.Histories
                    .Where(h => h.StatusId.HasValue &&
                        completedStatusIds.Contains(h.StatusId.Value))
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
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.STANDARD, AppUserRoleEnum.IT, AppUserRoleEnum.ADMIN })]
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
                        e.StatusId.HasValue &&
                        activeStatusIds.Contains(e.StatusId.Value)
                    )
                    .Select(e => new
                    {
                        e.Id,
                        e.Model,
                        e.AssetTag,
                        e.TypeId
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
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.STANDARD })]
        public ActionResult EditDescription(Request technicalServiceRequestParam, int id)
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

                var currentUser = GetAppUserSession();
                if (currentUser == null)
                {
                    throw new Exception("User not found.");
                }

                var technicalServiceRequest = _db.Requests
                    .Include(r => r.Client)
                    .Include(r => r.Histories)
                    .FirstOrDefault(r => r.Id == id);
                if (technicalServiceRequest == null)
                {
                    throw new Exception("Technical service request not found.");
                }

                if (currentUser.Id != technicalServiceRequest.ClientId)
                {
                    throw new Exception("You are not allowed to perform this action.");
                }

                technicalServiceRequest.Description = technicalServiceRequestParam.Description?.Trim();
                _db.Entry(technicalServiceRequest).State = EntityState.Modified;
                _db.SaveChanges();

                RequestHub.RefreshRequestDescription(
                    technicalServiceRequest.Id,
                    technicalServiceRequest.Description
                );

                var latestTechnicianId = technicalServiceRequest.Histories
                    .OrderByDescending(h => h.UpdatedAt)
                    .Select(h => h.ActionTakenById)
                    .FirstOrDefault();
                var serviceType = technicalServiceRequest.TypeId.HasValue
                    ? technicalServiceRequest.TypeId.Value
                    : 0;
                if (latestTechnicianId.HasValue && serviceType > 0)
                {
                    /**
                     * Notify the assigned technician about the updated description if the request is an assisted service, otherwise,
                     * notify all technicians about the updated description for non-assisted service
                     */
                    var isNonAssisted = RequestTypeEnum.IsNonAssistedRequest(serviceType);
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
                Log.Error(ex, $"An error occurred while user with ID {GetAppUserSession()?.Id.ToString() ?? "Unknown"} was editing description of request with ID {id}.");
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
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.STANDARD })]
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

                var technicalServiceRequest = _db.Requests.Find(id);
                if (technicalServiceRequest == null)
                {
                    throw new Exception("Technical Service Request not found.");
                }

                var client = _db.AppUsers
                    .Where(i => i.Id == technicalServiceRequest.ClientId)
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
                if (client.Id != technicalServiceRequest.ClientId)
                {
                    throw new Exception("Your are not allowed to perform this action.");
                }

                var currentStatus = technicalServiceRequest.StatusId;
                if (!currentStatus.HasValue)
                {
                    throw new Exception("Status not found.");
                }

                var currentStatusName = technicalServiceRequest.Status.Name;

                // Check is current status can still be cancelled
                var cancellableStatus = RequestStatusEnum.GetCancellableStatusIds();
                if (!cancellableStatus.Contains(currentStatus.Value))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Your request cannot be cancelled as it is already in " + currentStatusName + " status.",
                    });
                }

                var history = _db.RequestHistories
                    .Where(h => h.RequestId == technicalServiceRequest.Id)
                    .OrderByDescending(h => h.UpdatedAt)
                    .FirstOrDefault();
                if (history != null)
                {
                    var technicianId = history.ActionTakenById;
                    _db.Notifications.Add(new Notification()
                    {
                        RecipientId = technicianId.Value,
                        Title = "Request Cancelled: " + technicalServiceRequest.ReferenceCode,
                        Message = "The request with reference code " + technicalServiceRequest.ReferenceCode + " has been cancelled by the client. Please check the request details for more information.",
                        IsRead = false,
                        CreatedAt = DateTime.Now,
                    });
                    (new NotificationService()).RefreshUserUi(technicianId.Value);
                }

                // Only status can be editted
                var newStatus = (int)RequestStatusEnum.CANCELLED;
                technicalServiceRequest.StatusId = newStatus;
                _db.Entry(technicalServiceRequest).State = EntityState.Modified;
                _db.SaveChanges();

                // Notify all connected clients about the cancelled request
                RequestHub.RefreshRequestList();
                RequestHub.RefreshRequestStatus(
                    technicalServiceRequest.Id,
                    RequestStatusEnum.DisplayName(newStatus)
                );

                var severityName = technicalServiceRequest.Severity != null
                    ? technicalServiceRequest.Severity.Name
                    : (technicalServiceRequest.SeverityId.HasValue
                        ? RequestSeverityEnum.DisplayName(
                            technicalServiceRequest.SeverityId.Value
                        ) : "Unknown severity");

                Log.Information($"Request with reference code {technicalServiceRequest.ReferenceCode} has been cancelled by client with email {client.Email}.");
                return Json(new
                {
                    success = true,
                    message = "Your request has been cancelled.",
                    redirectUrl = Url.Action(
                        "Details",
                        "Request",
                        new { id = id }
                    ),
                    severity = severityName
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occurred while user with ID {GetAppUserSession()?.Id.ToString() ?? "Unknown"} was cancelling request with ID {id}.");
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
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.IT })]
        public ActionResult UpdateSeverity(int id, int severityId)
        {
            try
            {
                if (severityId < (int)RequestSeverityEnum.LOW ||
                    severityId > (int)RequestSeverityEnum.CRITICAL)
                {
                    throw new Exception("Invalid severity level.");
                }

                var currentUser = GetAppUserSession();
                if (currentUser == null)
                {
                    throw new Exception("User not found.");
                }

                var technicalServiceRequest = _db.Requests
                    .Include(t => t.Histories)
                    .FirstOrDefault(t => t.Id == id);
                if (technicalServiceRequest == null)
                {
                    throw new Exception("Technical service request not found.");
                }

                /**
                 * Check if the current user has acted on the technical service request,
                 * if not, throw an error to prevent unauthorized severity update.
                 */
                var involvedTechnicianIds = technicalServiceRequest.Histories
                    .Select(h => h.ActionTakenById)
                    .ToList();
                if (!involvedTechnicianIds.Contains(currentUser.Id))
                {
                    throw new Exception("You are not allowed to perform this action.");
                }

                var currentSeverityId = technicalServiceRequest.SeverityId;
                if (severityId != currentSeverityId)
                {
                    if (technicalServiceRequest.StatusId.HasValue &&
                        technicalServiceRequest.StatusId.Value == (int)RequestStatusEnum.CANCELLED ||
                        technicalServiceRequest.StatusId.Value == (int)RequestStatusEnum.CLOSED)
                    {
                        throw new Exception("You cannot update severity when the status is already cancelled / closed.");
                    }

                    technicalServiceRequest.SeverityId = severityId;
                    _db.Entry(technicalServiceRequest).State = EntityState.Modified;

                    // Notify all connected clients about the updated severity
                    RequestHub.RefreshRequestSeverity(
                        technicalServiceRequest.Id,
                        technicalServiceRequest.Severity.Name
                    );

                    // Notify client about the updated severity
                    var notificationService = new NotificationService();
                    _db.Notifications.Add(new Notification()
                    {
                        RecipientId = technicalServiceRequest.ClientId,
                        Title = "Severity Updated: " + technicalServiceRequest.ReferenceCode,
                        Message = notificationService.BuildRecipientMessageFromRequestSeverity(
                                severityId, technicalServiceRequest.ReferenceCode, currentSeverityId
                            ),
                        IsRead = false,
                        CreatedAt = DateTime.Now,
                    });
                    notificationService.RefreshUserUi(technicalServiceRequest.ClientId);
                }

                _db.SaveChanges();

                Log.Information($"Severity level of request with reference code {technicalServiceRequest.ReferenceCode} has been updated to {RequestSeverityEnum.DisplayName(severityId)} by user with ID {currentUser.Id}.");
                return Json(new
                {
                    success = true,
                    message = "Severity level has been updated.",
                    redirectUrl = Url.Action(
                        "Details",
                        "Request",
                        new { id = id }
                    ),
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occurred while user with ID {GetAppUserSession()?.Id.ToString() ?? "Unknown"} was updating severity of request with ID {id}.");
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
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.STANDARD })]
        public ActionResult GetFullyBookedDayByLimit(int scheduleServiceTypeId)
        {
            try
            {
                var validScheduleServiceTypeIds = RequestTypeEnum.GetScheduledServiceIds();
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
                    case RequestTypeEnum.AUDIO_VISUAL_SETUP:
                        selectedTypeId = (int)RequestTypeEnum.AUDIO_VISUAL_SETUP;
                        selectedTypeLimit = (int)RequestScheduleLimitEnum.AUDIO_VISUAL_SETUP;
                        break;
                    case RequestTypeEnum.LIVESTREAM_SETUP:
                        selectedTypeId = (int)RequestTypeEnum.LIVESTREAM_SETUP;
                        selectedTypeLimit = (int)RequestScheduleLimitEnum.LIVESTREAM_SETUP;
                        break;
                    case RequestTypeEnum.ZOOM_WEBEX_LINK:
                        selectedTypeId = (int)RequestTypeEnum.ZOOM_WEBEX_LINK;
                        selectedTypeLimit = (int)RequestScheduleLimitEnum.ZOOM_WEBEX_LINK;
                        break;
                    default:
                        throw new Exception("Invalid service type.");
                }

                var inactiveStatusIds = new int[]
                {
                    (int)RequestStatusEnum.RESOLVED,
                    (int)RequestStatusEnum.CANCELLED,
                    (int)RequestStatusEnum.CLOSED
                };

                /**
                 * Get fully booked days based on the count of scheduled requests for the selected
                 * service type, within the next 30 days, that have reached the per-day limit
                 */
                var fullyBookedStringDates = _db.Requests
                    .Where(r =>
                        r.StatusId.HasValue &&
                        !inactiveStatusIds.Contains(r.StatusId.Value) &&
                        r.TypeId == selectedTypeId &&
                        r.ScheduledControlProcessDetail.ScheduledDate >= startDate &&
                        r.ScheduledControlProcessDetail.ScheduledDate < endDate)
                    .GroupBy(r => DbFunctions.TruncateTime(r.ScheduledControlProcessDetail.ScheduledDate))
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
                Log.Error(ex, $"An error occurred while user with ID {GetAppUserSession()?.Id.ToString() ?? "Unknown"} was fetching fully booked days by limit for service type ID {scheduleServiceTypeId}.");
                return Json(new
                {
                    success = false,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize2]
        [HttpGet]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.STANDARD })]
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
                     (int)RequestStatusEnum.RESOLVED,
                     (int)RequestStatusEnum.CANCELLED,
                     (int)RequestStatusEnum.CLOSED
                };

                var fullyBookedStringDates = new List<string>();

                /**
                 * Get fully booked days based on the schedule of requests for the selected service type, 
                 * within the next 30 days, that have no available time slots between 8:00 AM to 4:00 PM
                 */
                for (var date = startDate; date < endDate; date = date.AddDays(1))
                {
                    var thisDate = date;
                    var events = _db.Requests
                        .Where(r =>
                            // Fetch only requests that are active 
                            r.StatusId.HasValue &&
                            !inactiveStatusIds.Contains(r.StatusId.Value) &&
                            // Check if the request is scheduled on the same date as the new request
                            DbFunctions.TruncateTime(r.ScheduledControlProcessDetail.ScheduledDate) ==
                            DbFunctions.TruncateTime(thisDate)
                        )
                        .Select(r => new
                        {
                            Start = r.ScheduledControlProcessDetail.ScheduledStartTime,
                            End = r.ScheduledControlProcessDetail.ScheduledEndTime
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
                Log.Error(ex, $"An error occurred while user with ID {GetAppUserSession()?.Id.ToString() ?? "Unknown"} was fetching fully booked days by schedule.");
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
            var lastCode = _db.Requests
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
            var activeStatusIds = RequestStatusEnum.GetActiveStatusIds();
            var scheduledControlProcessRequestIds = RequestTypeEnum.GetScheduledServiceIds();
            var nonAssistedRequestIds = RequestTypeEnum.GetNonAssistedServiceIds();

            var today = DateTime.Now.Date;

            // Build a set of technicians currently busy with active requests
            var busyTechnicianIds = _db.Requests
                .Where(r =>
                    // Check only active requests
                    r.StatusId.HasValue &&
                    activeStatusIds.Contains(r.StatusId.Value) &&
                    // Check only assisted service requests
                    r.TypeId.HasValue &&
                    !nonAssistedRequestIds.Contains(r.TypeId.Value) &&
                    (
                        // Busy now
                        !scheduledControlProcessRequestIds.Contains(r.TypeId.Value)
                        ||
                        // Busy only on scheduled day
                        (r.ScheduledControlProcessDetail != null &&
                         r.ScheduledControlProcessDetail.ScheduledDate.HasValue &&
                         DbFunctions.TruncateTime(r.ScheduledControlProcessDetail.ScheduledDate.Value) == DbFunctions.TruncateTime(today))
                    )
                )
                .Select(r => r.Histories
                    .OrderByDescending(h => h.UpdatedAt)
                    .Select(h => h.ActionTakenById)
                    .FirstOrDefault())
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Distinct()
                .ToList();

            // Pick the least recently assigned, then by Id.
            var availableTechnicianId = _db.AppUsers
               .Where(r =>
                   r.IsActive &&
                   r.RoleId == AppUserRoleEnum.IT)
               .Where(r => !r.ITAvailabilities.Any(a =>
                   DbFunctions.TruncateTime(a.BlockDate) == today))
               .Where(r => !busyTechnicianIds.Contains(r.Id))
               .Select(r => new
               {
                   r.Id,
                   LastAssignedAt = _db.RequestHistories
                       .Where(h => h.ActionTakenById == r.Id)
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

            var itAccountTypeName = AppUserRoleEnum.DisplayName(AppUserRoleEnum.IT);

            // Requests in these statuses are still actively consuming a technician
            var activeStatusIds = RequestStatusEnum.GetActiveStatusIds();
            var scheduledControlProcessRequestIds = RequestTypeEnum.GetScheduledServiceIds();
            var nonAssistedRequestIds = RequestTypeEnum.GetNonAssistedServiceIds();

            // Build a set of technicians currently busy with active requests
            var busyTechnicianIds = _db.Requests
                .Where(r =>
                    // Check only active requests
                    r.StatusId.HasValue &&
                    activeStatusIds.Contains(r.StatusId.Value) &&
                    // Check only scheduled control process requests
                    r.TypeId.HasValue &&
                    !nonAssistedRequestIds.Contains(r.TypeId.Value) &&
                    scheduledControlProcessRequestIds.Contains(r.TypeId.Value) &&
                    // Check if the request is scheduled on the same date as the new request
                    r.ScheduledControlProcessDetail != null &&
                    DbFunctions.TruncateTime(r.ScheduledControlProcessDetail.ScheduledDate) == DbFunctions.TruncateTime(scheduleDate) &&
                    // Check if the scheduled time is overlapping with the new request's schedule
                    r.ScheduledControlProcessDetail.ScheduledStartTime.HasValue &&
                    r.ScheduledControlProcessDetail.ScheduledEndTime.HasValue &&
                    startTime.HasValue &&
                    endTime.HasValue &&
                    (
                        (startTime >= r.ScheduledControlProcessDetail.ScheduledStartTime &&
                         startTime < r.ScheduledControlProcessDetail.ScheduledEndTime) ||
                        (endTime > r.ScheduledControlProcessDetail.ScheduledStartTime &&
                         endTime <= r.ScheduledControlProcessDetail.ScheduledEndTime) ||
                        (startTime <= r.ScheduledControlProcessDetail.ScheduledStartTime &&
                         endTime >= r.ScheduledControlProcessDetail.ScheduledEndTime)
                    )
                )
                .Select(r => r.Histories
                    .OrderByDescending(h => h.UpdatedAt)
                    .Select(h => h.ActionTakenById)
                    .FirstOrDefault())
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Distinct()
                .ToList();


            // Pick the least recently assigned (fairness), then by Id.
            var availableTechnicianId = _db.AppUsers
                .Where(r =>
                    r.IsActive &&
                    r.RoleId == AppUserRoleEnum.IT
                )
                .Where(r => !r.ITAvailabilities.Any(a =>
                    DbFunctions.TruncateTime(a.BlockDate) == DbFunctions.TruncateTime(scheduleDate)))
                .Where(r => !busyTechnicianIds.Contains(r.Id))
                .Select(r => new
                {
                    r.Id,
                    LastAssignedAt = _db.RequestHistories
                        .Where(h => h.ActionTakenById == r.Id)
                        .Select(h => (DateTime?)h.UpdatedAt)
                        .Max()
                })
                .OrderBy(r => r.LastAssignedAt ?? DateTime.MinValue)
                .ThenBy(r => r.Id)
                .Select(r => (int?)r.Id)
                .FirstOrDefault();

            return availableTechnicianId;
        }

        private void CreateEquipmentRepairTroubleshootingRequest(
            ref Request technicalServiceRequest, 
            ref bool isQueued, 
            out Equipment updatedEquipment)
        {
            updatedEquipment = null;

            int? availableTechnicianId = GetAvailableTechnician();
            if (availableTechnicianId.HasValue)
            {
                // If there is available technician, create a new history record
                technicalServiceRequest.Histories = new List<RequestHistory>
                {
                    new RequestHistory
                    {
                        ActionTakenById = availableTechnicianId,
                        StatusId = (int)RequestStatusEnum.ONGOING,
                        DateAction = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }
                };
                technicalServiceRequest.StatusId =
                    (int)RequestStatusEnum.ONGOING;
            }
            else
            {
                isQueued = true;
                technicalServiceRequest.StatusId = (int)RequestStatusEnum.PENDING;
            }

            /**
            * If equipment details are provided, try to find the equipment  
            * in the database and associate it with the request. 
            */
            var serviceType = technicalServiceRequest.TypeId.HasValue
                ? technicalServiceRequest.TypeId.Value
                : 0;
            if (serviceType == (int)RequestTypeEnum.EQUIPMENT_REPAIR_TROUBLESHOOTING)
            {
                var equipmentDetails = technicalServiceRequest.Equipment;
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
                            existingEquipment.StatusId.HasValue &&
                            activeEquipmentIds.Contains(existingEquipment.StatusId.Value);
                        if (!isActive)
                        {
                            throw new Exception("You cannot select an inactive equipment.");
                        }

                        // Check if equipment is already under repair
                        var isUnderRepair = existingEquipment.StatusId == (int)EquipmentStatusEnum.UNDER_REPAIR;
                        if (isUnderRepair)
                        {
                            throw new Exception("Selected equipment is currently under repair.");
                        }

                        existingEquipment.StatusId = (int)EquipmentStatusEnum.UNDER_REPAIR;
                        existingEquipment.RepairCount++;
                        updatedEquipment = existingEquipment;

                        // Associate existing equipment with the request
                        technicalServiceRequest.EquipmentId = existingEquipment.Id;
                        technicalServiceRequest.Equipment = null; // Set to null to avoid creating a new record

                        _db.Entry(existingEquipment).State = EntityState.Modified;
                    }
                    else
                    {
                        // New equipment will be created
                        equipmentDetails.Model = equipmentDetails.Model.Trim().ToUpperInvariant();
                        equipmentDetails.LocationId = null;
                        equipmentDetails.StatusId = (int)EquipmentStatusEnum.UNDER_REPAIR;
                        equipmentDetails.RepairCount = 1;
                        equipmentDetails.CreatedById = GetAppUserSession()?.Id;
                        equipmentDetails.CreatedAt = DateTime.UtcNow;
                        equipmentDetails.UpdatedAt = DateTime.UtcNow;

                        _db.Equipments.Add(equipmentDetails);
                    }
                }
                else
                {
                    // No equipment details provided for repair request
                    technicalServiceRequest.EquipmentId = null;
                    technicalServiceRequest.Equipment = null;
                }
            }
            else
            {
                // Service type is not Equipment Repair/Troubleshooting, invalidate equipment
                technicalServiceRequest.EquipmentId = null;
                technicalServiceRequest.Equipment = null;
            }

            // Invalidate schedule fields
            technicalServiceRequest.ScheduledControlProcessDetailId = null;
            technicalServiceRequest.ScheduledControlProcessDetail = null;
        }

        private void CreateScheduleControlProcessRequest(ref Request technicalServiceRequest)
        {
            var scheduledControlProcessDetail = technicalServiceRequest.ScheduledControlProcessDetail;
            var scheduledDate = scheduledControlProcessDetail.ScheduledDate;
            if (!scheduledDate.HasValue)
            {
                throw new Exception("Schedule is not defined.");
            }

            var serviceType = technicalServiceRequest.TypeId;
            if (!serviceType.HasValue)
            {
                throw new Exception("Please select a valid service type.");
            }

            var newScheduledDate = scheduledControlProcessDetail.ScheduledDate;
            var newStartTime = scheduledControlProcessDetail.ScheduledStartTime;
            var newEndTime = scheduledControlProcessDetail.ScheduledEndTime;
            var inactiveStatusIds = new int[]
            {
                (int)RequestStatusEnum.RESOLVED,
                (int)RequestStatusEnum.CANCELLED,
                (int)RequestStatusEnum.CLOSED
            };

            // Get all schedules on the same day
            var sameDaySchedules = _db.Requests
                .Where(i =>
                    DbFunctions.TruncateTime(i.ScheduledControlProcessDetail.ScheduledDate) == DbFunctions.TruncateTime(newScheduledDate) &&
                    i.StatusId.HasValue &&
                    !inactiveStatusIds.Contains(i.StatusId.Value)
                )
                .ToList();

            // Check whether the schedule is not conflictiing with other requests
            var isConflicting = sameDaySchedules.Any(i =>
            (newStartTime >= i.ScheduledControlProcessDetail.ScheduledStartTime && newStartTime < i.ScheduledControlProcessDetail.ScheduledEndTime) ||
            (newEndTime > i.ScheduledControlProcessDetail.ScheduledStartTime && newEndTime <= i.ScheduledControlProcessDetail.ScheduledEndTime) ||
            (newStartTime <= i.ScheduledControlProcessDetail.ScheduledStartTime && newEndTime >= i.ScheduledControlProcessDetail.ScheduledEndTime)
        );
            if (isConflicting == true)
            {
                throw new Exception("Selected Date and Time is already reserved. Please select another schedule.");
            }

            // Check for limits per day
            switch (serviceType)
            {
                case (int)RequestTypeEnum.ZOOM_WEBEX_LINK:
                    if (sameDaySchedules.Count(i =>
                        i.TypeId == serviceType &&
                        i.StatusId == (int)RequestStatusEnum.PENDING) >= RequestScheduleLimitEnum.ZOOM_WEBEX_LINK)
                    {
                        throw new Exception("The maximum number of requests for Zoom/Webex Link on the selected date has been reached. Please select another schedule.");
                    }
                    break;
                case (int)RequestTypeEnum.LIVESTREAM_SETUP:
                    if (sameDaySchedules.Count(i =>
                        i.TypeId == serviceType &&
                        i.StatusId == (int)RequestStatusEnum.PENDING) >=
                        RequestScheduleLimitEnum.LIVESTREAM_SETUP)
                    {
                        throw new Exception("The maximum number of requests for Livestream Setup on the selected date has been reached. Please select another schedule.");
                    }
                    break;
                case (int)RequestTypeEnum.AUDIO_VISUAL_SETUP:
                    if (sameDaySchedules.Count(i =>
                        i.TypeId == serviceType &&
                        i.StatusId == (int)RequestStatusEnum.PENDING) >=
                        RequestScheduleLimitEnum.AUDIO_VISUAL_SETUP)
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

            technicalServiceRequest.Histories = new List<RequestHistory>
            {
               // Create a history record for the assigned technician with a status of "Pending" since the action is not yet taken
                new RequestHistory
                {
                    ActionTakenById = assignedTechnician,
                    ActionTaken = "",
                    DateAction = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    StatusId = (int)RequestStatusEnum.PENDING
                }
            };
            technicalServiceRequest.StatusId = (int)RequestStatusEnum.PENDING;

            // Invalidate equipment fields
            technicalServiceRequest.EquipmentId = null;
            technicalServiceRequest.Equipment = null;
        }

        private void CreateNonAssistedRequest(ref Request technicalServiceRequest)
        {
            // Assign an automatic status of "Ongoing" 
            technicalServiceRequest.StatusId = (int)RequestStatusEnum.ONGOING;

            // Invalidate equipment fields
            technicalServiceRequest.EquipmentId = null;
            technicalServiceRequest.Equipment = null;
            // Invalidate schedule fields
            technicalServiceRequest.ScheduledControlProcessDetailId = null;
            technicalServiceRequest.ScheduledControlProcessDetail = null;
        }

        #endregion

    }
}
