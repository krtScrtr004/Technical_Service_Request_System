using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;
using System.Web.SessionState;

namespace TECHNICAL_SERVICE_REQUEST.Utilities
{
    public class GeneralUtilities
    {
        public static string ToTitleCaseString(string stringInput)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(stringInput);
        }

        public static string DateToWord(DateTime date)
        {
            return date.ToString("MMMM dd, yyyy");
        }

        public static DateTime GetStartOfWeek(DateTime date, DayOfWeek startOfWeek)
        {
            var diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.Date.AddDays(-diff);
        }

        public static string CreateFullName(
            string firstName,
            string lastName,
            string middleName = null,
            string extension = null
        )
        {
            string middle = string.IsNullOrEmpty(middleName) ? "" : $"{middleName[0]}. ";
            string ext = string.IsNullOrEmpty(extension) ? "" : $" {extension}";
            return $"{firstName} {middle}{lastName}{ext}";
        }
    }
}
