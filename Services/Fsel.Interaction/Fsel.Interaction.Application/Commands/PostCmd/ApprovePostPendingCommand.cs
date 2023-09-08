// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.PostCmd
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Posts;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ApprovePostPendingCommand : ApprovePostPendingCommandModel, IRequest<MethodResult<PostModel>>
    {
    }

    public class ApprovePostPendingCommandHandler : IRequestHandler<ApprovePostPendingCommand, MethodResult<PostModel>>
    {
        private readonly IMapper _mapper;
        private readonly IPostRepository _postRepository;

        public ApprovePostPendingCommandHandler(IMapper mapper, IPostRepository postRepository)
        {
            _mapper = mapper;
            _postRepository = postRepository;
        }

        public async Task<MethodResult<PostModel>> Handle(ApprovePostPendingCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PostModel> methodResult = new MethodResult<PostModel>();

            var post = await _postRepository.GetByIdAsync(request.PostId);
            if (post == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPostErrorCode.PostNotExist), nameof(request.PostId), request.PostId);
                return methodResult;
            }
            if (post.Status != EnumPostStatus.Pending)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPostErrorCode.PostIsNotPending));
                return methodResult;
            }
            await _postRepository.ExecuteTransactionAsync(async () =>
            {
                post.Status = request.IsApprove ? EnumPostStatus.Active : EnumPostStatus.Reject;
                _postRepository.Update(post);

                await _postRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<PostModel>(post);
                return methodResult;
            });

            return methodResult;
        }
    }
}
