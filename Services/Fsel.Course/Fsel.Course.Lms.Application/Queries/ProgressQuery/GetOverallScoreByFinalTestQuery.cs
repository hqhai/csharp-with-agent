// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetOverallScoreByFinalTestQuery : IRequest<MethodResult<OverallScoreReportModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetOverallScoreByFinalTestQueryHandler : IRequestHandler<GetOverallScoreByFinalTestQuery, MethodResult<OverallScoreReportModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUserService _userService;

        public GetOverallScoreByFinalTestQueryHandler(AuthContext authContext
            , IFinalTestResultRepository finalTestResultRepository
            , ICourseRepository courseRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _finalTestResultRepository = finalTestResultRepository;
            _courseRepository = courseRepository;
            _userService = userService;
        }

        public async Task<MethodResult<OverallScoreReportModel>> Handle(GetOverallScoreByFinalTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<OverallScoreReportModel> methodResult = new MethodResult<OverallScoreReportModel>();
            OverallScoreReportModel overallScoreReport = new OverallScoreReportModel();

            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var studentId = student.Id;

            var course = await _courseRepository.GetByIdAsync(request.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }
            else if (course.CourseLevel.GetEnumCourseType() != EnumCourseType.Academic)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseNotTypeAcademic), nameof(course));
                return methodResult;
            }

            var finalTestResults = await _finalTestResultRepository.ReadQueryable.Where(x => x.StudentId == studentId && x.CourseId == request.CourseId)
                                                                   .Where(x => x.Status == EnumResultStatus.Done)
                                                                   .ToListAsync(cancellationToken);
            if (!finalTestResults.Any())
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var skillScores = finalTestResults.Where(x => x.SkillScores != null && x.SkillScores.Count > 0)
                .SelectMany(x => x.SkillScores!)
                .GroupBy(x => x.Skill)
                .Select(x => new SkillScores
                {
                    Skill = x.Key,
                    CorrectCount = x.Sum(x => x.CorrectCount),
                    TotalCount = x.Sum(x => x.TotalCount),
                    CountQuestion = x.Sum(x => x.CountQuestion),
                    TotalQuestion = x.Sum(x => x.TotalQuestion),
                    Scores = x.Average(x => x.Scores)
                }).ToList();
            overallScoreReport.SkillScores = skillScores;
            overallScoreReport.TotalQuestion = skillScores.Sum(x => x.TotalQuestion);
            overallScoreReport.CountQuestion = skillScores.Sum(x => x.CountQuestion);
            overallScoreReport.CourseSkills = new List<EnumCourseSkill> { EnumCourseSkill.Reading, EnumCourseSkill.Grammar, EnumCourseSkill.Vocabulary };
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = overallScoreReport;
            return methodResult;
        }
    }
}
