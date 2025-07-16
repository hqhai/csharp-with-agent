// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.IRepositories;

    public class ExamPracticeAnswerRepository : BaseRepository<ExamPracticeAnswer>, IExamPracticeAnswerRepository
    {
        public ExamPracticeAnswerRepository(ExamPracticesDBContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
