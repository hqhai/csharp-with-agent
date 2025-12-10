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
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateVideoTimeCodeResultCommand : IRequest<MethodResult<VideoTimeCodeResultModel>>
    {
        public Guid VideoResultId { get; set; }
        public Guid VideoTimeCodeId { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreateVideoTimeCodeResultCommandHandler : IRequestHandler<CreateVideoTimeCodeResultCommand, MethodResult<VideoTimeCodeResultModel>>
    {
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IMapper _mapper;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;

        public CreateVideoTimeCodeResultCommandHandler(IVideoResultRepository videoResultRepository,
            IVideoTimeCodeRepository videoTimeCodeRepository,
            IMapper mapper,
            IVideoTimeCodeResultRepository videoTimeCodeResultRepository)
        {
            _videoResultRepository = videoResultRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _mapper = mapper;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
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
            methodResult.Result = _mapper.Map<VideoTimeCodeResultModel>(await GetAndUpdateVideoTimeCodeResultAsync(videoTimeCode, videoResult));
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<VideoTimeCodeResult> GetAndUpdateVideoTimeCodeResultAsync(VideoTimeCode videoTimeCode, VideoResult videoResult)
        {
            var videoTimeCodeResult = await _videoTimeCodeResultRepository.Queryable
                                                .Where(x => x.VideoTimeCodeId == videoTimeCode.Id && x.VideoResultId == videoResult.Id)
                                                .FirstOrDefaultAsync();
            if (videoTimeCodeResult == null)
            {
                videoTimeCodeResult = new VideoTimeCodeResult
                {
                    VideoResultId = videoResult.Id,
                    StudentId = videoResult.StudentId,
                    IsWorking = true,
                    Status = EnumResultStatus.New,
                    WorkingTime = default,
                    VideoTimeCodeId = videoTimeCode.Id,
                };

                try
                {
                    await _videoTimeCodeResultRepository.BulkMergeAsync(new List<VideoTimeCodeResult> { videoTimeCodeResult }, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = entity => new { entity.VideoResultId, entity.VideoTimeCodeId, entity.IsDeleted };
                    });
                }
                catch
                {
                }
                await UpdateVideoResult(videoResult, videoTimeCode.Id).ConfigureAwait(false);
            }

            return videoTimeCodeResult;
        }

        private async Task UpdateVideoResult(VideoResult videoResult, Guid videoTimeCodeId)
        {
            videoResult.CurrentVideoTimeCodeId = videoTimeCodeId;
            await _videoResultRepository.BulkUpdateList(new List<VideoResult> { videoResult }, bulk =>
            {
                bulk.ColumnInputExpression = entity => new { entity.CurrentVideoTimeCodeId };
            });
        }
    }
}
