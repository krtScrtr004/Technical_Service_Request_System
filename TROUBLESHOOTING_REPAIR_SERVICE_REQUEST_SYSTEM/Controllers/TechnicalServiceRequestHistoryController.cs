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
    public class TechnicalServiceRequestHistoryController : BaseController
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.IT })]
        public ActionResult Create(
            int technicalServiceRequestId,
            TechnicalServiceRequestHistory technicalServiceRequestHistory
        )
        {
            // Set the ActionTakenByRegistrationId to the current user's registration ID
            var technicalServiceRequest = _db.TechnicalServiceRequests.Find(technicalServiceRequestId);
            if (technicalServiceRequest == null)
            {
                throw new HttpException(404, "Not found");
            }

            if (technicalServiceRequest.TechnicalServiceRequestStatusId.HasValue &&
                technicalServiceRequest.TechnicalServiceRequestStatusId.Value == (int)TechnicalServiceRequestStatusEnum.CANCELLED)
            {
                throw new Exception("You cannot add a new action history when the status is already cancelled.");
            }

            if (ModelState.IsValid)
            {
                var notificationService = new NotificationService();

                using (var transaction = _db.Database.BeginTransaction())
                {
                    try
                    {
                        var technician = GetUserSession();
                        if (technician == null)
                        {
                            throw new Exception("An error occured.");
                        }

                        // Set the ActionTakenByRegistrationId to the current user's registration ID
                        technicalServiceRequestHistory.ActionTakenByRegistrationId = technician.Id;

                        technicalServiceRequestHistory.TechnicalServiceRequestId = technicalServiceRequestId;
                        technicalServiceRequestHistory.DateAction = DateTime.Now;
                        technicalServiceRequestHistory.UpdatedAt = DateTime.Now;

                        // Add the new history entry to the database
                        technicalServiceRequest.TechnicalServiceRequestHistories.Add(technicalServiceRequestHistory);
                        // Update the status of the request to the action history's status
                        technicalServiceRequest.TechnicalServiceRequestStatusId = technicalServiceRequestHistory.TechnicalServiceRequestStatusId;
                        _db.Entry(technicalServiceRequest).State = EntityState.Modified;

                        var clientId = _db.Registrations
                            .Where(i => i.Email == technicalServiceRequest.ClientEmailAddress)
                            .Select(i => (int?)i.Id)
                            .FirstOrDefault();
                        if (clientId.HasValue)
                        {
                            var notificationMessage = notificationService.BuildRecipientMessageFromRequestStatus(
                                technicalServiceRequest
                                    .TechnicalServiceRequestStatusId.Value,
                                technicalServiceRequest.ReferenceCode,
                                technician.FirstName
                            );

                            _db.Notifications.Add(new Notification()
                            {
                                RecipientRegistrationId = clientId.Value,
                                Title = "Technical Service Request Update",
                                Message = notificationMessage,
                                ForAdmin = false,
                                ForIT = false,
                                IsActive = true,
                                IsRead = false,
                                CreatedAt = DateTime.Now,
                            });
                        }

                        var completedTaskIds = TechnicalServiceRequestStatusEnum.GetCompletedStatusIds();
                        var nonAssistedRequestIds = TechnicalServiceTypeEnum.GetNonAssistedServiceIds();

                        var isCompleted = completedTaskIds.Contains(technicalServiceRequest.TechnicalServiceRequestStatusId.Value);
                        var isNonAssisted = technicalServiceRequest.TechnicalServiceTypeId.HasValue && 
                                            nonAssistedRequestIds.Contains(technicalServiceRequest.TechnicalServiceTypeId.Value);

                        // Check if technician is available now
                        var isAvailableNow = !_db.ITAvailabilities
                            .Where(i => i.Id == technician.Id)
                            .Any(i => DbFunctions.TruncateTime(i.BlockDate) == 
                                      DbFunctions.TruncateTime(DateTime.Now));

                        // When status is completed, closed, or cancelled and service type is not assisted, assign the top request from the queue to the IT
                        if (isAvailableNow && 
                            technicalServiceRequest
                                .TechnicalServiceRequestStatusId
                                .HasValue && 
                            isCompleted && 
                            !isNonAssisted
                        )
                        {
                            var queuedRequest = AssignTechnicianToPendingRequest(technician.Id);
                            if (queuedRequest != null)
                            {
                                // Notify the technician about the assignment
                                notificationService.NotifyTechnicianAssignment(technician.Id, queuedRequest.ReferenceCode);

                                var queuedRequestClientId = _db.Registrations
                                    .Where(i => i.Email == queuedRequest.ClientEmailAddress)
                                    .Select(i => (int?)i.Id)
                                    .FirstOrDefault();
                                if (queuedRequestClientId.HasValue)
                                {
                                    // Notify the client about the assignment
                                    notificationService.NotifyClientOnEnqueuedRequest(
                                        queuedRequestClientId.Value,
                                        queuedRequest.ReferenceCode,
                                        technician.FirstName
                                    );

                                    TechnicalServiceRequestHub.RefreshTechnicalServiceRequestList();
                                }
                            }
                        }

                        _db.SaveChanges();
                        transaction.Commit();

                        TechnicalServiceRequestHub.RefreshTechnicalServiceRequestList();
                        TechnicalServiceRequestHub.RefreshTechnicalServiceRequestStatus(
                               TechnicalServiceRequestStatusEnum.DisplayName(
                                   technicalServiceRequestHistory.TechnicalServiceRequestStatusId.Value
                                )
                        );
                        TechnicalServiceRequestHub.RefreshTechnicalServiceRequestActionHistory(technicalServiceRequestHistory.Id);

                        // If the new status is cancelled, refresh the details page
                        if (technicalServiceRequest.TechnicalServiceRequestStatusId == (int)TechnicalServiceRequestStatusEnum.CANCELLED)
                        {
                            TechnicalServiceRequestHub.RefreshTechnicalServiceRequestStatus(
                                technicalServiceRequest.TechnicalServiceRequestStatus.TechnicalServiceRequestStatusName
                            );
                        }

                        // Notify the client
                        notificationService.RefreshUserUi(clientId.Value);
                        // Notify the IT
                        notificationService.RefreshUserUi(technicalServiceRequestHistory.ActionTakenByRegistrationId.Value);

                        TempData["alertModal"] = new AlertModalUtility
                        {
                            Title = "Success",
                            Status = AlertModalStatus.Success,
                            Message = "Technical Service Request history has been added successfully.",
                        };
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
                        ModelState.AddModelError("", "An error occurred while making a request: " + ex.Message);
                    }
                }
            }

            return RedirectToAction(
                "Details",
                "TechnicalServiceRequests",
                new { id = technicalServiceRequestId }
            );
        }

        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.IT })]
        private TechnicalServiceRequest AssignTechnicianToPendingRequest(int technicianId)
        {
            var queue = new TechnicalServiceRequestQueueService();
            var poppedRequest = queue.Pop();
            if (poppedRequest == null)
            {
                return null;
            }

            // Reload using controller _db (same context used in this controller)
            var queuedRequest = _db.TechnicalServiceRequests
                .Include(r => r.TechnicalServiceRequestHistories)
                .FirstOrDefault(r => r.Id == poppedRequest.Id);
            if (queuedRequest == null)
            {
                return null;
            }

            var technicalServiceTypeId = queuedRequest.TechnicalServiceTypeId;
            if (technicalServiceTypeId.HasValue && TechnicalServiceTypeEnum.IsRepairTroubleshootingRequest(technicalServiceTypeId.Value))
            {
                // Assign only to a repair and troubleshooting request
                queuedRequest.TechnicalServiceRequestHistories.Add(new TechnicalServiceRequestHistory
                {
                    TechnicalServiceRequestId = queuedRequest.Id,
                    ActionTakenByRegistrationId = technicianId,
                    TechnicalServiceRequestStatusId = (int)TechnicalServiceRequestStatusEnum.ONGOING,
                    DateAction = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
                queuedRequest.TechnicalServiceRequestStatusId = (int)TechnicalServiceRequestStatusEnum.ONGOING;
                queuedRequest.DateReceived = DateTime.Now;
                _db.Entry(queuedRequest).State = EntityState.Modified;

                return queuedRequest;
            }

            return null;
        }

        [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD, AccountTypeEnum.IT, AccountTypeEnum.ADMIN })]
        public ActionResult GetTechnicalServiceRequestActionHistory(int id)
        {
            var technicalServiceRequestHistory = _db.TechnicalServiceRequestHistories
                .Include(h => h.TechnicalServiceRequest)
                .Include(h => h.ActionTakenByRegistration)
                .Include(h => h.TechnicalServiceRequestStatus)
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
                html = RenderPartialViewToString("~/Views/TechnicalServiceRequests/Partial/_ActionHistoryRow.cshtml", technicalServiceRequestHistory)
            }, JsonRequestBehavior.AllowGet);
        }

    }
}