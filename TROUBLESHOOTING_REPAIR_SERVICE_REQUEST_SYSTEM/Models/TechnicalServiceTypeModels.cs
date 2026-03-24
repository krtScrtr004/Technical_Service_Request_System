using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models
{
    public class TechnicalServiceType
    {
        public int Id { get; set; }
        public string TechnicalServiceTypeName { get; set; }
        public bool IsActive { get; set; }
    }
}