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
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Helpers;
    using MediatR;

    public class GetOverallScoreByClassForumQuery : IRequest<MethodResult<OverallScoreReportModel>>
    {
        public Guid CourseId { get; set; }
    }

    public sealed class GetOverallScoreByClassForumQueryHandler
        : IRequestHandler<GetOverallScoreByClassForumQuery, MethodResult<OverallScoreReportModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ICourseService _courseService;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;

        public GetOverallScoreByClassForumQueryHandler(
            AuthContext authContext,
            IUserService userService,
            ICourseService courseService,
            IClassForumRepository classForumRepository,
            IClassForumResultRepository classForumResultRepository)
        {
            _authContext = authContext;
            _userService = userService;
            _courseService = courseService;
            _classForumRepository = classForumRepository;
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task<MethodResult<OverallScoreReportModel>> Handle(GetOverallScoreByClassForumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<OverallScoreReportModel>();

            var student = await GetStudentOrErrorAsync(methodResult);
            if (student == null)
            {
                return methodResult;
            }

            // 1) build course theo cấu trúc mới
            var courseBuild = await _courseService.GetCourseBuildModel(request.CourseId);
            if (courseBuild?.CourseModules == null || courseBuild.CourseModules.Count == 0)
            {
                methodResult.Result = new OverallScoreReportModel();
                return methodResult;
            }

            // 2) lấy result của class forum theo course + student
            var classForumResults = await _classForumResultRepository.GetClassForumResultsAsync(request.CourseId, student.Id);

            // 3) lấy "case id" theo build nhưng ưu tiên result (giống HomeWork)
            var classForumCases = BaseLinqProgress.ExtractIdsPreferResult(courseBuild, classForumResults);
            if (classForumCases.Count == 0)
            {
                methodResult.Result = new OverallScoreReportModel();
                return methodResult;
            }

            // 4) total theo cấu hình (master)
            var skillTotals = await _classForumRepository.GetSkillScoresAsync(classForumCases);

            // 5) result theo student (detail)
            var skillResults = await _classForumResultRepository.GetSkillScoreResultsAsync(request.CourseId, student.Id);

            // 6) merge
            var merged = BaseLinqProgress.MergeSkillScores(skillTotals, skillResults);

            var report = new OverallScoreReportModel
            {
                SkillScores = merged.ToList(),
                TotalQuestion = merged.Sum(x => x.TotalQuestion),
                CountQuestion = merged.Sum(x => x.CountQuestion),
                CorrectCount = merged.Sum(x => x.CorrectCount),
                CorrectTotal = merged.Sum(x => x.TotalCount),
                CourseSkills = merged.Select(x => x.Skill).Distinct().ToList(),
                Skills = merged.Where(x => !string.IsNullOrWhiteSpace(x.SkillName))
                               .Select(x => new SkillViewModel
                               {
                                   Id = x.SkillId,
                                   Name = x.SkillName,
                                   FilePath = x.SkillFilePath
                               }).ToList()
            };

            methodResult.Result = report;
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
