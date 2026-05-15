using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace TECHNICAL_SERVICE_REQUEST.Core
{
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
}