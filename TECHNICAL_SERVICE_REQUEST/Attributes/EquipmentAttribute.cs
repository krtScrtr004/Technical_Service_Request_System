using System;
using System.ComponentModel.DataAnnotations;
using TECHNICAL_SERVICE_REQUEST.Enumerables;

namespace TECHNICAL_SERVICE_REQUEST.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CompleteEquipmentFieldAttribute : ValidationAttribute
    {
        private readonly string _propertyDisplayName;
        private readonly string[] _relatedProperties;
        private readonly string _serviceTypePropertyName;

        public CompleteEquipmentFieldAttribute(string propertyDisplayName, string[] relatedProperties)
        {
            _propertyDisplayName = propertyDisplayName.Trim();
            _relatedProperties = relatedProperties;
            _serviceTypePropertyName = null;
        }

        public CompleteEquipmentFieldAttribute(
            string propertyDisplayName,
            string[] relatedProperties,
            string serviceTypePropertyName = "TechnicalServiceTypeId")
        {
            _propertyDisplayName = propertyDisplayName.Trim();
            _relatedProperties = relatedProperties;
            _serviceTypePropertyName = serviceTypePropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(_serviceTypePropertyName))
            {
                // Check service type - only validate if EQUIPMENT_REPAIR_TROUBLESHOOTING
                var serviceTypeProp = validationContext.ObjectType.GetProperty(_serviceTypePropertyName);
                if (serviceTypeProp == null)
                {
                    return new ValidationResult($"Unknown property: {_serviceTypePropertyName}");
                }

                var serviceTypeValue = (int?)serviceTypeProp.GetValue(validationContext.ObjectInstance);
                if (serviceTypeValue != RequestTypeEnum.EQUIPMENT_REPAIR_TROUBLESHOOTING)
                {
                    return ValidationResult.Success;
                }
            }
          

            // Collect all equipment field values
            var currentValue = value;
            var relatedValues = new System.Collections.Generic.Dictionary<string, object>();

            foreach (var propName in _relatedProperties)
            {
                var prop = validationContext.ObjectType.GetProperty(propName);
                if (prop != null)
                {
                    relatedValues[propName] = prop.GetValue(validationContext.ObjectInstance);
                }
            }

            // Check if ANY field is filled
            bool anyFieldFilled = IsFieldFilled(currentValue);
            foreach (var relatedValue in relatedValues.Values)
            {
                if (IsFieldFilled(relatedValue))
                {
                    anyFieldFilled = true;
                    break;
                }
            }

            // If ANY field is filled, check that ALL are filled
            if (anyFieldFilled)
            {
                if (!IsFieldFilled(currentValue))
                {
                    return new ValidationResult($"{_propertyDisplayName} is required when providing equipment details.");
                }

                foreach (var relatedProp in relatedValues)
                {
                    if (!IsFieldFilled(relatedProp.Value))
                    {
                        return new ValidationResult($"{_propertyDisplayName} is required when providing equipment details.");
                    }
                }
            }

            return ValidationResult.Success;
        }

        private bool IsFieldFilled(object value)
        {
            if (value == null)
            {
                return false;
            }

            if (value is string str)
            {
                return !string.IsNullOrWhiteSpace(str);
            }

            if (value is int intValue)
            {
                return intValue > 0;
            }

            if (value is int nullableInt)
            {
                return nullableInt > 0;
            }

            return false;
        }
    }
}