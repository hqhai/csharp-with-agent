// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.IntegrationQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class IntegrationPlacementTestResultsQuery : IRequest<MethodResult<IList<IntegrationPlacementTestResults>>>
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public IList<Guid>? UserIds { get; set; }
    }

    public class IntegrationPlacementTestResultsQueryHandler : IRequestHandler<IntegrationPlacementTestResultsQuery, MethodResult<IList<IntegrationPlacementTestResults>>>
    {
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IUserService _userService;

        public IntegrationPlacementTestResultsQueryHandler(IPlacementTestResultRepository placementTestResultRepository, IUserService userService)
        {
            _placementTestResultRepository = placementTestResultRepository;
            _userService = userService;
        }
        public async Task<MethodResult<IList<IntegrationPlacementTestResults>>> Handle(IntegrationPlacementTestResultsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<IntegrationPlacementTestResults>>();

            if (request.UserIds == null)
            {
                var placementTestResultHasTimes = await _placementTestResultRepository.Queryable
                                                                 .Where(x => x.UpdatedDate == null ? (x.CreatedDate >= request.StartDate && x.CreatedDate <= request.EndDate) : (x.UpdatedDate.Value >= request.StartDate && x.UpdatedDate.Value <= request.EndDate))
                                                                 .ToListAsync(cancellationToken);

                var datas = placementTestResultHasTimes.GroupBy(x => x.CreatedUserId)
                                              .Select(x => new IntegrationPlacementTestResults
                                              {
                                                  UserId = x.Key,
                                              }).ToList();

                methodResult.Result = datas;
                return methodResult;
            }

            var placementTestResults = await _placementTestResultRepository.Queryable
                                                                           .WhereBulkContains(request.UserIds, x => x.CreatedUserId)
                                                                           .ToListAsync(cancellationToken);

            var userIds = placementTestResults.Select(x => x.CreatedUserId).Distinct().ToList();
            var users = await _userService.GetUserByIds(userIds);
            if (!users.IsSuccessStatusCode)
            {
                methodResult.AddError(users.Error);
                return methodResult;
            }

            var userResults = users.Content?.Result;

            var querys = placementTestResults.GroupBy(x => x.CreatedUserId)
                                             .Select(x => new IntegrationPlacementTestResults
                                             {
                                                 UserId = x.Key,
                                                 Level = (x.OrderByDescending(x => x.CreatedDate).FirstOrDefault() != null &&
                                                          x.OrderByDescending(x => x.CreatedDate).FirstOrDefault()!.Status == EnumResultStatus.Done &&
                                                          userResults != null && userResults.Any(c => c?.UserId == x.Key)) ?
                                                          x.OrderByDescending(x => x.CreatedDate).FirstOrDefault()!.Level.GetLevelInScore(x.OrderByDescending(x => x.CreatedDate)
                                                          .FirstOrDefault()?.Percent, IeltsScoreHelper.GetInitialAge(x.OrderBy(x => x.CreatedDate)
                                                          .FirstOrDefault()?.Level, DateTimeHelper.GetYearOld(userResults.FirstOrDefault(c => c?.UserId == x.Key)?.User?.Birthday))).Item1!.Value : null,
                                                 Status = x.OrderByDescending(x => x.CreatedDate).FirstOrDefault() != null ? x.OrderByDescending(x => x.CreatedDate).FirstOrDefault()!.Status.ToString() : default,
                                                 PlacementTestResults = x.OrderBy(x => x.CreatedDate).Select(c => new IntegrationPlacementTestResultModels
                                                 {
                                                     Level = c.Level,
                                                     CorrectCount = c.CorrectCount,
                                                     CorrectTotal = c.CorrectTotal,
                                                     SkillScores = c.SkillScores
                                                 }).ToList(),
                                                 DateEdit = x.Max(c => c.CreatedDate > c.UpdatedDate ? c.CreatedDate : c.UpdatedDate)
                                             }).ToList();

            methodResult.Result = querys;
            return methodResult;
        }
    }

    public class IntegrationPlacementTestResultModels
    {
        public EnumPlacementTestLevel Level { get; set; }

        public int CorrectCount { get; set; }

        public int CorrectTotal { get; set; }

        public IList<SkillScores>? SkillScores { get; set; }
    }

    public class IntegrationPlacementTestResults
    {
        public Guid UserId { get; set; }

        public EnumCourseLevel? Level { get; set; }

        public string? Status { get; set; }

        public IList<IntegrationPlacementTestResultModels>? PlacementTestResults { get; set; }

        public DateTime? DateEdit { get; set; }
    }
}
