using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models
{
    public class ScheduledControlProcessDetail
    {
        public int Id { get; set; }

        [Index]
        public DateTime? TechnicalServiceRequestScheduledDate { get; set; }
        public TimeSpan? TechnicalServiceRequestScheduledStartTime { get; set; }
        public TimeSpan? TechnicalServiceRequestScheduledEndTime { get; set; }
    }
}