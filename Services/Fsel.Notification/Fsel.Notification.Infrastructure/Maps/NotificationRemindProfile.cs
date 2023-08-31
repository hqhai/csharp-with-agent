// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Notification.Domain.Entities;
    using Fsel.Notification.Domain.Model.EntityModels;
    using Fsel.Core.Extensions;

    public class NotificationRemindProfile : Profile
    {
        public NotificationRemindProfile()
        {
            CreateMap<NotificationRemind, NotificationRemindModel>().IgnoreAllNonExisting();
        }
    }
}
