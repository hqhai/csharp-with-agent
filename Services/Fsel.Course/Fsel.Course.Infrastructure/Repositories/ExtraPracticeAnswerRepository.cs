// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Microsoft.EntityFrameworkCore;

    public class ExtraPracticeAnswerRepository : BaseRepository<ExtraPracticeAnswer>, IExtraPracticeAnswerRepository
    {
        public ExtraPracticeAnswerRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
