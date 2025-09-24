// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.VideoQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetVideoDetailByOriginalIdQuery : IRequest<MethodResult<VideoModel>>
    {
        public Guid OriginalId { get; set; }
    }

    public class GetVideoDetailByOriginalIdQueryHandler : IRequestHandler<GetVideoDetailByOriginalIdQuery, MethodResult<VideoModel>>
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IMapper _mapper;

        public GetVideoDetailByOriginalIdQueryHandler(IVideoRepository videoRepository, IMapper mapper)
        {
            _videoRepository = videoRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<VideoModel>> Handle(GetVideoDetailByOriginalIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();

            var video = await _videoRepository.Queryable
                                              .Where(x => x.OriginalId == request.OriginalId && x.VersionStatus == EnumVersionStatus.LastVersion)
                                              .AsNoTracking()
                                              .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (video == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(video));
                return methodResult;
            }

            var videoModel = await _videoRepository.GetIncludeAllAsync(video.Id);
            if (videoModel == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(video));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<VideoModel>(video);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
