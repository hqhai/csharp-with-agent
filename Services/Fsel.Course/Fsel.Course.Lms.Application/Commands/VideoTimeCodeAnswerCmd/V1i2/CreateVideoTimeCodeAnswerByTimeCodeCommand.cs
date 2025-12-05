// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd.V1i2
{
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading;
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
    using Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd.V1i1;
    using Fsel.Course.Lms.Application.Queries.VideoQuery.V1i2;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.Models;
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
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly VideoConverter _videoConverter;
        private readonly ISystemService _systemService;
        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;
        private readonly ICourseRepository _courseRepository;
        private readonly CreateTokenHistoryPublisher _createTokenHistoryPublisher;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly RankedStudentPublisher _rankedStudentPublisher;
        private readonly IVideoService _videoService;

        public CreateVideoTimeCodeAnswerByTimeCodeCommandHandler(QuestBoardPublisher questBoardPublisher,
            ICourseResultRepository courseResultRepository,
            IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository,
            DisconnectSocketCalculateTimePublisher disconnectSocketCalculateTimePublisher,
            IVideoResultRepository videoResultRepository,
            IVideoTimeCodeResultRepository videoTimeCodeResultRepository,
            IVideoTimeCodeRepository videoTimeCodeRepository,
            VideoConverter videoConverter,
            ISystemService systemService,
            AuthContext authContext,
            IMediator mediator,
            ICourseRepository courseRepository,
            CreateTokenHistoryPublisher createTokenHistoryPublisher,
            RankedStudentPublisher rankedStudentPublisher,
            IVideoService videoService)
        {
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _disconnectSocketCalculateTimePublisher = disconnectSocketCalculateTimePublisher;
            _videoResultRepository = videoResultRepository;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _videoConverter = videoConverter;
            _systemService = systemService;
            _authContext = authContext;
            _mediator = mediator;
            _courseRepository = courseRepository;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
            _questBoardPublisher = questBoardPublisher;
            _courseResultRepository = courseResultRepository;
            _rankedStudentPublisher = rankedStudentPublisher;
            _videoService = videoService;
        }

        public async Task<MethodResult<VideoTimeCodeModel>> Handle(CreateVideoTimeCodeAnswerByTimeCodeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VideoTimeCodeModel>();

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

                if (videoTimeCodeResult.Status == EnumResultStatus.New && videoTimeCode.TimeCodeType == EnumTimeCodeType.Standalone)
                {
                    videoResult.HighestStreak = await _videoConverter.GetHighestStreak(videoResult);
                }
                else if (videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone)
                {
                    videoTimeCodeResult.HighestStreak = await _videoConverter.GetHighestStreak(videoTimeCodeResult);
                }
            }

            await UpdateVideoTimeCodeResultAsync(videoTimeCode, videoResult, videoTimeCodeResult, request.IsSubmit, cancellationToken);
            if (request.IsSubmit)
            {
                videoResult.TimeCodeHighestStreak = await GetHighestStreak(videoResult);
            }
            await _videoResultRepository.BulkUpdateList(new List<VideoResult> { videoResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = entity => new { entity.LessonResultId, entity.StudentId, entity.VideoId };
            });

            if (videoTimeCodeResult.Status == EnumResultStatus.Done)
            {
                await UpdateVideoResultAsync(videoResult, cancellationToken);

                #region Do QuestBoard

                var countAnswers = await _videoTimeCodeAnswerRepository.ReadQueryable.Where(p => p.VideoTimeCodeResultId == videoTimeCodeResult.Id).CountAsync(cancellationToken);

                await DoQuestBoard(videoResult.StudentId, EnumQuestBoardCategory.DecodingTheNebula, countAnswers, cancellationToken);
                await DoQuestBoard(videoResult.StudentId, EnumQuestBoardCategory.JourneyOfKnowledge, countAnswers, cancellationToken);
                await PublishRankedStudent(videoResult.CreatedUserId, cancellationToken);

                #endregion Do QuestBoard
            }

            var videoTimeCodeMethod = await _mediator.Send(new GetTimeCodeDetailQuery
            {
                VideoTimeCodeId = request.VideoTimeCodeId,
                VideoResultId = request.VideoResultId,
                IsShowSubStatus = videoTimeCodeResult.Status == EnumResultStatus.Process && request.IsSubmit,
                IsCreateAnswer = true
            }, cancellationToken);

            methodResult.Result = videoTimeCodeMethod.Result;
            return methodResult;
        }

        private async Task<int> GetHighestStreak(VideoResult videoResult)
        {
            var videoTimeCodeResults = await _videoTimeCodeResultRepository.ReadQueryable.Where(x => x.VideoResultId == videoResult.Id && x.Status == EnumResultStatus.Done)
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

        private async Task<MethodResult<(VideoResult, VideoTimeCode, VideoTimeCodeResult)>> HandleAnswerAsync(CreateVideoTimeCodeAnswerByTimeCodeCommand request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<(VideoResult, VideoTimeCode, VideoTimeCodeResult)>();
            var method = await Validate(request, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            var (videoResult, videoTimeCode, videoTimeCodeResult) = method.Result;
            if (request.Answers != null && request.Answers.Any())
            {
                var methodCreateAnswer = await _videoService.CreateAnswers(request, videoTimeCode, videoTimeCodeResult, cancellationToken);
                if (!methodCreateAnswer.IsOK)
                {
                    methodResult.AddErrorBadRequest(methodCreateAnswer.ErrorMessages);
                    return methodResult;
                }
            }
            methodResult.Result = (videoResult, videoTimeCode, videoTimeCodeResult);
            return methodResult;
        }

        private async Task UpdateVideoResultAsync(VideoResult videoResult, CancellationToken cancellationToken)
        {
            var videoTimeCodeCount = await _videoTimeCodeRepository.ReadQueryable.Where(x => x.VideoId == videoResult.VideoId).CountAsync(cancellationToken);
            var videoTimeCodeResultCount = await _videoTimeCodeResultRepository.ReadQueryable.Where(x => x.VideoResultId == videoResult.Id && x.Status == EnumResultStatus.Done).CountAsync(cancellationToken);
            if (videoTimeCodeCount == videoTimeCodeResultCount)
            {
                await _mediator.Send(new VideoResultCmd.V1i2.ReviewLessonVideoCommand { VideoResultId = videoResult.Id }, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task UpdateVideoTimeCodeResultAsync(VideoTimeCode videoTimeCode, VideoResult videoResult, VideoTimeCodeResult videoTimeCodeResult, bool isSubmit, CancellationToken cancellationToken)
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

            var courseResultId = await _courseResultRepository.ReadQueryable.Where(x => x.CourseId == videoResult.LessonResult.CourseId && x.StudentId == videoResult.StudentId)
                                                              .Select(x => x.Id)
                                                              .FirstOrDefaultAsync(cancellationToken);

            var shouldForceDoneAnswers = videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone || videoTimeCodeResult.Status == EnumResultStatus.Process;
            if (isSubmit)
            {
                await _mediator.Send(new UpdateVideoTimeCodeAnswersCommand { VideoTimeCodeResultId = videoTimeCodeResult.Id }, cancellationToken).ConfigureAwait(false);

                var correctCount = await _videoService.UpdateVideoAnswers(videoTimeCode, videoTimeCodeResult, shouldForceDoneAnswers, isSubmit, cancellationToken);
                videoTimeCodeResult = await GetTokenVideoTimeCodeResult(videoTimeCodeResult, videoTimeCode, course.CourseType, correctCount);
                await SendTokenHistoryAsync(videoTimeCodeResult, videoTimeCode, courseResultId, cancellationToken).ConfigureAwait(false);

                var (skillScoreUngradeds, skillScores, isDone) = await _videoService.GetSkillScoresAsync(videoTimeCodeResult, videoTimeCode, cancellationToken);
                skillScores ??= new List<SkillScores>();
                skillScoreUngradeds ??= new List<SkillScores>();

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
            else
            {
                await HandleNonSubmitAsync(videoTimeCode, videoTimeCodeResult);
            }
        }

        private async Task HandleNonSubmitAsync(VideoTimeCode videoTimeCode, VideoTimeCodeResult videoTimeCodeResult)
        {
            if (videoTimeCode.TimeCodeType == EnumTimeCodeType.Standalone || videoTimeCodeResult.Status != EnumResultStatus.New)
            {
                return;
            }

            videoTimeCodeResult.Status = EnumResultStatus.Process;
            videoTimeCodeResult.IsWorking = false;
            await _videoTimeCodeResultRepository.BulkUpdateList(new List<VideoTimeCodeResult> { videoTimeCodeResult }, bulk =>
            {
                bulk.ColumnInputExpression = entity => new { entity.Status, entity.IsWorking };
            });
        }

        public async Task<MethodResult<(VideoResult, VideoTimeCode, VideoTimeCodeResult)>> Validate(CreateVideoTimeCodeAnswerByTimeCodeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<(VideoResult, VideoTimeCode, VideoTimeCodeResult)>();
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
            var videoTimeCodeResult = await _videoTimeCodeResultRepository.Queryable.FirstOrDefaultAsync(x => x.VideoResultId == videoResult.Id && x.VideoTimeCodeId == videoTimeCode.Id, cancellationToken);
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

            methodResult.Result = (videoResult, videoTimeCode, videoTimeCodeResult);
            return methodResult;
        }

        #region Publish Quest Board and Ranked Student

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

        private async Task PublishRankedStudent(Guid userId, CancellationToken cancellationToken)
        {
            StudentRankingEventModel baseQueue = new StudentRankingEventModel { UserId = userId };
            await _rankedStudentPublisher.Publish(baseQueue, cancellationToken);
        }

        #endregion Publish Quest Board and Ranked Student

        #region Send Token History

        private async Task SendTokenHistoryAsync(VideoTimeCodeResult videoTimeCodeResult, VideoTimeCode videoTimeCode, Guid? courseResultId, CancellationToken cancellationToken)
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
                    UserId = _authContext.CurrentUserId,
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

        #endregion Send Token History
    }
}
