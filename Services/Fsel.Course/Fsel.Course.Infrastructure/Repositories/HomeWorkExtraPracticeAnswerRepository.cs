// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;

    public class HomeWorkExtraPracticeAnswerRepository : BaseRepository<HomeWorkExtraPracticeAnswer>, IHomeWorkExtraPracticeAnswerRepository
    {
        public HomeWorkExtraPracticeAnswerRepository(CourseDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
