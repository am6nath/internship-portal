using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InternshipPortal.API.Entities;

namespace InternshipPortal.API.Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<Entities.Notification>> GetMyNotificationsAsync(Guid userId);
        Task CreateNotificationAsync(Guid userId, string title, string message, string type);
        Task MarkAsReadAsync(Guid userId, Guid notificationId);
        Task MarkAllAsReadAsync(Guid userId);
    }
}
