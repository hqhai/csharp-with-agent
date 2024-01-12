// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;

    public class AiGradeSettingRepository : BaseRepository<AiGradeSetting>, IAiGradeSettingRepository
    {
        public AiGradeSettingRepository(CourseDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
