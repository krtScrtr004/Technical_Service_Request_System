using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using TECHNICAL_SERVICE_REQUEST.Core;

namespace TECHNICAL_SERVICE_REQUEST.Models
{
    public class ITAvailability
    {
        public int Id { get; set; }
        [Index]
        public int UserId { get; set; }
        public virtual AppUser User { get; set; }
        public DateTime BlockDate { get; set; }
    }

    public class ITAvailabilityManageViewModel
    {
        public int UserId { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string SelectedStringDates { get; set; }
    }

}