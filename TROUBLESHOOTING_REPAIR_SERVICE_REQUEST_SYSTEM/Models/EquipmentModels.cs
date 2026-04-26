using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Attributes;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Enumerables;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models
{
    public class Equipment
    {
        public int Id { get; set; }

        public string EquipmentModel { get; set; }

        [MaxLength(100)]
        [Index(IsUnique = true)]
        public string AssetTag { get; set; } // Unique internal (organization) identifier for the equipment

        public int? EquipmentTypeId { get; set; }
        public virtual EquipmentType EquipmentType { get; set; }

        public int? EquipmentLocationId { get; set; }
        public virtual EquipmentLocation EquipmentLocation { get; set; }

        public int? EquipmentStatusId { get; set; }
        public virtual EquipmentStatus EquipmentStatus { get; set; }

        public int? CreatedByRegistrationId { get; set; }
        public virtual Registration CreatedByRegistration { get; set; }

        public int RepairCount { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class EquipmentFormViewModel
    {
        [HiddenInput]
        public int Id { get; set; }

        [Required]
        [DisplayName("Model")]
        [DataType(DataType.Text)]
        [MinLength(1, ErrorMessage = "The minimum length is 1")]
        [MaxLength(100, ErrorMessage = "The maximum length is 100")]
        [RegularExpression("^[\\w\\s-_\\/]+$",
            ErrorMessage = "Position field must only contain letters, numbers, spaces, hyphens ( - ), and underscores ( _ )")]
        public string EquipmentModel { get; set; }

        [Required]
        [DisplayName("Asset Tag")]
        [DataType(DataType.Text)]
        [MinLength(1, ErrorMessage = "The minimum length is 1")]
        [MaxLength(100, ErrorMessage = "The maximum length is 100")]
        [RegularExpression("^[\\w\\s-_\\/]+$",
           ErrorMessage = "Position field must only contain letters, numbers, spaces, hyphens ( - ), and underscores ( _ )")]
        public string AssetTag { get; set; }

        [Required]
        [DisplayName("Type")]
        public int EquipmentTypeId { get; set; }
        public IEnumerable<SelectListItem> EquipmentTypes { get; set; }

        [Required]
        [DisplayName("Status")]
        public int EquipmentStatusId { get; set; }
        public IEnumerable<SelectListItem> EquipmentStatuses { get; set; }

        [HiddenInput]
        public int? EquipmentLocationId { get; set; }

        [DisplayName("Building Nuber")]
        [CompleteEquipmentField(
            propertyDisplayName: "Building Number",
            relatedProperties: new[] { "FloorNumber", "Office" }
        )]
        [Range(1, 5, ErrorMessage = "Building Number must be between 1 and 5")]
        public int? BuildingNumber {  get; set; }
        public SelectList BuildingNumbers { get; set; }

        [DisplayName("Floor Nuber")]
        [CompleteEquipmentField(
            propertyDisplayName: "Floor Number",
            relatedProperties: new[] { "BuildingNumber", "Office" }
        )]
        [Range(1, 3, ErrorMessage = "Floor Number must be between 1 and 3")]
        public int? FloorNumber { get; set; }
        public SelectList FloorNumbers { get; set; }

        [Display(Name = "Office")]
        [MinLength(length: 2, ErrorMessage = "The minimum length is 2.")]
        [MaxLength(100, ErrorMessage = "The maximum length is 100.")]
        [CompleteEquipmentField(
            propertyDisplayName: "Office",
            relatedProperties: new[] { "BuildingNumber", "BuildingNumber" }
        )]
        [RegularExpression("^[a-zA-Z0-9\\s\\-\\.]+$", ErrorMessage = "Office field must contain only letters, numbers, spaces, hyphens, and periods.")]
        public string Office { get; set; }


        public EquipmentFormViewModel()
        {
            EquipmentTypes = EquipmentTypeEnum.GetSelectListItems();
            EquipmentStatuses = EquipmentStatusEnum.GetSelectListItems();
            BuildingNumbers = new SelectList(new List<int> { 1, 2, 3, 4, 5 });
            FloorNumbers = new SelectList(new List<int> { 1, 2, 3 });
        }
    }

    public class EquipmentDetailsViewModel
    {
        public int Id { get; set; }

        public string EquipmentModel { get; set; }
        public string AssetTag { get; set; }
        public int EquipmentTypeId { get; set; }

        public int? BuildingNumber { get; set; }
        public int? FloorNumber { get; set; }
        public string Office { get; set; }

        public int EquipmentStatusId { get; set; }
        public int RepairCount { get; set; }

        public string CreatedByRegistrationFirstName { get; set; }
        public string CreatedByRegistrationLastName { get; set; }
        public string CreatedByRegistrationMiddleName { get; set; }
        public string CreatedByRegistrationExtensionName { get; set; }
    }
}