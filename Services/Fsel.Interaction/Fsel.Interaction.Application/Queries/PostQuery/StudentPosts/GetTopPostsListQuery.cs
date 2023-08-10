// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.PostQuery
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Domain.Models.QueryModels.Posts;
    using Fsel.Interaction.Infrastructure.Repositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTopPostsListQuery : GetTopPostQueryModel, IRequest<MethodResult<PagingItemsModel<PostSearchModel>>>
    {
    }

    public class GetTopPostsListQueryHandler : IRequestHandler<GetTopPostsListQuery, MethodResult<PagingItemsModel<PostSearchModel>>>
    {
        private readonly IPostRepository _postRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IInteractionActionRepository _interactionActionRepository;
        private readonly ICommentRepository _commentRepository;


        public GetTopPostsListQueryHandler(IPostRepository postRepository, AuthContext authContext, IUserService userService, IInteractionActionRepository interactionActionRepository, ICommentRepository commentRepository)
        {
            _postRepository = postRepository;
            _authContext = authContext;
            _userService = userService;
            _interactionActionRepository = interactionActionRepository;
            _commentRepository = commentRepository;
        }

        public async Task<MethodResult<PagingItemsModel<PostSearchModel>>> Handle(GetTopPostsListQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<PostSearchModel>>();

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);

            var courseLevel = student?.Content?.Result?.CourseLevel;

            var postQuery = _postRepository.Queryable
                .Include(post => post.PostTags.Where(y => !y.IsDeleted))
                .Where(post => post.CourseLevel == courseLevel)
                .OrderByDescending(post => post.PostTags.Count)
                .ThenByDescending(post => post.CreatedDate)
                .Select(post => new PostSearchModel
                {
                    Id = post.Id,
                    Title = post.Title,
                    Content = post.Content,
                    Status = post.Status,
                    CourseLevel = post.CourseLevel,
                    UserId = post.UserId,
                    CreatedUserId = post.CreatedUserId,
                    CreatedDate = post.CreatedDate,
                    UpdatedDate = post.UpdatedDate,
                    UpdatedUserId = post.UpdatedUserId,
                });

            int totalItem = await postQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            var lists = await postQuery
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

            foreach (var post in lists)
            {
                int likeCount = await _interactionActionRepository.Queryable
                    .CountAsync(interaction => interaction.Type == EnumInteractionActionType.Like && interaction.ObjectId == post.Id, cancellationToken);

                int commentCount = await _commentRepository.Queryable
                    .CountAsync(comment => comment.ObjectId == post.Id, cancellationToken);

                post.LikeCount = likeCount;
                post.CommentCount = commentCount;
            }

            methodResult.Result = new PagingItemsModel<PostSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
