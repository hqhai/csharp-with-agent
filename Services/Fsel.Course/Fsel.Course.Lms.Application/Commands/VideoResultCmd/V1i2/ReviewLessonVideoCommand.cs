// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoResultCmd.V1i2
{
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ReviewLessonVideoCommand : IRequest<MethodResult<VideoResultModel>>
    {
        public Guid VideoResultId { get; set; }
        public double NumberOfStars { get; set; }
        public string? Feedback { get; set; }
    }

    public class ReviewLessonVideoCommandHandler : IRequestHandler<ReviewLessonVideoCommand, MethodResult<VideoResultModel>>
    {
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IMapper _mapper;
        private readonly VideoConverter _videoConverter;
        private readonly IVideoRepository _videoRepository;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly IVideoCachingService _videoCachingService;

        public ReviewLessonVideoCommandHandler(IVideoResultRepository videoResultRepository,
            IVideoTimeCodeResultRepository videoTimeCodeResultRepository,
            IMapper mapper,
            VideoConverter videoConverter,
            IVideoRepository videoRepository,
            QuestBoardPublisher questBoardPublisher,
            IVideoCachingService videoCachingService)
        {
            _videoResultRepository = videoResultRepository;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _mapper = mapper;
            _videoConverter = videoConverter;
            _videoRepository = videoRepository;
            _questBoardPublisher = questBoardPublisher;
            _videoCachingService = videoCachingService;
        }

        public async Task<MethodResult<VideoResultModel>> Handle(ReviewLessonVideoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoResultModel> methodResult = new MethodResult<VideoResultModel>();

            var videoResult = await _videoResultRepository.Queryable.Include(x => x.LessonResult)
                                                          .FirstOrDefaultAsync(x => x.Id == request.VideoResultId, cancellationToken);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }
            videoResult.NumberOfStars = request.NumberOfStars;
            videoResult.Feedback = request.Feedback;
            if (!videoResult.IsValid())
            {
                methodResult.AddErrorBadRequest(videoResult.ErrorMessages);
                return methodResult;
            }
            var videoTimeCodeResults = await _videoTimeCodeResultRepository.ReadQueryable.Where(x => x.VideoResultId == videoResult.Id).ToListAsync(cancellationToken);
            var isVideoTimeCodeDone = videoTimeCodeResults.Any(x => x.Status != EnumResultStatus.Done);
            if (isVideoTimeCodeDone)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(isVideoTimeCodeDone));
                return methodResult;
            }

            var video = await GetVideoAsync(videoResult.VideoId);
            if (video == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(video));
                return methodResult;
            }

            if (videoTimeCodeResults.Count != video.VideoTimeCodes.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoTimeCodeErrorCode.VideoTimeCodesNotCompleted), nameof(videoTimeCodeResults));
                return methodResult;
            }
            if (videoResult.Status != EnumResultStatus.Done)
            {
                await DoQuestBoard(videoResult.StudentId, cancellationToken);
            }

            videoResult = await GetVideoResult(videoResult, cancellationToken);
            await _videoResultRepository.BulkUpdateList(new List<VideoResult> { videoResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = entity => new { entity.LessonResultId, entity.StudentId, entity.VideoId };
            });
            await _videoResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<VideoResultModel>(videoResult);
            return methodResult;
        }

        private async Task<Video?> GetVideoAsync(Guid id)
        {
            return await _videoCachingService.GetOrSetAsync(id.ToString(), async (ctx, _) =>
            {
                var video = await _videoRepository.ReadQueryable
                                                  .Where(x => x.Id == id)
                                                  .Include(v => v.VideoTimeCodes)
                                                  .FirstOrDefaultAsync(_);

                return video;
            });
        }

        private async Task<VideoResult> GetVideoResult(VideoResult videoResult, CancellationToken cancellationToken)
        {
            var method = await _videoConverter.GetSkillScoreAndTokens(videoResult, cancellationToken);
            var skillScores = method.Item1.FirstOrDefault(x => x.Type == EnumTimeCodeType.Standalone)?.SkillScores;
            if (skillScores != null)
            {
                videoResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
                videoResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
                videoResult.TokenFirstTime = method.Item2;
                videoResult.TokenLastTime = method.Item3;
            }
            videoResult.Status = EnumResultStatus.Done;
            videoResult.VideoSkillScores = method.Item1;
            return videoResult;
        }

        private async Task DoQuestBoard(Guid studentId, CancellationToken cancellationToken)
        {
            await _questBoardPublisher.Publish(new QuestBoardQueueModel()
            {
                StudentID = studentId,
                Type = EnumQuestBoardType.BeginnerQuests,
                Category = EnumQuestBoardCategory.CompleteTheFirstVideoLesson,
                Value = 1
            }, cancellationToken);
        }
    }
}
