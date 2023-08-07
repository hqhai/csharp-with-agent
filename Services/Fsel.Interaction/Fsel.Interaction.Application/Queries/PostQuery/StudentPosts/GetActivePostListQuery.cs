// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.PostQuery.StudentPosts
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Domain.Models.QueryModels.Posts;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetActivePostListQuery : GetActivePostListQueryModel, IRequest<MethodResult<PagingItemsModel<PostSearchModel>>>
    {
    }

    public class GetActivePostListQueryHandler : IRequestHandler<GetActivePostListQuery, MethodResult<PagingItemsModel<PostSearchModel>>>
    {
        private readonly IPostRepository _postRepository;
        private readonly IInteractionActionRepository _interactionActionRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly ICommentRepository _commentRepository;

        public GetActivePostListQueryHandler
            (
             IPostRepository postRepository,
             IInteractionActionRepository interactionActionRepository,
             AuthContext authContext, IUserService userService,
             ICommentRepository commentRepository
            )
        {
            _postRepository = postRepository;
            _interactionActionRepository = interactionActionRepository;
            _authContext = authContext;
            _userService = userService;
            _commentRepository = commentRepository;
        }

        public async Task<MethodResult<PagingItemsModel<PostSearchModel>>> Handle(GetActivePostListQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<PostSearchModel>>();

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var courseLevel = student?.Content?.Result?.CourseLevel;

            var postQuery = _postRepository.Queryable.Where(post => !_interactionActionRepository.Queryable
                                                     .Any(interaction => interaction.ObjectId == post.Id &&
                                                      interaction.Type == EnumInteractionActionType.Disable &&
                                                      interaction.UserId == _authContext.CurrentUserId) && post.Status == EnumPostStatus.Active);

            IQueryable<Post> sortedQuery = postQuery;
            switch (request.PostType)
            {
                case EnumPostType.Recent:
                    sortedQuery = postQuery.Include(post => post.PostTags.Where(y => !y.IsDeleted)).OrderByDescending(post => post.CreatedDate);
                    //sortedQuery2 = postQuery.Select(post=> new { Post = post}).OrderByDescending(item => item.CreatedDate);
                    break;

                case EnumPostType.Relevant:
                    sortedQuery = postQuery.Include(post => post.PostTags.Where(y => !y.IsDeleted)).Where(post => post.CourseLevel == courseLevel).OrderByDescending(post => post.CreatedDate);
                    break;

                case EnumPostType.Trending:
                    var date7DaysAgo = DateTime.Now.AddDays(-7);
                    //sortedQuery = from post in postQuery
                    //              join interaction in _interactionActionRepository.Queryable
                    //              on post.Id equals interaction.ObjectId
                    //              join comment in _commentRepository.Queryable
                    //              on post.Id equals comment.ObjectId
                    //              where interaction.Type == EnumInteractionActionType.Like && post.CreatedDate >= date7DaysAgo
                    //              group interaction by post into g
                    //              orderby g.Count() descending
                    //              select g.Key;

                    sortedQuery = postQuery
                        .Include(post => post.PostTags.Where(y => !y.IsDeleted)).Select(post => new
                        {
                            Post = post,
                            InteractionCount = _interactionActionRepository.Queryable
                                        .Count(interaction => interaction.ObjectId == post.Id && interaction.Type == EnumInteractionActionType.Like),
                            CommentCount = _commentRepository.Queryable
                                        .Count(comment => comment.ObjectId == post.Id)
                        })
                                        .Where(item => item.InteractionCount > 0 && item.Post.CreatedDate >= date7DaysAgo)
                                        .OrderByDescending(item => item.InteractionCount)
                                        .ThenByDescending(item => item.CommentCount)
                                        .Select(item => new Post
                                        {
                                            Id = item.Post.Id,
                                            Title = item.Post.Title,
                                            Content = item.Post.Content,
                                            Status = item.Post.Status,
                                            CourseLevel = item.Post.CourseLevel,
                                            UserId = item.Post.UserId,
                                            CreatedUserId = item.Post.CreatedUserId,
                                            CreatedDate = item.Post.CreatedDate,
                                            UpdatedDate = item.Post.UpdatedDate,
                                            UpdatedUserId = item.Post.UpdatedUserId,
                                        });

                    break;

                case EnumPostType.Top:
                    sortedQuery = postQuery
                            .Include(post => post.PostTags.Where(y => !y.IsDeleted))
                            .OrderByDescending(post => post.PostTags.Count)
                            .ThenByDescending(post => post.CreatedDate);
                    break;

                default:
                    break;
            }

            var result = sortedQuery.Select(post => new PostSearchModel
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
                FilePaths = post.FilePaths,
                PostTags = post.PostTags
                               .Where(postTag => postTag.TopicTag != null)
                               .Select(postTag => new TopicTagModel
                               {
                                   Name = postTag.TopicTag!.Name,
                                   Color = postTag.TopicTag!.Color,
                               }).ToList()

            });

            int totalItem = await sortedQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            var lists = await result
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
