// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
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

    public class GetOverallScoreByUnitTestQuery : IRequest<MethodResult<OverallScoreReportModel>>
    {
        public Guid CourseId { get; set; }
    }

    public class GetOverallScoreByUnitTestQueryHandler : IRequestHandler<GetOverallScoreByUnitTestQuery, MethodResult<OverallScoreReportModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUnitRepository _unitRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IUserService _userService;

        public GetOverallScoreByUnitTestQueryHandler(AuthContext authContext
            , IUnitRepository unitRepository
            , IQuestionRepository questionRepository
            , IExerciseRepository exerciseRepository
            , IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository
            , ICourseResultRepository courseResultRepository
            , ILessonRepository lessonRepository
            , IVideoTimeCodeRepository videoTimeCodeRepository
            , ICourseRepository courseRepository
            , ILessonResultRepository lessonResultRepository
            , IVideoResultRepository videoResultRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _unitRepository = unitRepository;
            _questionRepository = questionRepository;
            _exerciseRepository = exerciseRepository;
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _courseResultRepository = courseResultRepository;
            _lessonRepository = lessonRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _courseRepository = courseRepository;
            _lessonResultRepository = lessonResultRepository;
            _videoResultRepository = videoResultRepository;
            _userService = userService;
        }

        public async Task<MethodResult<OverallScoreReportModel>> Handle(GetOverallScoreByUnitTestQuery request, CancellationToken cancellationToken)
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
            var (videoIds, courseResult) = method.Result;
            if (courseResult == null)
            {
                methodResult.Result = overallScoreReport;
                return methodResult;
            }

            var lessonResultIds = await GetLessonResultIdsAsync(request, courseResult.StudentId);
            var videoDuplicateIds = videoIds.GroupBy(x => x)
                .Select(x => new
                {
                    Id = x.Key,
                    NumberOfDuplicate = x.Count()
                }).ToList();

            var videoResultIds = await _videoResultRepository.Queryable.Where(x => lessonResultIds.Contains(x.LessonResultId)).Select(x => x.Id).ToListAsync(cancellationToken);
            var skillScores = new List<SkillScores>();
            foreach (var videoId in videoIds.Distinct())
            {
                var videoTimeCodeIds = await GetVideoTimeCodeUnitTestsAsync(videoId, cancellationToken);
                var groupExercises = await _exerciseRepository.Queryable.Where(x => x.TimeCodeExercises.Any(x => videoTimeCodeIds.Contains(x.VideoTimeCodeId)))
                    .GroupBy(x => x.CourseSkill)
                    .Select(x => new { x.Key, ExerciseIds = x.Select(x => x.Id).ToList() })
                    .ToListAsync(cancellationToken);
                var numberOfDuplicate = videoDuplicateIds.Where(vid => vid.Id == videoId).Max(x => x.NumberOfDuplicate);

                foreach (var item in groupExercises)
                {
                    var (skillScoreQuestion, questionIds) = await GetSkillScoreQuestionAsync(item.ExerciseIds, cancellationToken);
                    var skillScoreAnswer = await GetSkillScoreVideoTimeCodeAnswerAsync(questionIds, videoResultIds, cancellationToken);
                    skillScores.Add(new SkillScores
                    {
                        Skill = item.Key,
                        CountQuestion = skillScoreAnswer.CountQuestion,
                        CorrectCount = skillScoreAnswer.CorrectCount,
                        TotalQuestion = skillScoreQuestion.TotalQuestion * numberOfDuplicate,
                        TotalCount = skillScoreQuestion.TotalCount * numberOfDuplicate,
                    });
                }
            }

            skillScores = skillScores.GroupBy(x => x.Skill).Select(x => new SkillScores
            {
                Skill = x.Key,
                CountQuestion = x.Sum(x => x.CountQuestion),
                CorrectCount = x.Sum(x => x.CorrectCount),
                TotalQuestion = x.Sum(x => x.TotalQuestion),
                TotalCount = x.Sum(x => x.TotalCount),
            }).ToList();

            overallScoreReport.SkillScores = skillScores;
            overallScoreReport.CountQuestion = skillScores.Sum(x => x.CountQuestion);
            overallScoreReport.TotalQuestion = skillScores.Sum(x => x.TotalQuestion);
            overallScoreReport.CourseSkills = skillScores.Select(x => x.Skill).ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = overallScoreReport;
            return methodResult;
        }

        private async Task<IList<Guid>> GetVideoTimeCodeUnitTestsAsync(Guid videoId, CancellationToken cancellationToken)
        {
            return await _videoTimeCodeRepository.Queryable.Where(x => x.VideoId == videoId && x.TimeCodeType == EnumTimeCodeType.UnitTest).Select(x => x.Id).ToListAsync(cancellationToken);
        }

        private async Task<SkillScores> GetSkillScoreVideoTimeCodeAnswerAsync(IList<Guid> questionIds, IList<Guid> videoResultIds, CancellationToken cancellationToken)
        {
            if (!videoResultIds.Any() || !questionIds.Any())
            {
                return new SkillScores
                {
                    CorrectCount = default,
                    CountQuestion = default,
                };
            }

            var listScore = await _videoTimeCodeAnswerRepository.Queryable
                                               .Where(x => x.VideoResultId.HasValue)
                                               .WhereBulkContains(questionIds, x => x.QuestionId)
                                               .WhereBulkContains(videoResultIds, x => x.VideoResultId)
                                               .Select(x => x.CorrectCount)
                                               .ToListAsync(cancellationToken);
            return new SkillScores
            {
                CorrectCount = listScore.Sum(x => x),
                CountQuestion = listScore.Count,
            };
        }

        private async Task<(SkillScores, IList<Guid>)> GetSkillScoreQuestionAsync(IList<Guid> exerciseIds, CancellationToken cancellationToken)
        {
            var listScore = await _questionRepository.Queryable.Where(x => x.ExerciseQuestions.Any(x => exerciseIds.Contains(x.ExerciseId)))
                                    .Where(x => !x!.Ungraded && x.QuestionType != EnumQuestionType.ExercisePreparation)
                                    .Select(x => new { x.CorrectTotal, x.Id }).ToListAsync(cancellationToken);
            return (new SkillScores
            {
                TotalCount = listScore.Sum(x => x.CorrectTotal),
                TotalQuestion = listScore.Count,
            }, listScore.Select(x => x.Id).ToList());
        }

        private async Task<MethodResult<(List<Guid>, CourseResult?)>> Validate(GetOverallScoreByUnitTestQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<(List<Guid>, CourseResult?)>();
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
                return methodResult;
            }

            var videoIds = await GetVideoIdsAsync(request);
            methodResult.Result = (videoIds, courseResult);
            return methodResult;
        }

        private async Task<IList<Guid>> GetLessonIdsAsync(GetOverallScoreByUnitTestQuery request)
        {
            return await _unitRepository.Queryable.Include(x => x.UnitLessons)
                                                  .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == request.CourseId))
                                                  .SelectMany(x => x.UnitLessons)
                                                  .Select(x => x.LessonId)
                                                  .ToListAsync();
        }

        private async Task<IList<Guid>> GetLessonResultIdsAsync(GetOverallScoreByUnitTestQuery request, Guid studentId)
        {
            return await _lessonResultRepository.Queryable.Where(x => x.StudentId == studentId && x.CourseId == request.CourseId)
                                                          .Select(x => x.Id)
                                                          .ToListAsync();
        }

        private async Task<List<Guid>> GetVideoIdsAsync(GetOverallScoreByUnitTestQuery request)
        {
            var lessonIds = await GetLessonIdsAsync(request);
            return await _lessonRepository.Queryable.Include(x => x.LessonVideos)
                                                  .WhereBulkContains(lessonIds, x => x.Id)
                                                  .SelectMany(x => x.LessonVideos)
                                                  .Select(x => x.VideoId)
                                                  .ToListAsync();
        }
    }
}
