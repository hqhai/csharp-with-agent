// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.PostCmd
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Queues.Publishers;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Posts;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ApprovePostPendingCommand : ApprovePostPendingCommandModel, IRequest<MethodResult<PostModel>>
    {
    }

    public class ApprovePostPendingCommandHandler : IRequestHandler<ApprovePostPendingCommand, MethodResult<PostModel>>
    {
        private readonly IMapper _mapper;
        private readonly IPostRepository _postRepository;
        private readonly CompleteApprovalPostPublisher _completeApprovalPostPublisher;
        private readonly AuthContext _authContext;

        public ApprovePostPendingCommandHandler(IMapper mapper, IPostRepository postRepository, CompleteApprovalPostPublisher completeApprovalPostPublisher, AuthContext authContext)
        {
            _mapper = mapper;
            _postRepository = postRepository;
            _completeApprovalPostPublisher = completeApprovalPostPublisher;
            _authContext = authContext;
        }

        public async Task<MethodResult<PostModel>> Handle(ApprovePostPendingCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PostModel> methodResult = new MethodResult<PostModel>();

            var post = await _postRepository.GetByIdAsync(request.PostId);
            if (post == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(post));
                return methodResult;
            }
            if (post.Status != EnumPostStatus.Pending)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(post.Status));
                return methodResult;
            }
            await _postRepository.ExecuteTransactionAsync(async () =>
            {
                post.Status = request.IsApprove ? EnumPostStatus.Active : EnumPostStatus.Reject;
                _postRepository.Update(post);


                await CompleteApprovalPost(post, cancellationToken);
                await _postRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<PostModel>(post);
                return methodResult;
            });

            return methodResult;
        }

        public async Task CompleteApprovalPost(Post post, CancellationToken cancellationToken)
        {
            List<Guid> userIds = new List<Guid>() { _authContext.CurrentUserId };

            if (post != null)
            {
                await _completeApprovalPostPublisher.Publish(new SetTimeCompleteApprovalModel()
                {
                    StartDate = post.CreatedDate,
                    ObjectId = post.Id,
                    ApprovalType = EnumApprovalTime.DiscussionBoard,
                    UserIds = userIds
                },
                cancellationToken);
            }
        }
    }
}
