using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Enumerables;
using TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Models;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Attributes
{
    public class SelectRequestTypeAttribute : ValidationAttribute
    {
        private readonly string _otherPropertyName;

        public SelectRequestTypeAttribute(string otherPropertyName = "Others")
        {
            _otherPropertyName = otherPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            var otherProp = validationContext.ObjectType.GetProperty(_otherPropertyName);
            if (otherProp == null)
            {
                return new ValidationResult($"Unknown property: {_otherPropertyName}");
            }

            var otherValue = otherProp.GetValue(validationContext.ObjectInstance, null) as string;
            var selectedType = value as int?;

            if ((!selectedType.HasValue || selectedType.Value == 0) && string.IsNullOrWhiteSpace(otherValue))
            {
                return new ValidationResult("Please select a technical service type or specify others.");
            }

            return ValidationResult.Success;
        }
    }

    public class ValidLivestreamScheduleAttribute : ValidationAttribute
    {
        private readonly string _serviceTypeProperty;

        public ValidLivestreamScheduleAttribute(string serviceTypeProperty = "TypeId")
        {
            _serviceTypeProperty = serviceTypeProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            var serviceTypeProp = validationContext.ObjectType.GetProperty(_serviceTypeProperty);
            if (serviceTypeProp == null)
            {
                throw new Exception($"Unkown property: {_serviceTypeProperty}.");
            }   

            var selectedServiceTypeValue = serviceTypeProp.GetValue(validationContext.ObjectInstance, null) as int?;
            if (selectedServiceTypeValue == null)
            {
                throw new Exception("Service type value must be provided.");
            }
            else if (selectedServiceTypeValue == (int)RequestTypeEnum.LIVESTREAM_SETUP)
            {
                // For livestream setup, the scheduled date must be at least 7 days in advance
                var scheduledDate = (DateTime)value;
                if (scheduledDate < DateTime.Now.AddDays(7))
                {
                    return new ValidationResult("Livestream setup must be scheduled at least 7 days in advance.");
                }
            }

            return ValidationResult.Success;
        }

            
    }
}