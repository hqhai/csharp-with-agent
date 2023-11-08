// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.CommentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Interaction.Application.Queues.Publishers;
    using Fsel.Interaction.Application.Services.CourseServices;
    using Fsel.Interaction.Application.Services.SystemService;
    using Fsel.Interaction.Application.Services.SystemService.Models;
    using Fsel.Interaction.Application.Services.UserServices;
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
        private readonly IUserService _userService;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private const float Archieve_Point = 1;

        public CreateCommentCommandHandler(IMapper mapper, ICommentRepository commentRepository, AuthContext authContext, DiscussionBoardCommentPublisher discussionBoardCommentPublisher, NotificationMessagePublisher classForumCommentPublisher, ICourseService courseService, ISystemService systemService, IUserService userService, QuestBoardPublisher questBoardPublisher)
        {
            _mapper = mapper;
            _commentRepository = commentRepository;
            _authContext = authContext;
            _discussionBoardCommentPublisher = discussionBoardCommentPublisher;
            _classForumCommentPublisher = classForumCommentPublisher;
            _courseService = courseService;
            _systemService = systemService;
            _userService = userService;
            _questBoardPublisher = questBoardPublisher;
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

                NotificationQueueModel model = new NotificationQueueModel();
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

                        model = new NotificationQueueModel()
                        {
                            ParamsMessage = new List<object> { _authContext.CurrentUsername! ?? string.Empty, },
                            ObjectId = request.ObjectId,
                            UserId = postOwner!.CreatedUserId,
                            Content = EnumNotificationContent.Comment,
                            Type = EnumNotificationType.LinkComment,
                            SenderId = _authContext.CurrentUserId,
                            ParamsLink = new List<object> { classForumResult?.UnitId ?? default, classForumResult?.CourseId ?? default, request.ObjectId, comment.Id },
                            PlatformCode = EnumPlatformCode.LMS
                        };

                        #region DoQuestBoard
                        if (_authContext.CurrentUserId != postOwner!.CreatedUserId)
                        {
                            await DoQuestBoard(cancellationToken);
                        }
                        #endregion

                        await _classForumCommentPublisher.Publish(model, cancellationToken).ConfigureAwait(false);

                        break;

                    case EnumInteractionType.ReplyComment:

                        var commentOwner = await _commentRepository.GetByIdAsync(request.ObjectId).ConfigureAwait(false);

                        //Không thông báo khi trả lời bình luận của chính mình
                        if (commentOwner?.CreatedUserId == _authContext.CurrentUserId)
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

                        model = new NotificationQueueModel()
                        {
                            ParamsMessage = new List<object> { _authContext.CurrentUsername ?? string.Empty },
                            ObjectId = request.ObjectId,
                            UserId = commentOwner?.CreatedUserId,
                            Content = EnumNotificationContent.ReplyComment,
                            Type = EnumNotificationType.LinkComment,
                            SenderId = _authContext.CurrentUserId,
                            ParamsLink = new List<object> { classForumResultOfCommentOwner?.UnitId ?? default, classForumResultOfCommentOwner?.CourseId ?? default, commentOwner?.ObjectId ?? default, request.ObjectId, comment.Id },
                            PlatformCode = EnumPlatformCode.LMS
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

        public async Task DoQuestBoard(CancellationToken cancellationToken)
        {
            IList<EnumQuestBoardCategory> categories = new List<EnumQuestBoardCategory>() { EnumQuestBoardCategory.CommentOnOtherPost };
            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentId = student?.Content?.Result?.Id;

            await _questBoardPublisher.Publish(new QuestBoardQueueModel
            {
                StudentId = (Guid)studentId!,
                Categories = categories,
                AchievedPoint = Archieve_Point,
            }, cancellationToken);  
        }
    }
}
