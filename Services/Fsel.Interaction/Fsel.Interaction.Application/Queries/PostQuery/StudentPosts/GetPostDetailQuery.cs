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
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetPostDetailQuery : GetPostsByStudentQueryModel, IRequest<MethodResult<PostModel>>
    {
        public Guid PostId { get; set; }
    }

    public class GetPostDetailQueryQueryHandler : IRequestHandler<GetPostDetailQuery, MethodResult<PostModel>>
    {
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;

        public GetPostDetailQueryQueryHandler(IMapper mapper, IPostRepository postRepository)
        {
            _mapper = mapper;
            _postRepository = postRepository;
        }

        public async Task<MethodResult<PostModel>> Handle(GetPostDetailQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PostModel>();

            var posts = await _postRepository.GetIncludeByIdAsync(request.PostId);


            if (posts == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPostErrorCode.PostNotExist));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<PostModel>(posts);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
