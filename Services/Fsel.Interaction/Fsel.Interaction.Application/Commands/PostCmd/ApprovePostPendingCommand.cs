// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.PostCmd
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Interaction.Application.Queues.Publishers;
    using Fsel.Interaction.Application.Services.UserServices;
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
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly IUserService _userService;

        public ApprovePostPendingCommandHandler(IMapper mapper, IPostRepository postRepository, QuestBoardPublisher questBoardPublisher, IUserService userService)
        {
            _mapper = mapper;
            _postRepository = postRepository;
            _questBoardPublisher = questBoardPublisher;
            _userService = userService;
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

                await _postRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<PostModel>(post);
                return methodResult;
            });

            return methodResult;
        }

        /*public async Task DoQuestBoard(Guid classForumResultId, CancellationToken cancellationToken)
        {
            IList<EnumQuestBoardCategory> categories = new List<EnumQuestBoardCategory>() { EnumQuestBoardCategory.SeeFiveTeacherReview, EnumQuestBoardCategory.SeeTenTeacherReview };
            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentId = student?.Content?.Result?.Id;

            var archievePoint = await _classForumResultRepository.Queryable
                            .Where(x => x.Id == classForumResultId && x.IsViewed && x.StudentId == studentId)
                            .ToListAsync(cancellationToken);
            var archievePointCount = archievePoint.Count;

            await _questBoardPublisher.Publish(new QuestBoardQueueModel
            {
                StudentId = (Guid)studentId!,
                Categories = categories,
                AchievedPoint = archievePointCount,
            }, cancellationToken);
        }*/
    }
}
