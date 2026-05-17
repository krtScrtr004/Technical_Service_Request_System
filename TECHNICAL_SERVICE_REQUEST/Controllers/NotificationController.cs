using PagedList;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using TECHNICAL_SERVICE_REQUEST.Attributes;
using TECHNICAL_SERVICE_REQUEST.Core;
using TECHNICAL_SERVICE_REQUEST.Enumerables;
using TECHNICAL_SERVICE_REQUEST.Models;
using TECHNICAL_SERVICE_REQUEST.Services;

namespace TECHNICAL_SERVICE_REQUEST.Controllers
{
    [Authorize2]
    [AuthenticateUserPrivilege(new int[] { AppUserRoleEnum.STANDARD, AppUserRoleEnum.IT, AppUserRoleEnum.ADMIN })]
    public class NotificationController : BaseController
    {
        // GET: Notification
        public ActionResult Index(int page = 1, int pageSize = 5)
        {
            var currentUser = GetAppUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            try
            {
                var notificationsQuery = _db.Notifications
                     .OrderByDescending(r => r.CreatedAt)
                     .ThenByDescending(r => r.Id)
                     .AsQueryable();

                if (AppUserRoleEnum.IsAdmin(currentUser.RoleId))
                {
                    notificationsQuery = notificationsQuery.Where(r =>
                        r.ForAdmin == true ||
                        r.RecipientId == currentUser.Id);
                }
                else if (AppUserRoleEnum.IsIT(currentUser.RoleId))
                {
                    notificationsQuery = notificationsQuery.Where(r =>
                        r.ForIT == true ||
                        r.RecipientId == currentUser.Id);
                }
                else
                {
                    notificationsQuery = notificationsQuery.Where(r =>
                        r.RecipientId == currentUser.Id);
                }

                var pagedNotifications = notificationsQuery.ToPagedList(page, pageSize);
                var notificationDetails = new StaticPagedList<NotificationDetailViewModels>(
                    pagedNotifications.Select(NotificationTypeCaster.ToNotificationDetailViewModels),
                    pagedNotifications.GetMetaData()
                );

                ViewBag.CurrentUser = currentUser;
                return View(new NotificationIndexViewModels() { Notifications = notificationDetails });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An error occured while loading notifications list page: {ex.Message}");
                return View("Error", "Error");
            }
        }

        public ActionResult ListPartial(int pageSize = 10)
        {
            var currentUser = GetAppUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            var notifications = _db.Notifications
                .OrderByDescending(r => r.CreatedAt)
                .ThenByDescending(r => r.Id)
                .Take(pageSize);

            if (AppUserRoleEnum.IsAdmin(currentUser.RoleId))
            {
                notifications = notifications.Where(i =>
                    i.ForAdmin == true ||
                    i.RecipientId == currentUser.Id
                );
            }
            else if (AppUserRoleEnum.IsIT(currentUser.RoleId))
            {
                notifications = notifications.Where(i =>
                    i.ForIT == true ||
                    i.RecipientId == currentUser.Id
                );
            }
            else
            {
                notifications = notifications.Where(i => i.RecipientId == currentUser.Id);
            }

            var notificationDetails = notifications
                .ToList()
                .Select(NotificationTypeCaster.ToNotificationDetailViewModels)
                .ToList();
            return PartialView("~/Views/Notification/Partial/_NotificationList.cshtml", notificationDetails);
        }

        public ActionResult HeaderNotificationBadge()
        {
            var currentUser = GetAppUserSession();
            if (currentUser == null)
            {
                throw new HttpException(403, "Forbidden");
            }

            var isAdmin = AppUserRoleEnum.IsAdmin(currentUser.RoleId);
            var query = _db.Notifications
                .Where(i => i.IsRead == false)
                .AsQueryable();
            if (AppUserRoleEnum.IsAdmin(currentUser.RoleId))
            {
                query = query.Where(i =>
                    i.ForAdmin == true ||
                    i.RecipientId == currentUser.Id
                );
            }
            else if (AppUserRoleEnum.IsIT(currentUser.RoleId))
            {
                query = query.Where(i =>
                    i.ForIT == true ||
                    i.RecipientId == currentUser.Id
                );
            }
            else
            {
                query = query.Where(i => i.RecipientId == currentUser.Id);
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
                var currentUser = GetAppUserSession();
                if (currentUser == null)
                {
                    throw new Exception("User not found.");
                }

                var notification = _db.Notifications.Find(id);
                if (notification == null)
                {
                    throw new Exception("Notification not found.");
                }

                if ((AppUserRoleEnum.IsAdmin(currentUser.RoleId) && notification.ForAdmin) ||
                    (AppUserRoleEnum.IsIT(currentUser.RoleId) && notification.ForIT) ||
                    (notification.RecipientId == currentUser.Id))
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
                    else if (notification.RecipientId.HasValue)
                    {
                        NotificationHub.RefreshNotificationBadge(notification.RecipientId.Value);
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
                Log.Error(ex, $"Error occurred while marking notification as read for user ID {GetAppUserSession()?.Id.ToString() ?? "Unknown"}.");
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