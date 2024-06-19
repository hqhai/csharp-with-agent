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
    using Fsel.Interaction.Application.Services.TrainingServices;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Posts;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ApprovePostPendingCommand : ApprovePostPendingCommandModel, IRequest<MethodResult<PostModel>>
    {
    }

    public class ApprovePostPendingCommandHandler : IRequestHandler<ApprovePostPendingCommand, MethodResult<PostModel>>
    {
        private readonly IMapper _mapper;
        private readonly IPostRepository _postRepository;
        private readonly CompleteApprovalPostPublisher _completeApprovalPostPublisher;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly ITrainingService _trainingService;

        public ApprovePostPendingCommandHandler(IMapper mapper, IPostRepository postRepository, AuthContext authContext, IUserService userService, CompleteApprovalPostPublisher completeApprovalPostPublisher, QuestBoardPublisher questBoardPublisher, ITrainingService trainingService)
        {
            _mapper = mapper;
            _postRepository = postRepository;
            _authContext = authContext;
            _userService = userService;
            _questBoardPublisher = questBoardPublisher;
            _completeApprovalPostPublisher = completeApprovalPostPublisher;
            _trainingService = trainingService;
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

            var studentResult = await _userService.GetStudentByUserIdAsync(post.CreatedUserId);
            var student = studentResult?.Content?.Result;
            if (student != null)
            {
                var classResult = await _trainingService.GetClassByStudentId(student.Id);
                var courseId = classResult.Content?.Result?.CourseId;
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
