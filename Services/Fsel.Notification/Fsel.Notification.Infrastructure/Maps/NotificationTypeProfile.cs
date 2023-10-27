// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Notification.Domain.Entities;
    using Fsel.Notification.Domain.Model.EntityModels;
    using Fsel.Core.Extensions;

    public class NotificationTypeProfile : Profile
    {
        public NotificationTypeProfile()
        {

            CreateMap<NotificationType, NotificationsTypeModel>().IgnoreAllNonExisting();
        }
    }
}
