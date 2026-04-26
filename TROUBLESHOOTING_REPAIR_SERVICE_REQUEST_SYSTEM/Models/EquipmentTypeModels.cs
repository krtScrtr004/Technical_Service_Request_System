using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models
{
    public class EquipmentType
    {
        public int Id { get; set; }
        public string EquipmentTypeName { get; set; }

        public int? EquipmentCategoryId { get; set; }
        public virtual EquipmentCategory EquipmentCategory { get; set; }
    }
}