using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TECHNICAL_SERVICE_REQUEST.Models
{
    public class EquipmentLocation
    {
        public int Id { get; set; }
        public int BuildingNumber { get; set; }
        public int FloorNumber { get; set; }
        public string Office { get; set; }
        public bool IsActive { get; set; }
    }
}