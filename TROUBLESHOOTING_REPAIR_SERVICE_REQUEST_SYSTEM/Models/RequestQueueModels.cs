using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models
{
    public class RequestQueue
    {
        public int Id { get; set; }
        [Index]
        public int RequestId { get; set; }
        public virtual Request Request { get; set; }
        public DateTime QueuedAt { get; set; }
        public bool IsProcessed { get; set; } = false;
    }
}