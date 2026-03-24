using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models
{
    public class Privilege
    {
        public int Id { get; set; }
        public string PrivilegeName { get; set; }
        public int? ProjectId { get; set; }
        public virtual Project Project { get; set; }
    }
}