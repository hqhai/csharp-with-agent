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
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly ITrainingService _trainingService;

        public ApprovePostPendingCommandHandler(IMapper mapper, IPostRepository postRepository, AuthContext authContext, IUserService userService, QuestBoardPublisher questBoardPublisher, ITrainingService trainingService)
        {
            _mapper = mapper;
            _postRepository = postRepository;
            _authContext = authContext;
            _userService = userService;
            _questBoardPublisher = questBoardPublisher;
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

                if (courseId != null)
                {
                    await DoQuestBoard(post.Id, (Guid)courseId, post.CreatedUserId, cancellationToken);
                }
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

        public async Task DoQuestBoard(Guid postId, Guid courseId, Guid userId, CancellationToken cancellationToken)
        {
            IList<EnumQuestBoardCategory> categories = new List<EnumQuestBoardCategory>() { EnumQuestBoardCategory.PostOneDiscussionBoard, EnumQuestBoardCategory.PostThreeDiscussionBoard, EnumQuestBoardCategory.PostFiveDiscussionBoard };
            var student = await _userService.GetStudentByUserIdAsync(userId);
            var studentId = student?.Content?.Result?.Id;

            var post = await _postRepository.Queryable.Where(c => c.CreatedUserId == userId && c.Status != EnumPostStatus.Pending && c.Status != EnumPostStatus.Draft).ToListAsync(cancellationToken);
            var postCount = post.Count;
            if (postCount > 0)
            {
                await _questBoardPublisher.Publish(new QuestBoardQueueModel
                {
                    StudentId = (Guid)studentId!,
                    Categories = categories,
                    AchievedPoint = postCount,
                    ObjectId = postId,
                    CourseId = courseId
                }, cancellationToken);
            }
        }
    }
}
