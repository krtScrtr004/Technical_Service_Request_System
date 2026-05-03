using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Attributes;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Core
{
    [Authorize2]
    public class NotificationHub : BaseHub
    {
        private static string RecipientGroupName(int recipientRegistrationId)
        {
            return "recipient:" + recipientRegistrationId;
        }

        public override async Task OnConnected()
        {
            await base.OnConnected();

            // The email is used to identify the recipient of the notification.
            var email = Context.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
            {
                return;
            }

            using (var _db = new ApplicationDbContext())
            {
                // Look up the registration ID associated with the email.
                var recipientRegistrationId = _db.Registrations
                    .Where(r => r.Email == email)
                    .Select(r => r.Id)
                    .FirstOrDefault();
                if (recipientRegistrationId > 0)
                {
                    // Add the connection to a group named "recipient:{recipientRegistrationId}" so that we can target notifications to this recipient.
                    await Groups.Add(Context.ConnectionId, RecipientGroupName(recipientRegistrationId));
                }
            }
        }

        // By ID

        public static void RefreshNotificationList(int id)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            context.Clients.Group(RecipientGroupName(id)).refreshNotificationList();

            DashboardHub.RefreshDashboard();
        }

        public static void RefreshNotificationBadge(int id)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            context.Clients.Group(RecipientGroupName(id)).refreshNotificationBadge();

            DashboardHub.RefreshDashboard();
        }

        // Admin

        public static void RefreshAdminNotificationList()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            context.Clients.Group(AdminGroupName).refreshAdminNotificationList();

            DashboardHub.RefreshDashboard();
        }

        public static void RefreshAdminNotificationBadge()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            context.Clients.Group(AdminGroupName).refreshAdminNotificationBadge();

            DashboardHub.RefreshDashboard();
        }

        // IT

        public static void RefreshITNotificationList()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            context.Clients.Group(ITGroupName).refreshITNotificationList();

            DashboardHub.RefreshDashboard();
        }

        public static void RefreshITNotificationBadge()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            context.Clients.Group(ITGroupName).refreshITNotificationBadge();

            DashboardHub.RefreshDashboard();
        }

    }

}