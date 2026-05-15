using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TECHNICAL_SERVICE_REQUEST.Models
{
    public class ScheduledControlProcessDetail
    {
        public int Id { get; set; }

        [Index]
        public DateTime? ScheduledDate { get; set; }
        public TimeSpan? ScheduledStartTime { get; set; }
        public TimeSpan? ScheduledEndTime { get; set; }
    }
}