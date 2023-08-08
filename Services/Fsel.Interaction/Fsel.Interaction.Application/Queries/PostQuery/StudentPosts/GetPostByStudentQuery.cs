// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.PostQuery
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Domain.Models.QueryModels.Posts;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetPostsByStudentQuery : GetPostsByStudentQueryModel, IRequest<MethodResult<List<PostModel>>>
    {
        public EnumPostStatus? Status { get; set; }
    }

    public class GetPostByStudentQueryHandler : IRequestHandler<GetPostsByStudentQuery, MethodResult<List<PostModel>>>
    {
        private readonly IPostRepository _postRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;

        public GetPostByStudentQueryHandler(IMapper mapper, IPostRepository postRepository, AuthContext authContext, IUserService userService)
        {
            _mapper = mapper;
            _postRepository = postRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<List<PostModel>>> Handle(GetPostsByStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<List<PostModel>>();

            var posts = await _postRepository.Queryable
                                            .Where(x => x.UserId == _authContext.CurrentUserId && x.Status == request.Status)
                                            .ToListAsync(cancellationToken);

            if (posts == null || posts.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(posts));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<List<PostModel>>(posts);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
