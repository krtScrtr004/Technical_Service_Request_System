using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using TECHNICAL_SERVICE_REQUEST.Enumerables;

namespace TECHNICAL_SERVICE_REQUEST.Models
{
    public class AppUser
    {
        // Personal Information
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string ExtensionName { get; set; }

        // Account Information
        public string UserName { get; set; }
        [MaxLength(255)]
        [Index(IsUnique = true)]
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Code { get; set; }
        public int RoleId { get; set; }
        public virtual AppUserRole Role { get; set; }

        public string Office { get; set; }
        public string Position { get; set; }

        // Other Information
        public bool IsActive { get; set; }
        public int? RegistrationId { get; set; }
        public int? DeactivatedById { get; set; }
        public string DeactivatedRemarks { get; set; }

        // Navigation Props
        public virtual ICollection<ITAvailability> ITAvailabilities { get; set; }
    }

    public class AppUserDetailsViewModel
    {
        public AppUser User { get; set; }
        public AppUserRegistration Registration { get; set; }
    }

    public class AppUserCreateViewModel
    {
        public AppUser User { get; set; }
        public AppUserRegistration Registration { get; set; }

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
        [Display(Name = "Email")]
        [RegularExpression(ValueConstants.VALID_EMAIL_REGEX,
            ErrorMessage = ValueConstants.VALID_EMAIL_REGEX_MESSAGE)]
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

        public string Code { get; set; }

        [Required]
        [Display(Name = "Role")]
        [Range(AppUserRoleEnum.ADMIN, AppUserRoleEnum.STANDARD, ErrorMessage = "Invalid role.")]
        public int RoleId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ExpiryDate { get; set; }
    }

    public class AppUserVerifyAccountModel
    {
        public int Id { get; set; }
        public virtual AppUserRegistration Registration { get; set; }
    }

    public class AppUserDeactivationViewModel
    {
        public AppUser User { get; set; }
        public DateTime DateDeactivated { get; set; }
        public int DeactivatedById { get; set; }
        public virtual AppUser DeactivatedBy { get; set; }
        public string Remark { get; set; }
    }
}