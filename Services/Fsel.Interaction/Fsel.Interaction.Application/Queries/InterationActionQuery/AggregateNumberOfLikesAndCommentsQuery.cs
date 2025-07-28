// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queries.InterationActionQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
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

            numberLike = await _interactionActionRepository.Queryable.WhereBulkContains(request.ClassForumResultIds, p => (p.ObjectId)).Where(p => p.Type == EnumInteractionActionType.Like).CountAsync(cancellationToken);

            var comments = await _commentRepository.Queryable.WhereBulkContains(request.ClassForumResultIds, p => (p.ObjectId)).Where(p => p.Type == EnumInteractionType.ClassForum && p.Status == EnumCommentStatus.Approver).ToListAsync(cancellationToken);

            var commentIds = comments.Select(p => p.Id).ToList();

            var replyComments = await _commentRepository.Queryable.WhereBulkContains(commentIds, p => (p.ObjectId)).Where(p => p.Type == EnumInteractionType.ReplyComment && p.Status == EnumCommentStatus.Approver).ToListAsync(cancellationToken);

            var commentsByOwner = comments.Where(n => n.UserId == request.UserId).Select(x => x.Id).ToList();
            var replyCommentsByOwner = replyComments.Where(n => n.UserId == request.UserId).Select(x => x.Id).ToList();

            var listCommentByOwner = commentsByOwner.Union(replyCommentsByOwner).ToList();

            numberLike += await _interactionActionRepository.Queryable.WhereBulkContains(listCommentByOwner, p => (p.ObjectId)).Where(p => p.Type == EnumInteractionActionType.Like).CountAsync(cancellationToken);

            aggregateNumberOfLikes.Receive = new AggregateNumberOfLikesAndCommentsModel
            {
                NumberComment = comments.Count + replyComments.Count,
                NumberLike = numberLike,
            };
        }

        private async Task Give(AggregateNumberOfLikesAndCommentsQueryModel request, AggregateNumberOfLikesAndCommentsModels aggregateNumberOfLikes, CancellationToken cancellationToken)
        {
            var numberComment = await _commentRepository.Queryable.WhereBulkNotContains(request.ClassForumResultIds, p => p.ObjectId).Where(p => p.UserId == request.UserId && p.CourseId == request.CourseId && p.Status == EnumCommentStatus.Approver).CountAsync(cancellationToken);

            var numberLike = await _interactionActionRepository.Queryable.WhereBulkNotContains(request.ClassForumResultIds, p => p.ObjectId).Where(p => p.Type == EnumInteractionActionType.Like && p.UserId == request.UserId && p.CourseId == request.CourseId).CountAsync(cancellationToken);

            aggregateNumberOfLikes.Give = new AggregateNumberOfLikesAndCommentsModel
            {
                NumberComment = numberComment,
                NumberLike = numberLike,
            };
        }
    }
}
