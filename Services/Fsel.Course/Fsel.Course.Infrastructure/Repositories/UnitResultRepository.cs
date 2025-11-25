// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class UnitResultRepository : BaseRepository<UnitResult>, IUnitResultRepository
    {
        public UnitResultRepository(CourseDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }

        public async Task<bool> IsDoneAsync(LessonResult lessonResult)
        {
            return await Queryable.Where(x => x.StudentId == lessonResult.StudentId && x.UnitId == lessonResult.UnitId)
                                  .AnyAsync(x => x.CourseId == lessonResult.CourseId && x.Status == EnumResultStatus.Done);
        }
    }
}
