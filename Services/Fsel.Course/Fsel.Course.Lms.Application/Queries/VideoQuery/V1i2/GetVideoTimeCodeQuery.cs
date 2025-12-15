// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.VideoQuery.V1i2
{
    using System.Threading;
    using AutoMapper;
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Domain.Enums;
    using Domain.IRepositories;
    using Domain.Models.EntityModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Queues.Publishers;
    using Shared.Enums;
    using Shared.Models.ShareModels;

    public class GetVideoTimeCodeQuery : IRequest<MethodResult<VideoModel>>
    {
        public Guid VideoResultId { get; set; }
    }

    public class GetVideoTimeCodeQueryHandler : IRequestHandler<GetVideoTimeCodeQuery, MethodResult<VideoModel>>
    {
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IMapper _mapper;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly IVideoService _videoService;

        public GetVideoTimeCodeQueryHandler(
            IVideoResultRepository videoResultRepository,
            IMapper mapper,
            QuestBoardPublisher questBoardPublisher,
            IVideoService videoService)
        {
            _videoResultRepository = videoResultRepository;
            _mapper = mapper;
            _questBoardPublisher = questBoardPublisher;
            _videoService = videoService;
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

            var videoModel = await _videoService.GetVideoModelAsync(videoResult, cancellationToken);
            if (videoModel == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(Video));
                return methodResult;
            }

            videoModel.VideoResult = _mapper.Map<VideoResultModel>(videoResult);
            methodResult.Result = videoModel;

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
            await _questBoardPublisher.Publish(new QuestBoardQueueModel
            {
                StudentID = studentId,
                Type = EnumQuestBoardType.LearningQuests,
                Category = EnumQuestBoardCategory.HistoryOfDiscovery,
                Value = 1
            }, cancellationToken);
        }
    }
}
