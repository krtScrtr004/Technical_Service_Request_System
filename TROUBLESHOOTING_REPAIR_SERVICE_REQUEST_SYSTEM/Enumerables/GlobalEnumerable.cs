using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Enumerables
{
    public static class AccountTypeEnum
    {
        public const int ADMIN = 1;
        public const int IT = 2;
        public const int STANDARD = 3;

        public static string DisplayName(this int accountTypeId)
        {
            switch (accountTypeId)
            {
                case 1:
                    return "Administrator";
                case 2:
                    return "IT";
                case 3:
                    return "Standard User";
                default:
                    return "Unknown Account Type";
            }
        }

        public static bool IsAdmin(int accountTypeId)
        {
            return accountTypeId == ADMIN ? true : false;
        }

        public static bool IsAdmin(string accountTypeName)
        {
            return (accountTypeName == DisplayName(ADMIN)) ? true : false;
        }

        public static bool IsAdmin(int[] accountTypeIds)
        {
            foreach (var accountTypeId in accountTypeIds)
            {
                if (accountTypeId == ADMIN)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsIT(int accountTypeId)
        {
            return accountTypeId == IT ? true : false;
        }

        public static bool IsIT(string accountTypeName)
        {
            return (accountTypeName == DisplayName(IT)) ? true : false;
        }

        public static bool IsIT(int[] accountTypeIds)
        {
            foreach (var accountTypeId in accountTypeIds)
            {
                if (accountTypeId == IT)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsStandard(int accountTypeId)
        {
            return accountTypeId == STANDARD ? true : false;
        }

        public static bool IsStandard(string accountTypeName)
        {
            return (accountTypeName == DisplayName(STANDARD)) ? true : false;
        }

        public static bool IsStandard(int[] accountTypeIds)
        {
            foreach (var accountTypeId in accountTypeIds)
            {
                if (accountTypeId == STANDARD)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public static class TechnicalServiceTypeEnum
    {
        public const int EQUIPMENT_REPAIR_TROUBLESHOOTING = 1;
        public const int ZOOM_WEBEX_LINK = 2;
        public const int GOVERNMENT_EMAIL_ACCOUNT = 3;
        public const int AUDIO_VISUAL_SETUP = 4;
        public const int INTERNET_CONNECTIVITY = 5;
        public const int LIVESTREAM_SETUP = 6;
        public const int ACCOUNT_CREATION = 7;
        public const int PASSWORD_ACCESS_ISSUE = 8;
        public const int SYSTEM_SUPPORT = 9;
        public const int DATA_CORRECTION = 10;
        public const int TECHNICAL_GUIDANCE = 11;
        public const int PRODUCTION_MATERIAL_PRINTING = 12;

        public static string DisplayName(this int typeId)
        {
            switch (typeId)
            {
                case 1:
                    return "Equipment Repair/Troubleshooting";
                case 2:
                    return "Zoom/Webex Link";
                case 3:
                    return "Government Email Account";
                case 4:
                    return "Audio Visual Setup";
                case 5:
                    return "Internet Connectivity";
                case 6:
                    return "Livestream Setup";
                case 7:
                    return "Account Creation";
                case 8:
                    return "Password/Access Issue";
                case 9:
                    return "System Support";
                case 10:
                    return "Data Correction";
                case 11:
                    return "Technical Guidance";
                case 12:
                    return "Production Material Printing";
                default:
                    return "Unknown Service Type";
            }
        }

        public static List<int> GetScheduledServiceIds()
        {
            return new List<int> 
            { 
                AUDIO_VISUAL_SETUP, 
                LIVESTREAM_SETUP, 
                ZOOM_WEBEX_LINK 
            };
        }

        public static List<int> GetNonAssistedServiceIds()
        {
            return new List<int>
            {
                GOVERNMENT_EMAIL_ACCOUNT,
                ACCOUNT_CREATION,
                PASSWORD_ACCESS_ISSUE,
                SYSTEM_SUPPORT,
                DATA_CORRECTION,
                TECHNICAL_GUIDANCE,
                PRODUCTION_MATERIAL_PRINTING
            };
        }

        public static bool IsRepairTroubleshootingRequest(this int id)
        {
            if (id == EQUIPMENT_REPAIR_TROUBLESHOOTING ||
                id == INTERNET_CONNECTIVITY)
            {
                return true;
            }
            return false;
        }

        public static bool IsScheduleControlProcessRequest(this int id)
        {
            if (id == AUDIO_VISUAL_SETUP ||
                id == LIVESTREAM_SETUP ||
                id == ZOOM_WEBEX_LINK)
            {
                return true;
            }
            return false;
        }

        public static bool IsNonAssistedRequest(this int id)
        {
            if (id == GOVERNMENT_EMAIL_ACCOUNT ||
                id == ACCOUNT_CREATION ||
                id == PASSWORD_ACCESS_ISSUE ||
                id == SYSTEM_SUPPORT ||
                id == DATA_CORRECTION ||
                id == TECHNICAL_GUIDANCE ||
                id == PRODUCTION_MATERIAL_PRINTING)
            {
                return true;
            }
            return false;
        }
    }

    public static class TechnicalServiceRequestStatusEnum
    {
        public const int PENDING = 1;
        public const int ONGOING = 2;
        public const int RESOLVED = 3;
        public const int CANCELLED = 4;
        public const int OPEN = 5;
        public const int CLOSED = 6;

        public static string DisplayName(this int statusId)
        {
            switch (statusId)
            {
                case 1:
                    return "PENDING";
                case 2:
                    return "ON GOING";
                case 3:
                    return "RESOLVED";
                case 4:
                    return "CANCELLED";
                case 5:
                    return "OPEN";
                case 6:
                    return "CLOSED";
                default:
                    return "Unknown Status";
            }
        }

        public static List<int> GetActiveStatusIds()
        {
            return new List<int>() { PENDING,  ONGOING, OPEN };
        }

        public static List<int> GetCancellableStatusIds()
        {
            return new List<int> { PENDING, OPEN };
        }

        public static List<int> GetCompletedStatusIds()
        {
            return new List<int> { RESOLVED, CLOSED };
        }
    }

    public static class TechnicalServicRequestSeverityEnum
    {
        public const int LOW = 1;
        public const int MEDIUM = 2;
        public const int HIGH = 3;
        public const int CRITICAL = 4;

        public static string DisplayName(this int severityId)
        {
            switch (severityId)
            {
                case 1:
                    return "LOW";
                case 2:
                    return "MEDIUM";
                case 3:
                    return "HIGH";
                case 4:
                    return "CRITICAL";
                default:
                    return "Unknown Severity";
            }
        }
    }
}