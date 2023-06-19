// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.PostQuery
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Domain.Models.QueryModels.Posts;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetActivePostListQuery : GetActivePostListQueryModel, IRequest<MethodResult<List<PostModel>>>
    {
    }

    public class GetActivePostListQueryHandler : IRequestHandler<GetActivePostListQuery, MethodResult<List<PostModel>>>
    {
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;

        public GetActivePostListQueryHandler(IMapper mapper, IPostRepository postRepository)
        {
            _mapper = mapper;
            _postRepository = postRepository;
        }

        public async Task<MethodResult<List<PostModel>>> Handle(GetActivePostListQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<List<PostModel>>();

            var posts = await _postRepository.Queryable
                                            .Where(x => x.Status == request.Status)
                                            .ToListAsync(cancellationToken);

            if (posts == null || posts.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPostErrorCode.PostNotExist));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<List<PostModel>>(posts);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }


    }
}
