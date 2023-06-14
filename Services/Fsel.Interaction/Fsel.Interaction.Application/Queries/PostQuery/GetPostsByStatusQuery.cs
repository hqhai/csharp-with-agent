// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.PostQuery
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Domain.Models.QueryModels.Posts;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetPostsByStatusQuery : GetPostsByStatusQueryModel, IRequest<MethodResult<PagingItemsModel<PostModel>>>
    {
    }

    public class GetPostsByStatusQueryHandler : IRequestHandler<GetPostsByStatusQuery, MethodResult<PagingItemsModel<PostModel>>>
    {
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;

        public GetPostsByStatusQueryHandler(IPostRepository postRepository, IMapper mapper)
        {
            _postRepository = postRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<PagingItemsModel<PostModel>>> Handle(GetPostsByStatusQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<PostModel>>();

            var query = _postRepository.Queryable
                                .Include(x => x.PostTags)
                                .ThenInclude(x => x.TopicTag)
                                .AsQueryable();

            if (request.Status.HasValue)
            {
                query = query.Where(m => m.Status == request.Status.Value);
            }

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = (await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false))
                    .Select(x =>
                    {
                        var model = _mapper.Map<PostModel>(x);
                        model.TopicTags = x.PostTags.Select(x => x.TopicTag!).ToList();
                        return model;
                    }).ToList();

            methodResult.Result = new PagingItemsModel<PostModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;

            return methodResult;
        }
    }
}
