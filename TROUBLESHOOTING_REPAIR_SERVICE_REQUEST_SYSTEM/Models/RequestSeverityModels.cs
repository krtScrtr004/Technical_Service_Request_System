using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models
{
    public class RequestSeverity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
    }
}