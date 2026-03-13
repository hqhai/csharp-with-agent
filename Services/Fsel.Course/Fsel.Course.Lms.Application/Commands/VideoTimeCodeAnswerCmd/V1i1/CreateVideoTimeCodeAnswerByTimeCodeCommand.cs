// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd.V1i1
{
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.VideoTimeCodeAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Commands.VideoResultCmd;
    using Fsel.Course.Lms.Application.Queries.VideoQuery;
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

    public class CreateVideoTimeCodeAnswerByTimeCodeCommand : CreateVideoTimeCodeAnswerV1i1CommandModel, IRequest<MethodResult<VideoTimeCodeModel>>
    {
    }

    public class CreateVideoTimeCodeAnswerByTimeCodeCommandHandler : IRequestHandler<CreateVideoTimeCodeAnswerByTimeCodeCommand, MethodResult<VideoTimeCodeModel>>
    {
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly DisconnectSocketCalculateTimePublisher _disconnectSocketCalculateTimePublisher;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly VideoConverter _videoConverter;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;
        private readonly ICourseRepository _courseRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly QuestionConverter _questionConverter;
        private readonly CreateTokenHistoryPublisher _createTokenHistoryPublisher;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly RankedStudentPublisher _rankedStudentPublisher;
        private readonly IRequestSafeCachingService _requestSafeCachingService;

        public CreateVideoTimeCodeAnswerByTimeCodeCommandHandler(QuestBoardPublisher questBoardPublisher,
            IMapper mapper,
            ICourseResultRepository courseResultRepository,
            IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository,
            DisconnectSocketCalculateTimePublisher disconnectSocketCalculateTimePublisher,
            IVideoResultRepository videoResultRepository,
            IExerciseRepository exerciseRepository,
            IVideoTimeCodeResultRepository videoTimeCodeResultRepository,
            IVideoTimeCodeRepository videoTimeCodeRepository,
            VideoConverter videoConverter,
            IUserService userService,
            ISystemService systemService,
            AuthContext authContext,
            IMediator mediator,
            ICourseRepository courseRepository,
            IQuestionRepository questionRepository,
            QuestionConverter questionConverter,
            CreateTokenHistoryPublisher createTokenHistoryPublisher,
            RankedStudentPublisher rankedStudentPublisher,
            IRequestSafeCachingService requestSafeCachingService)
        {
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _disconnectSocketCalculateTimePublisher = disconnectSocketCalculateTimePublisher;
            _videoResultRepository = videoResultRepository;
            _exerciseRepository = exerciseRepository;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _videoConverter = videoConverter;
            _userService = userService;
            _systemService = systemService;
            _authContext = authContext;
            _mediator = mediator;
            _courseRepository = courseRepository;
            _questionRepository = questionRepository;
            _questionConverter = questionConverter;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
            _questBoardPublisher = questBoardPublisher;
            _courseResultRepository = courseResultRepository;
            _rankedStudentPublisher = rankedStudentPublisher;
            _requestSafeCachingService = requestSafeCachingService;
        }

        public async Task<MethodResult<VideoTimeCodeModel>> Handle(CreateVideoTimeCodeAnswerByTimeCodeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VideoTimeCodeModel>();

            StudentModel? student;
            if (request.StudentId.HasValue)
            {
                var studentResult = await _userService.GetUserByStudentId(request.StudentId.Value);
                if (!studentResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                    return methodResult;
                }
                student = studentResult.Content?.Result;
            }
            else
            {
                var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
                if (!studentResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                    return methodResult;
                }
                student = studentResult.Content?.Result;
            }
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var method = await HandleAnswerAsync(request, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }

            var (videoResult, videoTimeCode, videoTimeCodeResult) = method.Result;
            if (request.IsSubmit)
            {
                await _disconnectSocketCalculateTimePublisher.Publish(new SetTimeModuleModel
                {
                    Type = nameof(Video),
                    ObjectId = videoTimeCodeResult.Id,
                    SubmissionCount = videoTimeCodeResult.Status == EnumResultStatus.New ? EnumSubmissionCount.FirstSubmit : EnumSubmissionCount.SecondSubmit
                }, cancellationToken);
            }

            if (request.IsSubmit)
            {
                if (videoTimeCodeResult.Status == EnumResultStatus.New && videoTimeCode.TimeCodeType == EnumTimeCodeType.Standalone)
                {
                    var highestStreak = await _videoConverter.GetHighestStreak(videoResult);

                    videoResult.HighestStreak = highestStreak.HighestStreakQuestion;
                    videoResult.HighestStreakSubQuestion = highestStreak.HighestStreakSubQuestion;
                }
                else if (videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone)
                {
                    var highestStreak = await _videoConverter.GetHighestStreak(videoTimeCodeResult);

                    videoTimeCodeResult.HighestStreak = highestStreak.HighestStreakQuestion;
                    videoTimeCodeResult.HighestStreakSubQuestion = highestStreak.HighestStreakSubQuestion;
                }
            }

            await UpdateVideoTimeCodeResultAsync(videoTimeCode, videoResult, request.IsSubmit, student, cancellationToken);
            if (request.IsSubmit)
            {
                if (videoTimeCodeResult.Status == EnumResultStatus.New && videoTimeCode.TimeCodeType == EnumTimeCodeType.Standalone)
                {
                    var highestStreak = await _videoConverter.GetHighestStreak(videoResult);

                    videoResult.HighestStreak = highestStreak.HighestStreakQuestion;
                    videoResult.HighestStreakSubQuestion = highestStreak.HighestStreakSubQuestion;
                }
                else
                {
                    var highestStreak = await _videoConverter.GetHighestStreak(videoTimeCodeResult);

                    videoTimeCodeResult.HighestStreak = highestStreak.HighestStreakQuestion;
                    videoTimeCodeResult.HighestStreakSubQuestion = highestStreak.HighestStreakSubQuestion;
                }
            }
            await _videoResultRepository.BulkUpdateList(new List<VideoResult> { videoResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = entity => new { entity.LessonResultId, entity.StudentId, entity.VideoId };
            });

            if (videoTimeCodeResult.Status == EnumResultStatus.Done)
            {
                await UpdateVideoResultAsync(videoResult, cancellationToken);

                #region Do QuestBoard

                var countAnswers = await _videoTimeCodeAnswerRepository.Queryable.Where(p => p.VideoTimeCodeResultId == videoTimeCodeResult.Id)
                    .Where(x => x.CreatedDate >= videoTimeCodeResult.CreatedDate)
                    .CountAsync(cancellationToken);

                await DoQuestBoard(videoResult.StudentId, EnumQuestBoardCategory.DecodingTheNebula, countAnswers, cancellationToken);
                await DoQuestBoard(videoResult.StudentId, EnumQuestBoardCategory.JourneyOfKnowledge, countAnswers, cancellationToken);
                await PublishRankedStudent(videoResult.CreatedUserId, cancellationToken);

                #endregion Do QuestBoard
            }

            var videoTimeCodeMethod = await _mediator.Send(new GetTimeCodeDetailQuery
            {
                VideoTimeCodeId = request.VideoTimeCodeId,
                VideoResultId = videoResult.Id,
                IsShowSubStatus = videoTimeCodeResult.Status == EnumResultStatus.Process && request.IsSubmit,
                IsCreateAnswer = true
            }, cancellationToken);

            methodResult.Result = videoTimeCodeMethod.Result;
            return methodResult;
        }

        private async Task<int> GetHighestStreak(VideoResult videoResult)
        {
            var videoTimeCodeResults = await _videoTimeCodeResultRepository.Queryable.Where(x => x.VideoResultId == videoResult.Id && x.Status == EnumResultStatus.Done && x.CreatedDate >= videoResult.CreatedDate)
                                                                                     .OrderBy(x => x.CreatedDate).ToListAsync();
            var highestStreak = 0;
            var maxHighestStreak = 0;
            foreach (var videoTimeCodeResult in videoTimeCodeResults)
            {
                if (videoTimeCodeResult.CorrectCount == videoTimeCodeResult.CorrectTotal)
                {
                    highestStreak++;
                    maxHighestStreak = Math.Max(maxHighestStreak, highestStreak);
                }
                else
                {
                    highestStreak = 0;
                }
            }
            return maxHighestStreak;
        }

        private async Task DoQuestBoard(Guid studentId, EnumQuestBoardCategory category, int value, CancellationToken cancellationToken)
        {
            await _questBoardPublisher.Publish(new QuestBoardQueueModel()
            {
                StudentID = studentId,
                Type = EnumQuestBoardType.LearningQuests,
                Category = category,
                Value = value
            }, cancellationToken);
        }

        private async Task<MethodResult<(VideoResult, VideoTimeCode, VideoTimeCodeResult)>> HandleAnswerAsync(CreateVideoTimeCodeAnswerByTimeCodeCommand request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<(VideoResult, VideoTimeCode, VideoTimeCodeResult)>();
            var method = await Validate(request, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            var (videoResult, videoTimeCode, videoTimeCodeResult, questions) = method.Result;
            if (request.Answers != null && request.Answers.Any())
            {
                var methodCreateAnswer = await CreateAnswerAsync(request, questions, videoTimeCode, videoTimeCodeResult, cancellationToken);
                if (!methodCreateAnswer.IsOK)
                {
                    methodResult.AddErrorBadRequest(methodCreateAnswer.ErrorMessages);
                    return methodResult;
                }
            }
            methodResult.Result = (videoResult, videoTimeCode, videoTimeCodeResult);
            return methodResult;
        }

        private async Task<MethodResult<bool>> CreateAnswerAsync(CreateVideoTimeCodeAnswerByTimeCodeCommand request, IList<Question> questions, VideoTimeCode videoTimeCode, VideoTimeCodeResult videoTimeCodeResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(questions);
            ArgumentNullException.ThrowIfNull(request.Answers);
            var methodResult = new MethodResult<bool>();
            var videoTimeCodeAnswers = new List<VideoTimeCodeAnswer>();
            var updateVideoTimeCodeAnswers = new List<VideoTimeCodeAnswer>();

            var answers = await _videoTimeCodeAnswerRepository.Queryable.WhereBulkContains(request.Answers.Select(x => x.QuestionId), x => x.QuestionId)
                                                              .Where(x => x.CreatedDate >= videoTimeCodeResult.CreatedDate)
                                                              .Where(x => x.VideoResultId == videoTimeCodeResult.VideoResultId && x.VideoTimeCodeId == request.VideoTimeCodeId)
                                                              .ToListAsync(cancellationToken);
            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                var exercise = question?.ExerciseQuestions.Select(x => x.Exercise).FirstOrDefault();
                var answer = answers.FirstOrDefault(x => x.QuestionId == item.QuestionId && x.VideoResultId == videoTimeCodeResult.VideoResultId);
                var questionResult = _questionConverter.HandleQuestionAnswer(question, item.Answer, request.IsSubmit, answer?.Answer, videoTimeCodeResult.Status == EnumResultStatus.Process, false);
                if (!questionResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                    return methodResult;
                }
                var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;
                var isFirstSubmit = videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone || videoTimeCodeResult.Status == EnumResultStatus.New;
                if (answer == null)
                {
                    answer = new VideoTimeCodeAnswer
                    {
                        VideoTimeCodeId = videoTimeCode.Id,
                        ExerciseId = exercise?.Id ?? default,
                        QuestionId = questionItem.Id,
                        VideoTimeCodeResultId = videoTimeCodeResult.Id,
                        VideoResultId = videoTimeCodeResult.VideoResultId,
                    };
                    answer = GetVideoTimeCodeAnswer(answer, questionItem, correctCount, answerConfig ?? item.Answer, isFirstSubmit, isAnswered);
                    if (!answer.IsValid())
                    {
                        methodResult.AddErrorBadRequest(answer.ErrorMessages);
                        return methodResult;
                    }
                    videoTimeCodeAnswers.Add(answer);
                }
                else if (answer.Status != EnumAnswerStatus.Done)
                {
                    answer = GetVideoTimeCodeAnswer(answer, questionItem, correctCount, answerConfig ?? item.Answer, isFirstSubmit, isAnswered);
                    if (!answer.IsValid())
                    {
                        methodResult.AddErrorBadRequest(answer.ErrorMessages);
                        return methodResult;
                    }
                    updateVideoTimeCodeAnswers.Add(answer);
                }
            }

            try
            {
                if (videoTimeCodeAnswers.Any())
                {
                    await _requestSafeCachingService.SafeRequest<List<VideoTimeCodeAnswer>>(
                    key: $"Add_VideoTimeCodeAnswers_{string.Join('_', videoTimeCodeAnswers.Select(x => $"{x.VideoTimeCodeResultId}_{x.QuestionId}_{x.IsDeleted}"))}",
                        safeFunction: async () =>
                        {
                            await _videoTimeCodeAnswerRepository.BulkMergeAsync(videoTimeCodeAnswers, bulk =>
                            {
                                bulk.ColumnPrimaryKeyExpression = entity => new { entity.VideoTimeCodeResultId, entity.QuestionId, entity.IsDeleted };
                            });
                            return videoTimeCodeAnswers;
                        });
                }
                else if (updateVideoTimeCodeAnswers.Any())
                {
                    await _videoTimeCodeAnswerRepository.BulkUpdateList(updateVideoTimeCodeAnswers, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = entity => new { entity.VideoResultId, entity.VideoTimeCodeResultId, entity.QuestionId };
                    });
                }
            }
            catch
            {
            }

            return methodResult;
        }

        private async Task UpdateVideoResultAsync(VideoResult videoResult, CancellationToken cancellationToken)
        {
            var videoTimeCodeCount = await _videoTimeCodeRepository.Queryable.Where(x => x.VideoId == videoResult.VideoId).CountAsync(cancellationToken);
            var videoTimeCodeResultCount = await _videoTimeCodeResultRepository.Queryable.Where(x => x.VideoResultId == videoResult.Id && x.Status == EnumResultStatus.Done && x.CreatedDate >= videoResult.CreatedDate).CountAsync(cancellationToken);
            if (videoTimeCodeCount == videoTimeCodeResultCount)
            {
                await _mediator.Send(new ReviewLessonVideoCommand { LessonResultId = videoResult.LessonResultId }, cancellationToken).ConfigureAwait(false);
            }
        }

        private static VideoTimeCodeAnswer GetVideoTimeCodeAnswer(VideoTimeCodeAnswer answer, Question question, short correctCount, object? answerConfig, bool isFirstSubmit, bool isAnswered)
        {
            answer.Answer = answerConfig;
            answer.CorrectCount = correctCount;
            answer.Status = EnumAnswerStatus.Process;
            answer.IsCorrect = isAnswered ? correctCount == question.CorrectTotal : null;
            answer.IsFirstSubmit = isFirstSubmit;
            return answer;
        }

        private async Task UpdateVideoTimeCodeResultAsync(VideoTimeCode videoTimeCode, VideoResult videoResult, bool isSubmit, StudentModel student, CancellationToken cancellationToken)
        {
            if (videoResult.LessonResult == null)
            {
                return;
            }
            var course = await _courseRepository.GetByIdAsync(videoResult.LessonResult.CourseId);
            if (course == null)
            {
                return;
            }
            var videoTimeCodeResult = await _videoTimeCodeResultRepository.Queryable.FirstOrDefaultAsync(x => x.VideoResultId == videoResult.Id && x.VideoTimeCodeId == videoTimeCode.Id && x.CreatedDate >= videoResult.CreatedDate, cancellationToken);
            if (videoTimeCodeResult == null)
            {
                return;
            }
            var courseResultId = await _courseResultRepository.Queryable.Where(x => x.CourseId == videoResult.LessonResult.CourseId && x.StudentId == student.Id)
                .Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);

            var isDoneTimeCode = videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone || videoTimeCodeResult.Status == EnumResultStatus.Process;
            if (isSubmit)
            {
                await _mediator.Send(new UpdateVideoTimeCodeAnswersCommand { VideoTimeCodeResultId = videoTimeCodeResult.Id }, cancellationToken).ConfigureAwait(false);

                var correctCount = await _videoConverter.UpdateVideoAnswers(videoTimeCode, videoTimeCodeResult, isDoneTimeCode);
                videoTimeCodeResult = await GetTokenVideoTimeCodeResult(videoTimeCodeResult, videoTimeCode, course.CourseType, correctCount);
                await SendTokenHistoryAsync(videoTimeCodeResult, videoTimeCode, courseResultId, student, cancellationToken).ConfigureAwait(false);
                var (skillScoreUngradeds, skillScores, isDone) = await GetSkillScoresAsync(videoTimeCodeResult, cancellationToken);
                if (skillScoreUngradeds != null && skillScoreUngradeds.Any())
                {
                    videoTimeCodeResult.CorrectCountUngraded = (int)skillScoreUngradeds.Sum(x => x.CorrectCount);
                    videoTimeCodeResult.CorrectTotalUngraded = (int)skillScoreUngradeds.Sum(x => x.TotalCount);
                }

                videoTimeCodeResult.Status = isDone ? EnumResultStatus.Process : EnumResultStatus.Done;
                videoTimeCodeResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
                videoTimeCodeResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
                videoTimeCodeResult.SkillScores = skillScores;
                videoTimeCodeResult.SkillScoreUngraded = skillScoreUngradeds;
                videoTimeCodeResult.IsWorking = false;

                await _videoTimeCodeResultRepository.BulkUpdateList(new List<VideoTimeCodeResult> { videoTimeCodeResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = entity => new { entity.RetryWorkingTime, entity.WorkingTime, entity.VideoResultId, entity.VideoTimeCodeId };
                });
            }
            else if (videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone && videoTimeCodeResult.Status == EnumResultStatus.New)
            {
                videoTimeCodeResult.Status = EnumResultStatus.Process;
                videoTimeCodeResult.IsWorking = false;
                await _videoTimeCodeResultRepository.BulkUpdateList(new List<VideoTimeCodeResult> { videoTimeCodeResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = entity => new { entity.RetryWorkingTime, entity.WorkingTime, entity.VideoResultId, entity.VideoTimeCodeId };
                });
            }
        }

        private async Task SendTokenHistoryAsync(VideoTimeCodeResult videoTimeCodeResult, VideoTimeCode videoTimeCode, Guid? courseResultId, StudentModel student, CancellationToken cancellationToken)
        {
            if (videoTimeCodeResult.Status == EnumResultStatus.Done)
            {
                return;
            }
            var tokensAchieved = (double)(videoTimeCodeResult.TokenLastTime.HasValue ? videoTimeCodeResult.TokenLastTime.Value : (videoTimeCodeResult.TokenFirstTime ?? default));
            if (tokensAchieved <= 0)
            {
                return;
            }

            var feature = videoTimeCode.TimeCodeType == EnumTimeCodeType.Standalone ? EnumTokenFeature.Learn : EnumTokenFeature.Test;
            var mission = videoTimeCode.TimeCodeType == EnumTimeCodeType.Standalone ? videoTimeCodeResult.Status == EnumResultStatus.New ? EnumTokenMission.TimeCodeFirstSubmit : EnumTokenMission.TimeCodeSecondSubmit :
                          videoTimeCode.TimeCodeType == EnumTimeCodeType.UnitTest ? EnumTokenMission.UnitTest : EnumTokenMission.SkillTest;
            var listToken = new List<TokenHistoryQueueModel>
            {
                new TokenHistoryQueueModel
                {
                    ObjectId = videoTimeCodeResult.Id,
                    VolatileToken = tokensAchieved,
                    Feature = feature,
                    CourseResultId =  courseResultId,
                    Mission = mission,
                    Type = EnumTokenHistoryType.Recevived,
                    UserId = student?.UserId ?? default,
                }
            };

            await _createTokenHistoryPublisher.Publish(listToken, cancellationToken).ConfigureAwait(false);
        }

        private async Task<VideoTimeCodeResult> GetTokenVideoTimeCodeResult(VideoTimeCodeResult videoTimeCodeResult, VideoTimeCode videoTimeCode, EnumCourseType courseType, long correctCount)
        {
            var token = await GetTokenConfig(videoTimeCode, videoTimeCodeResult, courseType);
            if (videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone || videoTimeCodeResult.Status == EnumResultStatus.New)
            {
                videoTimeCodeResult.TokenFirstTime = (int)(token * correctCount);
            }
            else
            {
                videoTimeCodeResult.TokenLastTime = (int)(token * correctCount);
            }
            return videoTimeCodeResult;
        }

        private async Task<long> GetTokenConfig(VideoTimeCode videoTimeCode, VideoTimeCodeResult videoTimeCodeResult, EnumCourseType courseType)
        {
            var getTokenQuery = new GetTokenQueryModel
            {
                CourseType = courseType
            };

            if (videoTimeCode.TimeCodeType == EnumTimeCodeType.Standalone)
            {
                getTokenQuery.Feature = EnumTokenFeature.Learn;
                getTokenQuery.Mission = videoTimeCodeResult.Status == EnumResultStatus.New ? EnumTokenMission.TimeCodeFirstSubmit : EnumTokenMission.TimeCodeSecondSubmit;
            }
            else
            {
                getTokenQuery.Feature = EnumTokenFeature.Test;
                getTokenQuery.Mission = videoTimeCode.TimeCodeType == EnumTimeCodeType.UnitTest ? EnumTokenMission.UnitTest : EnumTokenMission.SkillTest;
            }
            var tokenConfigResults = await _systemService.GetTokenConfigAsync(getTokenQuery);
            if (!tokenConfigResults.IsSuccessStatusCode)
            {
                return default;
            }
            var tokenConfig = tokenConfigResults?.Content?.Result;
            return tokenConfig.GetTokenConfig<TokenCoinConfigs>()?.BaseValue ?? default;
        }

        public async Task<MethodResult<(VideoResult, VideoTimeCode, VideoTimeCodeResult, IList<Question>)>> Validate(CreateVideoTimeCodeAnswerByTimeCodeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<(VideoResult, VideoTimeCode, VideoTimeCodeResult, IList<Question>)>();
            var videoResult = await _videoResultRepository.Queryable.Include(x => x.LessonResult).FirstOrDefaultAsync(x => x.Id == request.VideoResultId, cancellationToken);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }
            var videoTimeCode = await _videoTimeCodeRepository.GetByIdAsync(request.VideoTimeCodeId);
            if (videoTimeCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.VideoTimeCodeId));
                return methodResult;
            }
            var videoTimeCodeResult = await _videoTimeCodeResultRepository.Queryable.FirstOrDefaultAsync(x => x.VideoResultId == videoResult.Id && x.CreatedDate >= videoResult.CreatedDate && x.VideoTimeCodeId == videoTimeCode.Id, cancellationToken);
            if (videoTimeCodeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCodeResult));
                return methodResult;
            }
            if (videoTimeCodeResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusDone), nameof(videoTimeCodeResult));
                return methodResult;
            }

            var questionIds = request.Answers.Select(x => x.QuestionId).ToList();
            var questions = new List<Question>();
            if (questionIds.Any())
            {
                questions = await _questionRepository.GetListAsync(questionIds);
                if (questions == null || !questions.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                    return methodResult;
                }
                var videoTimeCodeIds = questions.SelectMany(x => x.ExerciseQuestions.Select(x => x.Exercise)).SelectMany(x => x!.TimeCodeExercises.Select(x => x.VideoTimeCodeId).Distinct()).ToList();
                if (!videoTimeCodeIds.Any(x => x == videoTimeCode.Id))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(questions), nameof(request.VideoTimeCodeId));
                    return methodResult;
                }
            }

            methodResult.Result = (videoResult, videoTimeCode, videoTimeCodeResult, questions);
            return methodResult;
        }

        private async Task<(IList<SkillScores> Ungraded, IList<SkillScores> Graded, bool HasNotDone)>
        GetSkillScoresAsync(
        VideoTimeCodeResult videoTimeCodeResult,
        CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(videoTimeCodeResult);

            var exerciseIds = await _videoTimeCodeAnswerRepository.Queryable
                .Where(x => x.VideoTimeCodeResultId == videoTimeCodeResult.Id)
                .Where(x => x.CreatedDate >= videoTimeCodeResult.CreatedDate)
                .Select(x => x.ExerciseId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var exercises = await _exerciseRepository.Queryable
                .Include(x => x.Skill)
                .Include(x => x.ExerciseQuestions)
                    .ThenInclude(eq => eq.Question)
                        .ThenInclude(q => q!.VideoTimeCodeAnswers
                            .Where(a => a.VideoTimeCodeResultId == videoTimeCodeResult.Id))
                .Where(x => exerciseIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            var hasNotDone = exercises
                .SelectMany(x => x.ExerciseQuestions)
                .Select(x => x.Question)
                .Where(q => q != null)
                .SelectMany(q => q!.VideoTimeCodeAnswers)
                .Any(a => a.Status != EnumAnswerStatus.Done);

            var groupedBySkill = exercises
                .GroupBy(x => new
                {
                    x.CourseSkill,
                    x.Skill
                });

            var ungradedScores = new List<SkillScores>();
            var gradedScores = new List<SkillScores>();

            foreach (var group in groupedBySkill)
            {
                var questions = group
                    .SelectMany(x => x.ExerciseQuestions)
                    .Select(x => x.Question)
                    .Where(q => q != null)
                    .Cast<Question>()
                    .ToList();

                var ungraded = BuildSkillScores(
                    questions.Where(q => q.Ungraded).ToList(),
                    group.Key.CourseSkill,
                    group.Key.Skill);

                if (ungraded != null)
                {
                    ungradedScores.Add(ungraded);
                }

                var graded = BuildSkillScores(
                    questions.Where(q => !q.Ungraded).ToList(),
                    group.Key.CourseSkill,
                    group.Key.Skill);

                if (graded != null)
                {
                    gradedScores.Add(graded);
                }
            }

            return (ungradedScores, gradedScores, hasNotDone);
        }

        private static SkillScores? BuildSkillScores(
        IList<Question> questions,
        EnumCourseSkill courseSkill,
        Skill? skill)
        {
            if (questions == null || !questions.Any())
            {
                return null;
            }

            var answers = questions
                .SelectMany(q => q.VideoTimeCodeAnswers)
                .ToList();

            var correctCount = answers.Sum(x => x.CorrectCount);

            return new SkillScores
            {
                Skill = courseSkill,

                SkillId = skill?.Id,
                SkillName = skill?.Name,
                SkillFilePath = skill?.FilePath,

                CorrectCount = correctCount,
                CorrectQuestion = answers.Count(x => x.IsCorrect == true),
                CountQuestion = answers.Count,

                TotalCount = questions.Sum(x => x.CorrectTotal),
                TotalQuestion = questions.Count,

                Scores = correctCount.GetIeltsScore(courseSkill),
                TokenReceived = answers.Sum(x => x.TokenReceived)
            };
        }

        private async Task PublishRankedStudent(Guid userId, CancellationToken cancellationToken)
        {
            StudentRankingEventModel baseQueue = new StudentRankingEventModel { UserId = userId };
            await _rankedStudentPublisher.Publish(baseQueue, cancellationToken);
        }
    }
}
