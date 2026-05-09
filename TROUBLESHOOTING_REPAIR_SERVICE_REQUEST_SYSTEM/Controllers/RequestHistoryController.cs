using Serilog;
using System;
using System.Collections;
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
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Services;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Utilities;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Controllers
{
    [Authorize2]
    public class RequestHistoryController : BaseController
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.IT })]
        public ActionResult Create(int id, RequestHistory technicalServiceRequestHistory)
        {
            if (!ModelState.IsValid)
            {
                var errors = GetModelStateErrors();
                Log.Warning($"Model state is invalid: {errors}");
                return View(technicalServiceRequestHistory);
            }

            // Set the ActionTakenByRegistrationId to the current user's registration ID
            var technicalServiceRequest = _db.Requests.Find(id);
            if (technicalServiceRequest == null)
            {
                throw new HttpException(404, "Not found");
            }

            if (technicalServiceRequest.StatusId.HasValue &&
                technicalServiceRequest.StatusId.Value == (int)RequestStatusEnum.CANCELLED)
            {
                throw new Exception("You cannot add a new action history when the status is already cancelled.");
            }

            var notificationService = new NotificationService();

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var technician = GetAppUserSession();
                    if (technician == null)
                    {
                        throw new Exception("An error occured.");
                    }

                    var hasEquipmentStatusModified = false;

                    var isEquipmentRepairTroubleshooting = technicalServiceRequest.TypeId == (int)RequestTypeEnum.EQUIPMENT_REPAIR_TROUBLESHOOTING;
                    var equipment = technicalServiceRequest.Equipment;
                    if (isEquipmentRepairTroubleshooting && equipment != null)
                    {
                        UpdateEquipmentStatusId(ref equipment, id);
                        hasEquipmentStatusModified = true;
                    }

                    // Prevent adding new action history on different date thatn schedule
                    if (!PreventActionHistoryOnDifferentSchedule(
                            ref technicalServiceRequest,
                            ref technicalServiceRequestHistory
                        ))
                    {
                        return RedirectToAction(
                            "Details",
                            "TechnicalServiceRequests",
                            new { id = technicalServiceRequest.Id }
                        );
                    }
                    
                    // Set the ActionTakenById to the current user's registration ID
                    technicalServiceRequestHistory.ActionTakenById = technician.Id;

                    technicalServiceRequestHistory.RequestId = id;
                    technicalServiceRequestHistory.DateAction = DateTime.Now;
                    technicalServiceRequestHistory.UpdatedAt = DateTime.Now;

                    // Add the new history entry to the database
                    technicalServiceRequest.Histories.Add(technicalServiceRequestHistory);
                    // Update the status of the request to the action history's status
                    technicalServiceRequest.StatusId = technicalServiceRequestHistory.StatusId;
                    _db.Entry(technicalServiceRequest).State = EntityState.Modified;

                    var notificationMessage = notificationService.BuildRecipientMessageFromRequestStatus(
                        technicalServiceRequest.StatusId.Value,
                        technicalServiceRequest.ReferenceCode,
                        technician.FirstName
                    );

                    _db.Notifications.Add(new Notification()
                    {
                        RecipientId = technicalServiceRequest.ClientId,
                        Title = "Technical Service Request Update",
                        Message = notificationMessage,
                        ForAdmin = false,
                        ForIT = false,
                        IsActive = true,
                        IsRead = false,
                        CreatedAt = DateTime.Now,
                    });

                    var completedTaskIds = RequestStatusEnum.GetCompletedStatusIds();
                    completedTaskIds.Add(RequestStatusEnum.CANCELLED);
                    var nonAssistedRequestIds = RequestTypeEnum.GetNonAssistedServiceIds();

                    var isCompleted = completedTaskIds.Contains(technicalServiceRequest.StatusId.Value);
                    var isNonAssisted = technicalServiceRequest.TypeId.HasValue &&
                            nonAssistedRequestIds.Contains(technicalServiceRequest.TypeId.Value);
                    // Check if technician is available now
                    var isAvailableNow = !_db.ITAvailabilities
                        .Where(i => i.Id == technician.Id)
                        .Any(i => DbFunctions.TruncateTime(i.BlockDate) ==
                                  DbFunctions.TruncateTime(DateTime.Now));

                    // When status is completed, closed, or cancelled and service type is not assisted, assign the top request from the queue to the IT
                    if (isAvailableNow &&
                        technicalServiceRequest.StatusId.HasValue &&
                        isCompleted &&
                        !isNonAssisted
                    )
                    {
                        var queuedRequest = AssignTechnicianToPendingRequest(technician.Id);
                        if (queuedRequest != null)
                        {
                            // Notify the technician about the assignment
                            notificationService.NotifyTechnicianAssignment(technician.Id, queuedRequest.ReferenceCode);

                            // Notify the client about the assignment
                            notificationService.NotifyClientOnEnqueuedRequest(
                                queuedRequest.ClientId,
                                queuedRequest.ReferenceCode,
                                technician.FirstName
                            );
                            Log.Information($"Technician (ID: {technician.Id}) has been assigned to queued request (ID: {queuedRequest.Id}) after completing a request. Request ID: {id}");
                        }
                    }

                    _db.SaveChanges();
                    transaction.Commit();

                    RequestHub.RefreshRequestStatus(
                        id,
                        RequestStatusEnum.DisplayName(
                            technicalServiceRequestHistory.StatusId.Value
                        )
                    );
                    RequestHub.RefreshRequestActionHistory(technicalServiceRequestHistory.Id, id);

                    var completedStatusIdsForForm = RequestStatusEnum.GetCompletedStatusIds();
                    if (completedStatusIdsForForm.Contains(technicalServiceRequestHistory.StatusId.Value))
                    {
                        RequestHub.RefreshRequestFormGeneration(id);
                    }

                    // If the new status is cancelled, refresh the details page
                    if (technicalServiceRequest.StatusId == (int)RequestStatusEnum.CANCELLED)
                    {
                        RequestHub.RefreshRequestStatus(
                            id,
                            technicalServiceRequest.Status.Name
                        );
                    }

                    // If the request is associated with an equipment and has been resolved, closed, or cancelled; refresh the details
                    if (hasEquipmentStatusModified)
                    {
                        EquipmentHub.RefreshEquipmentStatus(equipment.Id, EquipmentStatusEnum.DisplayName((int)equipment.StatusId.Value));
                        EquipmentHub.RefreshEquipmentList();
                    }

                    // Notify the client
                    notificationService.RefreshUserUi(technicalServiceRequest.ClientId);
                    // Notify the IT
                    notificationService.RefreshUserUi(technicalServiceRequestHistory.ActionTakenById.Value);

                    RequestHub.RefreshRequestList();

                    TempData["alertModal"] = new AlertModalUtility
                    {
                        Title = "Success",
                        Status = AlertModalStatus.Success,
                        Message = "Technical Service Request history has been added successfully.",
                    };
                    Log.Information($"Technical Service Request history added successfully. Request ID: {id}, History ID: {technicalServiceRequestHistory.Id}");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["alertModal"] = new AlertModalUtility
                    {
                        Title = "Error",
                        Status = AlertModalStatus.Error,
                        Message = "An error occured. Please try again.",
                    };
                    Log.Error(ex, $"An error occurred while adding Technical Service Request history. Request ID: {id}");
                    ModelState.AddModelError("", "An error occurred while making a request: " + ex.Message);
                }
            }

            return RedirectToAction(
                "Details",
                "Request",
                new { id = id }
            );
        }

        private void UpdateEquipmentStatusId(ref Equipment equipment, int actionHistoryStatusId)
        {
            var completedStatusIds = RequestStatusEnum.GetCompletedStatusIds();
            completedStatusIds.Add((int)RequestStatusEnum.CANCELLED);

            // If resolved, closed, or cancelled, set the equipment status to operational again
            var isResolved = completedStatusIds.Contains(actionHistoryStatusId);
            if (isResolved)
            {
                equipment.StatusId = (int)EquipmentStatusEnum.OPERATIONAL;
                _db.Entry(equipment).State = EntityState.Modified;
            }
        }

        private bool PreventActionHistoryOnDifferentSchedule(ref Request technicalServiceRequest, ref RequestHistory technicalServiceRequestHistory)
        {
            // Prevent adding new action history if the request is scheduled for a specific date
            // and the current date is different from the scheduled date
            var modifieableStatusIds = new int[]
            {
                (int)RequestStatusEnum.CANCELLED,
                (int)RequestStatusEnum.CLOSED
            };

            var isPending = technicalServiceRequest.StatusId.HasValue &&
                technicalServiceRequest.StatusId.Value == (int)RequestStatusEnum.PENDING;
            var isModifiable = technicalServiceRequestHistory.StatusId.HasValue &&
                modifieableStatusIds.Contains(technicalServiceRequestHistory.StatusId.Value);
            if (isPending && !isModifiable)
            {
                var isScheduledService = technicalServiceRequest.TypeId.HasValue &&
                    RequestTypeEnum.IsScheduleControlProcessRequest(
                        technicalServiceRequest.TypeId.Value
                    );

                var scheduledControlProcessDetail = technicalServiceRequest.ScheduledControlProcessDetail;
                var isToday = scheduledControlProcessDetail.ScheduledDate.HasValue &&
                    DateTime.Today == scheduledControlProcessDetail.ScheduledDate.Value.Date;
                if (isScheduledService && !isToday)
                {
                    TempData["alertModal"] = new AlertModalUtility
                    {
                        Title = "Error",
                        Status = AlertModalStatus.Error,
                        Message = "You cannot add a new action history on a different date than the scheduled date.",
                    };
                    return false;
                }
            }
            return true;
        }


        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.IT })]
        private Request AssignTechnicianToPendingRequest(int technicianId)
        {
            var queue = new RequestQueueService();
            var poppedRequest = queue.Pop();
            if (poppedRequest == null)
            {
                return null;
            }

            // Reload using controller _db (same context used in this controller)
            var queuedRequest = _db.Requests
                .Include(r => r.Histories)
                .FirstOrDefault(r => r.Id == poppedRequest.Id);
            if (queuedRequest == null)
            {
                return null;
            }

            var isEquipmentRepairService = queuedRequest.TypeId.HasValue &&
                RequestTypeEnum.IsRepairTroubleshootingRequest(queuedRequest.TypeId.Value);
            var isOthers = !String.IsNullOrEmpty(queuedRequest.Others);
            if (isEquipmentRepairService || isOthers)
            {
                // Assign only to a repair and troubleshooting request
                queuedRequest.Histories.Add(new RequestHistory
                {
                    RequestId = queuedRequest.Id,
                    ActionTakenById = technicianId,
                    StatusId = (int)RequestStatusEnum.ONGOING,
                    DateAction = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
                queuedRequest.StatusId = (int)RequestStatusEnum.ONGOING;
                queuedRequest.DateReceived = DateTime.Now;
                _db.Entry(queuedRequest).State = EntityState.Modified;

                return queuedRequest;
            }

            return null;
        }

        [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.STANDARD, AppUserRoleEnum.IT, AppUserRoleEnum.ADMIN })]
        public ActionResult GetTechnicalServiceRequestActionHistory(int id)
        {
            var technicalServiceRequestHistory = _db.RequestHistories
                .Include(h => h.Request)
                .Include(h => h.ActionTakenBy)
                .Include(h => h.Status)
                .FirstOrDefault(h => h.Id == id);
            if (technicalServiceRequestHistory == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Technical Service Request history not found."
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                success = true,
                html = RenderPartialViewToString("~/Views/Request/Partial/_ActionHistoryRow.cshtml", technicalServiceRequestHistory)
            }, JsonRequestBehavior.AllowGet);
        }

    }
}