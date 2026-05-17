using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TECHNICAL_SERVICE_REQUEST.Utilities;

namespace TECHNICAL_SERVICE_REQUEST.Attributes
{
    public class DateRangeAttribute : ValidationAttribute
    {
        private int MinimumDate { get; set; }
        private int MaximumDate { get; set; }

        public DateRangeAttribute(int minimumDateFromNow, int maximumDateFromNow)
        {
            MinimumDate = minimumDateFromNow;
            MaximumDate = maximumDateFromNow;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }
            if (value is DateTime)
            {
                DateTime min = DateTime.Now.AddDays(MinimumDate).Date;
                DateTime max = DateTime.Now.AddDays(MaximumDate).Date;

                DateTime dateValue = Convert.ToDateTime(value);
                if (dateValue < min || dateValue > max)
                {
                    return new ValidationResult($"The date must be between {GeneralUtilities.DateToWord(min)} and {GeneralUtilities.DateToWord(max)}");
                }
            }
            return ValidationResult.Success;
        } 
    }
}