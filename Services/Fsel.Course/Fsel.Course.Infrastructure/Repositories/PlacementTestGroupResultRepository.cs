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
    using AutoMapper;

    public class PlacementTestGroupResultRepository : BaseRepository<PlacementTestGroupResult>, IPlacementTestGroupResultRepository
    {
        public PlacementTestGroupResultRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper)
            : base(dbContext, readDbContext, authContext, mapper)
        {
        }

        public async Task<IList<Guid>> GetStudentIdsAsync(IList<EnumCompletionStatus>? completionStatuses, IList<Guid> studentIds, IList<EnumCourseLevel>? suggetLevels, IList<EnumCourseLevel>? courseLevels)
        {
            var query = Queryable.WhereBulkContains(studentIds, x => x.StudentId);
            if (completionStatuses?.Any() == true)
            {
                var hasCompleted = completionStatuses.Contains(EnumCompletionStatus.Completed);
                var hasInProgress = completionStatuses.Contains(EnumCompletionStatus.InProgress);
                query = query.Where(x => (hasCompleted && x.Status == EnumResultStatus.Done) || (hasInProgress && x.Status != EnumResultStatus.Done));
            }
            if (suggetLevels?.Any() == true)
            {
                query = query.Where(x => x.SuggetLevel.HasValue && suggetLevels.Contains(x.SuggetLevel.Value));
            }

            if (courseLevels?.Any() == true)
            {
                query = query.Where(x => x.ChooseLevel.HasValue && courseLevels.Contains(x.ChooseLevel.Value));
            }
            return await query.Select(x => x.StudentId).ToListAsync();
        }
    }
}
