using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models
{
    public class TechnicalServiceRequestSeverity
    {
        public int Id { get; set; }
        public string SeverityName { get; set; }
        public string Level { get; set; }
        public string TechnicalServiceRequestSeverityDescription { get; set; }
        public bool IsActive { get; set; }
    }
}