// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
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
    }
}
