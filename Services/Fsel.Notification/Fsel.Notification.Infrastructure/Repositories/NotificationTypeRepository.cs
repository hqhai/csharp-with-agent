// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Notification.Infrastructure.Repositories
{
    using Fsel.Notification.Domain.Entities;
    using Fsel.Notification.Domain.IRepositories;
    using Fsel.Core.Base;

    public class NotificationTypeRepository : BaseRepository<NotificationType>, INotificationTypeRepository
    {
        public NotificationTypeRepository(NotificationsDBContext dbContext, NotificationsReadDbContext readDbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
