// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Notification.Domain.Entities;
    using Fsel.Notification.Domain.Model.CommandModels.Notification;
    using Fsel.Notification.Domain.Model.EntityModels;
    using Fsel.Core.Extensions;
    using Fsel.Common.Models;

    public class NotificationMessageProfile : Profile
    {
        public NotificationMessageProfile()
        {
            CreateMap<NotificationMessage, NotificationMessageModel>().IgnoreAllNonExisting();
            CreateMap<NotificationMessage, OneSignalMessageModel>().IgnoreAllNonExisting();
            CreateMap<CreateNotificationCommandModel, NotificationMessage>().IgnoreAllNonExisting();
            CreateMap<UpdateNotificationCommandModel, NotificationMessage>().IgnoreAllNonExisting();
            CreateMap<NotificationType, NotificationsTypeModel>().IgnoreAllNonExisting();
        }
    }
}
