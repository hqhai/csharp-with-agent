// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.IRepositories;

    public class ExamPracticeSectionResultRepository : BaseRepository<ExamPracticeSectionResult>, IExamPracticeSectionResultRepository
    {
        public ExamPracticeSectionResultRepository(ExamPracticesDBContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
