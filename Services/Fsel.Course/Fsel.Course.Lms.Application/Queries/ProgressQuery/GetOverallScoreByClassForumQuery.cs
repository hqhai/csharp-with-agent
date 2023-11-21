// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities.SkillScoresConfigs;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Lms.Application.Services.UserServices;
using Fsel.Shared.Enums;
using Fsel.Shared.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    public class GetOverallScoreByClassForumQuery : IRequest<MethodResult<OverallScoreReportModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetOverallScoreByClassForumQueryHandler : IRequestHandler<GetOverallScoreByClassForumQuery, MethodResult<OverallScoreReportModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUnitRepository _unitRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUserService _userService;

        public GetOverallScoreByClassForumQueryHandler(AuthContext authContext
            , IUnitRepository unitRepository
            , IClassForumRepository classForumRepository
            , ICourseRepository courseRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _unitRepository = unitRepository;
            _classForumRepository = classForumRepository;
            _courseRepository = courseRepository;
            _userService = userService;
        }

        public async Task<MethodResult<OverallScoreReportModel>> Handle(GetOverallScoreByClassForumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<OverallScoreReportModel> methodResult = new MethodResult<OverallScoreReportModel>();
            OverallScoreReportModel overallScoreReport = new OverallScoreReportModel();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var studentId = studentResult?.Content?.Result?.Id;

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

            var units = await _unitRepository.Queryable.Include(x => x.UnitLessons)
                  .ThenInclude(x => x.Lesson)
                  .ThenInclude(x => x!.ClassForum)
                  .Include(x => x.CourseUnitMockTests)
                  .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == request.CourseId))
                  .ToListAsync(cancellationToken);
            if (units == null || units.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(units));
                return methodResult;
            }
            var classForumIds = units.SelectMany(x => x.UnitLessons).Select(x => x.Lesson).Select(x => x!.ClassForum).Select(x => x!.Id).ToList();

            var classForums = await _classForumRepository.Queryable.Include(x => x.ClassForumResults.Where(x => x.StudentId == studentId))
                                                                         .ThenInclude(x => x.ClassForumScores)
                                                                         .Where(x => classForumIds.Contains(x.Id))
                                                                         .ToListAsync(cancellationToken);
            if (classForums == null || classForums.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(units));
                return methodResult;
            }
            var skillScores = classForums.GroupBy(x => x.CourseSkill).Select(x => new SkillScores
            {
                Skill = x.Key,
                CorrectCount = x.SelectMany(x => x.ClassForumResults).SelectMany(x => x.ClassForumScores).Sum(x => x.Score),
                TotalCount = 36,
            }).ToList();
            overallScoreReport.SkillScores = skillScores;
            overallScoreReport.CourseSkills = classForums.Select(x => x.CourseSkill).Distinct().ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = overallScoreReport;
            return methodResult;
        }
    }
}
