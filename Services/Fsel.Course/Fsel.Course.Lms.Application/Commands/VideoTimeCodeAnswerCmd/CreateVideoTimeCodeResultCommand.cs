// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateVideoTimeCodeResultCommand : IRequest<MethodResult<VideoTimeCodeResultModel>>
    {
        public Guid VideoResultId { get; set; }
        public Guid StudentId { get; set; }
        public Guid VideoTimeCodeId { get; set; }
    }

    public class CreateVideoTimeCodeResultCommandHandler : IRequestHandler<CreateVideoTimeCodeResultCommand, MethodResult<VideoTimeCodeResultModel>>
    {
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IMapper _mapper;
        private readonly GetTimeToCompleteTestPublisher _getTimeToCompleteTestPublisher;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;

        public CreateVideoTimeCodeResultCommandHandler(
            IVideoResultRepository videoResultRepository
            , IVideoTimeCodeRepository videoTimeCodeRepository
            , IMapper mapper
            , GetTimeToCompleteTestPublisher getTimeToCompleteTestPublisher
            , IVideoTimeCodeResultRepository videoTimeCodeResultRepository)
        {
            _videoResultRepository = videoResultRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _mapper = mapper;
            _getTimeToCompleteTestPublisher = getTimeToCompleteTestPublisher;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
        }

        public async Task<MethodResult<VideoTimeCodeResultModel>> Handle(CreateVideoTimeCodeResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoTimeCodeResultModel> methodResult = new MethodResult<VideoTimeCodeResultModel>();
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.VideoResultId, cancellationToken);
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
            methodResult.Result = _mapper.Map<VideoTimeCodeResultModel>(await GetAndUpdateVideoTimeCodeResultAsync(request, videoTimeCode));
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<VideoTimeCodeResult> GetAndUpdateVideoTimeCodeResultAsync(CreateVideoTimeCodeResultCommand request, VideoTimeCode videoTimeCode)
        {
            var videoTimeCodeResult = await _videoTimeCodeResultRepository.Queryable.Where(x => x.VideoTimeCodeId == request.VideoTimeCodeId && x.VideoResultId == request.VideoResultId).FirstOrDefaultAsync();
            if (videoTimeCodeResult == null)
            {
                videoTimeCodeResult = new VideoTimeCodeResult
                {
                    VideoResultId = request.VideoResultId,
                    StudentId = request.StudentId,
                    IsWorking = true,
                    RemainingTime = videoTimeCode.ExecutionTime,
                    VideoTimeCodeId = request.VideoTimeCodeId,
                };
                videoTimeCodeResult = _videoTimeCodeResultRepository.Add(videoTimeCodeResult);
                await _videoTimeCodeResultRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
                if (videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone)
                {
                    await _getTimeToCompleteTestPublisher.Publish(new SetTimeToCompleteTestModel
                    {
                        ExecutionTime = videoTimeCode.ExecutionTime,
                        ObjectResultId = videoTimeCodeResult.Id,
                        ObjectResultType = videoTimeCode.TimeCodeType == EnumTimeCodeType.UnitTest ? nameof(EnumTimeCodeType.UnitTest) : nameof(EnumTimeCodeType.SkillTest)
                    }, CancellationToken.None).ConfigureAwait(false);
                }
            }
            else
            {
                if (videoTimeCodeResult.Status != EnumResultStatus.Done)
                {
                    if (!videoTimeCodeResult.IsWorking)
                    {
                        videoTimeCodeResult.IsWorking = true;
                    }
                    videoTimeCodeResult.RemainingTime = videoTimeCodeResult.RemainingTime - Shared.Helpers.DateTimeHelper.GetWorkingTime(GetDate(videoTimeCodeResult, videoTimeCode.TimeCodeType), DateTime.UtcNow, videoTimeCode.ExecutionTime);
                    videoTimeCodeResult = _videoTimeCodeResultRepository.Update(videoTimeCodeResult);
                    await _videoTimeCodeResultRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
                }
            }

            return videoTimeCodeResult;
        }

        private static DateTime GetDate(VideoTimeCodeResult videoTimeCodeResult, EnumTimeCodeType type)
        {
            if (videoTimeCodeResult.UpdatedDate.HasValue && type == EnumTimeCodeType.Standalone)
            {
                return videoTimeCodeResult.UpdatedDate.Value;
            }
            return videoTimeCodeResult.CreatedDate;
        }
    }
}
