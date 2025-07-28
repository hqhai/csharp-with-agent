// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.System.Domain.Entities.Chatbots;
    using Fsel.System.Domain.IRepositories;

    public class ChatbotConfigRepository : BaseRepository<ChatbotConfig>, IChatbotConfigRepository
    {
        public ChatbotConfigRepository(SystemDbContext dbContext, SystemReadDbContext readDbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
