using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Attributes
{
    /**
     * Check whether time is within the specified range. The minimum time is calculated
     * from the current time, while the maximum time is a fixed value.
     */
    public class TimeAllowed : ValidationAttribute
    {
        private int MinimumTime { get; set; } // Minimum time in hours from the current time (e.g., 1 for 1 hour from now)
        private string MaximumTime { get; set; } // Maximum time allowed (e.g., "17:00" for 5:00 PM)
        private string ScheduleDatePropertyName { get; set; } // Property name of the schedule date to compare with the current date


        public TimeAllowed(int minimumHourFromNow, string maximumHour, string scheduleDatePropertyName = "TechnicalServiceRequestScheduledDate")
        {
            MinimumTime = minimumHourFromNow;
            MaximumTime = maximumHour;
            ScheduleDatePropertyName = scheduleDatePropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            var scheduleDateProp = validationContext.ObjectType.GetProperty(ScheduleDatePropertyName);
            if (scheduleDateProp == null)
            {
                return new ValidationResult($"Unknown property: {ScheduleDatePropertyName}");
            }

            // Normalize input to TimeSpan
            TimeSpan selectedTime;
            if (value is TimeSpan span)
            {
                selectedTime = span;
            }
            else if (value is DateTime dateTime)
            {
                selectedTime = dateTime.TimeOfDay;
            }
            else
            {
                return ValidationResult.Success;
            }

            if (!TimeSpan.TryParse(MaximumTime, out TimeSpan maximumTime))
            {
                return new ValidationResult("Invalid maximum time configuration.");
            }

            var now = DateTime.Now;
            var specifiedDate = scheduleDateProp.GetValue(validationContext.ObjectInstance, null) as DateTime?;

            // If schedule date is today, enforce minimum hours from now
            if (specifiedDate.HasValue && specifiedDate.Value.Date == now.Date)
            {
                var minimumTimeToday = now.AddHours(MinimumTime).TimeOfDay;

                if (minimumTimeToday > maximumTime)
                {
                    return new ValidationResult("No available time window for the current time and configured limits.");
                }

                if (selectedTime < minimumTimeToday || selectedTime > maximumTime)
                {
                    return new ValidationResult(
                        $"The time must be between {DateTime.Today.Add(minimumTimeToday):hh:mm tt} and {DateTime.Today.Add(maximumTime):hh:mm tt}.");
                }

                return ValidationResult.Success;
            }

            // For non-today schedules, only enforce configured max time
            if (selectedTime > maximumTime)
            {
                return new ValidationResult(
                    $"The time must be on or before {DateTime.Today.Add(maximumTime):hh:mm tt}.");
            }

            return ValidationResult.Success;
        }
    }

    /**
     * Check whether the scheduled time is not earlier than the current time if the scheduled date
     * is the same as the current date. Also check whether the scheduled time is on or before 5:00 PM.
     */
    public class ScheduledTimeAttribute : ValidationAttribute
    {
        private string ScheduleDatePropertyName { get; set; } // Property name of the schedule date to compare with the current date
        private string MinimumHours { get; set; } // Minimum time allowed for scheduling (e.g., "08:00" for 8:00 AM)
        private string MaximumHours { get; set; } // Maximum time allowed for scheduling (e.g., "17:00" for 5:00 PM)

        public ScheduledTimeAttribute(
            string scheduleDatePropertyName = "TechnicalServiceRequestScheduledDate", 
            string minimumHours = "8:00",  
            string maximumHours = "16:00"
        )
        {
            ScheduleDatePropertyName = scheduleDatePropertyName;
            MinimumHours = minimumHours;
            MaximumHours = maximumHours;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var scheduleDateProp = validationContext.ObjectType.GetProperty(ScheduleDatePropertyName);
            if (scheduleDateProp == null)
            {
                return new ValidationResult($"Unknown property: {ScheduleDatePropertyName}");
            }

            // Get the value of the schedule date property
            var scheduleDateValue = scheduleDateProp.GetValue(validationContext.ObjectInstance, null) as DateTime?;
            if (!scheduleDateValue.HasValue || value == null)
            {
                return ValidationResult.Success;
            }

            TimeSpan timeValue;
            if (value is TimeSpan span)
            {
                timeValue = span;
            }
            else if (value is DateTime)
            {
                timeValue = ((DateTime)value).TimeOfDay;
            }
            else
            {
                return ValidationResult.Success;
            }

            var scheduleDate = scheduleDateValue.Value.Date;
            var now = DateTime.Now;

            if (!TimeSpan.TryParse(MinimumHours, out TimeSpan minimumTime))
            {
                return new ValidationResult("Invalid minimum hours configuration.");
            }

            // Check if the scheduled date is today and the scheduled time is earlier than the current time
            if (scheduleDate == now.Date && timeValue < now.TimeOfDay)
            {
                return new ValidationResult("The scheduled time must not be earlier than the current time.");
            }
            else if (scheduleDate > now.Date && timeValue < minimumTime)
            {
                // For future dates, enforce the minimum time (e.g., 8:00 AM)
                return new ValidationResult($"The scheduled time must be on or after {DateTime.Today.Add(minimumTime):hh:mm tt}.");
            }

            if (!TimeSpan.TryParse(MaximumHours, out TimeSpan maximumTime))
            {
                return new ValidationResult("Invalid maximum time configuration.");
            }

            if (timeValue > maximumTime)
            {
                return new ValidationResult($"The scheduled time must be on or before {DateTime.Today.Add(maximumTime):hh:mm tt}.");
            }

            return ValidationResult.Success;
        }

    }

    // Check whether the scheduled start time is earlier than the scheduled end time.
    public class StartEndTimeCollisionAttribute : ValidationAttribute
    {
        private string StartTimePropertyName { get; set; }
        private string EndTimePropertyName { get; set; }

        public StartEndTimeCollisionAttribute(
            string startTimePropertyName = "TechnicalServiceRequestScheduledStartTime",
            string endTimePropertyName = "TechnicalServiceRequestScheduledEndTime"
        )
        {
            StartTimePropertyName = startTimePropertyName;
            EndTimePropertyName = endTimePropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            var startTimeProp = validationContext.ObjectType.GetProperty(StartTimePropertyName);
            var endTimeProp = validationContext.ObjectType.GetProperty(EndTimePropertyName);
            if (startTimeProp == null || endTimeProp == null)
            {
                return new ValidationResult($"Unknown properties: {StartTimePropertyName} or {EndTimePropertyName}");
            }

            // Get the values of the start time and end time properties
            var startTimeValue = startTimeProp.GetValue(validationContext.ObjectInstance, null) as TimeSpan?;
            var endTimeValue = endTimeProp.GetValue(validationContext.ObjectInstance, null) as TimeSpan?;
            if (!startTimeValue.HasValue || !endTimeValue.HasValue)
            {
                return ValidationResult.Success;
            }

            // Check if the start time is on or after the end time
            if (startTimeValue.Value >= endTimeValue.Value)
            {
                return new ValidationResult("The scheduled start time must be earlier than the scheduled end time.");
            }

            return ValidationResult.Success;
        }
    }


}