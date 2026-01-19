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
    using Fsel.Course.Domain.Entities.V1i1;
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

            var timeCodeResult = await ValidateResult(request, cancellationToken);
            if (!timeCodeResult.IsOK)
            {
                methodResult.AddErrorBadRequest(timeCodeResult.ErrorMessages);
                return methodResult;
            }
            var videoTimeCodeResultContext = timeCodeResult.Result!;

            if (request.IsSubmit)
            {
                await ProcessSubmitAsync(request, videoTimeCodeResultContext, cancellationToken);
            }
            else
            {
                await ProcessNonSubmitAsync(request, videoTimeCodeResultContext, cancellationToken);
            }
            var videoTimeCodeResult = videoTimeCodeResultContext.VideoTimeCodeResult;

            if (videoTimeCodeResult.Status == EnumResultStatus.Done)
            {
                await UpdateVideoResultAsync(videoTimeCodeResultContext.VideoResult, cancellationToken);

                #region Do QuestBoard

                var countAnswers = await _videoTimeCodeAnswerRepository.ReadQueryable.Where(p => p.VideoTimeCodeResultId == videoTimeCodeResult.Id).CountAsync(cancellationToken);

                await DoQuestBoard(videoTimeCodeResultContext.VideoResult.StudentId, EnumQuestBoardCategory.DecodingTheNebula, countAnswers, cancellationToken);
                await DoQuestBoard(videoTimeCodeResultContext.VideoResult.StudentId, EnumQuestBoardCategory.JourneyOfKnowledge, countAnswers, cancellationToken);
                await PublishRankedStudent(videoTimeCodeResultContext.VideoResult.CreatedUserId, cancellationToken);

                #endregion Do QuestBoard
            }

            return await _mediator.Send(new GetTimeCodeDetailQuery
            {
                VideoTimeCodeId = request.VideoTimeCodeId,
                VideoResultId = request.VideoResultId,
                IsShowSubStatus = videoTimeCodeResult.Status == EnumResultStatus.Process && request.IsSubmit,
                IsCreateAnswer = true
            }, cancellationToken);
        }

        private async Task<VoidMethodResult> ProcessSubmitAsync(CreateVideoTimeCodeAnswerByTimeCodeCommand request, VideoTimeCodeContext ctx, CancellationToken ct)
        {
            var methodResult = new VoidMethodResult();
            await NotifyDisconnectAsync(ctx, ct);

            var createAnswer = await _videoService.CreateAnswers(request, ctx.VideoTimeCodeResult, ct);
            if (!createAnswer.IsOK)
            {
                methodResult.AddErrorBadRequest(createAnswer.ErrorMessages);
                return methodResult;
            }

            ctx.VideoTimeCodeResult.HighestStreak = await _videoConverter.GetHighestStreak(ctx.VideoTimeCodeResult);

            await UpdateVideoTimeCodeResultAsync(ctx, request.IsTimeUp, ct);
            return methodResult;
        }

        private async Task<VoidMethodResult> ProcessNonSubmitAsync(CreateVideoTimeCodeAnswerByTimeCodeCommand request, VideoTimeCodeContext ctx, CancellationToken ct)
        {
            var methodResult = new VoidMethodResult();
            var createAnswer = await _videoService.CreateAnswers(request, ctx.VideoTimeCodeResult, ct);
            if (!createAnswer.IsOK)
            {
                methodResult.AddErrorBadRequest(createAnswer.ErrorMessages);
                return methodResult;
            }

            if (ctx.VideoTimeCode.TimeCodeType == EnumTimeCodeType.Standalone || ctx.VideoTimeCodeResult.Status != EnumResultStatus.New)
            {
                return methodResult;
            }

            ctx.VideoTimeCodeResult.Status = EnumResultStatus.Process;
            ctx.VideoTimeCodeResult.IsWorking = false;

            await _videoTimeCodeResultRepository.BulkUpdateList(new[] { ctx.VideoTimeCodeResult },
            bulk => bulk.ColumnInputExpression = e => new
            {
                e.Status,
                e.IsWorking
            });
            return methodResult;
        }

        private async Task<MethodResult<VideoTimeCodeContext>> ValidateResult(CreateVideoTimeCodeAnswerByTimeCodeCommand request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<VideoTimeCodeContext>();
            var videoTimeCodeResult = await _videoTimeCodeResultRepository.Queryable.Include(x => x.VideoTimeCode)
                                                                          .Where(x => x.VideoResultId == request.VideoResultId)
                                                                          .Where(x => x.VideoTimeCodeId == request.VideoTimeCodeId)
                                                                          .FirstOrDefaultAsync(cancellationToken);

            if (videoTimeCodeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCodeResult));
                return methodResult;
            }

            if (videoTimeCodeResult.VideoTimeCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCodeResult.VideoTimeCode));
                return methodResult;
            }

            if (videoTimeCodeResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusDone), nameof(videoTimeCodeResult));
                return methodResult;
            }
            var videoResult = await _videoResultRepository.Queryable.Include(x => x.LessonResult)
                                                          .FirstOrDefaultAsync(x => x.Id == request.VideoResultId, cancellationToken);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }

            if (videoResult.LessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult.LessonResult));
                return methodResult;
            }

            var course = await _courseRepository.GetByIdAsync(videoResult.LessonResult.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            var courseResult = await _courseResultRepository.ReadQueryable.Where(x => x.Id == videoResult.LessonResult.CourseResultId)
                                                            .FirstOrDefaultAsync(cancellationToken);
            if (courseResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseResult));
                return methodResult;
            }
            methodResult.Result = new VideoTimeCodeContext
            {
                VideoResult = videoResult,
                VideoTimeCodeResult = videoTimeCodeResult,
                VideoTimeCode = videoTimeCodeResult.VideoTimeCode,
                Course = course,
                CourseResult = courseResult,
                LessonResult = videoResult.LessonResult,
            };
            return methodResult;
        }

        private async Task NotifyDisconnectAsync(VideoTimeCodeContext ctx, CancellationToken cancellationToken)
        {
            await _disconnectSocketCalculateTimePublisher.Publish(new SetTimeModuleModel
            {
                Type = nameof(Video),
                ObjectId = ctx.VideoTimeCodeResult.Id,
                SubmissionCount = ctx.VideoTimeCodeResult.Status == EnumResultStatus.New ? EnumSubmissionCount.FirstSubmit : EnumSubmissionCount.SecondSubmit
            }, cancellationToken);
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

        private async Task UpdateVideoTimeCodeResultAsync(VideoTimeCodeContext context, bool isTimeUp, CancellationToken cancellationToken)
        {
            var videoTimeCode = context.VideoTimeCode;
            var videoTimeCodeResult = context.VideoTimeCodeResult;

            var shouldForceDoneAnswers = videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone || videoTimeCodeResult.Status == EnumResultStatus.Process;
            await _mediator.Send(new UpdateVideoTimeCodeAnswersCommand { VideoTimeCodeResultId = videoTimeCodeResult.Id }, cancellationToken).ConfigureAwait(false);

            var correctCount = await _videoService.UpdateVideoAnswers(videoTimeCode, videoTimeCodeResult, shouldForceDoneAnswers, true, cancellationToken);
            videoTimeCodeResult = await GetTokenVideoTimeCodeResult(videoTimeCodeResult, videoTimeCode, context.Course.CourseType, correctCount);
            await SendTokenHistoryAsync(videoTimeCodeResult, videoTimeCode, context.CourseResult.Id, cancellationToken).ConfigureAwait(false);

            var (skillScoreUngradeds, skillScores, isDone) = await _videoService.GetSkillScoresAsync(videoTimeCodeResult, videoTimeCode, isTimeUp, cancellationToken);
            skillScores ??= new List<SkillScores>();
            skillScoreUngradeds ??= new List<SkillScores>();

            if (skillScoreUngradeds != null && skillScoreUngradeds.Any())
            {
                videoTimeCodeResult.CorrectCountUngraded = (int)skillScoreUngradeds.Sum(x => x.CorrectCount);
                videoTimeCodeResult.CorrectTotalUngraded = (int)skillScoreUngradeds.Sum(x => x.TotalCount);
            }
            videoTimeCodeResult.Status = isDone ? EnumResultStatus.Done : EnumResultStatus.Process;
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
