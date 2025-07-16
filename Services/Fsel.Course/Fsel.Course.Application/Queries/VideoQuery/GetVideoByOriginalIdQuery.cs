// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.VideoQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetVideoByOriginalIdQuery : IRequest<MethodResult<VideoModel>>
    {
        public Guid OriginalId { get; set; }
    }

    public class GetVideoByOriginalIdQueryHandler : IRequestHandler<GetVideoByOriginalIdQuery, MethodResult<VideoModel>>
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IMapper _mapper;

        public GetVideoByOriginalIdQueryHandler(IVideoRepository videoRepository,
                                                IMapper mapper)
        {
            _videoRepository = videoRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<VideoModel>> Handle(GetVideoByOriginalIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();

            var video = await _videoRepository.Queryable
                                              .Include(x => x.VideoTimeCodes)
                                              .Where(x => x.OriginalId == request.OriginalId && x.VersionStatus == Common.Enums.EnumVersionStatus.LastVersion)
                                              .AsNoTracking()
                                              .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (video == null)
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
