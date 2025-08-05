// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.IRepositories;

    public class ProsodyScoreRepository : BaseRepository<ProsodyScore>, IProsodyScoreRepository
    {
        public ProsodyScoreRepository(ExamPracticesDBContext dbContext, ExamPracticesReadDBContext readDbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
