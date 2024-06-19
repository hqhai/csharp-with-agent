// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class VideoTimeCodeRepository : BaseRepository<VideoTimeCode>, IVideoTimeCodeRepository
    {
        public VideoTimeCodeRepository(CourseDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
