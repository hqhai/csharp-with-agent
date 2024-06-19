// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Infrastructure.Repositories
{
    using Fsel.Notification.Domain.Entities;
    using Fsel.Notification.Domain.IRepositories;
    using Fsel.Core.Base;

    public class NotificationRemindRepository : BaseRepository<NotificationRemind>, INotificationRemindRepository
    {
        public NotificationRemindRepository(NotificationsDBContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
