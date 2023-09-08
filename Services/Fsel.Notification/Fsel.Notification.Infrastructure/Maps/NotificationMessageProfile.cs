// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Notification.Domain.Entities;
    using Fsel.Notification.Domain.Model.CommandModels.Notification;
    using Fsel.Notification.Domain.Model.EntityModels;
    using Fsel.Core.Extensions;

    public class NotificationMessageProfile : Profile
    {
        public NotificationMessageProfile()
        {
            CreateMap<NotificationMessage, NotificationMessageModel>().IgnoreAllNonExisting();
            CreateMap<CreateNotificationCommandModel, NotificationMessage>().IgnoreAllNonExisting();
        }
    }
}
