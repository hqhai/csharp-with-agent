// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class GameVocabularyRepository : BaseRepository<GameVocabulary>, IGameVocabularyRepository
    {
        public GameVocabularyRepository(SystemDbContext dbContext, SystemReadDbContext readDbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
        }
        public override async Task<GameVocabulary?> GetIncludeByIdAsync(Guid id)
        {
            return await Queryable.Include(x => x.GameVocabularyTypes).Include(p => p.GameVocabularyPlatforms).Include(t => t.GameTopic).FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
