// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels.ManagerReportModels;
    using Fsel.System.Domain.Models.QueryModels.ManagerReports;
    using Microsoft.EntityFrameworkCore;

    public class FeatureAccessTimeRepository : BaseRepository<FeatureAccessTime>, IFeatureAccessTimeRepository
    {
        public FeatureAccessTimeRepository(SystemDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }

        public async Task<IList<OverallFeatureAccessTimeModel>> GetOverallFeatureAccessTimesAsync(IList<GetFeatureAccessTimeReportQueryModel> queryModels, DateTime? startDate, DateTime? endDate)
        {
            var courseIds = queryModels.Select(x => x.CourseId).ToList();
            var userIds = queryModels.Select(x => x.UserId).ToList();
            var query = Queryable.Where(x => x.CourseId.HasValue && courseIds.Contains(x.CourseId.Value) && userIds.Contains(x.CreatedUserId))
                                 .Where(x => x.EnumFeature != EnumFeature.Other);
            if (startDate.HasValue)
            {
                query = query.Where(x => startDate.Value.Date <= (x.UpdatedDate ?? x.CreatedDate).Date);
            }
            if (endDate.HasValue)
            {
                query = query.Where(x => endDate.Value.Date >= (x.UpdatedDate ?? x.CreatedDate).Date);
            }
            return await query.GroupBy(x => new { x.CourseId, x.CreatedUserId })
                                      .Select(x => new OverallFeatureAccessTimeModel
                                      {
                                          UserId = x.Key.CreatedUserId,
                                          TotalTimeVideo = x.Where(x => x.EnumFeature == EnumFeature.VideoLesson).Sum(x => x.AccessTime),
                                          TotalTimeHomeWork = x.Where(x => x.EnumFeature == EnumFeature.HomeWork).Sum(x => x.AccessTime),
                                          TotalTimeClassForum = x.Where(x => x.EnumFeature == EnumFeature.ClassForum).Sum(x => x.AccessTime),
                                          TotalTime = x.Sum(x => x.AccessTime),
                                          TotalVisit = x.Sum(x => x.Visit),
                                          CurrentDate = x.Select(x => x.LastVisited).OrderByDescending(x => x).FirstOrDefault(),
                                      }).ToListAsync();
        }
    }
}
