using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Attributes;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Enumerables;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Utilities;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models
{
    public class Request
    {
        public int Id { get; set; }

        public DateTime? DateRequest { get; set; }
        public DateTime? DateReceived { get; set; }

        [MaxLength(450)]
        [Index(IsUnique = true)]
        public string ReferenceCode { get; set; }

        public int ClientId { get; set; }
        public virtual Registration Client{ get; set; }

        // TECHNICAL SERVICE
        [Index]
        public int? TypeId { get; set; }
        public virtual RequestType Type { get; set; }

        [Index]
        public int? SeverityId { get; set; }
        public virtual RequestSeverity Severity { get; set; }

        [Index]
        public int? StatusId { get; set; }
        public virtual RequestStatus Status { get; set; }

        public string Others { get; set; } = string.Empty;
        public string Description { get; set; }

        // CONDITIONAL PROP: If the service requested is either Equipment Repair/Troubleshooting
        public int? EquipmentId { get; set; }
        public virtual Equipment Equipment { get; set; }

        // CONDITIONAL PROP: If the service requested is either Zoom/Webex Link, Livestream Setup, or Audio/Visual Setup
        public int? ScheduledControlProcessDetailId { get; set; }
        public virtual ScheduledControlProcessDetail ScheduledControlProcessDetail { get; set; }

        // Histories
        public virtual List<RequestHistory> Histories { get; set; }
    }

    // Index View Model
    public class RequestIndexViewModel
    {
        public List<Request> Requests { get; set; }

        public List<RequestStatus> RequestStatus { get; set; }

        public List<RequestSeverity> RequestSeverities { get; set; }
    }

    // Create View Model
    public class RequestCreateViewModel
    {
        protected readonly ApplicationDbContext _db;

        [HiddenInput]
        public int Id { get; set; }

        public string ReferenceCode { get; set; }

        // CLIENT INFORMATION

        [Required]
        [HiddenInput]
        public int ClientId { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [DisplayName("Last Name")]
        [MinLength(1, ErrorMessage = "The minimum length is 1")]
        [MaxLength(100, ErrorMessage = "The maximum length is 100")]
        [RegularExpression(ValueConstants.VALID_NAME_REGEX, 
            ErrorMessage = ValueConstants.VALID_NAME_REGEX_MESSAGE)]
        public string ClientLastName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [DisplayName("First Name")]
        [MinLength(1, ErrorMessage = "The minimum length is 1")]
        [MaxLength(100, ErrorMessage = "The maximum length is 100")]
        [RegularExpression(ValueConstants.VALID_NAME_REGEX,
            ErrorMessage = ValueConstants.VALID_NAME_REGEX_MESSAGE)]
        public string ClientFirstName { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Middle Name")]
        [MinLength(1, ErrorMessage = "The minimum length is 1")]
        [MaxLength(100, ErrorMessage = "The maximum length is 100")]
        [RegularExpression(ValueConstants.VALID_NAME_REGEX,
            ErrorMessage = ValueConstants.VALID_NAME_REGEX_MESSAGE)]
        public string ClientMiddleName { get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Extension Name")]
        [MinLength(1, ErrorMessage = "The minimum length is 1")]
        [MaxLength(5, ErrorMessage = "The maximum length is 5")]
        [RegularExpression(ValueConstants.VALID_NAME_REGEX, 
            ErrorMessage = ValueConstants.VALID_NAME_REGEX_MESSAGE)]
        public string ClientExtensionName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [DisplayName("Office")]
        [MinLength(1, ErrorMessage = "The minimum length is 1")]
        [MaxLength(50, ErrorMessage = "The maximum length is 50")]
        [RegularExpression("^[\\w\\s-_]+$", 
            ErrorMessage = "Office field must only contain letters, numbers, spaces, hyphens ( - ), and underscores ( _ ")]
        public string ClientOffice { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [DisplayName("Position")]
        [MinLength(1, ErrorMessage = "The minimum length is 1")]
        [MaxLength(50, ErrorMessage = "The maximum length is 50")]
        [RegularExpression("^[\\w\\s-_\\/]+$",
            ErrorMessage = "Position field must only contain letters, numbers, spaces, hyphens ( - ), and underscores ( _ )")]
        public string ClientPosition { get; set; }

        [Required]
        [Phone]
        [DataType(DataType.PhoneNumber)]
        [DisplayName("Contact Number")]
        [RegularExpression(ValueConstants.VALID_CONTACT_NUMBER_REGEX,
            ErrorMessage = ValueConstants.VALID_CONTACT_NUMBER_REGEX_MESSAGE)]
        public string ClientContactNumber { get; set; }

        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [DisplayName("Email Address")]
        [RegularExpression(ValueConstants.VALID_EMAIL_REGEX, 
            ErrorMessage = ValueConstants.VALID_EMAIL_REGEX_MESSAGE)]
        public string ClientEmailAddress { get; set; }

        // TECHNICAL SERVICE
        [SelectRequestType(otherPropertyName: "Others")]
        public int? TypeId { get; set; }
        [DisplayName("Type")]
        public IEnumerable<SelectListItem> Types { get; set; }

        public int? SeverityId { get; set; }
        [DisplayName("Severity")]
        public IEnumerable<SelectListItem> Severities {  get; set; }

        [DataType(DataType.Text)]
        [DisplayName("Specify Other Request")]
        [MinLength(1, ErrorMessage = "The minimum length is 1")]
        [MaxLength(100, ErrorMessage = "The maximum length is 100")]
        [RegularExpression("^[\\w\\s-_\\/]+$", 
            ErrorMessage = "Others field must only contain letters, numbers, spaces, hyphens ( - ), underscores ( _ ), and slashes ( / )")]
        public string Others { get; set; } = null;

        [DataType(DataType.Text)]
        [DisplayName("Description")]
        [MinLength(1, ErrorMessage = "The minimum length is 1")]
        [MaxLength(500, ErrorMessage = "The maximum length is 500")]
        [RegularExpression("^[\\w\\s-_\\/.,\\!\\?\\&\\(\\)\\\\{\\}\\[\\]]+$", 
            ErrorMessage = "Description field must only contain letters, numbers, spaces, hyphens ( - ), underscores ( _ ), slashes ( / ), dots ( . ), commas ( , ), exclamation marks ( ! ), question marks ( ? ), ampersands ( & ), parentheses ( ( ) ), curly braces ( {{ }} ), and square brackets ( [ ] )")]
        public string Description { get; set; } = null;


        // CONDITIONAL PROP: If the service requested is either Equipment Repair/Troubleshooting
        [HiddenInput]
        public int? EquipmentId { get; set; }

        [DisplayName("Model")]
        [DataType(DataType.Text)]
        [MinLength(1, ErrorMessage = "The minimum length is 1")]
        [MaxLength(100, ErrorMessage = "The maximum length is 100")]
        [CompleteEquipmentField(
            propertyDisplayName: "Model",
            relatedProperties: new[] { "EquipmentTypeId" },
            serviceTypePropertyName: "TypeId"
        )]
        [RegularExpression("^[\\w\\s-_\\/]+$",
            ErrorMessage = "Position field must only contain letters, numbers, spaces, hyphens ( - ), and underscores ( _ )")]
        public string EquipmentModel { get; set; }

        [DisplayName("Asset Tag")]
        [DataType(DataType.Text)]
        [MinLength(1, ErrorMessage = "The minimum length is 1")]
        [MaxLength(100, ErrorMessage = "The maximum length is 100")]
        [CompleteEquipmentField(
            propertyDisplayName: "Asset Tag",
            relatedProperties: new[] { "EquipmentTypeId", "EquipmentModel" },
            serviceTypePropertyName: "TypeId"
        )]
        [RegularExpression("^[\\w\\s-_\\/]+$",
            ErrorMessage = "Position field must only contain letters, numbers, spaces, hyphens ( - ), and underscores ( _ )")]
        public string EquipmentAssetTag{ get; set; }

        [CompleteEquipmentField(
            propertyDisplayName: "Type",
            relatedProperties: new[] { "EquipmentAssetTag", "EquipmentModel" },
            serviceTypePropertyName: "TypeId"
        )]
        public int? EquipmentTypeId { get; set; }
        [DisplayName("Type")]
        public IEnumerable<SelectListItem> EquipmentTypes { get; set; }


        // CONDITIONAL PROP: If the service requested is either Zoom/Webex Link, Livestream Setup, or Audio/Visual Setup
        [DataType(DataType.Date)]
        [DisplayName("Date")]
        [DateRange(0, 30)]
        [ValidLivestreamSchedule(serviceTypeProperty: "TypeId")]
        public DateTime? ScheduledDate { get; set; }

        [DataType(DataType.Time)]
        [DisplayName("Start")]
        // 4:00 PM is the latest time allowed for scheduling a service request on the same day
        [TimeAllowed(minimumHourFromNow: 1, maximumHour: "16:00")]
        [ScheduledTime(scheduleDatePropertyName: "ScheduledDate", minimumHours: "8:00", maximumHours: "16:00")]
        [StartEndTimeCollision(startTimePropertyName: "ScheduledStartTime", endTimePropertyName: "ScheduledEndTime")]
        public TimeSpan? ScheduledStartTime { get; set; }

        [DataType(DataType.Time)]
        [DisplayName("End")]
        // 5:00 PM is the latest time allowed for scheduling a service request on the same day
        [TimeAllowed(minimumHourFromNow: 0, maximumHour: "17:00")]
        [StartEndTimeCollision(startTimePropertyName: "ScheduledStartTime", endTimePropertyName: "ScheduledEndTime")]
        public TimeSpan? ScheduledEndTime { get; set; }

        public RequestCreateViewModel()
        {
            InitializeModel();
        }

        public RequestCreateViewModel(
            string clientFirstName,
            string clientLastName,
            string clientMiddleName,
            string clientExtensionName,
            string clientOffice,
            string clientPosition,
            string clientContactNumber,
            string clientEmailAddress
        )
        {
            ClientFirstName = clientFirstName;
            ClientLastName = clientLastName;
            ClientMiddleName = clientMiddleName;
            ClientExtensionName = clientExtensionName;
            ClientOffice = clientOffice;
            ClientPosition = clientPosition;
            ClientContactNumber = clientContactNumber;
            ClientEmailAddress = clientEmailAddress;

            InitializeModel();
        }

        #region Helpers

        private void InitializeModel()
        {
            Types = RequestTypeEnum.GetSelectListItems();
            Severities = RequestSeverityEnum.GetSelectListItems();
            EquipmentTypes = EquipmentTypeEnum.GetSelectListItems();
        }
    }

    // Detail View Model
    public class RequestDetailsViewModel
    {
        private readonly ApplicationDbContext _db;

        [HiddenInput]
        public int Id { get; set; }

        [DisplayName("Reference Code")]
        public string ReferenceCode { get; set; }

        // CLIENT INFORMATION

        [HiddenInput]
        public int ClientId { get; set; }

        [DisplayName("Last Name")]
        public string ClientLastName { get; set; }

        [DisplayName("First Name")]
        public string ClientFirstName { get; set; }

        [DisplayName("Middle Name")]
        public string ClientMiddleName { get; set; }

        [DisplayName("Extension Name")]
        public string ClientExtensionName { get; set; }

        [DisplayName("Office")]
        public string ClientOffice { get; set; }

        [DisplayName("Position")]
        public string ClientPosition { get; set; }

        [DisplayName("Contact Number")]
        public string ClientContactNumber { get; set; }

        [DisplayName("Email Address")]
        public string ClientEmailAddress { get; set; }

        // TECHNICAL SERVICE
        public int? TypeId { get; set; }
        [DisplayName("Type")]
        public virtual RequestType Type { get; set; }

        public int? SeverityId { get; set; }

        [DisplayName("Severity")]
        public virtual RequestSeverity Severity { get; set; }
        [Required]
        public SelectList Severities { get; set; }

        [DisplayName("Status")]
        public int StatusId { get; set; }
        public virtual RequestStatus Status { get; set; }
        [Required]
        public SelectList Statuses { get; set; }

        [DisplayName("Request")]
        public string Others { get; set; } = string.Empty;

        [DisplayName("Description")]
        public string Description { get; set; } = string.Empty;

        [DisplayName("Date Requested")]
        public DateTime? DateRequest { get; set; }

        [DisplayName("Date Recieved")]
        public DateTime? DateRecieved { get; set; }

        // CONDITIONAL PROP: If the service requested is either Equipment Repair/Troubleshooting
        public string EquipmentModel { get; set; }
        public string EquipmentAssetTag { get; set; }
        public string EquipmentTypeName { get; set; }

        // CONDITIONAL PROP: If the service requested is either Zoom/Webex Link, Livestream Setup, or Audio/Visual Setup
        [DisplayName("Scheduled Date")]
        public DateTime? ScheduledDate { get; set; }

        [DisplayName("Scheduled Start Time")]
        public TimeSpan? ScheduledStartTime { get; set; }

        [DisplayName("Scheduled End Time")]
        public TimeSpan? ScheduledEndTime { get; set; }

        // This property is used to determine if the "Generate TSRF Form" button should be enabled or disabled in the view.
        public int? FormGeneratableHistoryId { get; set; }


        // Histories
        public virtual List<RequestHistory> RequestHistories { get; set; }


        public RequestDetailsViewModel()
        {
            Severities = new SelectList(
                RequestSeverityEnum.GetSelectListItems(),
                "Value", "Text"
            );

            Statuses = new SelectList(
                RequestStatusEnum.GetSelectListItems(),
                "Value", "Text"
            );
        }

    }

    public class RequestFormViewModel
    {
        public Request Request { get; set; }
        public RequestHistory History { get; set; }
    }

    #endregion

    #region StaticClasses

    public static class RequestTypeCaster
    {
        public static Request ToRequest(RequestCreateViewModel technicalServiceRequest)
        {
            var equipment = new Equipment
            {
                Model = technicalServiceRequest.EquipmentModel,
                AssetTag = technicalServiceRequest.EquipmentAssetTag,
                TypeId = technicalServiceRequest.EquipmentTypeId
            };

            var scheduledControlProcessDetail = new ScheduledControlProcessDetail
            {
                // For the scheduled date, combine the date and time properties from the view model into a single DateTime property in the model
                ScheduledDate = technicalServiceRequest.ScheduledDate.HasValue && technicalServiceRequest.ScheduledStartTime.HasValue
                    ? (DateTime?)technicalServiceRequest.ScheduledDate.Value.Date + technicalServiceRequest.ScheduledStartTime.Value
                    : null,
                ScheduledStartTime = technicalServiceRequest.ScheduledStartTime.HasValue
                    ? technicalServiceRequest.ScheduledStartTime.Value
                    : (TimeSpan?)null,
                ScheduledEndTime = technicalServiceRequest.ScheduledEndTime.HasValue
                    ? technicalServiceRequest.ScheduledEndTime.Value
                    : (TimeSpan?)null,
            };

            return new Request
            {
                Id = technicalServiceRequest.Id,
                ReferenceCode = technicalServiceRequest.ReferenceCode,

                ClientId = technicalServiceRequest.ClientId,

                TypeId = technicalServiceRequest.TypeId,
                SeverityId = technicalServiceRequest.SeverityId,
                Others = technicalServiceRequest.Others,
                Description = technicalServiceRequest.Description,

                EquipmentId = equipment != null ? equipment.Id : (int?)null,
                Equipment = equipment ?? null,

                ScheduledControlProcessDetailId = scheduledControlProcessDetail != null ? scheduledControlProcessDetail.Id : (int?)null,
                ScheduledControlProcessDetail = scheduledControlProcessDetail ?? null
            };
        }

        public static RequestDetailsViewModel ToRequestDetailsViewModel(Request technicalServiceRequest)
        {
            var _db = new ApplicationDbContext();

            return new RequestDetailsViewModel
            {
                Id = technicalServiceRequest.Id,
                ReferenceCode = technicalServiceRequest.ReferenceCode,

                ClientId = technicalServiceRequest.ClientId,
                ClientFirstName = technicalServiceRequest.Client.FirstName,
                ClientMiddleName = technicalServiceRequest.Client.MiddleName,
                ClientLastName = technicalServiceRequest.Client.LastName,
                ClientExtensionName = technicalServiceRequest.Client.ExtensionName,
                ClientEmailAddress = technicalServiceRequest.Client.Email,
                ClientContactNumber = technicalServiceRequest.Client.ContactNumber,
                ClientOffice = technicalServiceRequest.Client.Office,
                ClientPosition = technicalServiceRequest.Client.Position,

                TypeId = technicalServiceRequest.TypeId.GetValueOrDefault(),
                Type = technicalServiceRequest.Type,
                SeverityId = technicalServiceRequest.SeverityId.GetValueOrDefault(),
                Severity = technicalServiceRequest.Severity,
                Severities = new SelectList(
                    RequestSeverityEnum.GetSelectListItems(),
                    "Value", "Text"
                ),
                StatusId = technicalServiceRequest.StatusId.GetValueOrDefault(),
                Status = technicalServiceRequest.Status,
                Statuses = new SelectList(
                    RequestStatusEnum.GetSelectListItems(),
                    "Value", "Text"
                ),
                Others = technicalServiceRequest.Others,
                Description = technicalServiceRequest.Description,

                DateRequest = technicalServiceRequest.DateRequest,
                DateRecieved = technicalServiceRequest.DateReceived,

                EquipmentModel = technicalServiceRequest.Equipment?.Model,
                EquipmentAssetTag = technicalServiceRequest.Equipment?.AssetTag,
                EquipmentTypeName = technicalServiceRequest.Equipment?.Type.Name,

                ScheduledDate = technicalServiceRequest.ScheduledControlProcessDetail?.ScheduledDate,
                ScheduledStartTime = technicalServiceRequest.ScheduledControlProcessDetail?.ScheduledStartTime,
                ScheduledEndTime = technicalServiceRequest.ScheduledControlProcessDetail?.ScheduledEndTime,

                RequestHistories = technicalServiceRequest.Histories,
            };
        }
    }

    #endregion
}