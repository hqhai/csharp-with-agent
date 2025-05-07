// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class CreateVideoTimeCodeResultCommand : IRequest<MethodResult<VideoTimeCodeResultModel>>
    {
        public Guid VideoResultId { get; set; }
        public Guid StudentId { get; set; }
        public Guid VideoTimeCodeId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreateVideoTimeCodeResultCommandHandler : IRequestHandler<CreateVideoTimeCodeResultCommand, MethodResult<VideoTimeCodeResultModel>>
    {
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IMapper _mapper;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly ILogger<CreateVideoTimeCodeResultCommand> _logger;
        private readonly TechieActionPublisher _techieActionPublisher;

        public CreateVideoTimeCodeResultCommandHandler(IVideoResultRepository videoResultRepository, IVideoTimeCodeRepository videoTimeCodeRepository, IMapper mapper, IVideoTimeCodeResultRepository videoTimeCodeResultRepository, ILogger<CreateVideoTimeCodeResultCommand> logger, TechieActionPublisher techieActionPublisher)
        {
            _videoResultRepository = videoResultRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _mapper = mapper;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _logger = logger;
            _techieActionPublisher = techieActionPublisher;
        }

        public async Task<MethodResult<VideoTimeCodeResultModel>> Handle(CreateVideoTimeCodeResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoTimeCodeResultModel> methodResult = new MethodResult<VideoTimeCodeResultModel>();

            var videoResult = await _videoResultRepository.GetByIdAsync(request.VideoResultId);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }
            var videoTimeCode = await _videoTimeCodeRepository.GetByIdAsync(request.VideoTimeCodeId);
            if (videoTimeCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCode));
                return methodResult;
            }
            methodResult.Result = _mapper.Map<VideoTimeCodeResultModel>(await GetAndUpdateVideoTimeCodeResultAsync(request, videoTimeCode, videoResult));
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<VideoTimeCodeResult> GetAndUpdateVideoTimeCodeResultAsync(CreateVideoTimeCodeResultCommand request, VideoTimeCode videoTimeCode, VideoResult videoResult)
        {
            var videoTimeCodeResult = await _videoTimeCodeResultRepository.Queryable.Where(x => x.VideoTimeCodeId == request.VideoTimeCodeId && x.VideoResultId == request.VideoResultId && x.CreatedDate >= videoResult.CreatedDate).FirstOrDefaultAsync();
            if (videoTimeCodeResult == null)
            {
                _logger.LoggerRequest(request);
                videoTimeCodeResult = new VideoTimeCodeResult
                {
                    VideoResultId = request.VideoResultId,
                    StudentId = request.StudentId,
                    IsWorking = true,
                    Status = EnumResultStatus.New,
                    WorkingTime = default,
                    VideoTimeCodeId = request.VideoTimeCodeId,
                };

                videoTimeCodeResult = _videoTimeCodeResultRepository.Add(videoTimeCodeResult);
                try
                {
                    await _videoTimeCodeResultRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Log Duplicate LessonResult : {ex.Message}");
                }
                await UpdateVideoResult(videoResult, request.VideoTimeCodeId).ConfigureAwait(false);
            }

            return videoTimeCodeResult;
        }

        private async Task UpdateVideoResult(VideoResult videoResult, Guid videoTimeCodeId)
        {
            videoResult.CurrentVideoTimeCodeId = videoTimeCodeId;
            await _videoResultRepository.BulkUpdateList(new List<VideoResult> { videoResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = entity => new { entity.LessonResultId, entity.StudentId, entity.VideoId };
            });
        }
    }
}
