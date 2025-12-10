// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
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
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IUserService _userService;

        public GetOverallScoreByClassForumQueryHandler(AuthContext authContext
            , IUnitRepository unitRepository
            , IClassForumRepository classForumRepository
            , ICourseRepository courseRepository
            , ICourseResultRepository courseResultRepository
            , ILessonResultRepository lessonResultRepository
            , IClassForumResultRepository classForumResultRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _unitRepository = unitRepository;
            _classForumRepository = classForumRepository;
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _classForumResultRepository = classForumResultRepository;
            _userService = userService;
        }

        public async Task<MethodResult<OverallScoreReportModel>> Handle(GetOverallScoreByClassForumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<OverallScoreReportModel> methodResult = new MethodResult<OverallScoreReportModel>();
            OverallScoreReportModel overallScoreReport = new OverallScoreReportModel();
            var method = await Validate(request, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            var courseResult = method.Result;
            if (courseResult == null)
            {
                methodResult.Result = overallScoreReport;
                return methodResult;
            }

            var lessonResultIds = await GetLessonResultIdsAsync(request, courseResult.StudentId);
            var classForumIds = await GetClassForumIdsAsync(request);
            if (classForumIds == null || !classForumIds.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForumIds));
                return methodResult;
            }

            var groupClassForumResults = await _classForumRepository.Queryable.WhereBulkContains(classForumIds, x => x.Id).ToListAsync(cancellationToken);
            var groupClassForum = groupClassForumResults.GroupBy(x => x.CourseSkill)
                                                        .Select(x => new { x.Key, ClassFourmIds = x.Select(x => x.Id).ToList() })
                                                        .ToList();

            if (groupClassForum == null || !groupClassForum.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(groupClassForum));
                return methodResult;
            }
            var skillScores = new List<SkillScores>();
            foreach (var item in groupClassForum)
            {
                var skillScoreClassFourm = await GetClassForumResultsAsync(request, item.ClassFourmIds, courseResult.StudentId);
                skillScores.Add(new SkillScores
                {
                    Skill = item.Key,
                    CorrectCount = skillScoreClassFourm.CorrectCount,
                    CountQuestion = skillScoreClassFourm.CountQuestion,
                    TotalCount = skillScoreClassFourm.TotalCount,
                    TotalQuestion = item.ClassFourmIds.Count
                });
            }
            skillScores = skillScores.OrderBy(x => x.Skill).ToList();
            overallScoreReport.SkillScores = skillScores;
            overallScoreReport.CourseSkills = skillScores.Select(x => x.Skill).ToList();
            methodResult.Result = overallScoreReport;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<MethodResult<CourseResult?>> Validate(GetOverallScoreByClassForumQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<CourseResult?>();
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
            var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseId == request.CourseId && x.StudentId == student.Id, cancellationToken);
            if (courseResult == null)
            {
                return methodResult;
            }
            methodResult.Result = courseResult;
            return methodResult;
        }

        private async Task<IList<Guid>> GetLessonResultIdsAsync(GetOverallScoreByClassForumQuery request, Guid studentId)
        {
            return await _lessonResultRepository.Queryable.Where(x => x.StudentId == studentId && x.CourseId == request.CourseId).Select(x => x.Id).ToListAsync();
        }

        private async Task<SkillScores> GetClassForumResultsAsync(GetOverallScoreByClassForumQuery request, IList<Guid> classForumIds, Guid studentId)
        {
            var lessonResultIds = await GetLessonResultIdsAsync(request, studentId);
            var classForumResults = await _classForumResultRepository.Queryable.Include(x => x.ClassForumScores).Where(x => lessonResultIds.Contains(x.LessonResultId) && classForumIds.Contains(x.ClassForumId)).ToListAsync();

            return new SkillScores
            {
                CountQuestion = classForumResults.Count,
                CorrectCount = classForumResults.Sum(x => x.CorrectCount),
                TotalCount = classForumResults.Sum(x => x.CorrectTotal)
            };
        }

        private async Task<IList<Guid>> GetLessonIdsAsync(GetOverallScoreByClassForumQuery request)
        {
            return await _unitRepository.Queryable.Include(x => x.UnitLessons)
                                                  .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == request.CourseId))
                                                  .SelectMany(x => x.UnitLessons)
                                                  .Select(x => x.LessonId)
                                                  .ToListAsync();
        }

        private async Task<IList<Guid>> GetClassForumIdsAsync(GetOverallScoreByClassForumQuery request)
        {
            var lessonIds = await GetLessonIdsAsync(request);
            return await _classForumRepository.Queryable.WhereBulkContains(lessonIds, x => x.LessonId).Select(x => x.Id).ToListAsync();
        }
    }
}
