// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.IRepositories;

    public class ExamPracticeRetryRepository : BaseRepository<ExamPracticeRetry>, IExamPracticeRetryRepository
    {
        public ExamPracticeRetryRepository(ExamPracticesDBContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
