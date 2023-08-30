// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Notification.Domain.Entities;
    using Fsel.Notification.Domain.Model.CommandModels.Notification;
    using Fsel.Notification.Domain.Model.EntityModels;
    using Fsel.Core.Extensions;

    public class NotificationProfile : Profile
    {
        public NotificationProfile() {
            CreateMap<Notifications, NotificationsModel>().IgnoreAllNonExisting();
            CreateMap<CreateNotificationCommandModel, Notifications>().IgnoreAllNonExisting();
        }
    }
}
