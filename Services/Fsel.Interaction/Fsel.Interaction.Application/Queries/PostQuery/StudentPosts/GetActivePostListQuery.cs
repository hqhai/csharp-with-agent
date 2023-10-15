// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.PostQuery.StudentPosts
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Application.Services.NotificationService;
    using Fsel.Interaction.Application.Services.NotificationService.Models;
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
        private readonly ITopicTagRepository _topicTagRepository;
        private readonly INotificationService _notificationService;

        public GetActivePostListQueryHandler
            (
             IPostRepository postRepository,
             IInteractionActionRepository interactionActionRepository,
             AuthContext authContext, IUserService userService,
             ICommentRepository commentRepository,
             ITopicTagRepository topicTagRepository
, INotificationService notificationService
            )
        {
            _postRepository = postRepository;
            _interactionActionRepository = interactionActionRepository;
            _authContext = authContext;
            _userService = userService;
            _commentRepository = commentRepository;
            _topicTagRepository = topicTagRepository;
            _notificationService = notificationService;
        }

        public async Task<MethodResult<PagingItemsModel<PostSearchModel>>> Handle(GetActivePostListQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<PostSearchModel>>();

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var courseLevel = student?.Content?.Result?.CourseLevel;

            //Get TopicTagID
            var topicTagId = _topicTagRepository.Queryable
                    .Where(topicTag => topicTag.Name == request.TopicTagName!)
                    .Select(topicTag => topicTag.Id)
                    .ToList();
            //Get All Post
            var postQuery = _postRepository.Queryable
                .Where(post => !_interactionActionRepository.Queryable
                    .Any(interaction => interaction.ObjectId == post.Id &&
                        interaction.Type == EnumInteractionActionType.Disable &&
                        interaction.UserId == _authContext.CurrentUserId)
                    && post.Status == EnumPostStatus.Active);

            //Get Post Contain TopicTag
            if (topicTagId.Any())
            {
                postQuery = postQuery.Where(post => post.PostTags
                    .Any(postTag => topicTagId.Contains(postTag.TopicTagId)));
            }

            IQueryable<Post> sortedQuery = postQuery;
            switch (request.PostType)
            {
                case EnumPostType.Recent:
                    sortedQuery = postQuery.Include(post => post.PostTags).OrderByDescending(post => post.CreatedDate);
                    //sortedQuery2 = postQuery.Select(post=> new { Post = post}).OrderByDescending(item => item.CreatedDate);
                    break;

                case EnumPostType.Relevant:
                    sortedQuery = postQuery.Include(post => post.PostTags).Where(post => post.CourseLevel == courseLevel).OrderByDescending(post => post.CreatedDate);
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
                        .Include(post => post.PostTags).Select(post => new
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
                                            FilePaths = item.Post.FilePaths,
                                            CreatedUserId = item.Post.CreatedUserId,
                                            CreatedDate = item.Post.CreatedDate,
                                            CreatedFullName = item.Post.CreatedFullName,
                                            UpdatedDate = item.Post.UpdatedDate,
                                            UpdatedUserId = item.Post.UpdatedUserId,
                                            PostTags = item.Post.PostTags,
                                        });

                    break;

                case EnumPostType.Top:
                    sortedQuery = postQuery
                            .Include(post => post.PostTags)
                            .OrderByDescending(post => post.PostTags.Count)
                            .ThenByDescending(post => post.CreatedDate);
                    break;

                default:
                    break;
            }

            int totalItem = await sortedQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            if (totalItem == 0)
            {
                methodResult.Result = new PagingItemsModel<PostSearchModel>(new List<PostSearchModel>(), request, totalItem);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
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
                FullName = post.CreatedFullName,
                CreatedDate = post.CreatedDate,
                UpdatedDate = post.UpdatedDate,
                UpdatedUserId = post.UpdatedUserId,
                FilePaths = post.FilePaths,
                IsLiked = false,
                IsTurnedOffNotification = false,
                PostTags = post.PostTags
                               .Select(postTag => new TopicTagModel
                               {
                                   Name = postTag.TopicTag!.Name,
                                   Color = postTag.TopicTag!.Color,
                                   Id = postTag.TopicTag!.Id,
                               }).ToList()

            });

            var lists = await result
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);


            GetListNotificationRemindQueryModel query = new GetListNotificationRemindQueryModel
            {
                ObjectIds = lists.Select(x => x.Id).ToList(),
                Status = EnumNotificationRemindStatus.Off
            };
            var notificationRemind = await _notificationService.GetListNotificationRemind(query);
            var notificationTurnOff = notificationRemind.Content?.Result;

            foreach (var post in lists)
            {
                var likeQuery = _interactionActionRepository.Queryable
                    .Where(interaction => interaction.Type == EnumInteractionActionType.Like && interaction.ObjectId == post.Id);

                int? likeCount = likeQuery.Count();

                int commentCount = await _commentRepository.Queryable
                    .CountAsync(comment => comment.ObjectId == post.Id, cancellationToken);

                var likeAction = likeQuery.Any(i => i.UserId == _authContext.CurrentUserId);

                post.IsLiked = likeAction;
                post.LikeCount = likeCount;
                post.CommentCount = commentCount;
                post.IsTurnedOffNotification = notificationTurnOff?.Any(p => p.ObjectId == post.Id) ?? false;
            }

            methodResult.Result = new PagingItemsModel<PostSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
