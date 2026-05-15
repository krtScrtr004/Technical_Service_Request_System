using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TECHNICAL_SERVICE_REQUEST.Models
{
    public class EquipmentCategory
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        [Index(IsUnique = true)]
        public string Name { get; set; }
    }
}