// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetOverallScoreByHomeWorkQuery : IRequest<MethodResult<OverallScoreReportModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetOverallScoreByHomeWorkQueryHandler : IRequestHandler<GetOverallScoreByHomeWorkQuery, MethodResult<OverallScoreReportModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IHomeWorkAnswerRepository _homeWorkAnswerRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonHomeWorkRepository _lessonHomeWorkRepository;
        private readonly IUserService _userService;

        public GetOverallScoreByHomeWorkQueryHandler(AuthContext authContext
            , IHomeWorkRepository homeWorkRepository
            , IUnitRepository unitRepository
            , IHomeWorkResultRepository homeWorkResultRepository
            , IHomeWorkAnswerRepository homeWorkAnswerRepository
            , IQuestionRepository questionRepository
            , ICourseRepository courseRepository
            , ICourseResultRepository courseResultRepository
            , ILessonResultRepository lessonResultRepository
            , ILessonHomeWorkRepository lessonHomeWorkRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _homeWorkRepository = homeWorkRepository;
            _unitRepository = unitRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _homeWorkAnswerRepository = homeWorkAnswerRepository;
            _questionRepository = questionRepository;
            _courseRepository = courseRepository;
            _courseResultRepository = courseResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _lessonHomeWorkRepository = lessonHomeWorkRepository;
            _userService = userService;
        }

        public async Task<MethodResult<OverallScoreReportModel>> Handle(GetOverallScoreByHomeWorkQuery request, CancellationToken cancellationToken)
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
            var courseResult = method.Result!;
            //Check Duplicate HomeWorkId

            #region Check Duplicate HomeWorkId

            var homeWorkDuplicates = await GetHomeWorksDupliateAsync(request, cancellationToken);
            if (homeWorkDuplicates == null || !homeWorkDuplicates.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkDuplicates));
                return methodResult;
            }

            #endregion Check Duplicate HomeWorkId

            var homeWorkIds = homeWorkDuplicates.Select(x => x.ObjectId).ToList() ?? new List<Guid>();

            var groupHomeWorkResult = await _homeWorkRepository.Queryable.WhereBulkContains(homeWorkIds, x => x.Id).ToListAsync(cancellationToken);

            var groupHomeWork = groupHomeWorkResult.GroupBy(x => x.CourseSkill)
                                                   .Select(x => new { x.Key, HomeWorkIds = x.Select(x => x.Id).ToList() })
                                                   .ToList();

            if (groupHomeWork == null || groupHomeWork.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(groupHomeWork));
                return methodResult;
            }
            var skillScores = new List<SkillScores>();
            foreach (var item in groupHomeWork)
            {
                var homeWorkResultIds = await GetHomeWorkResultIdsAsync(request, item.HomeWorkIds, courseResult.StudentId);
                var skillScoreQuestion = await GetSkillScoreQuestionAsync(item.HomeWorkIds, homeWorkDuplicates, cancellationToken);
                var skillScoreHomeWorkAnswer = await GetSkillScoreHomeWorkAnswerAsync(homeWorkResultIds, cancellationToken);
                skillScores.Add(new SkillScores
                {
                    Skill = item.Key,
                    CountQuestion = skillScoreHomeWorkAnswer.CountQuestion,
                    TotalQuestion = skillScoreQuestion.TotalQuestion,
                    CorrectCount = skillScoreHomeWorkAnswer.CorrectCount,
                    TotalCount = skillScoreQuestion.TotalCount,
                });
            }
            skillScores = skillScores.OrderBy(x => x.Skill).ToList();
            overallScoreReport.SkillScores = skillScores;
            overallScoreReport.CountQuestion = skillScores.Sum(x => x.CountQuestion);
            overallScoreReport.TotalQuestion = skillScores.Sum(x => x.TotalQuestion);
            overallScoreReport.CourseSkills = skillScores.Select(x => x.Skill).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = overallScoreReport;
            return methodResult;
        }

        private async Task<MethodResult<CourseResult>> Validate(GetOverallScoreByHomeWorkQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<CourseResult>();
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
            var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseId == request.CourseId && x.StudentId == student.Id, cancellationToken);
            if (courseResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseResult));
                return methodResult;
            }
            methodResult.Result = courseResult;
            return methodResult;
        }

        private async Task<IList<Guid>> GetHomeWorkResultIdsAsync(GetOverallScoreByHomeWorkQuery request, IList<Guid> homeWorkIds, Guid studentId)
        {
            var lessonResultIds = await GetLessonResultIdsAsync(request, studentId);
            return await _homeWorkResultRepository.Queryable.WhereBulkContains(homeWorkIds, x => x.HomeWorkId).WhereBulkContains(lessonResultIds, x => x.LessonResultId).Select(x => x.Id).ToListAsync();
        }

        private async Task<SkillScores> GetSkillScoreHomeWorkAnswerAsync(IList<Guid> homeWorkResultIds, CancellationToken cancellationToken)
        {
            var listScore = await _homeWorkAnswerRepository.Queryable.WhereBulkContains(homeWorkResultIds, x => x.HomeWorkResultId)
                                    .Select(x => x.CorrectCount)
                                    .ToListAsync(cancellationToken);
            return new SkillScores
            {
                CorrectCount = listScore.Sum(x => x),
                CountQuestion = listScore.Count,
            };
        }

        private async Task<IList<ModuleDuplicateModel>> GetHomeWorksDupliateAsync(GetOverallScoreByHomeWorkQuery request, CancellationToken cancellationToken)
        {
            var lessonIds = await GetLessonIdsAsync(request) ?? new List<Guid>();
            var lessonDuplicateIds = lessonIds.GroupBy(x => x).Select(x => new ModuleDuplicateModel
            {
                ObjectId = x.Key,
                NumberOfDuplicate = x.Count(),
            }).ToList();

            var homeWorkDuplicates = await _lessonHomeWorkRepository.Queryable
                                                                    .WhereBulkContains(lessonIds, x => x.LessonId)
                                                                    .Select(x => new { x.HomeWorkId, x.LessonId })
                                                                    .ToListAsync(cancellationToken);

            return homeWorkDuplicates.Select(x => new ModuleDuplicateModel
            {
                ObjectId = x.HomeWorkId,
                NumberOfDuplicate = lessonDuplicateIds.Where(y => y.ObjectId == x.LessonId).Max(x => x.NumberOfDuplicate),
            }).ToList();
        }

        private async Task<SkillScores> GetSkillScoreQuestionAsync(IList<Guid> homeWorkIds, IList<ModuleDuplicateModel> moduleDuplicates, CancellationToken cancellationToken)
        {
            var listScore = await _questionRepository.Queryable.Where(x => x.HomeWorkQuestions.Any(x => homeWorkIds.Contains(x.HomeWorkId)))
                                    .Select(x => new { x.CorrectTotal, HomeWorkId = x.HomeWorkQuestions.Select(x => x.HomeWorkId).FirstOrDefault() })
                                    .ToListAsync(cancellationToken);
            var skillScores = listScore.GroupBy(x => x.HomeWorkId).Select(x =>
            {
                var skillScore = new SkillScores();
                var numberOfDuplicate = moduleDuplicates.Where(y => y.ObjectId == x.Key).Max(x => x.NumberOfDuplicate);
                skillScore.TotalCount = x.Sum(x => x.CorrectTotal) * numberOfDuplicate;
                skillScore.TotalQuestion = x.Count() * numberOfDuplicate;
                return skillScore;
            }).ToList();
            return new SkillScores
            {
                TotalCount = skillScores.Sum(x => x.TotalCount),
                TotalQuestion = skillScores.Sum(x => x.TotalQuestion),
            };
        }

        private async Task<IList<Guid>> GetLessonResultIdsAsync(GetOverallScoreByHomeWorkQuery request, Guid studentId)
        {
            return await _lessonResultRepository.Queryable.Where(x => x.StudentId == studentId && x.CourseId == request.CourseId)
                                                          .Select(x => x.Id)
                                                          .ToListAsync();
        }

        private async Task<IList<Guid>> GetLessonIdsAsync(GetOverallScoreByHomeWorkQuery request)
        {
            return await _unitRepository.Queryable.Include(x => x.UnitLessons)
                                                  .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == request.CourseId))
                                                  .SelectMany(x => x.UnitLessons)
                                                  .Select(x => x.LessonId)
                                                  .ToListAsync();
        }
    }
}
