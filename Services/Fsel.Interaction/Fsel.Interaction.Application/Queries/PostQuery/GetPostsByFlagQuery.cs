// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.PostQuery
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetPostsByFlagQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<PostModel>>>
    {
    }

    public class GetPostsByFlagQueryHandler : IRequestHandler<GetPostsByFlagQuery, MethodResult<PagingItemsModel<PostModel>>>
    {
        private readonly IPostRepository _postRepository;
        private readonly IInteractionActionRepository _interactionActionRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetPostsByFlagQueryHandler(IPostRepository postRepository, IMapper mapper, IInteractionActionRepository interactionActionRepository, ICommentRepository commentRepository, IUserService userService)
        {
            _postRepository = postRepository;
            _mapper = mapper;
            _interactionActionRepository = interactionActionRepository;
            _commentRepository = commentRepository;
            _userService = userService;
        }

        public async Task<MethodResult<PagingItemsModel<PostModel>>> Handle(GetPostsByFlagQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<PostModel>>();

            var commentQuery = from c in _commentRepository.Queryable
                               join ia in _interactionActionRepository.Queryable on c.Id equals ia.ObjectId
                               where ia.Type == EnumInteractionActionType.Flag
                               select c;
            var comments = await commentQuery.ToListAsync(cancellationToken);

            var postsbyCommentFlag = comments.Select(async (x) => new { Comment = x, PostId = await GetPostId(x.Id) })
                               .Select(t => t.Result)
                               .Where(i => i != null)
                               .GroupBy(i => i.PostId)
                               .Select(x => new
                               {
                                   PostId = x.Key,
                                   Comments = x.Select(n => n.Comment).ToList()
                               }).ToList();

            var query = from p in _postRepository.Queryable
                        join ia in _interactionActionRepository.Queryable on p.Id equals ia.ObjectId
                        where ia.Type == EnumInteractionActionType.Flag || postsbyCommentFlag.Select(x => x.PostId).Contains(p.Id)
                        select p;

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var userIds = lists.Select(x => x.UserId).ToList();
            userIds.AddRange(postsbyCommentFlag.Where(x => lists.Select(n => n.Id).Contains(x.PostId)).SelectMany(x => x.Comments).Select(x => x.UserId));

            var studentsResult = await _userService.GetStudentByUserIdsAsync(userIds);
            var students = studentsResult.Content?.Result;

            var results = lists.Select(x =>
            {
                var model = _mapper.Map<PostModel>(x);
                model.TopicTags = _mapper.Map<IList<TopicTagModel>>(x.PostTags.Select(x => x.TopicTag!));
                GetStudentToObj(x.UserId, model);

                var comments = postsbyCommentFlag.FirstOrDefault(n => n.PostId == x.Id)?.Comments;
                if (comments != null)
                {
                    model.Comments = new List<CommentModel>();
                    comments.ForEach(x =>
                    {
                        var comment = _mapper.Map<CommentModel>(x);
                        model.Comments.Add(GetStudentToObj(x.UserId, comment));
                    });
                }

                return model;
            }).ToList();

            dynamic? GetStudentToObj(Guid id, dynamic obj)
            {
                var student = students?.FirstOrDefault(x => x.Id == id);
                obj.FullName = student?.FullName;
                obj.AvatarPath = student?.AvatarPath;
                return obj;
            }

            methodResult.Result = new PagingItemsModel<PostModel>(results, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<Guid> GetPostId(Guid id)
        {
            var comment = await _commentRepository.Queryable.FirstOrDefaultAsync(n => n.Id == id);
            if (comment != null)
            {
                return await GetPostId(comment.ObjectId);
            }
            return id;
        }
    }
}
