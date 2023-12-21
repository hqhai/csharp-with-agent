// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.CommentCmd
{
    using System.ComponentModel.Design;
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
        private readonly IUserService _userService;
        private readonly QuestBoardPublisher _questBoardPublisher;

        public CreateCommentCommandHandler(IMapper mapper, ICommentRepository commentRepository, AuthContext authContext, DiscussionBoardCommentPublisher discussionBoardCommentPublisher, NotificationMessagePublisher classForumCommentPublisher, ICourseService courseService, ISystemService systemService, IUserService userService, QuestBoardPublisher questBoardPublisher)
        {
            _mapper = mapper;
            _commentRepository = commentRepository;
            _discussionBoardCommentPublisher = discussionBoardCommentPublisher;
            _classForumCommentPublisher = classForumCommentPublisher;
            _authContext = authContext;
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
            var fullname = await _commentRepository.Queryable.FirstOrDefaultAsync(cancellationToken);

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

                        #region DoQuestBoard
                        if (_authContext.CurrentUserId != postOwner!.CreatedUserId)
                        {
                            await DoMainQuest(classForumResult!.CourseId, comment.Id, cancellationToken);
                            await DoDailyQuest(classForumResult!.CourseId, comment.Id, cancellationToken);
                        }
                        #endregion

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
                var commentModel = _mapper.Map<CommentModel>(comment);

                var userResult = await _userService.GetUserByIdAsync(_authContext.CurrentUserId.ToString());
                commentModel.FullName = userResult.Content?.Result?.FullName;

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = commentModel;
                return methodResult;
            });

            return methodResult;
        }

        public async Task DoMainQuest(Guid courseId, Guid commentId, CancellationToken cancellationToken)
        {
            IList<EnumQuestBoardCategory> categories = new List<EnumQuestBoardCategory>() { EnumQuestBoardCategory.CommentOnOtherPost };
            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentId = student?.Content?.Result?.Id;

            var hasFirstComment = _commentRepository.Queryable.Any(c => c.CreatedUserId == _authContext.CurrentUserId && c.Type == EnumInteractionType.ClassForum);

            if (!hasFirstComment && studentId.HasValue)
            {
                QuestBoardQueueModel questBoardModel = new QuestBoardQueueModel()
                {
                    StudentId = (Guid)studentId!,
                    Categories = categories,
                    AchievedPoint = ValueSettings.QuestBoardPoint.Achieved_Point,
                    ObjectId = commentId,
                    CourseId = courseId,
                };
                await DoQuestBoard(questBoardModel, cancellationToken);
            }
        }

        public async Task DoDailyQuest(Guid courseId, Guid commentId, CancellationToken cancellationToken)
        {
            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentResult = student?.Content?.Result;

            QuestBoardQueueModel questBoardModel = new QuestBoardQueueModel
            {
                StudentId = studentResult!.Id,
                Categories = new List<EnumQuestBoardCategory>(),
                ObjectId = commentId,
                CourseId = courseId,
                AchievedPoint = 0
            };

            await MissionDailyDiscussionBoard(questBoardModel, cancellationToken);
            await MissionDailyClassForum(questBoardModel, studentResult, cancellationToken);
        }

        private async Task DoQuestBoard(QuestBoardQueueModel questBoardModel, CancellationToken cancellationToken)
        {
            await _questBoardPublisher.Publish(questBoardModel, cancellationToken);
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


        private async Task MissionDailyDiscussionBoard(QuestBoardQueueModel questBoardModel, CancellationToken cancellationToken)
        {
            IList<EnumQuestBoardCategory> categories = new List<EnumQuestBoardCategory>() { };
            var isCommentDiscussionBoard = _commentRepository.Queryable.Any(c => c.CreatedUserId == _authContext.CurrentUserId &&
                                                                         c.Type == EnumInteractionType.DiscussionBoard &&
                                                                         c.CreatedDate.Date == DateTime.UtcNow.Date &&
                                                                         c.CreatedDate.Month == DateTime.UtcNow.Month &&
                                                                         c.CreatedDate.Year == DateTime.UtcNow.Year);

            if (isCommentDiscussionBoard)
            {
                questBoardModel.Categories!.Add(EnumQuestBoardCategory.DiscussionBoardInteract);
                questBoardModel.AchievedPoint = ValueSettings.QuestBoardPoint.Achieved_Point;

                await DoQuestBoard(questBoardModel, cancellationToken);
            }

        }

        private async Task MissionDailyClassForum(QuestBoardQueueModel questBoardModel, StudentModel studentResult, CancellationToken cancellationToken)
        {
            var listOwnerObjectId = _commentRepository.Queryable.Where(c => c.CreatedUserId == _authContext.CurrentUserId &&
                                                                          c.Type == EnumInteractionType.ClassForum &&
                                                                          c.CreatedDate.Date == DateTime.UtcNow.Date &&
                                                                          c.CreatedDate.Month == DateTime.UtcNow.Month &&
                                                                          c.CreatedDate.Year == DateTime.UtcNow.Year)
                                                              .Select(x => x.ObjectId)
                                                              .Distinct()
                                                              .ToList();

            var ownerPostIds = await GetListPostOwner(listOwnerObjectId);
            var listStudentOwnerPost = await _userService.GetStudentByUserIdsAsync(ownerPostIds);
            if (listStudentOwnerPost?.Content?.Result != null && studentResult != null)
            {
                var countCommentOnClassMatePost = listStudentOwnerPost?.Content?.Result.Count(x => x.ClassId == studentResult.ClassId);
                questBoardModel.AchievedPoint = (countCommentOnClassMatePost ?? 0);
                questBoardModel.Categories!.Add(EnumQuestBoardCategory.CommentOnNewLessonOfTwoClassMate);

                await DoQuestBoard(questBoardModel, cancellationToken);
            }
        }
    }
}
