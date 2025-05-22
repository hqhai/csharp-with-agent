// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.CommentCmd
{
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Models;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Application.Commands.AiCmd;
    using Fsel.Interaction.Application.Queues.Publishers;
    using Fsel.Interaction.Application.Services.AIService.Models;
    using Fsel.Interaction.Application.Services.CourseServices;
    using Fsel.Interaction.Application.Services.CourseServices.Models;
    using Fsel.Interaction.Application.Services.CourseServices.QueryModel;
    using Fsel.Interaction.Application.Services.SystemService;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.Enums.ErrorCodes;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Comments;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Interaction.Infrastructure.ValueSettings;
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
        private readonly IMediator _mediator;
        private readonly AppSetting _appSetting;
        private const int MinLength = 20;
        private const int MaxLength = 225;

        public CreateCommentCommandHandler(IMapper mapper,
                                           ICommentRepository commentRepository,
                                           AuthContext authContext,
                                           DiscussionBoardCommentPublisher discussionBoardCommentPublisher,
                                           NotificationMessagePublisher notificationMessagePublisher,
                                           ICourseService courseService,
                                           ISystemService systemService,
                                           IUserService userService,
                                           QuestBoardPublisher questBoardPublisher,
                                           IPostRepository postRepository,
                                           IMediator mediator,
                                           AppSetting appSetting)
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
            _mediator = mediator;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<CommentModel>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CommentModel> methodResult = new MethodResult<CommentModel>();

            if (string.IsNullOrEmpty(request.Content))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCommentErrorCode.ContentNotNull), nameof(request.Content), request.Content);
                return methodResult;
            }

            var userResult = await _userService.GetUserByIdAsync(_authContext.CurrentUserId.ToString());
            var user = userResult.Content?.Result;

            Comment comment = new Comment();
            if (request.IsUpdate)
            {
                var commentQuery = await _commentRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.ObjectId, cancellationToken);
                if (commentQuery == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.ObjectId), request.ObjectId);
                    return methodResult;
                }

                comment = commentQuery;
                comment.Content = request.Content;
            }
            else
            {
                comment = _mapper.Map<Comment>(request);
                comment.UserId = _authContext.CurrentUserId;
                comment.CourseId = user?.CourseId;
            }

            if (!comment.IsValid())
            {
                methodResult.AddErrorBadRequest(comment.ErrorMessages);
                return methodResult;
            }

            // check các tiêu chí
            await AIHandler(request, comment);

            await _commentRepository.ExecuteTransactionAsync(async () =>
            {
                comment = request.IsUpdate ? _commentRepository.Update(comment) : _commentRepository.Add(comment);
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

                        FeatureModuleQuery query = new FeatureModuleQuery
                        {
                            FeatureModule = EnumFeatureModule.ClassForumResult,
                            ObjectId = classForumResult.Id
                        };

                        var moduleResult = await _courseService.GetModuleModel(query);
                        var featureModuleModel = moduleResult?.Content?.Result;

                        paramLinksValue = new List<object>
                                     {
                                        featureModuleModel?.CourseId ?? default,
                                        featureModuleModel?.UnitId ?? default,
                                        featureModuleModel?.LessonId ?? default,
                                        featureModuleModel?.ClassForumResultId ?? default,
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

                            FeatureModuleQuery queryReply = new FeatureModuleQuery
                            {
                                FeatureModule = EnumFeatureModule.ClassForumResult,
                                ObjectId = classForumResultOfCommentOwner?.Id ?? default
                            };

                            var moduleResultReply = await _courseService.GetModuleModel(queryReply);
                            var featureModuleReplyModel = moduleResultReply?.Content?.Result;

                            paramLinksValue = new List<object>
                                     {
                                        featureModuleReplyModel?.CourseId ?? default,
                                        featureModuleReplyModel?.UnitId ?? default,
                                        featureModuleReplyModel?.LessonId ?? default,
                                        featureModuleReplyModel?.ClassForumDetailResultId ?? default,
                                     };

                            await PublishNotification(paramLinksValue, request.ObjectId, EnumNotificationContent.ReplyComment, EnumNotificationType.LinkComment, new List<Guid> { commentOwner!.CreatedUserId }, cancellationToken);
                        }
                        break;
                }

                var commentModel = _mapper.Map<CommentModel>(comment);

                commentModel.FullName = user?.FullName;

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

        private async Task<VoidMethodResult> AIHandler(CreateCommentCommandModel request, Comment comment)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Content);
            VoidMethodResult methodResult = new VoidMethodResult();

            // Check từ khoá cấm
            var listForbiddenWordResult = await _systemService.CheckContainForbiddenWord(request.Content);
            var forbiddenWord = listForbiddenWordResult.Content?.Result;

            // Check max length
            int length = request.Content.Count(c => char.IsLetterOrDigit(c));

            if ((forbiddenWord != null && forbiddenWord.Any()) || length <= MinLength || length >= MaxLength)
            {
                comment.Status = EnumCommentStatus.NotValid;
                return methodResult;
            }

            // Call ChatGpt
            var contentAICheckComment = File.ReadAllText(ResourceSettings.ContentAICheckComment);
            var checkContentForAI = await _mediator.Send(new SubmitAICommand { SettingModel = _appSetting.OpenAiConfig?.ModelCommentAI, SystemRoleAlConfig = contentAICheckComment, UserAIConfig = request.Content });
            if (string.IsNullOrEmpty(checkContentForAI))
            {
                comment.Status = EnumCommentStatus.Pending;
                return methodResult;
            }

            var determination = JsonSerializer.Deserialize<DeterminationData>(checkContentForAI);
            if (determination != null && !string.IsNullOrEmpty(determination.Determination))
            {
                comment.Status = determination.Determination == "YES" ? EnumCommentStatus.NotValid : EnumCommentStatus.Approver;
            }
            else
            {
                comment.Status = EnumCommentStatus.Pending;
            }

            return methodResult;
        }
    }
}
