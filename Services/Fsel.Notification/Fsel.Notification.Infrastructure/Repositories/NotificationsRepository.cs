// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Infrastructure.Repositories
{
    using Fsel.Notification.Domain.Entities;
    using Fsel.Notification.Domain.IRepositories;
    using Fsel.Core.Base;

    public class NotificationsRepository : BaseRepository<NotificationMessage>, INotificationsRepository
    {
        public NotificationsRepository(NotificationsDBContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
