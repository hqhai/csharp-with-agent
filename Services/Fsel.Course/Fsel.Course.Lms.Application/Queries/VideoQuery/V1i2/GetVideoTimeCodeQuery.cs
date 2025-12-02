// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.VideoQuery.V1i2
{
    using AutoMapper;
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Domain.Enums;
    using Domain.IRepositories;
    using Domain.Models.EntityModels;
    using Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Queues.Publishers;
    using Shared.Enums;
    using Shared.Models.ShareModels;

    public class GetVideoTimeCodeQuery : IRequest<MethodResult<VideoModel>>
    {
        public Guid VideoId { get; set; }
        public Guid VideoResultId { get; set; }
    }

    public class GetVideoTimeCodeQueryHandler : IRequestHandler<GetVideoTimeCodeQuery, MethodResult<VideoModel>>
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly VideoConverter _videoConverter;
        private readonly IMapper _mapper;
        private readonly QuestBoardPublisher _questBoardPublisher;

        public GetVideoTimeCodeQueryHandler(IVideoRepository videoRepository,
            IVideoResultRepository videoResultRepository,
            VideoConverter videoConverter,
            IMapper mapper,
            QuestBoardPublisher questBoardPublisher)
        {
            _videoRepository = videoRepository;
            _videoResultRepository = videoResultRepository;
            _videoConverter = videoConverter;
            _mapper = mapper;
            _questBoardPublisher = questBoardPublisher;
        }

        public async Task<MethodResult<VideoModel>> Handle(GetVideoTimeCodeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VideoModel>();

            var videoResult = await _videoResultRepository.ReadQueryable.Where(x => x.Id == request.VideoResultId).FirstOrDefaultAsync(cancellationToken);

            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }

            var video = await _videoRepository.ReadQueryable
                .Where(x => x.Id == request.VideoId)
                .Include(v => v.VideoTimeCodes)
                .FirstOrDefaultAsync(cancellationToken);

            if (video == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(video));
                return methodResult;
            }

            var videoModel = _mapper.Map<VideoModel>(video);
            videoModel.VideoTimeCodes = await _videoConverter.GetTimeCodes(video, videoResult);
            videoModel.VideoResult = _mapper.Map<VideoResultModel>(videoResult);

            methodResult.Result = videoModel;
            methodResult.StatusCode = StatusCodes.Status200OK;

            #region Do QuestBoard

            if (videoResult.Status == EnumResultStatus.Done)
            {
                await DoQuestBoard(videoResult.StudentId, cancellationToken);
            }

            #endregion Do QuestBoard

            return methodResult;
        }

        private async Task DoQuestBoard(Guid studentId, CancellationToken cancellationToken)
        {
            await _questBoardPublisher.Publish(
                new QuestBoardQueueModel { StudentID = studentId, Type = EnumQuestBoardType.LearningQuests, Category = EnumQuestBoardCategory.HistoryOfDiscovery, Value = 1 },
                cancellationToken);
        }
    }
}
