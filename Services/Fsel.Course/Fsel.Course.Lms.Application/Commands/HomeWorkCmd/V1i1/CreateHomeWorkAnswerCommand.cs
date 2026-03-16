// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.HomeWorkCmd.V1i1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.HomeWorkAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Queries.HomeWorkQuery;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.ApplicationServices.CacheServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class CreateHomeWorkAnswerCommand : CreateHomeWorkAnswerV1i1CommandModel, IRequest<MethodResult<HomeWorkModel>>
    {
    }

    public class CreateHomeWorkAnswerCommandHandler : IRequestHandler<CreateHomeWorkAnswerCommand, MethodResult<HomeWorkModel>>
    {
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly QuestionConverter _questionConverter;
        private readonly IHomeWorkAnswerRepository _homeWorkAnswerRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IMediator _mediator;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ISystemService _systemService;
        private readonly FinishOneHomeWorkPublisher _finishOneHomeWorkPublisher;
        private readonly IQuestionRepository _questionRepository;
        private readonly CreateTokenHistoryPublisher _createTokenHistoryPublisher;
        private readonly ILogger<CreateHomeWorkAnswerCommand> _logger;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly RankedStudentPublisher _rankedStudentPublisher;
        private readonly IHomeWorkQuestionRepository _homeWorkQuestionRepository;
        private readonly IRequestSafeCachingService _requestSafeCachingService;

        public CreateHomeWorkAnswerCommandHandler(
            IHomeWorkResultRepository homeWorkResultRepository,
            ICourseResultRepository courseResultRepository,
            QuestionConverter questionConverter,
            IHomeWorkAnswerRepository homeWorkAnswerRepository,
            IHomeWorkRepository homeWorkRepository,
            IMediator mediator,
            IUserService userService,
            AuthContext authContext,
            ICourseRepository courseRepository,
            ISystemService systemService,
            FinishOneHomeWorkPublisher finishOneHomeWorkPublisher,
            IQuestionRepository questionRepository,
            CreateTokenHistoryPublisher createTokenHistoryPublisher,
            ILogger<CreateHomeWorkAnswerCommand> logger,
            QuestBoardPublisher questionBoardPublisher,
            RankedStudentPublisher rankedStudentPublisher,
            IHomeWorkQuestionRepository homeWorkQuestionRepository,
            IRequestSafeCachingService requestSafeCachingService)
        {
            _homeWorkResultRepository = homeWorkResultRepository;
            _courseResultRepository = courseResultRepository;
            _questionConverter = questionConverter;
            _homeWorkAnswerRepository = homeWorkAnswerRepository;
            _homeWorkRepository = homeWorkRepository;
            _mediator = mediator;
            _userService = userService;
            _authContext = authContext;
            _courseRepository = courseRepository;
            _systemService = systemService;
            _finishOneHomeWorkPublisher = finishOneHomeWorkPublisher;
            _questionRepository = questionRepository;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
            _logger = logger;
            _questBoardPublisher = questionBoardPublisher;
            _rankedStudentPublisher = rankedStudentPublisher;
            _homeWorkQuestionRepository = homeWorkQuestionRepository;
            _requestSafeCachingService = requestSafeCachingService;
        }

        public async Task<MethodResult<HomeWorkModel>> Handle(CreateHomeWorkAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<HomeWorkModel>();

            // Không có answer:
            if (request.Answers == null || !request.Answers.Any())
            {
                if (request.IsSubmit)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Answers));
                }

                return methodResult;
            }

            // Validate tổng + load context (Student, Questions, HomeWorkResult)
            var validationResult = await ValidateRequestAsync(request, cancellationToken);
            if (!validationResult.IsOK)
            {
                methodResult.AddErrorBadRequest(validationResult.ErrorMessages);
                return methodResult;
            }

            var (student, questions, homeWorkResult) = validationResult.Result;

            // Lưu / cập nhật câu trả lời
            var saveResult = await SaveAnswersAsync(homeWorkResult, questions, request, cancellationToken);
            if (!saveResult.IsOK)
            {
                methodResult.AddErrorBadRequest(saveResult.ErrorMessages);
                return methodResult;
            }

            // Cập nhật kết quả bài tập
            var updateResult = await UpdateHomeWorkResultAsync(homeWorkResult, request.IsSubmit, student, cancellationToken);
            if (!updateResult.IsOK)
            {
                methodResult.AddErrorBadRequest(updateResult.ErrorMessages);
                return methodResult;
            }

            // Trả lại kết quả homework
            methodResult = await _mediator.Send(
                new GetHomeWorkQuery
                {
                    HomeWorkId = homeWorkResult.HomeWorkId,
                    LessonResultId = homeWorkResult.LessonResultId,
                    IsShowSubStatus = request.IsSubmit
                },
                cancellationToken);

            return methodResult;
        }

        #region Validation & Context

        private async Task<MethodResult<(StudentModel Student, IList<Question> Questions, HomeWorkResult HomeWorkResult)>> ValidateRequestAsync(
            CreateHomeWorkAnswerCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<(StudentModel, IList<Question>, HomeWorkResult)>();

            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }

            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var homeWorkResult = await _homeWorkResultRepository.Queryable
                .Include(x => x.LessonResult)
                .FirstOrDefaultAsync(x => x.Id == request.HomeWorkResultId, cancellationToken);

            if (homeWorkResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkResult));
                return methodResult;
            }

            if (homeWorkResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusDone));
                return methodResult;
            }

            if (homeWorkResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusUnfinished));
                return methodResult;
            }

            var groupedQuestions = request.Answers
                .Select(x => x.QuestionId)
                .GroupBy(x => x)
                .Select(x => new
                {
                    QuestionId = x.Key,
                    TotalQuestion = x.Count()
                })
                .ToList();

            if (groupedQuestions.Any(x => x.TotalQuestion > 1))
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionsDuplicate), nameof(groupedQuestions));
                return methodResult;
            }

            var questionIds = groupedQuestions.Select(x => x.QuestionId).Distinct().ToList();

            var homeWork = await _homeWorkRepository.GetByIdAsync(homeWorkResult.HomeWorkId);
            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWork));
                return methodResult;
            }

            var questions = await _questionRepository.GetIncludeByHomeWorkAsync(questionIds);
            if (questions == null || !questions.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                return methodResult;
            }

            var homeWorkIds = questions
                .SelectMany(x => x.HomeWorkQuestions)
                .Select(x => x.HomeWorkId)
                .Distinct()
                .ToList();

            if (!homeWorkIds.Any(x => x == homeWork.Id))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(questions), nameof(homeWork));
                return methodResult;
            }

            methodResult.Result = (student, questions, homeWorkResult);
            return methodResult;
        }

        #endregion Validation & Context

        #region Save Answers

        private async Task<MethodResult<bool>> SaveAnswersAsync(
            HomeWorkResult homeWorkResult,
            IList<Question> questions,
            CreateHomeWorkAnswerCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            ArgumentNullException.ThrowIfNull(homeWorkResult);
            ArgumentNullException.ThrowIfNull(questions);

            var methodResult = new MethodResult<bool>();

            var isFirstSubmit = homeWorkResult.SubmissionCount == EnumSubmissionCount.FirstSubmit;
            var isSecondSubmit = homeWorkResult.SubmissionCount == EnumSubmissionCount.SecondSubmit;

            var newAnswers = new List<HomeWorkAnswer>();
            var updatedAnswers = new List<HomeWorkAnswer>();

            var existingAnswers = await _homeWorkAnswerRepository.Queryable
                .Where(x => x.HomeWorkResultId == homeWorkResult.Id)
                .Where(x => x.CreatedDate >= homeWorkResult.CreatedDate)
                .ToListAsync(cancellationToken);

            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                var homeWorkQuestion = question?.HomeWorkQuestions.FirstOrDefault();

                if (homeWorkQuestion == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkQuestion));
                    return methodResult;
                }

                var existingAnswer = existingAnswers.FirstOrDefault(x =>
                    x.HomeWorkQuestionId == homeWorkQuestion.Id &&
                    x.HomeWorkResultId == homeWorkResult.Id);

                var questionResult = _questionConverter.HandleQuestionAnswer(
                    question,
                    item.Answer,
                    request.IsSubmit,
                    existingAnswer?.Answer,
                    isSecondSubmit,
                    request.IsSubmit);

                if (!questionResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                    return methodResult;
                }

                var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;

                if (existingAnswer == null)
                {
                    var newAnswer = new HomeWorkAnswer
                    {
                        HomeWorkQuestionId = homeWorkQuestion.Id,
                        HomeWorkResultId = homeWorkResult.Id
                    };

                    newAnswer = BuildHomeWorkAnswer(
                        newAnswer,
                        answerConfig,
                        isAnswered,
                        correctCount,
                        questionItem.CorrectTotal,
                        isFirstSubmit);

                    if (!newAnswer.IsValid())
                    {
                        methodResult.AddErrorBadRequest(newAnswer.ErrorMessages);
                        return methodResult;
                    }

                    newAnswers.Add(newAnswer);
                }
                else if (existingAnswer.Status != EnumAnswerStatus.Done)
                {
                    existingAnswer = BuildHomeWorkAnswer(
                        existingAnswer,
                        answerConfig,
                        isAnswered,
                        correctCount,
                        questionItem.CorrectTotal,
                        isFirstSubmit);

                    if (!existingAnswer.IsValid())
                    {
                        methodResult.AddErrorBadRequest(existingAnswer.ErrorMessages);
                        return methodResult;
                    }

                    updatedAnswers.Add(existingAnswer);
                }
            }

            try
            {
                if (newAnswers.Any())
                {
                    await _requestSafeCachingService.SafeRequest<List<HomeWorkAnswer>>(
                        key: $"Add_HomeWorkAnswers_{string.Join("_", newAnswers.Select(na => $"{na.HomeWorkQuestionId}_{na.HomeWorkResultId}_{na.IsDeleted}"))}",
                        safeFunction: async () =>
                        {
                            await _homeWorkAnswerRepository.BulkMergeAsync(newAnswers, bulk =>
                            {
                                bulk.ColumnPrimaryKeyExpression = entity => new
                                {
                                    entity.HomeWorkQuestionId,
                                    entity.HomeWorkResultId,
                                    entity.IsDeleted
                                };
                            });
                            return newAnswers;
                        });
                }

                if (updatedAnswers.Any())
                {
                    await _homeWorkAnswerRepository.BulkUpdateList(updatedAnswers, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = entity => new
                        {
                            entity.HomeWorkResultId,
                            entity.HomeWorkQuestionId
                        };
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Log Duplicate HomeWorkAnswer : {ex.Message}");
            }

            methodResult.Result = true;
            return methodResult;
        }

        private static HomeWorkAnswer BuildHomeWorkAnswer(
            HomeWorkAnswer homeWorkAnswer,
            object? answerConfig,
            bool isAnswered,
            short correctCount,
            int correctTotal,
            bool isFirstSubmit)
        {
            homeWorkAnswer.Status = EnumAnswerStatus.Process;
            homeWorkAnswer.Answer = answerConfig;
            homeWorkAnswer.CorrectCount = correctCount;
            homeWorkAnswer.IsCorrect = isAnswered ? correctCount == correctTotal : null;
            homeWorkAnswer.IsFirstSubmit = isFirstSubmit;

            return homeWorkAnswer;
        }

        #endregion Save Answers

        #region Update HomeWorkResult

        private async Task<MethodResult<bool>> UpdateHomeWorkResultAsync(
            HomeWorkResult homeWorkResult,
            bool isSubmit,
            StudentModel student,
            CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<bool>();

            if (homeWorkResult.LessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkResult.LessonResult));
                return methodResult;
            }

            var course = await _courseRepository.GetByIdAsync(homeWorkResult.LessonResult.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            var statistics = await GetHomeWorkStatisticsAsync(homeWorkResult.Id, cancellationToken);
            if (statistics == null)
            {
                return methodResult;
            }

            if (homeWorkResult.Status == EnumResultStatus.New)
            {
                homeWorkResult.ProcessDate = DateTime.UtcNow;
                homeWorkResult.Status = EnumResultStatus.Process;
                if (!isSubmit)
                {
                    await _homeWorkResultRepository.BulkUpdateList(new List<HomeWorkResult> { homeWorkResult }, bulk =>
                    {
                        bulk.ColumnInputExpression = c => new { c.ProcessDate, c.Status };
                    });
                }
            }

            if (isSubmit)
            {
                var tokenUnitValue = await GetTokenBaseValueAsync(homeWorkResult.SubmissionCount, course.CourseType);

                var isHomeWorkDone = statistics.CorrectCount == statistics.CorrectTotal || homeWorkResult.SubmissionCount == EnumSubmissionCount.SecondSubmit;
                if (statistics.TotalAnswer > statistics.TotalQuestion)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAnswerErrorCode.DuplicateAnswers));
                    return methodResult;
                }

                if (statistics.TotalAnswer < statistics.TotalQuestion)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAnswerErrorCode.NotAnsweredEnough));
                    return methodResult;
                }

                var totalCorrect = await UpdateHomeWorkAnswersAsync(homeWorkResult, isHomeWorkDone);
                var tokensAchieved = totalCorrect * tokenUnitValue;

                // Tính HighestStreak mỗi lần submit
                homeWorkResult.HighestStreak = await CalculateHighestCorrectStreakAsync(homeWorkResult.Id, cancellationToken);
                if (tokensAchieved > 0)
                {
                    await PublishTokenHistoryAsync(homeWorkResult, student, course, tokensAchieved, cancellationToken);
                }
                await UpdateHomeWorkResultStatisticsAsync(homeWorkResult, statistics, isHomeWorkDone, (int)tokensAchieved);

                await _homeWorkResultRepository.BulkUpdateList(new List<HomeWorkResult> { homeWorkResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.LessonResultId, c.StudentId, c.HomeWorkId };
                });
                if (homeWorkResult.Status == EnumResultStatus.Done)
                {
                    await _homeWorkResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    await PublishStudentRankingAsync(homeWorkResult.CreatedUserId, cancellationToken).ConfigureAwait(false);
                }
            }

            methodResult.Result = true;
            return methodResult;
        }

        private sealed class HomeWorkStatistics
        {
            public Guid? SkillId { get; init; }
            public string? SkillName { get; init; }
            public string? SkillFilePath { get; init; }
            public EnumCourseSkill CourseSkill { get; init; }
            public int CorrectQuestion { get; init; }
            public int CorrectCount { get; init; }
            public int CorrectTotal { get; init; }
            public int TotalQuestion { get; init; }
            public int TotalAnswer { get; init; }
        }

        private async Task<HomeWorkStatistics?> GetHomeWorkStatisticsAsync(Guid homeWorkResultId, CancellationToken cancellationToken)
        {
            return await _homeWorkResultRepository.ReadQueryable
                .Where(x => x.Id == homeWorkResultId)
                .Select(x => new HomeWorkStatistics
                {
                    SkillId = x.HomeWork != null ? x.HomeWork.SkillId : null,
                    SkillName = x.HomeWork != null && x.HomeWork.Skill != null ? x.HomeWork.Skill.Name : string.Empty,
                    SkillFilePath = x.HomeWork != null && x.HomeWork.Skill != null ? x.HomeWork.Skill.FilePath : string.Empty,
                    CourseSkill = x.HomeWork!.CourseSkill,
                    CorrectQuestion = x.HomeWorkAnswers.Count(a => a.IsCorrect == true),
                    CorrectCount = x.HomeWorkAnswers.Sum(a => a.CorrectCount),
                    CorrectTotal = x.HomeWork.HomeWorkQuestions
                        .Select(hq => hq.Question)
                        .Sum(q => q!.CorrectTotal),
                    TotalQuestion = x.HomeWork.HomeWorkQuestions.Count,
                    TotalAnswer = x.HomeWorkAnswers.Count
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<long> GetTokenBaseValueAsync(EnumSubmissionCount? submissionCount, EnumCourseType courseType)
        {
            var tokenConfigs = await _systemService.GetTokenConfigAsync(new GetTokenQueryModel
            {
                Feature = EnumTokenFeature.Learn,
                Mission = submissionCount == EnumSubmissionCount.FirstSubmit
                    ? EnumTokenMission.HomeworkFirstSubmit
                    : EnumTokenMission.HomeworkSecondSubmit,
                CourseType = courseType
            });

            if (!tokenConfigs.IsSuccessStatusCode)
            {
                return default;
            }

            var tokenConfig = tokenConfigs.Content?.Result;
            return tokenConfig.GetTokenConfig<TokenCoinConfigs>()?.BaseValue ?? default;
        }

        private async Task PublishTokenHistoryAsync(
            HomeWorkResult homeWorkResult,
            StudentModel student,
            Course course,
            long tokensAchieved,
            CancellationToken cancellationToken)
        {
            var courseResultId = await _courseResultRepository.ReadQueryable
                                                              .Where(x => x.CourseId == course.Id && x.StudentId == student.Id)
                                                              .Where(x => x.WorkingStatus == EnumWorkingStatus.Active)
                                                              .Select(x => x.Id)
                                                              .FirstOrDefaultAsync(cancellationToken);

            var tokenHistories = new List<TokenHistoryQueueModel>
            {
                new TokenHistoryQueueModel
                {
                    ObjectId = homeWorkResult.Id,
                    VolatileToken = tokensAchieved,
                    CourseResultId = courseResultId,
                    Feature = EnumTokenFeature.Learn,
                    Mission = homeWorkResult.SubmissionCount == EnumSubmissionCount.FirstSubmit
                        ? EnumTokenMission.HomeworkFirstSubmit
                        : EnumTokenMission.HomeworkSecondSubmit,
                    Type = EnumTokenHistoryType.Recevived,
                    UserId = student?.UserId ?? default,
                }
            };

            await _createTokenHistoryPublisher.Publish(tokenHistories, cancellationToken).ConfigureAwait(false);
        }

        private async Task UpdateHomeWorkResultStatisticsAsync(
            HomeWorkResult homeWorkResult,
            HomeWorkStatistics statistics,
            bool isHomeWorkDone,
            int tokensAchieved)
        {
            homeWorkResult.CorrectCount = statistics.CorrectCount;
            homeWorkResult.CorrectTotal = statistics.CorrectTotal;
            homeWorkResult.Percent = NumberHelper.GetPercent(statistics.CorrectCount, statistics.CorrectTotal);

            if (homeWorkResult.SubmissionCount == EnumSubmissionCount.FirstSubmit)
            {
                homeWorkResult.TokenFirstTime = tokensAchieved;
            }
            else
            {
                homeWorkResult.TokenLastTime = tokensAchieved;
            }

            if (isHomeWorkDone)
            {
                homeWorkResult.CompletionDate = DateTime.UtcNow;
                homeWorkResult.Status = EnumResultStatus.Done;

                await _finishOneHomeWorkPublisher.Publish(homeWorkResult, CancellationToken.None);

                await PublishQuestBoardProgressAsync(homeWorkResult.StudentId, EnumQuestBoardType.BeginnerQuests, EnumQuestBoardCategory.CompleteHomeworkFirst, CancellationToken.None);
                await PublishQuestBoardProgressAsync(homeWorkResult.StudentId, EnumQuestBoardType.LearningQuests, EnumQuestBoardCategory.TheMysteryOfTheStars, CancellationToken.None);

                if (homeWorkResult.Percent > 50)
                {
                    await PublishQuestBoardProgressAsync(homeWorkResult.StudentId, EnumQuestBoardType.LearningQuests, EnumQuestBoardCategory.ConqueringAsteroids, CancellationToken.None);
                }
            }
            else
            {
                homeWorkResult.SubmissionCount = EnumSubmissionCount.SecondSubmit;
            }

            var skillScores = new SkillScores
            {
                CorrectCount = statistics.CorrectCount,
                TotalCount = statistics.CorrectTotal,
                CountQuestion = statistics.TotalAnswer,
                TotalQuestion = statistics.TotalQuestion,
                SkillFilePath = statistics.SkillFilePath,
                SkillName = statistics.SkillName,
                SkillId = statistics.SkillId,
                CorrectQuestion = statistics.CorrectQuestion,
            };

            homeWorkResult.SkillScores = new List<SkillScores> { skillScores };
        }

        #endregion Update HomeWorkResult

        #region HomeWorkAnswer Finalization & Streak

        private async Task<long> UpdateHomeWorkAnswersAsync(HomeWorkResult homeWorkResult, bool isDone = false)
        {
            ArgumentNullException.ThrowIfNull(homeWorkResult);

            var answers = await _homeWorkAnswerRepository.Queryable
                .Include(x => x.HomeWorkQuestion)
                .ThenInclude(x => x!.Question)
                .Where(x => x.CreatedDate >= homeWorkResult.CreatedDate)
                .Where(x => x.HomeWorkResultId == homeWorkResult.Id && x.Status == EnumAnswerStatus.Process)
                .ToListAsync();

            if (answers != null && answers.Any())
            {
                answers.ForEach(x =>
                {
                    var correctTotal = x.HomeWorkQuestion?.Question?.CorrectTotal ?? default;
                    x.Status = (x.CorrectCount == correctTotal || isDone) ? EnumAnswerStatus.Done : EnumAnswerStatus.Process;
                    x.IsCorrect = x.IsCorrect.HasValue ? x.CorrectCount == correctTotal : null;
                });

                await _homeWorkAnswerRepository.BulkUpdateList(answers, bulk =>
                {
                    bulk.ColumnInputExpression = entity => new
                    {
                        entity.Status,
                        entity.IsCorrect
                    };
                });

                return answers.Where(x => x.Status == EnumAnswerStatus.Done).Sum(x => x.CorrectCount);
            }

            return answers?.Sum(x => x.CorrectCount) ?? default;
        }

        private async Task<int> CalculateHighestCorrectStreakAsync(Guid homeWorkResultId, CancellationToken cancellationToken)
        {
            var answers = await (from baseQ in _homeWorkAnswerRepository.ReadQueryable
                                 join hq in _homeWorkQuestionRepository.ReadQueryable on baseQ.HomeWorkQuestionId equals hq.Id
                                 where baseQ.HomeWorkResultId == homeWorkResultId && baseQ.Status == EnumAnswerStatus.Done
                                 orderby hq.CreatedDate
                                 select baseQ.IsCorrect == true && baseQ.IsFirstSubmit).ToListAsync(cancellationToken);
            if (!answers.Any())
            {
                return 0;
            }
            return answers.GetHighestStreak();
        }

        #endregion HomeWorkAnswer Finalization & Streak

        #region Quest & Ranking

        private async Task PublishQuestBoardProgressAsync(
            Guid studentId,
            EnumQuestBoardType type,
            EnumQuestBoardCategory category,
            CancellationToken cancellationToken)
        {
            await _questBoardPublisher.Publish(new QuestBoardQueueModel
            {
                StudentID = studentId,
                Type = type,
                Category = category,
                Value = 1
            }, cancellationToken);
        }

        private async Task PublishStudentRankingAsync(Guid userId, CancellationToken cancellationToken)
        {
            var baseQueue = new StudentRankingEventModel { UserId = userId };
            await _rankedStudentPublisher.Publish(baseQueue, cancellationToken);
        }

        #endregion Quest & Ranking
    }
}
