using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models
{
    public class RegistrationRequest
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public string Code { get; set; }
        public bool UserProfilePicture { get; set; }


        public bool IsVerified { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDenied { get; set; }
        public DateTime? RequestDate { get; set; }

        public bool AccountInformation { get; set; }
        public bool UserPrivilegeInformation { get; set; }
        public bool EmployeeInformation { get; set; }
    }

    public class RegistrationRequestIndexViewModel
    {
        public List<RegistrationRequest> RegistrationRequests { get; set; }
    }

    public class RegistrationRequestCreateViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [MinLength(length: 1, ErrorMessage = "The minimum length is 1.")]
        [MaxLength(100, ErrorMessage = "The maximum length is 100.")]
        [RegularExpression("^[\\w\\s\\-\\.\\,]+$", ErrorMessage = "Input contains invalid character(s)")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        [MinLength(length: 1, ErrorMessage = "The minimum length is 1.")]
        [MaxLength(100, ErrorMessage = "The maximum length is 100.")]
        [RegularExpression("^[\\w\\s\\-\\.\\,]+$", ErrorMessage = "Input contains invalid character(s)")]
        public string MiddleName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [MinLength(length: 1, ErrorMessage = "The minimum length is 1.")]
        [MaxLength(100, ErrorMessage = "The maximum length is 100.")]
        [RegularExpression("^[\\w\\s\\-\\.\\,]+$", ErrorMessage = "Input contains invalid character(s)")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Contact Number")]
        [DataType(DataType.PhoneNumber)]
        public string ContactNumber { get; set; }
    }
}