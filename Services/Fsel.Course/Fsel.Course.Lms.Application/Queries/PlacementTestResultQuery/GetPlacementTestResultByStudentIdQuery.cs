// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestResultQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queries.IntegrationQuery;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;

    public class GetPlacementTestResultByStudentIdQuery : IRequest<MethodResult<GetPlacementTestResultByStudentModel>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetPlacementTestResultByStudentIdQueryHandler : IRequestHandler<GetPlacementTestResultByStudentIdQuery, MethodResult<GetPlacementTestResultByStudentModel>>
    {
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;

        public GetPlacementTestResultByStudentIdQueryHandler(IPlacementTestGroupResultRepository _placementTestGroupResultRepository)
        {
            this._placementTestGroupResultRepository = _placementTestGroupResultRepository;
        }

        public async Task<MethodResult<GetPlacementTestResultByStudentModel>> Handle(GetPlacementTestResultByStudentIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<GetPlacementTestResultByStudentModel> methodResult = new MethodResult<GetPlacementTestResultByStudentModel>();

            var placementTestGroupResult = await _placementTestGroupResultRepository.Queryable
                                                                                    .Include(x => x.PlacementTestResults)
                                                                                    .FirstOrDefaultAsync(x => x.StudentId == request.StudentId, cancellationToken);
            if (placementTestGroupResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(placementTestGroupResult), placementTestGroupResult);
                return methodResult;
            }

            methodResult.Result = new GetPlacementTestResultByStudentModel
            {
                CurrentLevel = placementTestGroupResult.CurrentLevel == EnumCourseLevel.A1 && placementTestGroupResult.Percent < MinCompletePercent ? ValueCourseLevel.PreA1 : EnumCourseLevelHelper.GetCodeByEnumCourseLevel(placementTestGroupResult.CurrentLevel),
                RecommendedLevel = placementTestGroupResult.SuggetLevel,
                PlacementTestResults = placementTestGroupResult.PlacementTestResults.OrderBy(x => x.CreatedDate).Select(c => new IntegrationPlacementTestResultModels
                {
                    Level = c.Level,
                    CorrectCount = c.CorrectCount,
                    CorrectTotal = c.CorrectTotal,
                    SkillScores = c.SkillScores
                }).ToList()
            };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }

    public class GetPlacementTestResultByStudentModel
    {
        public string? CurrentLevel { get; set; }

        public EnumCourseLevel? RecommendedLevel { get; set; }

        public IList<IntegrationPlacementTestResultModels>? PlacementTestResults { get; set; }
    }
}
