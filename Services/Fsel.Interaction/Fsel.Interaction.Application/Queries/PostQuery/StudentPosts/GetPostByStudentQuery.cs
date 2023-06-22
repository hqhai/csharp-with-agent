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

    public class GetPostsByStudentQuery : GetPostsByStudentQueryModel, IRequest<MethodResult<List<PostModel>>>
    {
        public Guid StudentId { get; set; }
        public EnumPostStatus? Status { get; set; }
    }

    public class GetPostByStudentQueryHandler : IRequestHandler<GetPostsByStudentQuery, MethodResult<List<PostModel>>>
    {
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;

        public GetPostByStudentQueryHandler(IMapper mapper, IPostRepository postRepository)
        {
            _mapper = mapper;
            _postRepository = postRepository;
        }

        public async Task<MethodResult<List<PostModel>>> Handle(GetPostsByStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<List<PostModel>>();

            var posts = await _postRepository.Queryable
                                            .Where(x => x.UserId == request.StudentId && x.Status == request.Status)
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
