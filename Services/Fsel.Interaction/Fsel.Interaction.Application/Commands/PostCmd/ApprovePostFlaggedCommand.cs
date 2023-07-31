// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.PostCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Posts;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ApprovePostFlaggedCommand : ApprovePostFlaggedCommandModel, IRequest<MethodResult<PostModel>>
    {
    }

    public class ApprovePostFlaggedCommandHandler : IRequestHandler<ApprovePostFlaggedCommand, MethodResult<PostModel>>
    {
        private readonly IPostRepository _postRepository;
        private readonly IInteractionActionRepository _interactionActionRepository;
        private readonly IMapper _mapper;

        public ApprovePostFlaggedCommandHandler(IPostRepository postRepository, IMapper mapper, IInteractionActionRepository interactionActionRepository)
        {
            _postRepository = postRepository;
            _mapper = mapper;
            _interactionActionRepository = interactionActionRepository;
        }

        public async Task<MethodResult<PostModel>> Handle(ApprovePostFlaggedCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PostModel>();
            var post = await _postRepository.Queryable.Include(x => x.PostTags)
                                                      .Where(e => e.Id == request.PostId).FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (post == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(post));
                return methodResult;
            }

            var actions = await _interactionActionRepository.Queryable.Where(x => x.ObjectId == request.PostId && x.Type == EnumInteractionActionType.Flag).ToListAsync(cancellationToken);
            if (actions == null || actions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPostErrorCode.PostNotFlagged));
                return methodResult;
            }
            await _postRepository.ExecuteTransactionAsync(async () =>
            {
                if (request.IsApprove)
                {
                    await _postRepository.DeleteAsync(post);
                    await _postRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
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
                methodResult.Result = _mapper.Map<PostModel>(post);
                return methodResult;
            });
            return methodResult;
        }
    }
}
