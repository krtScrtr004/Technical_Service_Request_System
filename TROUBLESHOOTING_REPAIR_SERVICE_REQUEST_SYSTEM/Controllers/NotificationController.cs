using PagedList;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Attributes;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Core;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Enumerables;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Services;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Controllers
{
    [Authorize2]
    [AuthenticateUserPrivilege(new int[] { AccountTypeEnum.STANDARD, AccountTypeEnum.IT, AccountTypeEnum.ADMIN })]
    public class NotificationController : BaseController
    {
        // GET: Notification
        public ActionResult Index(int page = 1, int pageSize = 5)
        {
            var currentUser = GetUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            var notificationsQuery = _db.Notifications
                 .OrderByDescending(r => r.CreatedAt)
                 .ThenByDescending(r => r.Id)
                 .AsQueryable();

            if (AccountTypeEnum.IsAdmin(currentUser.PrivilegeIds))
            {
                notificationsQuery = notificationsQuery.Where(r =>
                    r.ForAdmin == true ||
                    r.RecipientRegistrationId == currentUser.Id);
            }
            else if (AccountTypeEnum.IsIT(currentUser.PrivilegeIds))
            {
                notificationsQuery = notificationsQuery.Where(r =>
                    r.ForIT == true ||
                    r.RecipientRegistrationId == currentUser.Id);
            }
            else
            {
                notificationsQuery = notificationsQuery.Where(r =>
                    r.RecipientRegistrationId == currentUser.Id);
            }

            var pagedNotifications = notificationsQuery.ToPagedList(page, pageSize);
            var notificationDetails = new StaticPagedList<NotificationDetailViewModels>(
                pagedNotifications.Select(NotificationTypeCaster.ToNotificationDetailViewModels),
                pagedNotifications.GetMetaData()
            );

            ViewBag.CurrentUser = currentUser;
            return View(new NotificationIndexViewModels() { Notifications = notificationDetails });
        }

        public ActionResult ListPartial(int pageSize = 10)
        {
            var currentUser = GetUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            var notifications = _db.Notifications
                .OrderByDescending(r => r.CreatedAt)
                .ThenByDescending(r => r.Id)
                .Take(pageSize);

            if (AccountTypeEnum.IsAdmin(currentUser.PrivilegeIds))
            {
                notifications = notifications.Where(i =>
                    i.ForAdmin == true ||
                    i.RecipientRegistrationId == currentUser.Id
                );
            }
            else if (AccountTypeEnum.IsIT(currentUser.PrivilegeIds))
            {
                notifications = notifications.Where(i =>
                    i.ForIT == true ||
                    i.RecipientRegistrationId == currentUser.Id
                );
            }
            else
            {
                notifications = notifications.Where(i => i.RecipientRegistrationId == currentUser.Id);
            }

            var notificationDetails = notifications
                .ToList()
                .Select(NotificationTypeCaster.ToNotificationDetailViewModels)
                .ToList();
            return PartialView("~/Views/Notification/Partial/_NotificationList.cshtml", notificationDetails);
        }

        public ActionResult HeaderNotificationBadge()
        {
            var currentUser = GetUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            var isAdmin = AccountTypeEnum.IsAdmin(currentUser.PrivilegeIds);
            var query = _db.Notifications
                .Where(i => i.IsRead == false)
                .AsQueryable();
            if (AccountTypeEnum.IsAdmin(currentUser.PrivilegeIds))
            {
                query = query.Where(i =>
                    i.ForAdmin == true ||
                    i.RecipientRegistrationId == currentUser.Id
                );
            }
            else if (AccountTypeEnum.IsIT(currentUser.PrivilegeIds))
            {
                query = query.Where(i =>
                    i.ForIT == true ||
                    i.RecipientRegistrationId == currentUser.Id
                );
            }
            else
            {
                query = query.Where(i => i.RecipientRegistrationId == currentUser.Id);
            }

            var model = new NotificationBadgeViewModels()
            {
                UnreadCount = query.Count()
            };

            return PartialView("~/Views/Notification/Partial/_HeaderNotificationBadge.cshtml", model);
        }

        #region API

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAsRead(int id)
        {
            try
            {
                var currentUser = GetUserSession();
                if (currentUser == null)
                {
                    throw new Exception("User not found.");
                }

                var notification = _db.Notifications.Find(id);
                if (notification == null)
                {
                    throw new Exception("Notification not found.");
                }

                if ((AccountTypeEnum.IsAdmin(currentUser.PrivilegeIds) && notification.ForAdmin) ||
                    (AccountTypeEnum.IsIT(currentUser.PrivilegeIds) && notification.ForIT) ||
                    (notification.RecipientRegistrationId == currentUser.Id))
                {
                    notification.IsRead = true;
                    _db.Entry(notification).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();

                    // Refresh the user's UI to reflect the change in notification status
                    if (notification.ForAdmin == true)
                    {
                        NotificationHub.RefreshAdminNotificationBadge();
                        NotificationHub.RefreshAdminNotificationList();
                    }
                    else if (notification.ForIT == true)
                    {
                        NotificationHub.RefreshITNotificationBadge();
                        NotificationHub.RefreshITNotificationList();
                    }
                    else if (notification.RecipientRegistrationId.HasValue)
                    {
                        NotificationHub.RefreshNotificationBadge(notification.RecipientRegistrationId.Value);
                    }

                    Log.Information($"User {currentUser.Id} marked notification {notification.Id} as read.");
                    return Json(new
                    {
                        success = true,
                        message = "Notification marked as read."
                    });
                }
                else
                {
                    Log.Warning($"User {currentUser.Id} attempted to mark notification {notification.Id} as read without permission.");
                    throw new Exception("You are not allowed to perform this action.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error occurred while marking notification as read for user ID {GetUserSession()?.Id.ToString() ?? "Unknown"}.");
                return Json(new
                {
                    success = false,
                    message = "An error occurred while marking the notification as read: " + ex.Message
                });
            }
        }

        #endregion


    }
}