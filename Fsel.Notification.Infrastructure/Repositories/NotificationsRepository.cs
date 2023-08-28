// Copyright (c) Atlantic. All rights reserved.

namespace FiveSIS.Notification.Infrastructure.Repositories
{
    using FiveSIS.Notification.Domain.Entities;
    using FiveSIS.Notification.Domain.IRepositories;
    using Fsel.Core.Base;

    public class NotificationsRepository : BaseRepository<Notifications>, INotificationsRepository
    {
        public NotificationsRepository(NotificationsDBContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
