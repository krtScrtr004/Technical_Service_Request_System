using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Utilities
{
    public abstract class AlertBaseUtility
    {
        public string Status { get; set; }
        public string Message { get; set; }


        public AlertBaseUtility() { }

        public AlertBaseUtility(string status, string message)
        {
            Status = status;
            Message = message;
        }
    }

    public static class  AlertBoxStatus
    {
        public const string Success = "success";
        public const string Warning = "warning";
        public const string Danger = "danger";
    }

    public class AlertBoxUtility : AlertBaseUtility
    {
        public bool Dismissible { get; set; }

        public AlertBoxUtility() : base() { }

        public AlertBoxUtility(string status, string message, bool dismissible = true) : base(status, message)
        {
            Dismissible = dismissible;
        }
    }

    public static class AlertModalStatus
    {
        public const string Success = "success";
        public const string Info = "info";
        public const string Error = "error";
    }

    public class AlertModalUtility : AlertBaseUtility
    {
        public string Title { get; set; }
        public AlertModalUtility() : base() { }

        public AlertModalUtility(string status, string title, string message) : base(status, message) 
        {
            Title = title;
        }
        
    }
}