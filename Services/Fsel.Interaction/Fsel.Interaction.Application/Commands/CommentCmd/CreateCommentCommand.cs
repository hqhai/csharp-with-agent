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
    using Fsel.Interaction.Application.Services.CourseServices.Models;
    using Fsel.Interaction.Application.Services.SystemService;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Application.Services.UserServices.Models;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Comments;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
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
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly AuthContext _authContext;
        private readonly ICourseService _courseService;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly IPostRepository _postRepository;

        public CreateCommentCommandHandler(IMapper mapper, ICommentRepository commentRepository, AuthContext authContext, DiscussionBoardCommentPublisher discussionBoardCommentPublisher, NotificationMessagePublisher notificationMessagePublisher, ICourseService courseService, ISystemService systemService, IUserService userService, QuestBoardPublisher questBoardPublisher, IPostRepository postRepository)
        {
            _mapper = mapper;
            _commentRepository = commentRepository;
            _discussionBoardCommentPublisher = discussionBoardCommentPublisher;
            _notificationMessagePublisher = notificationMessagePublisher;
            _authContext = authContext;
            _courseService = courseService;
            _systemService = systemService;
            _userService = userService;
            _questBoardPublisher = questBoardPublisher;
            _postRepository = postRepository;
        }

        public async Task<MethodResult<CommentModel>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Content);
            MethodResult<CommentModel> methodResult = new MethodResult<CommentModel>();

            Comment comment = _mapper.Map<Comment>(request);
            comment.UserId = _authContext.CurrentUserId;
            if (!comment.IsValid())
            {
                methodResult.AddErrorBadRequest(comment.ErrorMessages);
                return methodResult;
            }

            #region Check từ khoá cấm

            var listForbiddenWordResult = await _systemService.CheckContainForbiddenWord(request.Content);
            var forbiddenWord = listForbiddenWordResult.Content?.Result;
            if (forbiddenWord == null || forbiddenWord.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumCommentErrorCode.ContainsForbiddenKeywords), string.Join(", ", forbiddenWord));
                return methodResult;
            }

            #endregion Check từ khoá cấm

            await _commentRepository.ExecuteTransactionAsync(async () =>
            {
                comment = _commentRepository.Add(comment);
                await _commentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                List<object> paramLinksValue = new List<object>();
                switch (request.Type)
                {
                    case EnumInteractionType.DiscussionBoard:
                        await _discussionBoardCommentPublisher.Publish(comment, cancellationToken).ConfigureAwait(false);

                        //Gửi thông báo
                        var post = _postRepository.Queryable.FirstOrDefault(x => x.Id == request.ObjectId);
                        if (post != null && post.CreatedUserId != _authContext.CurrentUserId)
                        {
                            await PublishNotification(paramLinksValue, request.ObjectId, EnumNotificationContent.CommentPost, EnumNotificationType.LinkComment, new List<Guid>() { post.CreatedUserId }, cancellationToken);
                        }

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
                        if (classForumResult == null)
                        {
                            break;
                        }

                        var lesson = await _courseService.GetLessonResult(classForumResult.LessonResultId ?? default);
                        paramLinksValue = new List<object>
                                     {
                                        lesson?.Content?.Result?.Id ?? default,
                                        classForumResult?.CourseId ?? default,
                                        classForumResult?.UnitId ?? default,
                                        classForumResult?.Id ?? default,
                                     };

                        await PublishNotification(paramLinksValue, request.ObjectId, EnumNotificationContent.Comment, EnumNotificationType.LinkComment, new List<Guid>() { postOwner.CreatedUserId }, cancellationToken);

                        #region Do QuestBoard

                        var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
                        if (studentResult.IsSuccessStatusCode)
                        {
                            var student = studentResult.Content?.Result;
                            var classForumResultsOfStudentResult = await _courseService.ExecuteListClassForumResultQueryAsync(new BaseQueryModel
                            {
                                Filters = new List<GenericFilterModel>
                                            {
                                                new GenericFilterModel
                                                {
                                                    Property = "StudentId",
                                                    Value = student!.Id,
                                                    Operator = Common.Enums.EnumFilterOperator.Equal
                                                }
                                            },
                                IncludePaths = new List<string> { "LessonResult" }
                            });
                            var classForumResultsOfStudent = classForumResultsOfStudentResult.Content?.Result;
                            var classForumResultOfStudent = classForumResultsOfStudent?.OrderByDescending(p => p.CreatedDate).FirstOrDefault();
                            if (classForumResultOfStudent?.Id == postOwner?.Id)
                            {
                                await DoQuestBoard(student.Id, cancellationToken);
                            }
                        }

                        #endregion Do QuestBoard

                        break;

                    case EnumInteractionType.ReplyComment:

                        var commentOwner = await _commentRepository.GetByIdAsync(request.ObjectId).ConfigureAwait(false);

                        //Không thông báo khi trả lời bình luận của chính mình
                        if (commentOwner == null || commentOwner?.CreatedUserId == _authContext.CurrentUserId)
                        {
                            break;
                        }

                        var postResult = await _postRepository.Queryable.FirstOrDefaultAsync(x => x.Id == commentOwner!.ObjectId);
                        if (postResult != null)
                        {
                            var resultId = postResult.Id; // sau để gán vào paramLink
                            await PublishNotification(paramLinksValue, request.ObjectId, EnumNotificationContent.ReplyCommentPost, EnumNotificationType.LinkComment, new List<Guid> { commentOwner!.CreatedUserId }, cancellationToken);
                        }
                        else
                        {
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
                            var lessonOfCommentOwner = await _courseService.GetLessonResult(classForumResultOfCommentOwner?.LessonResultId ?? default);

                            paramLinksValue = new List<object>
                                    {
                                        lessonOfCommentOwner?.Content?.Result?.Id ?? default,
                                        classForumResultOfCommentOwner?.CourseId ?? default,
                                        classForumResultOfCommentOwner?.UnitId ?? default,
                                        classForumResultOfCommentOwner?.Id ?? default,
                                    };

                            await PublishNotification(paramLinksValue, request.ObjectId, EnumNotificationContent.ReplyComment, EnumNotificationType.LinkComment, new List<Guid> { commentOwner!.CreatedUserId }, cancellationToken);
                        }
                        break;
                }

                var commentModel = _mapper.Map<CommentModel>(comment);
                var userResult = await _userService.GetUserByIdAsync(_authContext.CurrentUserId.ToString());
                commentModel.FullName = userResult.Content?.Result?.FullName;

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = commentModel;
                return methodResult;
            });

            return methodResult;
        }

        private async Task DoQuestBoard(Guid studentId, CancellationToken cancellationToken)
        {
            await _questBoardPublisher.Publish(new QuestBoardQueueModel()
            {
                StudentID = studentId,
                Type = EnumQuestBoardType.LearningQuests,
                Category = EnumQuestBoardCategory.ConnectingAllies,
                Value = 1
            }, cancellationToken);
        }

        private async Task<List<Guid>> GetListPostOwner(IList<Guid> objectIds)
        {
            var classForumResultResult = await _courseService.ExecuteListClassForumResultQueryAsync(new BaseQueryModel
            {
                Filters = new List<GenericFilterModel>
                 {
                     new GenericFilterModel
                     {
                         Property = nameof(ClassForumResultModel.Id),
                         Value = objectIds,
                         Operator = Common.Enums.EnumFilterOperator.In
                     }
                 }
            });

            var result = classForumResultResult?.Content?.Result?.Select(x => x.CreatedUserId).Distinct().ToList() ?? new List<Guid>();

            return result;
        }

        private async Task PublishNotification(List<object> paramLinks, Guid objectId, EnumNotificationContent enumNotificationContent, EnumNotificationType enumNotificationType, List<Guid> userIds, CancellationToken cancellationToken)
        {
            var model = new NotificationSendingQueueModel()
            {
                ParamsMessage = new List<object> { _authContext.CurrentUsername ?? string.Empty },
                ObjectId = objectId,
                Content = enumNotificationContent,
                Type = enumNotificationType,
                SenderId = _authContext.CurrentUserId,
                ParamsLink = paramLinks,
                UserIds = userIds,
            };

            await _notificationMessagePublisher.Publish(model, cancellationToken).ConfigureAwait(false);
        }
    }
}
