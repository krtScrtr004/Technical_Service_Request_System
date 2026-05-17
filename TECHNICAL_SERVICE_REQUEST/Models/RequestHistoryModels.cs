using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using TECHNICAL_SERVICE_REQUEST.Enumerables;

namespace TECHNICAL_SERVICE_REQUEST.Models
{
    [Serializable]
    public class RequestHistory
    {
        public int Id { get; set; }

        [Index]
        public int? RequestId { get; set; }
        public virtual Request Request { get; set; }

        [Index]
        public int? StatusId { get; set; }
        public virtual RequestStatus Status { get; set; }

        public DateTime? DateAction { get; set; }
        public string ActionTaken { get; set; }

        [Index]
        public int? ActionTakenById { get; set; }
        [ForeignKey("ActionTakenById")]
        public virtual AppUser ActionTakenBy { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}