using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TECHNICAL_SERVICE_REQUEST.Core;
using TECHNICAL_SERVICE_REQUEST.Enumerables;
using TECHNICAL_SERVICE_REQUEST.Models;

namespace TECHNICAL_SERVICE_REQUEST.Services
{
    public class NotificationService
    {
        #region Specific Notification Methods

        public void RefreshUserUi(int userId)
        {
            NotificationHub.RefreshNotificationList(userId);
            NotificationHub.RefreshNotificationBadge(userId);
        }

        #endregion

        #region Technician Notification Methods

        public void RefreshAllTechniciansUi()
        {
            NotificationHub.RefreshITNotificationList();
            NotificationHub.RefreshITNotificationBadge();
        }

        public void NotifyTechnicianAssignment(int technicianId, string referenceCode)
        {
            using (var db = new ApplicationDbContext())
            {
                db.Notifications.Add(new Notification()
                {
                    RecipientId = technicianId,
                    Title = "New Request Assignment",
                    Message = "You have been assigned to a new request (" + referenceCode + "). " + 
                        "Please check your assigned requests for details.",
                    ForAdmin = false,
                    ForIT = false,
                    IsActive = true,
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                });
                db.SaveChanges();

                RefreshUserUi(technicianId);
                RequestHub.RefreshRequestList();
            }
        }

        public void NotifyTechnicianNonAssistedService(string referenceCode)
        {
            using (var db = new ApplicationDbContext())
            {
                db.Notifications.Add(new Notification()
                {
                    RecipientId = null,
                    Title = "Non-Assisted Service Request",
                    Message = "A new non-assisted service request (" + referenceCode + ") has been submitted. " + 
                        "Please check requests list for details.",
                    ForAdmin = false,
                    ForIT = true,
                    IsActive = true,
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                });
                db.SaveChanges();

                RefreshAllTechniciansUi();
            }
        }

        public void NotifyTechnicianDescriptionUpdate(int? technicianId, string referenceCode)
        {
            using (var db = new ApplicationDbContext())
            {
                var forAll = technicianId == null ? true : false;

                db.Notifications.Add(new Notification()
                {
                    RecipientId = technicianId,
                    Title = "Request Description Update",
                    Message = "The description for request (" + referenceCode + ") has been updated. " +
                        "Please check requests list for details.",
                    ForAdmin = false,
                    ForIT = forAll,
                    IsActive = true,
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                });
                db.SaveChanges();

                if (forAll)
                {
                    RefreshAllTechniciansUi();
                }
                else
                {
                    RefreshUserUi(technicianId.Value);
                }
            }
        }

        public void NotifyTechnicianUpcomingService(int technicianId, string referenceCode, TimeSpan scheduledStartTime)
        {
            using (var db = new ApplicationDbContext())
            {
                db.Notifications.Add(new Notification()
                {
                    RecipientId = technicianId,
                    Title = "Upcoming Scheduled Service",
                    Message = "You have an upcoming scheduled service for request (" + referenceCode + ") starting at "
                        + DateTime.Today.Add(scheduledStartTime).ToString("hh:mm tt")
                        + " today. Please prepare accordingly.",
                    ForAdmin = false,
                    ForIT = false,
                    IsActive = true,
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                });
                db.SaveChanges();
                RefreshUserUi(technicianId);
            }
        }

        #endregion

        #region Admin Notification Methods

        public void RefreshAllAdminsUi()
        {
            NotificationHub.RefreshAdminNotificationList();
            NotificationHub.RefreshAdminNotificationBadge();
        }

        public void NotifyAdminNewRegistrationRequest()
        {
            using (var db = new ApplicationDbContext())
            {
                db.Notifications.Add(new Notification()
                {
                    RecipientId = null,
                    Title = "New Registration Request",
                    Message = "A new user registration has been submitted. Please review and approve or reject the request.",
                    ForAdmin = true,
                    ForIT = false,
                    IsActive = true,
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                });
                db.SaveChanges();

                RefreshAllAdminsUi();
                AppUserRegistrationHub.RefreshAppUserRequestList();
            }
        }

        #endregion

        #region Client Notification Methods

        public void NotifyClientOnEnqueuedRequest(int clientId, string refenceCode, string technicianFirstName)
        {
            using (var db = new ApplicationDbContext())
            {
                db.Notifications.Add(new Notification()
                {
                    RecipientId = clientId,
                    Title = "Queued Request Status Update",
                    Message = "Your queued (" + refenceCode + ") request is now on processed." + 
                        (string.IsNullOrEmpty(technicianFirstName) ? (" Assigned technician: " + technicianFirstName + ".") : ""),
                    ForAdmin = false,
                    ForIT = false,
                    IsActive = true,
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                });
                db.SaveChanges();

                RefreshUserUi(clientId);
            }
        }

        #endregion

        #region General Notification Methods

        public void NotifyNewEquipmentEntry(string equipmentAssetTag, string creatorFirstName)
        {
            using (var db = new ApplicationDbContext())
            {
                db.Notifications.Add(new Notification()
                {
                    RecipientId = null,
                    Title = "New Equipment Entry",
                    Message = "A new equipment entry has been added by " + creatorFirstName + ". Equipment Asset Tag: " + equipmentAssetTag + ".",
                    ForAdmin = true,
                    ForIT = true,
                    IsActive = true,
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                });
                db.SaveChanges();

                RefreshAllAdminsUi();
                RefreshAllTechniciansUi();
            }
        }

        #endregion

        #region Utilities

        public string BuildRecipientMessageFromRequestStatus(int statusId, string referenceCode, string technicianFirstName)
        {
            var statusName = RequestStatusEnum.DisplayName(statusId).ToLowerInvariant();
            var reference = string.IsNullOrWhiteSpace(referenceCode) ? "your request" : ("request " + referenceCode);

            switch (statusId)
            {
                case RequestStatusEnum.PENDING:
                    return "Your " + reference + " is pending. We will assign a technician soon.";

                case RequestStatusEnum.ONGOING:
                    return "Your " + reference + " is now in progress. Assigned technician: " + technicianFirstName + ".";

                case RequestStatusEnum.RESOLVED:
                    return "Your " + reference + " has been resolved. Please verify the solution.";

                case RequestStatusEnum.CLOSED:
                    return "Your " + reference + " is now closed.";

                case RequestStatusEnum.CANCELLED:
                    return "Your " + reference + " has been cancelled. Please contact support if this is unexpected.";

                default:
                    return "Your " + reference + " status was updated to " + statusName + ".";
            }
        }

        public string BuildRecipientMessageFromRequestSeverity(int severityId, string referenceCode, int? oldSeverityId = null)
        {
            var severityName = RequestSeverityEnum.DisplayName(severityId).ToLowerInvariant();

            var isLowerSeverity = oldSeverityId.HasValue && severityId < oldSeverityId.Value;
            if (oldSeverityId.HasValue)
            {
                return "The severity of your request was " + (isLowerSeverity ? "de-escalated" : "escalted") + " to " + severityName + ". Reference: " + referenceCode + ".";
            }
            return "The severity of your request was changed to " + severityName + ". Reference: " + referenceCode + ".";
        }

        #endregion

    }

}