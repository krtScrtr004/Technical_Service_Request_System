using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Attributes;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Core
{
    [Authorize2]
    public class RequestHub : BaseHub
    {
        public override async Task OnConnected()
        {
            await base.OnConnected();
        }

        public static void RefreshRequestList()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<RequestHub>();
            context.Clients.All.refreshRequestList();

            DashboardHub.RefreshDashboard();
        }

        public static void RefreshRequestSeverity(int id, string severity)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<RequestHub>();
            context.Clients.All.refreshRequestSeverity(id, severity);

            DashboardHub.RefreshDashboard();
        }

        public static void RefreshRequestStatus(int id, string status)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<RequestHub>();
            context.Clients.All.refreshRequestStatus(id, status);

            DashboardHub.RefreshDashboard();
        }

        public static void RefreshRequestDescription(int id, string description)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<RequestHub>();
            context.Clients.All.refreshRequestDescription(id, description);

            DashboardHub.RefreshDashboard();
        }

        public static void RefreshRequestActionHistory(int historyId, int requestId)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<RequestHub>();
            context.Clients.All.refreshRequestActionHistory(historyId, requestId);

            DashboardHub.RefreshDashboard();
        }

        public static void RefreshRequestFormGeneration(int id)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<RequestHub>();
            context.Clients.All.refreshRequestFormGeneration(id);

            DashboardHub.RefreshDashboard();
        }

    }
}