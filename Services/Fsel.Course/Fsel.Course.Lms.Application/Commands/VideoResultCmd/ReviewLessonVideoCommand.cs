// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoResultCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.VideoResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ReviewLessonVideoCommand : ReviewLessonVideoCommandModel, IRequest<MethodResult<VideoResultModel>>
    {
    }

    public class ReviewLessonVideoCommandHandler : IRequestHandler<ReviewLessonVideoCommand, MethodResult<VideoResultModel>>
    {
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly VideoHelper _videoConverter;
        private readonly IMapper _mapper;

        public ReviewLessonVideoCommandHandler(IVideoResultRepository videoResultRepository,
            IMapper mapper,
            VideoHelper videoConverter)
        {
            _videoConverter = videoConverter;
            _videoResultRepository = videoResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<VideoResultModel>> Handle(ReviewLessonVideoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoResultModel> methodResult = new MethodResult<VideoResultModel>();

            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == request.LessonResulttId, cancellationToken: cancellationToken);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }

            _mapper.Map(request, videoResult);
            if (!videoResult.IsValid())
            {
                methodResult.AddErrorBadRequest(videoResult.ErrorMessages);
                return methodResult;
            }
            var method = await _videoConverter.GetVideoResultDone(videoResult, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
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
