// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Queries.VideoQuery
{
    public class GetVideoQuery : IRequest<MethodResult<VideoModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetVideoQueryHandler : IRequestHandler<GetVideoQuery, MethodResult<VideoModel>>
    {
        private readonly IVideoRepository _videoRepository;

        public GetVideoQueryHandler(IVideoRepository videoRepository
            )
        {
            _videoRepository = videoRepository;
        }

        public async Task<MethodResult<VideoModel>> Handle(GetVideoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();

            var video = await _videoRepository.GetIncludeAllAsync(request.Id);

            if (video == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VideoIdNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }

            methodResult.Result = video;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
