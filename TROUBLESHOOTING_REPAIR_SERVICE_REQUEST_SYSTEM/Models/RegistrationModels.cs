using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models
{
    public class Registration
    {
        // Personal Information
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }

        // Account Information
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string AccountType { get; set; }
        public string Code { get; set; }
        public int? ProjectId { get; set; }
        public virtual Project Project { get; set; }

        // Other Information
        public bool IsActive { get; set; }
        public bool IsUpdated { get; set; }
        public int? RegistrationRequestId { get; set; }
        public int? DeactivatedByRegistrationId { get; set; }
        public string DeactivatedRemarks { get; set; }
         public int? SessionPrivilegeId { get; set; }

        // Navigation Props
        public virtual ICollection<UserPrivilege> UserPrivileges { get; set; }
        public virtual ICollection<ITAvailability> ITAvailabilities { get; set; }
    }

    public class RegistrationDetailsViewModel
    {
        public Registration User { get; set; }
        public RegistrationRequest UserRegistrationRequest { get; set; }
        public List<UserPrivilege> UserPrivileges { get; set; }
    }

    public class RegistrationCreateViewModel
    {
        public Registration Registration { get; set; }
        public RegistrationRequest RegistrationRequest { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [MinLength(length: 1, ErrorMessage = "The minimum length is 1.")]
        [MaxLength(100, ErrorMessage = "The maximum length is 100.")]
        [RegularExpression("^[A-Za-z\\s\\-\\.\\,]+$", ErrorMessage = "Input contains invalid character(s)")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        [MinLength(length: 1, ErrorMessage = "The minimum length is 1.")]
        [MaxLength(100, ErrorMessage = "The maximum length is 100.")]
        [RegularExpression("^[A-Za-z\\s\\-\\.\\,]+$", ErrorMessage = "Input contains invalid character(s)")]
        public string MiddleName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [MinLength(length: 1, ErrorMessage = "The minimum length is 1.")]
        [MaxLength(100, ErrorMessage = "The maximum length is 100.")]
        [RegularExpression("^[A-Za-z\\s\\-\\.\\,]+$", ErrorMessage = "Input contains invalid character(s)")]
        public string LastName { get; set; }

        //public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Contact Number")]
        [DataType(DataType.PhoneNumber)]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string ContactNumber { get; set; }
        
        public string Code { get; set; }

        public string AccountType { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ExpiryDate { get; set; }
    }

    public class RegistrationVerifyAccountModel
    {
        public int Id { get; set; }
        public virtual RegistrationRequest RegistrationRequest { get; set; }
    }

    public class Deactivation
    {
        public Registration Registration { get; set; }
        public DateTime DateDeactivated { get; set; }
        public int DeactivatedByRegistrationId { get; set; }
        public virtual Registration DeactivatedByRegistration { get; set; }
        public string Remark { get; set; }
    }
}