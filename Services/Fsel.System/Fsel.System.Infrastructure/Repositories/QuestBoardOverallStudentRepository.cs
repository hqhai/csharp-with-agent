// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.System.Domain.Entities.QuestBoards;
    using Fsel.System.Domain.IRepositories;

    public class QuestBoardOverallStudentRepository : BaseRepository<QuestBoardOverallStudent>, IQuestBoardOverallStudentRepository
    {
        public QuestBoardOverallStudentRepository(SystemDbContext dbContext, AuthContext authContext, IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
