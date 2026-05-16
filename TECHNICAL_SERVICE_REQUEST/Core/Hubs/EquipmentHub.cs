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
}