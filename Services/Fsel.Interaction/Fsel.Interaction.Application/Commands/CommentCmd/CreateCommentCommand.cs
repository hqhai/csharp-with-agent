// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.CommentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Comments;
    using Fsel.Interaction.Application.Queues.Publishers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MassTransit.Initializers;
    using Fsel.Shared.Constants;
    using Kros.Extensions;
    using Fsel.Interaction.Application.Services.CourseServices;

    public class CreateCommentCommand : CreateCommentCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, MethodResult<bool>>
    {
        private readonly IMapper _mapper;
        private readonly ICommentRepository _commentRepository;
        private readonly DiscussionBoardCommentPublisher _discussionBoardCommentPublisher;
        private readonly ClassForumCommentPublisher _classForumCommentPublisher;
        private readonly AuthContext _authContext;
        private readonly ICourseService _courseService;

        public CreateCommentCommandHandler(IMapper mapper, ICommentRepository commentRepository, AuthContext authContext, DiscussionBoardCommentPublisher discussionBoardCommentPublisher, ClassForumCommentPublisher classForumCommentPublisher, ICourseService courseService)
        {
            _mapper = mapper;
            _commentRepository = commentRepository;
            _authContext = authContext;
            _discussionBoardCommentPublisher = discussionBoardCommentPublisher;
            _classForumCommentPublisher = classForumCommentPublisher;
            _courseService = courseService;
        }

        public async Task<MethodResult<bool>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            Comment comment = _mapper.Map<Comment>(request);
            await _commentRepository.Queryable.FirstOrDefaultAsync(x => x.ObjectId == request.ObjectId, cancellationToken);
            comment.UserId = _authContext.CurrentUserId;
            if (!comment.IsValid())
            {
                methodResult.AddErrorBadRequest(comment.ErrorMessages);
                return methodResult;
            }
            await _commentRepository.ExecuteTransactionAsync(async () =>
            {
                comment = _commentRepository.Add(comment);
                await _commentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                NotificationQueueModel model = new NotificationQueueModel();
                switch (request.Type)
                {
                    case EnumCommentType.DiscussionBoard:
                        await _discussionBoardCommentPublisher.Publish(comment, cancellationToken).ConfigureAwait(false);

                        break;
                    case EnumCommentType.ClassForum:

                        var postOwner = await _courseService.GetClassForumResultByIdAsync(request.ObjectId).Select(x => x.Content?.Result?.ClassForum?.ClassForumResults?.FirstOrDefault()).ConfigureAwait(false);

                        //Không thông báo khi comment bài viết của chính mình
                        if (postOwner!.CreatedUserId == _authContext.CurrentUserId)
                        {
                            break;
                        }

                        model = new NotificationQueueModel()
                        {
                            ParamsMessage = new List<object> { _authContext.CurrentUsername! ?? string.Empty, },
                            ObjectId = request.ObjectId,
                            UserId = postOwner!.CreatedUserId,
                            Content = EnumNotificationContent.Comment,
                            Type = EnumNotificationType.LinkComment

                        };
                        await _classForumCommentPublisher.Publish(model, cancellationToken).ConfigureAwait(false);
                        break;
                    case EnumCommentType.ReplyComment:

                        var commentOwnerId = await _commentRepository.GetByIdAsync(request.ObjectId).Select(x => x!.CreatedUserId).ConfigureAwait(false);

                        //Không thông báo khi trả lời bình luận của chính mình
                        if (commentOwnerId == _authContext.CurrentUserId)
                        {
                            break;
                        }

                        model = new NotificationQueueModel()
                        {
                            ParamsMessage = new List<object> { _authContext.CurrentUsername ?? string.Empty },
                            ObjectId = request.ObjectId,
                            UserId = commentOwnerId,
                            Content = EnumNotificationContent.Comment,
                            Type = EnumNotificationType.LinkComment
                        };
                        await _classForumCommentPublisher.Publish(model, cancellationToken).ConfigureAwait(false);
                        break;
                }

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
