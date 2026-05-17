using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TECHNICAL_SERVICE_REQUEST.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Index]
        public int? RecipientId { get; set; }
        public virtual AppUser Recipient { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }
        
        public bool ForAdmin { get; set; }
        public bool ForIT {  get; set; }

        public bool IsRead { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NotificationIndexViewModels
    {
        public IPagedList<NotificationDetailViewModels> Notifications { get; set; }
    }

    public class NotificationDetailViewModels
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NotificationBadgeViewModels 
    {
        public int UnreadCount { get; set; }
    }

    #region StaticClasses

    public class NotificationTypeCaster
    {
        public static NotificationDetailViewModels ToNotificationDetailViewModels(Notification notification)
        {
            return new NotificationDetailViewModels()
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
            };
        }
    }

    #endregion
}