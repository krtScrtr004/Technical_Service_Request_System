using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TECHNICAL_SERVICE_REQUEST.Attributes;

namespace TECHNICAL_SERVICE_REQUEST.Core
{
    [Authorize2]
    public class AppUserHub : BaseHub
    {
        public override async Task OnConnected()
        {
            await base.OnConnected();
        }

        public static void RefreshAppUserList()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<AppUserHub>();
            context.Clients.Group(AdminGroupName).refreshAppUserList();

            DashboardHub.RefreshDashboard();
        }
    }
}