using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Services.Description;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Enumerables
{
    public static class ValueConstants
    {
        public const string VALID_NAME_REGEX = "^[A-Za-z\\s\\-.,'']+$";
        public const string VALID_NAME_REGEX_MESSAGE = "Name must only contain letters, spaces, hyphens ( - ), dots ( . ), commas ( , ), and apostrophes ( ' )";

        public const string VALID_EMAIL_REGEX = "^[a-zA-Z0-9._%+\\-!#$&'*\\/=?^{|}~]+@[a-zA-Z0-9-]+(\\.[a-zA-Z0-9-]+)*\\.[a-zA-Z]{2,}$";
        public const string VALID_EMAIL_REGEX_MESSAGE = "Email must contain a valid local part, followed by @, and a valid domain with a 2-or-more letter extension (e.g., .com, .ph)";

        public const string VALID_CONTACT_NUMBER_REGEX = "^[+\\d\\s-]{7,20}$";
        public const string VALID_CONTACT_NUMBER_REGEX_MESSAGE = "Contact number must be between 7 and 20 characters and can include digits, spaces, dashes, and an optional leading plus sign";
    }
   
}