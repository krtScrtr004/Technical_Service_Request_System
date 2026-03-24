using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models
{
    public class TechnicalServiceRequestQueue
    {
        public int Id { get; set; }
        public int TechnicalServiceRequestId { get; set; }
        public virtual TechnicalServiceRequest TechnicalServiceRequest { get; set; }
        public DateTime QueuedAt { get; set; }
        public bool IsProcessed { get; set; } = false;
    }
}