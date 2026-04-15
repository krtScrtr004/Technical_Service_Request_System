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
    public class TechnicalServiceRequest
    {
        public int Id { get; set; }

        public DateTime? DateRequest { get; set; }
        public DateTime? DateReceived { get; set; }

        [MaxLength(450)]
        [Index(IsUnique = true)]
        public string ReferenceCode { get; set; }


        // CLIENT INFORMATION
        public string ClientLastName { get; set; }
        public string ClientFirstName { get; set; }
        public string ClientMiddleName { get; set; }
        public string ClientExtensionName { get; set; }

        public string ClientOffice { get; set; }
        public string ClientPosition { get; set; }
        public string ClientContactNumber { get; set; }
        [MaxLength(255)]
        [Index]
        public string ClientEmailAddress { get; set; }

        // TECHNICAL SERVICE
        [Index]
        public int? TechnicalServiceTypeId { get; set; }
        public virtual TechnicalServiceType TechnicalServiceType { get; set; }

        [Index]
        public int? TechnicalServiceRequestSeverityId { get; set; }
        public virtual TechnicalServiceRequestSeverity TechnicalServiceRequestSeverity { get; set; }

        [Index]
        public int? TechnicalServiceRequestStatusId { get; set; }
        public virtual TechnicalServiceRequestStatus TechnicalServiceRequestStatus { get; set; }

        public string Others { get; set; } = string.Empty;
        public string TechnicalServiceRequestDescription { get; set; }

        // CONDITIONAL PROP: If the service requested is either Zoom/Webex Link, Livestream Setup, or Audio/Visual Setup
        [Index]
        public DateTime? TechnicalServiceRequestScheduledDate { get; set; }
        public TimeSpan? TechnicalServiceRequestScheduledStartTime { get; set; }
        public TimeSpan? TechnicalServiceRequestScheduledEndTime { get; set; }

        // Histories
        public virtual List<TechnicalServiceRequestHistory> TechnicalServiceRequestHistories { get; set; }
    }

    // Index View Model
    public class TechnicalServiceRequestIndexViewModel
    {
        public List<TechnicalServiceRequest> TechnicalServiceRequests { get; set; }

        public List<TechnicalServiceRequestStatus> TechnicalServiceRequestStatus { get; set; }

        public List<TechnicalServiceRequestSeverity> TechnicalServiceRequestSeverities { get; set; }
    }

    // Create View Model
    public class TechnicalServiceRequestCreateViewModel
    {
        protected readonly ApplicationDbContext _db;

        [HiddenInput]
        public int Id { get; set; }

        public string ReferenceCode { get; set; }

        // CLIENT INFORMATION
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
            ErrorMessage = "Position field must only contain letters, numbers, spaces, hyphens ( - ), and underscores ( _ ")]
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
        [SelectTechnicalServiceRequestType(otherPropertyName: "Others")]
        public int? TechnicalServiceTypeId { get; set; }
        [DisplayName("Type")]
        public SelectList TechnicalServiceTypes { get; set; }

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
        public string TechnicalServiceRequestDescription { get; set; } = null;

        // CONDITIONAL PROP: If the service requested is either Zoom/Webex Link, Livestream Setup, or Audio/Visual Setup
        [DataType(DataType.Date)]
        [DisplayName("Date")]
        [DateRange(0, 30)]
        [ValidLivestreamSchedule(serviceTypeProperty: "TechnicalServiceTypeId")]
        public DateTime? TechnicalServiceRequestScheduledDate { get; set; }

        [DataType(DataType.Time)]
        [DisplayName("Start")]
        // 4:00 PM is the latest time allowed for scheduling a service request on the same day
        [TimeAllowed(minimumHourFromNow: 1, maximumHour: "16:00")]
        [ScheduledTime(scheduleDatePropertyName: "TechnicalServiceRequestScheduledDate", minimumHours: "8:00", maximumHours: "16:00")]
        [StartEndTimeCollision(startTimePropertyName: "TechnicalServiceRequestScheduledStartTime", endTimePropertyName: "TechnicalServiceRequestScheduledEndTime")]
        public TimeSpan? TechnicalServiceRequestScheduledStartTime { get; set; }

        [DataType(DataType.Time)]
        [DisplayName("End")]
        [StartEndTimeCollision(startTimePropertyName: "TechnicalServiceRequestScheduledStartTime", endTimePropertyName: "TechnicalServiceRequestScheduledEndTime")]
        public TimeSpan? TechnicalServiceRequestScheduledEndTime { get; set; }

        public TechnicalServiceRequestCreateViewModel()
        {
            _db = new ApplicationDbContext();
            InitializeModel();
        }

        public TechnicalServiceRequestCreateViewModel(
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
            _db = new ApplicationDbContext();

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
            TechnicalServiceTypes = new SelectList(
                _db.TechnicalServiceTypes.OrderBy(i => i.TechnicalServiceTypeName), 
                "Id", "TechnicalServiceTypeName"
            );
        }
    }

    // Detail View Model
    public class TechnicalServiceRequestDetailsViewModel
    {
        private readonly ApplicationDbContext _db;

        [HiddenInput]
        public int Id { get; set; }

        [DisplayName("Reference Code")]
        public string ReferenceCode { get; set; }

        // CLIENT INFORMATION
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
        public int? TechnicalServiceTypeId { get; set; }
        [DisplayName("Type")]
        public virtual TechnicalServiceType TechnicalServiceType { get; set; }

        public int? TechnicalServiceRequestSeverityId { get; set; }

        [DisplayName("Severity")]
        public virtual TechnicalServiceRequestSeverity TechnicalServiceRequestSeverity { get; set; }
        [Required]
        public SelectList TechnicalServiceRequestSeverities { get; set; }

        [DisplayName("Status")]
        public int TechnicalServiceRequestStatusId { get; set; }
        public virtual TechnicalServiceRequestStatus TechnicalServiceRequestStatus { get; set; }
        [Required]
        public SelectList TechnicalServiceRequestStatuses { get; set; }

        [DisplayName("Request")]
        public string Others { get; set; } = string.Empty;

        [DisplayName("Description")]
        public string TechnicalServiceRequestDescription { get; set; } = string.Empty;

        // CONDITIONAL PROP: If the service requested is either Zoom/Webex Link, Livestream Setup, or Audio/Visual Setup
        [DisplayName("Scheduled Date")]
        public DateTime? TechnicalServiceRequestScheduledDate { get; set; }

        [DisplayName("Scheduled Start Time")]
        public TimeSpan? TechnicalServiceRequestScheduledStartTime { get; set; }

        [DisplayName("Scheduled End Time")]
        public TimeSpan? TechnicalServiceRequestScheduledEndTime { get; set; }


        // Histories
        public virtual List<TechnicalServiceRequestHistory> TechnicalServiceRequestHistories { get; set; }

        public TechnicalServiceRequestDetailsViewModel()
        {
            _db = new ApplicationDbContext();

            TechnicalServiceRequestSeverities = new SelectList(
                _db.TechnicalServiceRequestSeverities,
                "Id", "SeverityName"
            );

            TechnicalServiceRequestStatuses = new SelectList(
                _db.TechnicalServiceRequestStatus,
                "Id", "TechnicalServiceRequestStatusName"
            );
        }

    }

    public class TechnicalServiceRequestFormViewModel
    {
        public TechnicalServiceRequest TechnicalServiceRequest { get; set; }
        public TechnicalServiceRequestHistory TechnicalServiceRequestHistory { get; set; }
    }

    #endregion

    #region StaticClasses

    public static class TechnicalServiceRequestTypeCaster
    {
        public static TechnicalServiceRequest ToTechnicalServiceRequest(TechnicalServiceRequestCreateViewModel technicalServiceRequest)
        {
            return new TechnicalServiceRequest
            {
                Id = technicalServiceRequest.Id,
                ReferenceCode = technicalServiceRequest.ReferenceCode,

                ClientLastName = technicalServiceRequest.ClientLastName,
                ClientFirstName = technicalServiceRequest.ClientFirstName,
                ClientMiddleName = technicalServiceRequest.ClientMiddleName,
                ClientExtensionName = technicalServiceRequest.ClientExtensionName,

                ClientOffice = technicalServiceRequest.ClientOffice,
                ClientPosition = technicalServiceRequest.ClientPosition,
                ClientContactNumber = technicalServiceRequest.ClientContactNumber,
                ClientEmailAddress = technicalServiceRequest.ClientEmailAddress,

                TechnicalServiceTypeId = technicalServiceRequest.TechnicalServiceTypeId,
                Others = technicalServiceRequest.Others,
                TechnicalServiceRequestDescription = technicalServiceRequest.TechnicalServiceRequestDescription,

                // For the scheduled date, combine the date and time properties from the view model into a single DateTime property in the model
                TechnicalServiceRequestScheduledDate = technicalServiceRequest.TechnicalServiceRequestScheduledDate,
                TechnicalServiceRequestScheduledStartTime = technicalServiceRequest.TechnicalServiceRequestScheduledStartTime,
                TechnicalServiceRequestScheduledEndTime = technicalServiceRequest.TechnicalServiceRequestScheduledEndTime,
            };
        }

        public static TechnicalServiceRequestDetailsViewModel ToTechnicalServiceRequestDetailsViewModel(TechnicalServiceRequest technicalServiceRequest)
        {
            var _db = new ApplicationDbContext();

            return new TechnicalServiceRequestDetailsViewModel
            {
                Id = technicalServiceRequest.Id,
                ReferenceCode = technicalServiceRequest.ReferenceCode,
                ClientLastName = technicalServiceRequest.ClientLastName,
                ClientFirstName = technicalServiceRequest.ClientFirstName,
                ClientMiddleName = technicalServiceRequest.ClientMiddleName,
                ClientExtensionName = technicalServiceRequest.ClientExtensionName,
                ClientOffice = technicalServiceRequest.ClientOffice,
                ClientPosition = technicalServiceRequest.ClientPosition,
                ClientContactNumber = technicalServiceRequest.ClientContactNumber,
                ClientEmailAddress = technicalServiceRequest.ClientEmailAddress,

                TechnicalServiceTypeId = technicalServiceRequest.TechnicalServiceTypeId.GetValueOrDefault(),
                TechnicalServiceType = technicalServiceRequest.TechnicalServiceType,
                TechnicalServiceRequestSeverityId = technicalServiceRequest.TechnicalServiceRequestSeverityId.GetValueOrDefault(),
                TechnicalServiceRequestSeverity = technicalServiceRequest.TechnicalServiceRequestSeverity,
                TechnicalServiceRequestSeverities = new SelectList(
                    _db.TechnicalServiceRequestSeverities,
                    "Id", "SeverityName"
                ),
                TechnicalServiceRequestStatusId = technicalServiceRequest.TechnicalServiceRequestStatusId.GetValueOrDefault(),
                TechnicalServiceRequestStatus = technicalServiceRequest.TechnicalServiceRequestStatus,
                TechnicalServiceRequestStatuses = new SelectList(
                    _db.TechnicalServiceRequestStatus,
                    "Id", "TechnicalServiceRequestStatusName"
                ),
                Others = technicalServiceRequest.Others,
                TechnicalServiceRequestDescription = technicalServiceRequest.TechnicalServiceRequestDescription,

                // For the scheduled date, combine the date and time properties from the view model into a single DateTime property in the model
                TechnicalServiceRequestScheduledDate = technicalServiceRequest.TechnicalServiceRequestScheduledDate,
                TechnicalServiceRequestScheduledStartTime = technicalServiceRequest.TechnicalServiceRequestScheduledStartTime,
                TechnicalServiceRequestScheduledEndTime = technicalServiceRequest.TechnicalServiceRequestScheduledEndTime,

                TechnicalServiceRequestHistories = technicalServiceRequest.TechnicalServiceRequestHistories,
            };
        }
    }

    #endregion
}