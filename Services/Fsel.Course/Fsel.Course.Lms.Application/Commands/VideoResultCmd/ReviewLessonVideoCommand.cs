// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoResultCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.VideoResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ReviewLessonVideoCommand : ReviewLessonVideoCommandModel, IRequest<MethodResult<VideoResultModel>>
    {
    }

    public class ReviewLessonVideoCommandHandler : IRequestHandler<ReviewLessonVideoCommand, MethodResult<VideoResultModel>>
    {
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IMapper _mapper;

        public ReviewLessonVideoCommandHandler(IVideoResultRepository videoResultRepository, IMapper mapper)
        {
            _videoResultRepository = videoResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<VideoResultModel>> Handle(ReviewLessonVideoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoResultModel> methodResult = new MethodResult<VideoResultModel>();

            var videoResult = await _videoResultRepository.GetByIdAsync(request.VideoResultId);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoResultErrorCode.VideoResultNotExist), nameof(request.VideoResultId), request.VideoResultId);
                return methodResult;
            }
            _mapper.Map(request, videoResult);
            videoResult.Status = EnumResultStatus.Done;
            //videoResult.Percent = (double)videoResult.CorrectCount / videoResult.CorrectTotal * 100;
            if (!videoResult.IsValid())
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoResultErrorCode.VideoResultValueError), nameof(request), request);
                return methodResult;
            }

            await _videoResultRepository.ExecuteTransactionAsync(async () =>
            {
                videoResult = _videoResultRepository.Update(videoResult);
                await _videoResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<VideoResultModel>(videoResult);
                return methodResult;
            });
            return methodResult;
        }
    }
}
