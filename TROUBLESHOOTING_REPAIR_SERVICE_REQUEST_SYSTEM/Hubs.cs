using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Attributes;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Enumerables;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM
{
    public abstract class BaseHub : Hub
    {
        protected const string AdminGroupName = "ADMIN";
        protected const string ITGroupName = "IT";

        public override async Task OnConnected()
        {
            var email = Context.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(email))
            {
                // If the email is not available, we cannot determine the recipient, so we won't add the connection to any group.
                await base.OnConnected();
                return;
            }

            using (var _db = new ApplicationDbContext())
            {
                // Look up the registration ID associated with the email.
                var recipientRegistrationAccountType = _db.Registrations
                    .Where(r => r.Email == email)
                    .Select(r => r.AccountType)
                    .FirstOrDefault();
                if (recipientRegistrationAccountType != null)
                {
                    if (AccountTypeEnum.IsAdmin(recipientRegistrationAccountType))
                    {
                        // Add the connection to a group named "ADMIN" so that we can target notifications to admins
                        await Groups.Add(Context.ConnectionId, AdminGroupName);
                    }
                    else if (AccountTypeEnum.IsIT(recipientRegistrationAccountType))
                    {
                        // Add the connection to a group named "IT" so that we can target notifications to IT staff
                        await Groups.Add(Context.ConnectionId, ITGroupName);
                    }
                }

                await base.OnConnected();
            }
        }

        private async Task JoinGroup(string groupName)
        {
            await Groups.Add(Context.ConnectionId, groupName);
        }

        private async Task LeaveGroup(string groupName)
        {
            await Groups.Remove(Context.ConnectionId, groupName);
        }
    }

    [Authorize2]
    public class RegistrationRequestHub : BaseHub
    {
        public override async Task OnConnected()
        {
            await base.OnConnected();
        }

        public static void RefreshRegistrationRequestList()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<RegistrationRequestHub>();
            context.Clients.Group(AdminGroupName).refreshRegistrationRequestList();
        }
    }

    [Authorize2]
    public class RegistrationHub : BaseHub
    {
        public override async Task OnConnected()
        {
            await base.OnConnected();
        }

        public static void RefreshRegistrationList()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<RegistrationHub>();
            context.Clients.Group(AdminGroupName).refreshRegistrationList();
        }
    }

    [Authorize2]
    public class TechnicalServiceRequestHub : BaseHub
    {
        public override async Task OnConnected()
        {
            await base.OnConnected();
        }

        public static void RefreshTechnicalServiceRequestList()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<TechnicalServiceRequestHub>();
            context.Clients.All.refreshTechnicalServiceRequestList();
        }

        public static void RefreshTechnicalServiceRequestSeverity(string severityName)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<TechnicalServiceRequestHub>();
            context.Clients.All.refreshTechnicalServiceRequestSeverity(severityName);
        }

        public static void RefreshTechnicalServiceRequestStatus(string statusName)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<TechnicalServiceRequestHub>();
            context.Clients.All.refreshTechnicalServiceRequestStatus(statusName);
        }

        public static void RefreshTechnicalServiceRequestActionHistory(int technicalServiceRequestHistoryId)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<TechnicalServiceRequestHub>();
            context.Clients.All.refreshTechnicalServiceRequestActionHistory(technicalServiceRequestHistoryId);
        }

    }

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

        public static void RefreshNotificationList(int recipientRegistrationId)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            context.Clients.Group(RecipientGroupName(recipientRegistrationId)).refreshNotificationList();
        }

        public static void RefreshNotificationBadge(int recipientRegistrationId)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            context.Clients.Group(RecipientGroupName(recipientRegistrationId)).refreshNotificationBadge();
        }

        // Admin

        public static void RefreshAdminNotificationList()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            context.Clients.Group(AdminGroupName).refreshAdminNotificationList();
        }

        public static void RefreshAdminNotificationBadge()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            context.Clients.Group(AdminGroupName).refreshAdminNotificationBadge();
        }

        // IT

        public static void RefreshITNotificationList()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            context.Clients.Group(ITGroupName).refreshITNotificationList();
        }

        public static void RefreshITNotificationBadge()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            context.Clients.Group(ITGroupName).refreshITNotificationBadge();
        }

    }
}