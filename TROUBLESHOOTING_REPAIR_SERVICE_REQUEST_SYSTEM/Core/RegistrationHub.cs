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
}