// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    public class PlacementTestGroupResultRepository : BaseRepository<PlacementTestGroupResult>, IPlacementTestGroupResultRepository
    {
        public PlacementTestGroupResultRepository(CourseDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }

        public async Task<IList<Guid>> GetStudentIdsAsync(EnumCompletionStatus? status, IList<Guid> studentIds, bool isCheckDate, EnumCourseLevel? currentLevel, EnumCourseLevel? courseLevel)
        {
            if (!(currentLevel.HasValue || status.HasValue || courseLevel.HasValue || (isCheckDate || studentIds.Any())))
            {
                return new List<Guid>();
            }
            var query = Queryable;
            if (status.HasValue)
            {
                switch (status.Value)
                {
                    case EnumCompletionStatus.Completed:
                        query = query.Where(x => x.Status == EnumResultStatus.Done);
                        break;

                    case EnumCompletionStatus.InProgress:
                        query = query.Where(x => x.Status != EnumResultStatus.Done);
                        break;
                }
            }
            if (currentLevel.HasValue)
            {
                query = query.Where(x => x.CurrentLevel == currentLevel.Value);
            }
            if (courseLevel.HasValue)
            {
                query = query.Where(x => x.ChooseLevel == courseLevel.Value);
            }
            if (isCheckDate)
            {
                query = query.Where(x => studentIds.Contains(x.StudentId));
            }
            return await query.Select(x => x.StudentId).ToListAsync();
        }
    }
}
