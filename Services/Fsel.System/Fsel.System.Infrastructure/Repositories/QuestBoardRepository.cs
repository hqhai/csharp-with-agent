// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;

    public class QuestBoardRepository : BaseRepository<QuestBoard>, IQuestBoardRepository
    {
        public QuestBoardRepository(SystemDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
