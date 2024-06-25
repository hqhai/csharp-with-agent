// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.InterationActionQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Domain.Models.QueryModels.InterationActions;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class AggregateNumberOfLikesAndCommentsQuery : AggregateNumberOfLikesAndCommentsQueryModel, IRequest<MethodResult<AggregateNumberOfLikesAndCommentsModels>>
    {
    }

    public class GetNumberLikeAndCommentForStudentQueryHandler : IRequestHandler<AggregateNumberOfLikesAndCommentsQuery, MethodResult<AggregateNumberOfLikesAndCommentsModels>>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IInteractionActionRepository _interactionActionRepository;

        public GetNumberLikeAndCommentForStudentQueryHandler(ICommentRepository commentRepository, IInteractionActionRepository interactionActionRepository)
        {
            _commentRepository = commentRepository;
            _interactionActionRepository = interactionActionRepository;
        }

        public async Task<MethodResult<AggregateNumberOfLikesAndCommentsModels>> Handle(AggregateNumberOfLikesAndCommentsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<AggregateNumberOfLikesAndCommentsModels>();

            var aggregateNumberOfLikes = new AggregateNumberOfLikesAndCommentsModels();
            await Receive(request, aggregateNumberOfLikes, cancellationToken);
            await Give(request, aggregateNumberOfLikes, cancellationToken);
            methodResult.Result = aggregateNumberOfLikes;
            return methodResult;
        }

        private async Task Receive(AggregateNumberOfLikesAndCommentsQueryModel request, AggregateNumberOfLikesAndCommentsModels aggregateNumberOfLikes, CancellationToken cancellationToken)
        {
            int numberLike = 0;

            numberLike = await _interactionActionRepository.Queryable.Where(p => request.ClassForumResultIds != null && request.ClassForumResultIds.Contains(p.ObjectId) && p.Type == EnumInteractionActionType.Like).CountAsync(cancellationToken);

            var comments = await _commentRepository.Queryable.Where(p => request.ClassForumResultIds != null && request.ClassForumResultIds.Contains(p.ObjectId) && p.Type == EnumInteractionType.ClassForum).ToListAsync(cancellationToken);

            var replyComments = await _commentRepository.Queryable.Where(p => comments != null && comments.Select(x => x.Id).Contains(p.ObjectId) && p.Type == EnumInteractionType.ReplyComment).ToListAsync(cancellationToken);

            var commentsByUser = comments.Where(n => n.UserId == request.UserId).Select(x => x.Id).ToList();
            var replyCommentsByUser = replyComments.Where(n => n.UserId == request.UserId).Select(x => x.Id).ToList();

            numberLike += await _interactionActionRepository.Queryable.Where(p => commentsByUser != null && commentsByUser.Contains(p.ObjectId) && p.Type == EnumInteractionActionType.Like).CountAsync(cancellationToken);

            numberLike += await _interactionActionRepository.Queryable.Where(p => replyCommentsByUser != null && replyCommentsByUser.Contains(p.ObjectId) && p.Type == EnumInteractionActionType.Like).CountAsync(cancellationToken);

            int numberComment = 0;

            var commentIds = await _commentRepository.Queryable.Where(p => request.ClassForumResultIds != null && request.ClassForumResultIds.Contains(p.ObjectId) && p.Type == EnumInteractionType.ClassForum).ToListAsync(cancellationToken);
            numberComment += commentIds.Count;

            var numberReplyComment = await _commentRepository.Queryable.Where(p => commentIds != null && commentIds.Select(x => x.Id).Contains(p.ObjectId) && p.Type == EnumInteractionType.ReplyComment).CountAsync(cancellationToken);
            numberComment += numberReplyComment;

            aggregateNumberOfLikes.Receive = new AggregateNumberOfLikesAndCommentsModel
            {
                NumberComment = numberComment,
                NumberLike = numberLike,
            };
        }

        private async Task Give(AggregateNumberOfLikesAndCommentsQueryModel request, AggregateNumberOfLikesAndCommentsModels aggregateNumberOfLikes, CancellationToken cancellationToken)
        {
            int numberComment = 0;

            var comments = await _commentRepository.Queryable.Where(p => request.ClassForumResultIds != null && !request.ClassForumResultIds.Contains(p.ObjectId) && request.CourseClassForumResultIds != null && request.CourseClassForumResultIds.Contains(p.ObjectId) && p.Type == EnumInteractionType.ClassForum).ToListAsync(cancellationToken);

            var replyComment = await _commentRepository.Queryable.Where(p => comments != null && comments.Select(x => x.Id).Contains(p.ObjectId) && p.Type == EnumInteractionType.ReplyComment).ToListAsync(cancellationToken);

            numberComment += comments.Where(p => p.UserId == request.UserId).Count();
            numberComment += replyComment.Where(p => p.UserId == request.UserId).Count();

            int numberLike = 0;

            var actionLikes = await _interactionActionRepository.Queryable.Where(p => p.Type == EnumInteractionActionType.Like && p.UserId == request.UserId).ToListAsync(cancellationToken);

            numberLike += actionLikes.Where(p => request.ClassForumResultIds != null && !request.ClassForumResultIds.Contains(p.ObjectId) && request.CourseClassForumResultIds != null && request.CourseClassForumResultIds.Contains(p.ObjectId)).Count();

            comments = comments.Where(p => p.UserId != request.UserId).ToList();
            replyComment = replyComment.Where(p => p.UserId != request.UserId).ToList();

            numberLike += actionLikes.Where(p => comments.Select(x => x.Id).Contains(p.ObjectId)).Count();
            numberLike += actionLikes.Where(p => replyComment.Select(x => x.Id).Contains(p.ObjectId)).Count();

            aggregateNumberOfLikes.Give = new AggregateNumberOfLikesAndCommentsModel
            {
                NumberComment = numberComment,
                NumberLike = numberLike,
            };
        }
    }
}
