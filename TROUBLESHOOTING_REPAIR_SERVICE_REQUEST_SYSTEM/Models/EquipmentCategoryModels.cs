using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models
{
    public class EquipmentCategory
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        [Index(IsUnique = true)]
        public string Name { get; set; }
    }
}