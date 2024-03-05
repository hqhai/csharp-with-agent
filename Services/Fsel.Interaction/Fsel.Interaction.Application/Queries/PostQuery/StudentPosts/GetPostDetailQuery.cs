// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.PostQuery
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Domain.Models.QueryModels.Posts;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetPostDetailQuery : GetPostsByStudentQueryModel, IRequest<MethodResult<PostModel>>
    {
        public Guid PostId { get; set; }
    }

    public class GetPostDetailQueryQueryHandler : IRequestHandler<GetPostDetailQuery, MethodResult<PostModel>>
    {
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IMapper _mapper;

        public GetPostDetailQueryQueryHandler(IMapper mapper, IPostRepository postRepository, ICommentRepository commentRepository)
        {
            _mapper = mapper;
            _postRepository = postRepository;
            _commentRepository = commentRepository;
        }

        public async Task<MethodResult<PostModel>> Handle(GetPostDetailQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PostModel>();

            var posts = await _postRepository.GetIncludeByIdAsync(request.PostId);

            var comments = _commentRepository.Queryable.Where(x => x.ObjectId == request.PostId);

            var postResult = _mapper.Map<PostModel>(posts);
            postResult.Comments = _mapper.Map<List<CommentModel>>(comments);

            if (posts == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(posts));
                return methodResult;
            }

            methodResult.Result = postResult;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
