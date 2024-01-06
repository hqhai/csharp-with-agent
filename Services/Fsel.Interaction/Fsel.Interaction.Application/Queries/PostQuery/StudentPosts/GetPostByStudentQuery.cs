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

    public class GetPostsByStudentQuery : GetPostsByStudentQueryModel, IRequest<MethodResult<IList<PostModel>>>
    {
        public EnumPostStatus? Status { get; set; }
    }

    public class GetPostByStudentQueryHandler : IRequestHandler<GetPostsByStudentQuery, MethodResult<IList<PostModel>>>
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

        public async Task<MethodResult<IList<PostModel>>> Handle(GetPostsByStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<PostModel>>();

            var posts = _postRepository.Queryable
                                            .Where(x => x.UserId == _authContext.CurrentUserId && x.Status == request.Status);

            return await _postRepository.GetListResultAsync<PostModel>(posts, cancellationToken: cancellationToken);
        }
    }
}
