using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

        public static List<SelectListItem> GetSelectListItems(int? selectedItem = null)
        {
            return new List<SelectListItem>
            {
                new SelectListItem {
                    Value = STANDARD.ToString(),
                    Text = DisplayName(STANDARD),
                    Selected = STANDARD == (int?)selectedItem
                },
                new SelectListItem {
                    Value = IT.ToString(),
                    Text = DisplayName(IT),
                    Selected = IT == (int?)selectedItem
                },
                new SelectListItem {
                    Value = ADMIN.ToString(),
                    Text = DisplayName(ADMIN),
                    Selected = ADMIN == (int?)selectedItem
                },
            };
        }
    }
}