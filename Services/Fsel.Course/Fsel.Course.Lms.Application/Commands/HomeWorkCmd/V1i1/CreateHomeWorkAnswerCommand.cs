// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.HomeWorkCmd.V1i1
{
    using System.Linq;
    using System.Threading;
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
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
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
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ISystemService _systemService;
        private readonly FinishOneHomeWorkPublisher _finishOneHomeWorkPublisher;
        private readonly IQuestionRepository _questionRepository;
        private readonly CreateTokenHistoryPublisher _createTokenHistoryPublisher;
        private readonly ILogger<CreateHomeWorkAnswerCommand> _logger;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly RankedStudentPublisher _rankedStudentPublisher;

        public CreateHomeWorkAnswerCommandHandler(IHomeWorkResultRepository homeWorkResultRepository,
            ICourseResultRepository courseResultRepository,
            QuestionConverter questionConverter,
            IHomeWorkAnswerRepository homeWorkAnswerRepository,
            IHomeWorkRepository homeWorkRepository,
            IMediator mediator,
            IUserService userService,
            AuthContext authContext,
            ICourseRepository courseRepository,
            ILessonResultRepository lessonResultRepository,
            ISystemService systemService,
            FinishOneHomeWorkPublisher finishOneHomeWorkPublisher,
            IQuestionRepository questionRepository,
            CreateTokenHistoryPublisher createTokenHistoryPublisher,
            ILogger<CreateHomeWorkAnswerCommand> logger,
            QuestBoardPublisher questionBoardPublisher,
            RankedStudentPublisher rankedStudentPublisher)
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
            _lessonResultRepository = lessonResultRepository;
            _systemService = systemService;
            _finishOneHomeWorkPublisher = finishOneHomeWorkPublisher;
            _questionRepository = questionRepository;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
            _logger = logger;
            _questBoardPublisher = questionBoardPublisher;
            _rankedStudentPublisher = rankedStudentPublisher;
        }

        public async Task<MethodResult<HomeWorkModel>> Handle(CreateHomeWorkAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<HomeWorkModel>();
            _logger.LoggerRequest(request);

            if (request.Answers == null || !request.Answers.Any())
            {
                if (request.IsSubmit)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Answers));
                }
                else
                {
                    methodResult.StatusCode = StatusCodes.Status200OK;
                }
                return methodResult;
            }
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

            var method = await Validate(request);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            var (questions, homeWorkResult) = method.Result;
            var methodSave = await SaveAnswer(homeWorkResult, questions, request, cancellationToken);
            if (!methodSave.IsOK)
            {
                methodResult.AddErrorBadRequest(methodSave.ErrorMessages);
                return methodResult;
            }

            var methodHomeWork = await UpdateHomeWorkResult(homeWorkResult, request.IsSubmit, student, cancellationToken);
            if (!methodHomeWork.IsOK)
            {
                methodResult.AddErrorBadRequest(methodHomeWork.ErrorMessages);
                return methodResult;
            }
            methodResult = await _mediator.Send(new GetHomeWorkQuery { HomeWorkId = homeWorkResult.HomeWorkId, LessonResultId = homeWorkResult.LessonResultId, IsShowSubStatus = request.IsSubmit }, cancellationToken);
            return methodResult;
        }

        private async Task<MethodResult<bool>> SaveAnswer(HomeWorkResult? homeWorkResult, IList<Question>? questions, CreateHomeWorkAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            ArgumentNullException.ThrowIfNull(homeWorkResult);
            ArgumentNullException.ThrowIfNull(questions);
            var methodResult = new MethodResult<bool>();
            var isTryAgain = homeWorkResult.SubmissionCount == EnumSubmissionCount.SecondSubmit;
            var createHomeWorkAnswers = new List<HomeWorkAnswer>();
            var updateHomeWorkAnswers = new List<HomeWorkAnswer>();

            var homeWorkAnswers = await _homeWorkAnswerRepository.Queryable.Where(x => x.HomeWorkResultId == homeWorkResult.Id)
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
                var homeWorkAnswer = homeWorkAnswers.FirstOrDefault(x => x.HomeWorkQuestionId == homeWorkQuestion.Id && x.HomeWorkResultId == homeWorkResult.Id);
                var questionResult = _questionConverter.HandleQuestionAnswer(question, item.Answer, request.IsSubmit, homeWorkAnswer?.Answer, isTryAgain, request.IsSubmit);
                if (!questionResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                    return methodResult;
                }
                var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;
                if (homeWorkAnswer == null)
                {
                    homeWorkAnswer = new HomeWorkAnswer { HomeWorkQuestionId = homeWorkQuestion.Id, HomeWorkResultId = homeWorkResult.Id };
                    homeWorkAnswer = GetHomeWorkAnswer(homeWorkAnswer, answerConfig, isAnswered, correctCount, questionItem.CorrectTotal);
                    if (!homeWorkAnswer.IsValid())
                    {
                        methodResult.AddErrorBadRequest(homeWorkAnswer.ErrorMessages);
                        return methodResult;
                    }
                    createHomeWorkAnswers.Add(homeWorkAnswer);
                }
                else if (homeWorkAnswer.Status != EnumAnswerStatus.Done)
                {
                    homeWorkAnswer = GetHomeWorkAnswer(homeWorkAnswer, answerConfig, isAnswered, correctCount, questionItem.CorrectTotal);
                    if (!homeWorkAnswer.IsValid())
                    {
                        methodResult.AddErrorBadRequest(homeWorkAnswer.ErrorMessages);
                        return methodResult;
                    }

                    updateHomeWorkAnswers.Add(homeWorkAnswer);
                }
            }

            try
            {
                if (createHomeWorkAnswers.Any())
                {
                    await _homeWorkAnswerRepository.BulkMergeAsync(createHomeWorkAnswers, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = entity => new { entity.HomeWorkQuestionId, entity.HomeWorkResultId, entity.IsDeleted };
                    });
                }
                if (updateHomeWorkAnswers.Any())
                {
                    await _homeWorkAnswerRepository.BulkUpdateList(updateHomeWorkAnswers, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = entity => new { entity.HomeWorkResultId, entity.HomeWorkQuestionId };
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

        private async Task<long> GetToken(EnumSubmissionCount? submissionCount, EnumCourseType courseType)
        {
            var tokenConfigs = await _systemService.GetTokenConfigAsync(new GetTokenQueryModel
            {
                Feature = EnumTokenFeature.Learn,
                Mission = submissionCount == EnumSubmissionCount.FirstSubmit ? EnumTokenMission.HomeworkFirstSubmit : EnumTokenMission.HomeworkSecondSubmit,
                CourseType = courseType
            });
            if (!tokenConfigs.IsSuccessStatusCode)
            {
                return default;
            }
            var tokenConfig = tokenConfigs.Content?.Result;
            return tokenConfig.GetTokenConfig<TokenCoinConfigs>()?.BaseValue ?? default;
        }

        private static HomeWorkAnswer GetHomeWorkAnswer(HomeWorkAnswer homeWorkAnswer, object? answerConfig, bool isAnswered, short correctCount, int correctTotal)
        {
            homeWorkAnswer.Status = EnumAnswerStatus.Process;
            homeWorkAnswer.Answer = answerConfig;
            homeWorkAnswer.CorrectCount = correctCount;
            homeWorkAnswer.IsCorrect = isAnswered ? correctCount == correctTotal : null;
            return homeWorkAnswer;
        }

        private async Task<MethodResult<bool>> UpdateHomeWorkResult(HomeWorkResult? homeWorkResult, bool isSubmit, StudentModel student, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(homeWorkResult);
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

            var homeWorkQuestionCount = await _homeWorkResultRepository.Queryable.Where(x => x.Id == homeWorkResult.Id).Select(x => new
            {
                CourseSkill = x.HomeWork!.CourseSkill,
                CourseLevel = x.HomeWork.CourseLevel,
                CorrectCount = x.HomeWorkAnswers.Sum(x => x.CorrectCount),
                CorrectTotal = x.HomeWork.HomeWorkQuestions.Select(x => x.Question).Sum(x => x!.CorrectTotal),
                TotalQuestion = x.HomeWork.HomeWorkQuestions.Count,
                TotalAnswer = x.HomeWorkAnswers.Count,
            }).FirstOrDefaultAsync(cancellationToken);

            if (homeWorkQuestionCount == null)
            {
                return methodResult;
            }

            if (homeWorkResult.Status == EnumResultStatus.New)
            {
                homeWorkResult.Status = EnumResultStatus.Process;
            }
            if (isSubmit)
            {
                var token = await GetToken(homeWorkResult.SubmissionCount, course.CourseType);

                var isHomeWorkDone = homeWorkQuestionCount.CorrectCount == homeWorkQuestionCount.CorrectTotal || homeWorkResult.SubmissionCount == EnumSubmissionCount.SecondSubmit;
                if (homeWorkQuestionCount.TotalAnswer > homeWorkQuestionCount.TotalQuestion)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAnswerErrorCode.DuplicateAnswers));
                    return methodResult;
                }
                if (homeWorkQuestionCount.TotalAnswer < homeWorkQuestionCount.TotalQuestion)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAnswerErrorCode.NotAnsweredEnough));
                    return methodResult;
                }

                var tokensAchieved = await UpdateHomeWorkAnswers(homeWorkResult, isHomeWorkDone) * token;
                if (tokensAchieved > 0)
                {
                    var tokenHistorys = new List<TokenHistoryQueueModel>
                    {
                        new TokenHistoryQueueModel
                        {
                            ObjectId = homeWorkResult.Id,
                            VolatileToken = tokensAchieved,
                            CourseResultId = _courseResultRepository.Queryable.FirstOrDefault(x => x.CourseId == course.Id && x.StudentId == student.Id)?.Id,
                            Feature = EnumTokenFeature.Learn,
                            Mission = homeWorkResult.SubmissionCount == EnumSubmissionCount.FirstSubmit ? EnumTokenMission.HomeworkFirstSubmit : EnumTokenMission.HomeworkSecondSubmit,
                            Type = EnumTokenHistoryType.Recevived,
                            UserId = student.Human?.UserId ?? default,
                        }
                    };
                    await _createTokenHistoryPublisher.Publish(tokenHistorys, cancellationToken).ConfigureAwait(false);
                }
                homeWorkResult = await GetHomeWorkResult(homeWorkResult, homeWorkQuestionCount, isHomeWorkDone, (int)tokensAchieved);
            }
            if (!homeWorkResult.ProcessDate.HasValue)
            {
                homeWorkResult.ProcessDate = DateTime.UtcNow;
            }
            await _homeWorkResultRepository.BulkUpdateList(new List<HomeWorkResult> { homeWorkResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.LessonResultId, c.StudentId, c.HomeWorkId };
            });
            await _homeWorkResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            await PublishRankedStudent(homeWorkResult.CreatedUserId, cancellationToken).ConfigureAwait(false);
            methodResult.Result = true;
            return methodResult;
        }

        private async Task<HomeWorkResult> GetHomeWorkResult(HomeWorkResult homeWorkResult, dynamic homeWorkQuestionCount, bool isHomeWorkDone, int tokensAchieved)
        {
            homeWorkResult.CorrectCount = homeWorkQuestionCount.CorrectCount;
            homeWorkResult.CorrectTotal = homeWorkQuestionCount.CorrectTotal;
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

                #region Do QuestBoard

                await DoQuestBoard(homeWorkResult.StudentId, EnumQuestBoardType.BeginnerQuests, EnumQuestBoardCategory.CompleteHomeworkFirst, CancellationToken.None);
                await DoQuestBoard(homeWorkResult.StudentId, EnumQuestBoardType.LearningQuests, EnumQuestBoardCategory.TheMysteryOfTheStars, CancellationToken.None);
                if (homeWorkResult.Percent > 50)
                {
                    await DoQuestBoard(homeWorkResult.StudentId, EnumQuestBoardType.LearningQuests, EnumQuestBoardCategory.ConqueringAsteroids, CancellationToken.None);
                }

                #endregion Do QuestBoard
            }
            else
            {
                homeWorkResult.SubmissionCount = EnumSubmissionCount.SecondSubmit;
            }
            var skillScores = new SkillScores
            {
                Skill = homeWorkQuestionCount.CourseSkill,
                CorrectCount = homeWorkQuestionCount.CorrectCount,
                TotalCount = homeWorkQuestionCount.CorrectTotal,
                CountQuestion = homeWorkQuestionCount.TotalAnswer,
                TotalQuestion = homeWorkQuestionCount.TotalQuestion,
            };
            homeWorkResult.SkillScores = new List<SkillScores> { skillScores };

            return homeWorkResult;
        }

        private async Task DoQuestBoard(Guid studentId, EnumQuestBoardType type, EnumQuestBoardCategory category, CancellationToken cancellationToken)
        {
            await _questBoardPublisher.Publish(new QuestBoardQueueModel()
            {
                StudentID = studentId,
                Type = type,
                Category = category,
                Value = 1
            }, cancellationToken);
        }

        public async Task<MethodResult<(IList<Question>, HomeWorkResult)>> Validate(CreateHomeWorkAnswerCommand request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<(IList<Question>, HomeWorkResult)>();

            var homeWorkResult = await _homeWorkResultRepository.Queryable.Include(x => x.LessonResult).FirstOrDefaultAsync(x => x.Id == request.HomeWorkResultId);
            if (homeWorkResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkResult));
                return methodResult;
            }
            else if (homeWorkResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusDone));
                return methodResult;
            }
            else if (homeWorkResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusUnfinished));
                return methodResult;
            }
            var listQuestion = request.Answers.Select(x => x.QuestionId).GroupBy(x => x).Select(x => new
            {
                QuestionId = x.Key,
                TotalQuestion = x.Count()
            }).ToList();

            if (listQuestion.Any(x => x.TotalQuestion > 1))
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionsDuplicate), nameof(listQuestion));
                return methodResult;
            }

            var questionIds = listQuestion.Select(x => x.QuestionId).Distinct().ToList();
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
            var homeWorkIds = questions.SelectMany(x => x.HomeWorkQuestions).Select(x => x.HomeWorkId).Distinct().ToList();
            if (!homeWorkIds.Any(x => x == homeWork.Id))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(questions), nameof(homeWork));
                return methodResult;
            }

            methodResult.Result = (questions, homeWorkResult);
            return methodResult;
        }

        public async Task<long> UpdateHomeWorkAnswers(HomeWorkResult? homeWorkResult, bool isDone = false)
        {
            ArgumentNullException.ThrowIfNull(homeWorkResult);
            var homeWorkAnswers = await _homeWorkAnswerRepository.Queryable.Include(x => x.HomeWorkQuestion).ThenInclude(x => x!.Question)
                                                                 .Where(x => x.CreatedDate >= homeWorkResult.CreatedDate)
                                                                 .Where(x => x.HomeWorkResultId == homeWorkResult.Id && x.Status == EnumAnswerStatus.Process).ToListAsync();
            if (homeWorkAnswers != null && homeWorkAnswers.Any())
            {
                homeWorkAnswers.ForEach(x =>
                {
                    var correctTotal = x.HomeWorkQuestion?.Question?.CorrectTotal ?? default;
                    x.Status = (x.CorrectCount == correctTotal || isDone) ? EnumAnswerStatus.Done : EnumAnswerStatus.Process;
                    x.IsCorrect = x.IsCorrect.HasValue ? x.CorrectCount == correctTotal : null;
                });
                await _homeWorkAnswerRepository.BulkUpdateList(homeWorkAnswers, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = entity => new { entity.HomeWorkResultId, entity.HomeWorkQuestionId };
                });
                return homeWorkAnswers.Where(x => x.Status == EnumAnswerStatus.Done).Sum(x => x.CorrectCount);
            }
            return homeWorkAnswers?.Sum(x => x.CorrectCount) ?? default;
        }

        private async Task PublishRankedStudent(Guid userId, CancellationToken cancellationToken)
        {
            StudentRankingEventModel baseQueue = new StudentRankingEventModel { UserId = userId };
            await _rankedStudentPublisher.Publish(baseQueue, cancellationToken);
        }
    }
}
