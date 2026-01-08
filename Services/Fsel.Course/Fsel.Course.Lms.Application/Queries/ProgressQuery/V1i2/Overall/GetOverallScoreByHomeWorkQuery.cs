// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery.V1i2.Overall
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.SkillModels;
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetOverallScoreByHomeWorkQuery : IRequest<MethodResult<OverallScoreReportModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetOverallScoreByHomeWorkQueryHandler : IRequestHandler<GetOverallScoreByHomeWorkQuery, MethodResult<OverallScoreReportModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IUserService _userService;
        private readonly ICourseService _courseService;

        public GetOverallScoreByHomeWorkQueryHandler(AuthContext authContext
            , IHomeWorkRepository homeWorkRepository
            , IHomeWorkResultRepository homeWorkResultRepository
            , IUserService userService
            , ICourseService courseService)
        {
            _authContext = authContext;
            _homeWorkRepository = homeWorkRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _userService = userService;
            _courseService = courseService;
        }

        public async Task<MethodResult<OverallScoreReportModel>> Handle(GetOverallScoreByHomeWorkQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<OverallScoreReportModel> methodResult = new MethodResult<OverallScoreReportModel>();
            var student = await GetStudentOrErrorAsync(methodResult);
            if (student == null)
            {
                return methodResult;
            }
            // 1) Lấy course build từ cache/service mới
            var courseBuild = await _courseService.GetCourseBuildModel(request.CourseId);
            if (courseBuild?.CourseModules == null || courseBuild.CourseModules.Count == 0)
            {
                methodResult.Result = new OverallScoreReportModel();
                return methodResult;
            }
            var homeWorkResults = await _homeWorkResultRepository.GetHomeWorkResultsAsync(request.CourseId, student.Id);

            var homeWorkCases = BaseLinqProgress.ExtractIdsPreferResult(courseBuild, homeWorkResults);
            if (homeWorkCases.Count == 0)
            {
                methodResult.Result = new OverallScoreReportModel();
                return methodResult;
            }
            OverallScoreReportModel overallScoreReport = new OverallScoreReportModel();
            var skillScores = await _homeWorkRepository.GetSkillScoresAsync(homeWorkCases);
            var skillScoreResults = await _homeWorkResultRepository.GetSkillScoreResultsAsync(request.CourseId, student.Id);

            var merged = BaseLinqProgress.MergeSkillScores(skillScores, skillScoreResults);

            overallScoreReport.SkillScores = merged.ToList();
            overallScoreReport.TotalQuestion = merged.Sum(x => x.TotalQuestion);
            overallScoreReport.CorrectCount = merged.Sum(x => x.CorrectCount);
            overallScoreReport.CorrectTotal = merged.Sum(x => x.TotalCount);
            overallScoreReport.CountQuestion = merged.Sum(x => x.CountQuestion);
            overallScoreReport.CourseSkills = merged.Select(x => x.Skill).Distinct().ToList();
            overallScoreReport.Skills = merged.Where(x => x.SkillName != null).Select(x => new SkillViewModel
            {
                Id = x.SkillId,
                Name = x.SkillName,
                FilePath = x.SkillFilePath
            }).ToList();
            methodResult.Result = overallScoreReport;
            return methodResult;
        }

        private async Task<StudentModel?> GetStudentOrErrorAsync(MethodResult<OverallScoreReportModel> methodResult)
        {
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return null;
            }

            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return null;
            }

            return student;
        }
    }
}
