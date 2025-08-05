// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Sender.Domain.Entities;
    using Fsel.Sender.Domain.IRepositories;

    public class MessageHistoryRepository : BaseRepository<MessageHistory>, IMessageHistoryRepository
    {
        public MessageHistoryRepository(SenderDBContext dbContext, SenderReadDbContext readDbContext, AuthContext authContext, IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
