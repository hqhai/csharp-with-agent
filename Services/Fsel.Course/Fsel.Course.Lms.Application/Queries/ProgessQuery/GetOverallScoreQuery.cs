// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgessQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetOverallScoreQuery : IRequest<MethodResult<OverallScoreModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetOverallScoreQueryHandler : IRequestHandler<GetOverallScoreQuery, MethodResult<OverallScoreModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly IUserService _userService;

        public GetOverallScoreQueryHandler(AuthContext authContext
            , IPlacementTestResultRepository placementTestResultRepository
            , IUnitResultRepository unitResultRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _placementTestResultRepository = placementTestResultRepository;
            _unitResultRepository = unitResultRepository;
            _userService = userService;
        }

        public async Task<MethodResult<OverallScoreModel>> Handle(GetOverallScoreQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<OverallScoreModel> methodResult = new MethodResult<OverallScoreModel>();
            OverallScoreModel overallScoreModel = new OverallScoreModel();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            var level = student?.CourseLevel;
            var studentId = student?.Id;

            var unitResults = await _unitResultRepository.Queryable.Include(x => x.Unit)
                                                            .Where(x => x.StudentId == studentId && x.Status == EnumResultStatus.Done && x.CourseId == request.CourseId)
                                                            .ToArrayAsync(cancellationToken);
            if (unitResults != null && unitResults.Any())
            {
                overallScoreModel.SkillScores = unitResults.Where(x => x.SkillScores != null && x.SkillScores.Any())
                    .SelectMany(x => x.SkillScores!)
                    .GroupBy(x => x.Skill)
                    .Select(x => new SkillScores
                    {
                        Skill = x.Key,
                        CorrectCount = x.Sum(x => x.CorrectCount),
                        TotalCount = x.Sum(x => x.TotalCount),
                        Scores = x.Average(x => x.Scores),
                        CountQuestion = x.Sum(x => x.CountQuestion),
                        TotalQuestion = x.Sum(x => x.TotalQuestion),
                        Percent = x.Average(x => x.Percent)
                    }).ToList();
                overallScoreModel.IsPlacement = false;
                if ((level?.GetEnumCourseType() ?? default) == EnumCourseType.Academic)
                {
                    overallScoreModel.Percent = overallScoreModel.SkillScores != null ? overallScoreModel.SkillScores.Average(x => x.Percent) : default;
                }
                else
                {
                    overallScoreModel.Percent = overallScoreModel.SkillScores != null ? overallScoreModel.SkillScores.Average(x => x.Percent) : default;
                }
            }
            else
            {
                var placementTestScore = await _placementTestResultRepository.Queryable.OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(x => x.StudentId == studentId && x.Status == EnumResultStatus.Done, cancellationToken);
                if (placementTestScore == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(placementTestScore));
                    return methodResult;
                }
                overallScoreModel.SkillScores = placementTestScore.SkillScores;
                overallScoreModel.IsPlacement = true;
                if ((level?.GetEnumCourseType() ?? default) == EnumCourseType.Academic)
                {
                    overallScoreModel.Percent = (overallScoreModel.SkillScores?.Average(x => x.Percent) ?? default);
                }
                else
                {
                    overallScoreModel.Percent = (overallScoreModel.SkillScores?.Average(x => x.Percent) ?? default);
                }
            }
            overallScoreModel.CourseLevel = level ?? default;

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = overallScoreModel;
            return methodResult;
        }
    }
}
