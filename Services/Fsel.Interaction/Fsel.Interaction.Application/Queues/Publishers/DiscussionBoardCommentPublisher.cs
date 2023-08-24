using AutoMapper;
using Fsel.Core.Base;
using Fsel.Core.Base.Interfaces;
using Fsel.Interaction.Domain.Entities;
using Fsel.Interaction.Domain.IRepositories;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.ShareModels;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Interaction.Application.Queues.Publishers
{
    public class DiscussionBoardCommentPublisher
    {
        private readonly IQueueProvider _queueProvider;
        private readonly ICommentRepository _commentRepository;
        private readonly IInteractionActionRepository _interactionActionRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;

        public DiscussionBoardCommentPublisher(IQueueProvider queueProvider, IMapper mapper, IInteractionActionRepository interactionActionRepository, ICommentRepository commentRepository, AuthContext authContext)
        {
            _queueProvider = queueProvider;
            _mapper = mapper;
            _interactionActionRepository = interactionActionRepository;
            _commentRepository = commentRepository;
            _authContext = authContext;
        }

        public async Task Publish(Comment? comment, CancellationToken cancellationToken)
        {
            if (comment == null)
            {
                return;
            }

            var commentQuery = from c in _commentRepository.Queryable
                               join ca in _interactionActionRepository.Queryable on c.Id equals ca.ObjectId into caJ
                               from p in caJ.DefaultIfEmpty()
                               where c.ObjectId == comment.ObjectId && (p == null || (p.Type != EnumInteractionActionType.Disable && p.UserId == _authContext.CurrentUserId))
                               select c;

            await _queueProvider.Publish(QueueSettings.RealtimeQueue.NameQueue.DiscussionBoard, new DiscussionBoardQueueModel
            {
                ObjectId = comment.ObjectId,
                CommentNumber = await commentQuery.CountAsync(cancellationToken),
                ChangeComment = _mapper.Map<CommentQueueModel>(comment)
            }, cancellationToken);
        }
    }
}
