// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;

    public class HomeWorkQuestionRepository : BaseRepository<HomeWorkQuestion>, IHomeWorkQuestionRepository
    {
        public HomeWorkQuestionRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
