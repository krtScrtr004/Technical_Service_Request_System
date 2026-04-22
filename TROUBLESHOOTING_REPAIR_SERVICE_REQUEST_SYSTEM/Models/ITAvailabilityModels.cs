using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Core;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models
{
    public class ITAvailability
    {
        public int Id { get; set; }
        [Index]
        public int RegistrationId { get; set; }
        public virtual Registration Registration { get; set; }
        public DateTime BlockDate { get; set; }
    }

    public class ITAvailabilityManageViewModel
    {
        public int UserId { get; set; }
        public DateTime UserRegistrationDate { get; set; }
        public string SelectedStringDates { get; set; }
    }

}