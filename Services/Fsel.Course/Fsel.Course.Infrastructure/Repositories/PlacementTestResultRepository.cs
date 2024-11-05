// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System.Collections.Generic;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;

    public class PlacementTestResultRepository : BaseRepository<PlacementTestResult>, IPlacementTestResultRepository
    {
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;

        public PlacementTestResultRepository(CourseDbContext dbContext, IPlacementTestGroupResultRepository placementTestGroupResultRepository, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
        }

        public async Task<(EnumCourseLevel?, bool)> CheckByPassPlacementTestAsync(Guid studentId, int age)
        {
            var placementTestGroupResult = await _placementTestGroupResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == studentId);
            if (placementTestGroupResult != null)
            {
                return (placementTestGroupResult.SuggetLevel, placementTestGroupResult.Status == EnumResultStatus.Done);
            }
            var placementTestResults = await Queryable.Where(x => x.StudentId == studentId)
                                                      .OrderByDescending(x => x.CreatedDate)
                                                      .ToListAsync();

            var placementTestResultLast = placementTestResults.Where(x => x.Status == EnumResultStatus.Done).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
            var placementTestResultInitial = placementTestResults.OrderBy(x => x.CreatedDate).FirstOrDefault();
            if (placementTestResultInitial == null || placementTestResultLast == null)
            {
                return (default, default);
            }
            var (currentLevel, isLockPT) = placementTestResultInitial.Level.GetLevelInScore(placementTestResultLast.Percent, IeltsScoreHelper.GetInitialAge(placementTestResultInitial.Level, age));
            return (currentLevel, isLockPT);
        }

        public async Task<List<Guid>> GetStudentPtIdsAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = Queryable;
            // Áp dụng bộ lọc theo ngày (nếu có)
            if (startDate.HasValue)
            {
                query = query.Where(x =>
                    (x.UpdatedDate.HasValue ? x.UpdatedDate.Value.Date >= startDate.Value.Date : x.CreatedDate.Date >= startDate.Value.Date));
            }
            if (endDate.HasValue)
            {
                query = query.Where(x =>
                    (x.UpdatedDate.HasValue ? x.UpdatedDate.Value.Date <= endDate.Value.Date : x.CreatedDate.Date <= endDate.Value.Date));
            }
            var groupedResults = await query.ToListAsync();
            return groupedResults.GroupBy(x => x.StudentId)
                                 .Select(g => g.OrderByDescending(x => x.UpdatedDate)
                                               .ThenByDescending(x => x.CreatedDate)
                                 .FirstOrDefault()).Select(x => x!.StudentId).ToList();
        }
    }
}
