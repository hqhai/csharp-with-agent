// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoResultCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;

    public class UpdatePlaybackSpeedCommand : IRequest<MethodResult<VideoResultModel>>
    {
        public Guid VideoResultId { get; set; }
        public EnumPlaybackSpeed PlaybackSpeed { get; set; }
    }

    public class UpdatePlaybackSpeedCommandHandler : IRequestHandler<UpdatePlaybackSpeedCommand, MethodResult<VideoResultModel>>
    {
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IMapper _mapper;

        public UpdatePlaybackSpeedCommandHandler(IVideoResultRepository videoResultRepository, IMapper mapper)
        {
            _videoResultRepository = videoResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<VideoResultModel>> Handle(UpdatePlaybackSpeedCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoResultModel> methodResult = new MethodResult<VideoResultModel>();

            var videoResult = await _videoResultRepository.GetByIdAsync(request.VideoResultId);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }
            videoResult.PlaybackSpeed = request.PlaybackSpeed;
            await _videoResultRepository.BulkUpdateList(new List<VideoResult> { videoResult }, bulk =>
            {
                bulk.ColumnInputExpression = entity => new { entity.PlaybackSpeed };
            });
            methodResult.Result = _mapper.Map<VideoResultModel>(videoResult);
            return methodResult;
        }
    }
}
