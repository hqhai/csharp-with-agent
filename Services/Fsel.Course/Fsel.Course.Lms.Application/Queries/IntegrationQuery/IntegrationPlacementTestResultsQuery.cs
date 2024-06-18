// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.IntegrationQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class IntegrationPlacementTestResultsQuery : IRequest<MethodResult<IList<IntegrationPlacementTestResults>>>
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class IntegrationPlacementTestResultsQueryHandler : IRequestHandler<IntegrationPlacementTestResultsQuery, MethodResult<IList<IntegrationPlacementTestResults>>>
    {
        private readonly IPlacementTestResultRepository _placementTestResultRepository;

        public IntegrationPlacementTestResultsQueryHandler(IPlacementTestResultRepository placementTestResultRepository)
        {
            _placementTestResultRepository = placementTestResultRepository;
        }
        public async Task<MethodResult<IList<IntegrationPlacementTestResults>>> Handle(IntegrationPlacementTestResultsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<IntegrationPlacementTestResults>>();

            var querys = await _placementTestResultRepository.Queryable
                                                             .Where(x => x.UpdatedDate == null ? (x.CreatedDate >= request.StartDate && x.CreatedDate <= request.EndDate) : (x.UpdatedDate.Value >= request.StartDate && x.UpdatedDate.Value <= request.EndDate))
                                                             .GroupBy(x => x.CreatedUserId)
                                                             .Select(x => new IntegrationPlacementTestResults
                                                             {
                                                                 UserId = x.Key,
                                                                 Level = x.OrderByDescending(x => x.CreatedDate).FirstOrDefault() != null ? x.OrderByDescending(x => x.CreatedDate).FirstOrDefault()!.Level : default,
                                                                 Status = x.OrderByDescending(x => x.CreatedDate).FirstOrDefault() != null ? x.OrderByDescending(x => x.CreatedDate).FirstOrDefault()!.Status.ToString() : default,
                                                                 PlacementTestResults = x.Select(c => new IntegrationPlacementTestResultModels
                                                                 {
                                                                     Level = c.Level,
                                                                     CorrectCount = c.CorrectCount,
                                                                     CorrectTotal = c.CorrectTotal,
                                                                     SkillScores = c.SkillScores
                                                                 }).ToList()
                                                             })
                                                             .ToListAsync(cancellationToken);

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

        public EnumPlacementTestLevel Level { get; set; }

        public string? Status { get; set; }

        public IList<IntegrationPlacementTestResultModels>? PlacementTestResults { get; set; }
    }
}
