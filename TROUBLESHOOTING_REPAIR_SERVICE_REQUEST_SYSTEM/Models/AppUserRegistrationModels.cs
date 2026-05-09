using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Enumerables;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models
{
    public class AppUserRegistration
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string ExtensionName { get; set; }
        [MaxLength(255)]
        [Index]
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public string Code { get; set; }

        public string Office { get; set; }
        public string Position { get; set; }

        // TODO: Remove IsDenied - use IsApproved with null value to indicate pending status
        public bool IsApproved { get; set; }
        public bool IsDenied { get; set; }
        public DateTime? RequestDate { get; set; }
    }

    public class AppUserRegistrationCreateViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [MinLength(length: 1, ErrorMessage = "The minimum length is 1.")]
        [MaxLength(100, ErrorMessage = "The maximum length is 100.")]
        [RegularExpression(ValueConstants.VALID_NAME_REGEX,
            ErrorMessage = ValueConstants.VALID_NAME_REGEX_MESSAGE)]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        [MinLength(length: 1, ErrorMessage = "The minimum length is 1.")]
        [MaxLength(100, ErrorMessage = "The maximum length is 100.")]
        [RegularExpression(ValueConstants.VALID_NAME_REGEX,
            ErrorMessage = ValueConstants.VALID_NAME_REGEX_MESSAGE)]
        public string MiddleName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [MinLength(length: 1, ErrorMessage = "The minimum length is 1.")]
        [MaxLength(100, ErrorMessage = "The maximum length is 100.")]
        [RegularExpression(ValueConstants.VALID_NAME_REGEX,
            ErrorMessage = ValueConstants.VALID_NAME_REGEX_MESSAGE)]
        public string LastName { get; set; }

        [Display(Name = "Extension Name")]
        [MinLength(length: 1, ErrorMessage = "The minimum length is 1.")]
        [MaxLength(100, ErrorMessage = "The maximum length is 100.")]
        [RegularExpression(ValueConstants.VALID_NAME_REGEX,
            ErrorMessage = ValueConstants.VALID_NAME_REGEX_MESSAGE)]
        public string ExtensionName { get; set; }

        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(ValueConstants.VALID_EMAIL_REGEX, 
            ErrorMessage = ValueConstants.VALID_EMAIL_REGEX_MESSAGE)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Contact Number")]
        [DataType(DataType.PhoneNumber)]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [RegularExpression(ValueConstants.VALID_CONTACT_NUMBER_REGEX,
            ErrorMessage = ValueConstants.VALID_CONTACT_NUMBER_REGEX_MESSAGE)]
        public string ContactNumber { get; set; }

        [Required]
        [Display(Name = "Office")]
        [MinLength(length: 2, ErrorMessage = "The minimum length is 2.")]
        [MaxLength(100, ErrorMessage = "The maximum length is 100.")]
        [RegularExpression("^[a-zA-Z0-9\\s\\-\\.]+$", ErrorMessage = "Office field must contain only letters, numbers, spaces, hyphens, and periods.")]
        public string Office { get; set; }

        [Required]
        [Display(Name = "Position")]
        [MinLength(length: 2, ErrorMessage = "The minimum length is 2.")]
        [MaxLength(100, ErrorMessage = "The maximum length is 100.")]
        [RegularExpression("^[a-zA-Z0-9\\s\\-\\.]+$", ErrorMessage = "Position field must contain only letters, numbers, spaces, hyphens, and periods.")]
        public string Position { get; set; }
    }

    public class AppUserRegistrationVerifyAccountModel
    {
        public AppUserRegistration Registration { get; set; }
    }
}