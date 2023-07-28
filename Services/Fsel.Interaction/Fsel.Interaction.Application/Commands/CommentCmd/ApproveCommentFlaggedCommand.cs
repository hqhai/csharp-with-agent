// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.CommentCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Comments;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ApproveCommentFlaggedCommand : ApproveCommentFlaggedCommandModel, IRequest<MethodResult<CommentModel>>
    {
    }

    public class ApproveCommentFlaggedCommandHandler : IRequestHandler<ApproveCommentFlaggedCommand, MethodResult<CommentModel>>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IInteractionActionRepository _interactionActionRepository;
        private readonly IMapper _mapper;

        public ApproveCommentFlaggedCommandHandler(ICommentRepository commentRepository, IMapper mapper, IInteractionActionRepository interactionActionRepository)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
            _interactionActionRepository = interactionActionRepository;
        }

        public async Task<MethodResult<CommentModel>> Handle(ApproveCommentFlaggedCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CommentModel>();
            var comment = await _commentRepository.GetByIdAsync(request.CommentId);
            if (comment == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.CommentId));
                return methodResult;
            }

            var actions = await _interactionActionRepository.Queryable.Where(x => x.ObjectId == request.CommentId && x.Type == EnumInteractionActionType.Flag).ToListAsync(cancellationToken);
            if (actions == null || actions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCommentErrorCode.CommentNotFlagged));
                return methodResult;
            }
            await _commentRepository.ExecuteTransactionAsync(async () =>
            {
                if (request.IsApprove)
                {
                    await _commentRepository.DeleteAsync(comment);
                    await _commentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    foreach (var item in actions)
                    {
                        await _interactionActionRepository.DeleteAsync(item);
                    }
                    await _interactionActionRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CommentModel>(comment);
                return methodResult;
            });
            return methodResult;
        }
    }
}
