// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.System.Domain.Entities.QuestBoards;
    using Fsel.System.Domain.IRepositories;

    public class QuestBoardOverallStudentRepository : BaseRepository<QuestBoardOverallStudent>, IQuestBoardOverallStudentRepository
    {
        public QuestBoardOverallStudentRepository(SystemDbContext dbContext, SystemReadDbContext readDbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
