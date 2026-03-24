using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Enumerables;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models
{
    [Serializable]
    public class TechnicalServiceRequestHistory
    {
        public int Id { get; set; }

        public int? TechnicalServiceRequestId { get; set; }
        public virtual TechnicalServiceRequest TechnicalServiceRequest { get; set; }

        public int? TechnicalServiceRequestStatusId { get; set; }
        public virtual TechnicalServiceRequestStatus TechnicalServiceRequestStatus { get; set; }

        public DateTime? DateAction { get; set; }
        public string ActionTaken { get; set; }

        public int? ActionTakenByRegistrationId { get; set; }
        [ForeignKey("ActionTakenByRegistrationId")]
        public virtual Registration ActionTakenByRegistration { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}