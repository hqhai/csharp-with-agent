// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.ActionCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Actions;
    using Fsel.Interaction.Application.Queues.Publishers;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Fsel.Shared.Models.ShareModels;

    public class CreateActionCommand : CreateActionCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateActionCommandHandler : IRequestHandler<CreateActionCommand, MethodResult<bool>>
    {
        private readonly IMapper _mapper;
        private readonly IInteractionActionRepository _interactionActionRepository;
        private readonly DiscussionBoardLikePublisher _discussionBoardLikePublisher;
        private readonly InterationActionPublisher _interationActionPublisher;
        private readonly AuthContext _authContext;

        public CreateActionCommandHandler(IMapper mapper, IInteractionActionRepository interactionActionRepository, AuthContext authContext, DiscussionBoardLikePublisher discussionBoardLikePublisher, InterationActionPublisher interationActionPublisher)
        {
            _mapper = mapper;
            _interactionActionRepository = interactionActionRepository;
            _authContext = authContext;
            _discussionBoardLikePublisher = discussionBoardLikePublisher;
            _interationActionPublisher = interationActionPublisher;
        }

        public async Task<MethodResult<bool>> Handle(CreateActionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            #region
            /* if (action != null)
             {
                 if (request.Type == EnumInteractionActionType.Like)
                 {
                     await _interactionActionRepository.DeleteAsync(action);
                 }
                 else if (request.Type != EnumInteractionActionType.Like)
                 {
                     methodResult.Result = true;
                     return methodResult;
                 }
             }*/
            #endregion
            var action = await _interactionActionRepository.Queryable
                            .Where(x => x.UserId == _authContext.CurrentUserId &&
                                        x.ObjectId == request.ObjectId &&
                                        x.Type == request.Type)
                            .FirstOrDefaultAsync(cancellationToken);

            await _interactionActionRepository.ExecuteTransactionAsync(async () =>
            {
                if (action == null)
                {
                    action = _mapper.Map<InteractionAction>(request);
                    action.UserId = _authContext.CurrentUserId;

                    if (!action.IsValid())
                    {
                        methodResult.AddErrorBadRequest(action.ErrorMessages);
                        return methodResult;
                    }

                    action = _interactionActionRepository.Add(action);

                    if (action.Type == EnumInteractionActionType.Like)
                    {
                        await _discussionBoardLikePublisher.Publish(action.ObjectId, cancellationToken).ConfigureAwait(false);
                    }
                }
                else if (action.Type == EnumInteractionActionType.Like)
                {
                    await _interactionActionRepository.DeleteAsync(action);
                }
                else if (action.Type == EnumInteractionActionType.Disable || action.Type == EnumInteractionActionType.Flag)
                {
                    InterationActionQueueModel model = new InterationActionQueueModel()
                    {
                        ObjectId = action.ObjectId,
                        InterationType = action.Type,
                        Type = EnumNotificationType.LinkPage,
                        Content = EnumNotificationContent.FlagClassForum,
                        UserId = _authContext.CurrentUserId,
                    };

                    await _interationActionPublisher.Publish(model, cancellationToken).ConfigureAwait(false);
                }

                await _interactionActionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);


                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
