using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TECHNICAL_SERVICE_REQUEST.Enumerables
{
    public static class RequestTypeEnum
    {
        public const int EQUIPMENT_REPAIR_TROUBLESHOOTING = 1;
        public const int ZOOM_WEBEX_LINK = 2;
        public const int COMPANY_EMAIL_ACCOUNT = 3;
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
                    return "Equipment Repair / Troubleshooting";
                case 2:
                    return "Zoom / Webex Link";
                case 3:
                    return "Company Email Account";
                case 4:
                    return "Audio Visual Setup";
                case 5:
                    return "Internet Connectivity";
                case 6:
                    return "Livestream Setup";
                case 7:
                    return "Account Creation";
                case 8:
                    return "Password / Access Issue";
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
                COMPANY_EMAIL_ACCOUNT,
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
            if (id == COMPANY_EMAIL_ACCOUNT ||
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

        public static List<SelectListItem> GetSelectListItems()
        {
            var assistedGroup = new SelectListGroup { Name = "Assisted" };
            var scheduledGroup = new SelectListGroup { Name = "Scheduled Control Process" };
            var nonAssistedGroup = new SelectListGroup { Name = "Non-Assisted" };

            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = RequestTypeEnum.EQUIPMENT_REPAIR_TROUBLESHOOTING.ToString(),
                    Text = RequestTypeEnum.DisplayName(RequestTypeEnum.EQUIPMENT_REPAIR_TROUBLESHOOTING),
                    Group = assistedGroup
                },
                new SelectListItem
                {
                    Value = RequestTypeEnum.INTERNET_CONNECTIVITY.ToString(),
                    Text = RequestTypeEnum.DisplayName(RequestTypeEnum.INTERNET_CONNECTIVITY),
                    Group = assistedGroup
                },

                new SelectListItem
                {
                    Value = RequestTypeEnum.AUDIO_VISUAL_SETUP.ToString(),
                    Text = RequestTypeEnum.DisplayName(RequestTypeEnum.AUDIO_VISUAL_SETUP),
                    Group = scheduledGroup
                },
                new SelectListItem
                {
                    Value = RequestTypeEnum.LIVESTREAM_SETUP.ToString(),
                    Text = RequestTypeEnum.DisplayName(RequestTypeEnum.LIVESTREAM_SETUP),
                    Group = scheduledGroup
                },
                new SelectListItem
                {
                    Value = RequestTypeEnum.ZOOM_WEBEX_LINK.ToString(),
                    Text = RequestTypeEnum.DisplayName(RequestTypeEnum.ZOOM_WEBEX_LINK),
                    Group = scheduledGroup
                },

                new SelectListItem
                {
                    Value = RequestTypeEnum.ACCOUNT_CREATION.ToString(),
                    Text = RequestTypeEnum.DisplayName(RequestTypeEnum.ACCOUNT_CREATION),
                    Group = nonAssistedGroup
                },
                new SelectListItem
                {
                    Value = RequestTypeEnum.DATA_CORRECTION.ToString(),
                    Text = RequestTypeEnum.DisplayName(RequestTypeEnum.DATA_CORRECTION),
                    Group = nonAssistedGroup
                },
                new SelectListItem
                {
                    Value = RequestTypeEnum.COMPANY_EMAIL_ACCOUNT.ToString(),
                    Text = RequestTypeEnum.DisplayName(RequestTypeEnum.COMPANY_EMAIL_ACCOUNT),
                    Group = nonAssistedGroup
                },
                new SelectListItem
                {
                    Value = RequestTypeEnum.PRODUCTION_MATERIAL_PRINTING.ToString(),
                    Text = RequestTypeEnum.DisplayName(RequestTypeEnum.PRODUCTION_MATERIAL_PRINTING),
                    Group = nonAssistedGroup
                },
                new SelectListItem
                {
                    Value = RequestTypeEnum.SYSTEM_SUPPORT.ToString(),
                    Text = RequestTypeEnum.DisplayName(RequestTypeEnum.SYSTEM_SUPPORT),
                    Group = nonAssistedGroup
                },
                new SelectListItem
                {
                    Value = RequestTypeEnum.TECHNICAL_GUIDANCE.ToString(),
                    Text = RequestTypeEnum.DisplayName(RequestTypeEnum.TECHNICAL_GUIDANCE),
                    Group = nonAssistedGroup
                },

                // Keep "Others" as -1 for your existing JS logic
                new SelectListItem { Value = "-1", Text = "Others" }
            };
        }
    }

    public static class RequestStatusEnum
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
                    return "Pending";
                case 2:
                    return "On Going";
                case 3:
                    return "Resolved";
                case 4:
                    return "Cancelled";
                case 5:
                    return "Open";
                case 6:
                    return "Closed";
                default:
                    return "Unknown Status";
            }
        }

        public static List<int> GetActiveStatusIds()
        {
            return new List<int>() { PENDING, ONGOING, OPEN };
        }

        public static List<int> GetCancellableStatusIds()
        {
            return new List<int> { PENDING, OPEN };
        }

        public static List<int> GetCompletedStatusIds()
        {
            return new List<int> { RESOLVED, CLOSED };
        }

        public static List<SelectListItem> GetSelectListItems()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = PENDING.ToString(), Text = DisplayName(PENDING) },
                new SelectListItem { Value = ONGOING.ToString(), Text = DisplayName(ONGOING) },
                new SelectListItem { Value = RESOLVED.ToString(), Text = DisplayName(RESOLVED) },
                new SelectListItem { Value = CANCELLED.ToString(), Text = DisplayName(CANCELLED) },
                new SelectListItem { Value = OPEN.ToString(), Text = DisplayName(OPEN) },
                new SelectListItem { Value = CLOSED.ToString(), Text = DisplayName(CLOSED) }
            };
        }
    }

    public static class RequestSeverityEnum
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
                    return "Low";
                case 2:
                    return "Medium";
                case 3:
                    return "High";
                case 4:
                    return "Critical";
                default:
                    return "Unknown Severity";
            }
        }

        public static List<SelectListItem> GetSelectListItems()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Not Applicable"},
                new SelectListItem {
                    Value = RequestSeverityEnum.LOW.ToString(),
                    Text = RequestSeverityEnum.DisplayName(RequestSeverityEnum.LOW)
                },
                new SelectListItem {
                    Value = RequestSeverityEnum.MEDIUM.ToString(),
                    Text = RequestSeverityEnum.DisplayName(RequestSeverityEnum.MEDIUM)
                },
                new SelectListItem
                {
                    Value = RequestSeverityEnum.HIGH.ToString(),
                    Text = RequestSeverityEnum.DisplayName(RequestSeverityEnum.HIGH)
                },
                new SelectListItem
                {
                    Value = RequestSeverityEnum.CRITICAL.ToString(),
                    Text = RequestSeverityEnum.DisplayName(RequestSeverityEnum.CRITICAL)
                }
            };
        }
    }

    public static class RequestScheduleLimitEnum
    {
        public const int AUDIO_VISUAL_SETUP = 3;
        public const int LIVESTREAM_SETUP = 2;
        public const int ZOOM_WEBEX_LINK = 4;
    }
}