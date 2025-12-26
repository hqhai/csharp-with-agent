// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using AutoMapper;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class QuestionExplanationErrorRepository : BaseRepository<QuestionExplanationError>, IQuestionExplanationErrorRepository
    {
        public QuestionExplanationErrorRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper)
            : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
