// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.System.Domain.Entities.QuestBoards;
    using Fsel.System.Domain.IRepositories;
    using AutoMapper;

    public class QuestBoardStudentRepository : BaseRepository<QuestBoardStudent>, IQuestBoardStudentRepository
    {
        public QuestBoardStudentRepository(SystemDbContext dbContext, SystemReadDbContext readDbContext, AuthContext authContext, AutoMapper.IMapper mapper)
            : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
