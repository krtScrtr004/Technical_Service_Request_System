using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
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
                var userPermission = _db.Registrations
                    .Where(r => r.Email == email)
                    .Select(r => r.Role.Name)
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(userPermission))
                {
                    if (AccountTypeEnum.IsAdmin(userPermission))
                    {
                        // Add the connection to a group named "ADMIN" so that we can target notifications to admins
                        await Groups.Add(Context.ConnectionId, AdminGroupName);
                    }
                    else if (AccountTypeEnum.IsIT(userPermission))
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

            DashboardHub.RefreshDashboard();
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

            DashboardHub.RefreshDashboard();
        }
    }

    [Authorize2]
    public class EquipmentHub : BaseHub
    {
        public override async Task OnConnected()
        {
            await base.OnConnected();
        }

        public static void RefreshEquipmentList()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<EquipmentHub>();
            context.Clients.All.refreshEquipmentList();

            DashboardHub.RefreshDashboard();
        }

        public static void RefreshEquipmentAssetTag(int id, string assetTag)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<EquipmentHub>();
            context.Clients.All.refreshEquipmentAssetTag(id, assetTag);
        }

        public static void RefreshEquipmentModel(int id, string model)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<EquipmentHub>();
            context.Clients.All.refreshEquipmentModel(id, model);
        }

        public static void RefreshEquipmentType(int id, string type)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<EquipmentHub>();
            context.Clients.All.refreshEquipmentType(id, type);
        }

        public static void RefreshEquipmentStatus(int id, string status)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<EquipmentHub>();
            context.Clients.All.refreshEquipmentStatus(id, status);
        }

        public static void RefreshEquipmentBuildingNumber(int id, int buildingNumber)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<EquipmentHub>();
            context.Clients.All.refreshEquipmentBuildingNumber(id, buildingNumber);
        }

        public static void RefreshEquipmentFloorNumber(int id, int floorNumber)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<EquipmentHub>();
            context.Clients.All.refreshEquipmentFloorNumber(id, floorNumber);
        }

        public static void RefreshEquipmentOffice(int id, string office)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<EquipmentHub>();
            context.Clients.All.refreshEquipmentOffice(id, office);
        }

        public static void RefreshEquipmentRepairCount(int id, int repairCount)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<EquipmentHub>();
            context.Clients.All.refreshEquipmentRepairCount(id, repairCount);
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

            DashboardHub.RefreshDashboard();
        }

        public static void RefreshTechnicalServiceRequestSeverity(int id, string severity)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<TechnicalServiceRequestHub>();
            context.Clients.All.refreshTechnicalServiceRequestSeverity(id, severity);

            DashboardHub.RefreshDashboard();
        }

        public static void RefreshTechnicalServiceRequestStatus(int id, string status)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<TechnicalServiceRequestHub>();
            context.Clients.All.refreshTechnicalServiceRequestStatus(id, status);

            DashboardHub.RefreshDashboard();
        }

        public static void RefreshTechnicalServiceRequestDescription(int id, string description)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<TechnicalServiceRequestHub>();
            context.Clients.All.refreshTechnicalServiceRequestDescription(id, description);

            DashboardHub.RefreshDashboard();
        }

        public static void RefreshTechnicalServiceRequestActionHistory(int historyId, int requestId)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<TechnicalServiceRequestHub>();
            context.Clients.All.refreshTechnicalServiceRequestActionHistory(historyId, requestId);

            DashboardHub.RefreshDashboard();
        }

        public static void RefreshTechnicalServiceRequestFormGeneration(int id)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<TechnicalServiceRequestHub>();
            context.Clients.All.refreshTechnicalServiceRequestFormGeneration(id);

            DashboardHub.RefreshDashboard();
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

    public class ITAvailabilityHub : BaseHub
    {
        public override async Task OnConnected()
        {
            await base.OnConnected();
        }

        public static void RefreshITAvailabilityTable(int id)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ITAvailabilityHub>();
            context.Clients.All.refreshITAvailabilityTable(id);

            DashboardHub.RefreshDashboard();
        }
    }

    [Authorize2]
    public class DashboardHub : BaseHub
    {
        public override async Task OnConnected()
        {
            await base.OnConnected();
        }

        public static void RefreshDashboard()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<DashboardHub>();
            context.Clients.All.refreshDashboard();
        }

    }

}