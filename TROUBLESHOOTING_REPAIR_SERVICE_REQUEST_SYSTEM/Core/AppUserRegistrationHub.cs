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
    public class AppUserRegistrationHub : BaseHub
    {
        public override async Task OnConnected()
        {
            await base.OnConnected();
        }

        public static void RefreshAppUserRequestList()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<AppUserRegistrationHub>();
            context.Clients.Group(AdminGroupName).refreshAppUserRequestList();

            DashboardHub.RefreshDashboard();
        }
    }
}