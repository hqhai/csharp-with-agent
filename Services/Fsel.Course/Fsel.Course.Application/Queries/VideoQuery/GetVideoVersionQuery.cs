// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.VideoQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetVideoVersionQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<VideoModel>>>
    {
        public Guid Id { get; set; }
    }

    public class GetVideoVersionQueryHandler : IRequestHandler<GetVideoVersionQuery, MethodResult<PagingItemsModel<VideoModel>>>
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IMapper _mapper;

        public GetVideoVersionQueryHandler(IVideoRepository videoRepository, IMapper mapper)
        {
            _videoRepository = videoRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<PagingItemsModel<VideoModel>>> Handle(GetVideoVersionQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<VideoModel>> methodResult = new MethodResult<PagingItemsModel<VideoModel>>();

            var video = await _videoRepository.GetByIdAsync(request.Id);
            if (video == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(video), request.Id);
                return methodResult;
            }
            var originalId = video.OriginalId ?? video.Id;
            var query = _videoRepository.Queryable.Where(x => x.OriginalId == originalId || x.Id == originalId);

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            methodResult.Result = new PagingItemsModel<VideoModel>(_mapper.Map<IList<VideoModel>>(lists), request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
