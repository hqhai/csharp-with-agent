// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.CommentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Models;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Application.Queues.Publishers;
    using Fsel.Interaction.Application.Services.CourseServices;
    using Fsel.Interaction.Application.Services.SystemService;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Comments;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;

    public class CreateCommentCommand : CreateCommentCommandModel, IRequest<MethodResult<CommentModel>>
    {
    }

    public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, MethodResult<CommentModel>>
    {
        private readonly IMapper _mapper;
        private readonly ICommentRepository _commentRepository;
        private readonly DiscussionBoardCommentPublisher _discussionBoardCommentPublisher;
        private readonly NotificationMessagePublisher _classForumCommentPublisher;
        private readonly AuthContext _authContext;
        private readonly ICourseService _courseService;
        private readonly ISystemService _systemService;

        public CreateCommentCommandHandler(IMapper mapper, ICommentRepository commentRepository, AuthContext authContext, DiscussionBoardCommentPublisher discussionBoardCommentPublisher, NotificationMessagePublisher classForumCommentPublisher, ICourseService courseService, ISystemService systemService)
        {
            _mapper = mapper;
            _commentRepository = commentRepository;
            _authContext = authContext;
            _discussionBoardCommentPublisher = discussionBoardCommentPublisher;
            _classForumCommentPublisher = classForumCommentPublisher;
            _courseService = courseService;
            _systemService = systemService;
        }

        public async Task<MethodResult<CommentModel>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CommentModel> methodResult = new MethodResult<CommentModel>();
            Comment comment = _mapper.Map<Comment>(request);
            // Check từ khoá cấm
            var listForbiddenWordResult = await _systemService.CheckContainForbiddenWord(request.Content);
            var forbiddenWord = listForbiddenWordResult.Content?.Result;
            if (forbiddenWord.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumCommentErrorCode.ContainsForbiddenKeywords), string.Join(", ", forbiddenWord));
                return methodResult;
            }
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

                NotificationSendingQueueModel model = new NotificationSendingQueueModel();
                List<object> paramLinksValue = new List<object>();
                switch (request.Type)
                {
                    case EnumInteractionType.DiscussionBoard:
                        await _discussionBoardCommentPublisher.Publish(comment, cancellationToken).ConfigureAwait(false);

                        break;

                    case EnumInteractionType.ClassForum:
                        var postOwnerResult = await _courseService.GetClassForumResultByIdAsync(request.ObjectId).ConfigureAwait(false);
                        var postOwner = postOwnerResult.Content?.Result;

                        //Không tìm thấy postOwner và Không thông báo khi comment bài viết của chính mình
                        if (postOwner == null || postOwner!.CreatedUserId == _authContext.CurrentUserId)
                        {
                            break;
                        }

                        var classForumResultResult = await _courseService.GetClassForumResultInfoByIdAsync(new BaseQueryModel
                        {
                            Filters = new List<GenericFilterModel>
                                            {
                                                new GenericFilterModel
                                                {
                                                    Property = "Id",
                                                    Value = request.ObjectId,
                                                    Operator = Common.Enums.EnumFilterOperator.Equal
                                                }
                                            },
                            IncludePaths = new List<string> { "LessonResult" }
                        });
                        var classForumResult = classForumResultResult.Content?.Result;

                        paramLinksValue = new List<object> { classForumResult?.UnitId ?? default, classForumResult?.CourseId ?? default, request.ObjectId, comment.Id };

                        // Chuyển đổi danh sách thành JSON
                        string paramLinks = JsonConvert.SerializeObject(paramLinksValue);

                        model = new NotificationSendingQueueModel()
                        {
                            ObjectId = request.ObjectId,
                            UserIds = new List<Guid>() { postOwner.CreatedUserId },
                            SenderId = _authContext.CurrentUserId,
                            Content = EnumNotificationContent.Comment,
                            Type = EnumNotificationType.LinkComment,
                            ParamsLink = paramLinksValue,
                            ParamsMessage = new List<object> { _authContext.CurrentUsername! ?? string.Empty, }
                        };
                        await _classForumCommentPublisher.Publish(model, cancellationToken).ConfigureAwait(false);
                        break;

                    case EnumInteractionType.ReplyComment:

                        var commentOwner = await _commentRepository.GetByIdAsync(request.ObjectId).ConfigureAwait(false);

                        //Không thông báo khi trả lời bình luận của chính mình
                        if (commentOwner == null && commentOwner?.CreatedUserId == _authContext.CurrentUserId)
                        {
                            break;
                        }

                        var classForumResultOfCommentOwnerResult = await _courseService.GetClassForumResultInfoByIdAsync(new BaseQueryModel
                        {
                            Filters = new List<GenericFilterModel>
                                            {
                                                new GenericFilterModel
                                                {
                                                    Property = "Id",
                                                    Value = commentOwner?.ObjectId,
                                                    Operator = Common.Enums.EnumFilterOperator.Equal
                                                }
                                            },
                            IncludePaths = new List<string> { "LessonResult" }
                        });
                        var classForumResultOfCommentOwner = classForumResultOfCommentOwnerResult.Content?.Result;

                        paramLinksValue = new List<object>
                                    {
                                        classForumResultOfCommentOwner?.UnitId ?? default,
                                        classForumResultOfCommentOwner?.CourseId ?? default,
                                        commentOwner?.ObjectId ?? default,
                                        request.ObjectId,
                                        comment.Id
                                    };

                        // Chuyển đổi danh sách thành JSON
                        paramLinks = JsonConvert.SerializeObject(paramLinksValue);

                        model = new NotificationSendingQueueModel()
                        {
                            ParamsMessage = new List<object> { _authContext.CurrentUsername ?? string.Empty },
                            ObjectId = request.ObjectId,
                            Content = EnumNotificationContent.ReplyComment,
                            Type = EnumNotificationType.LinkComment,
                            SenderId = _authContext.CurrentUserId,
                            ParamsLink = paramLinksValue,
                            UserIds = new List<Guid> { commentOwner!.CreatedUserId },
                        };

                        await _classForumCommentPublisher.Publish(model, cancellationToken).ConfigureAwait(false);
                        break;
                }

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CommentModel>(comment);
                return methodResult;
            });

            return methodResult;
        }
    }
}
