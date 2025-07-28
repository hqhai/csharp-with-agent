// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.ActionCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Actions;
    using Fsel.Interaction.Application.Queues.Publishers;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Application.Services.TrainingServices;
    using Fsel.Interaction.Application.Services.CourseServices.Models;
    using Fsel.Interaction.Application.Services.CourseServices;
    using Fsel.Shared.Constants;
    using Fsel.Interaction.Application.Services.UserServices.Models;
    using Kros.Extensions;
    using Fsel.Interaction.Infrastructure.Repositories;
    using Fsel.Interaction.Application.Services.CourseServices.QueryModel;

    public class CreateActionCommand : CreateActionCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateActionCommandHandler : IRequestHandler<CreateActionCommand, MethodResult<bool>>
    {
        private readonly IMapper _mapper;
        private readonly IInteractionActionRepository _interactionActionRepository;
        private readonly DiscussionBoardLikePublisher _discussionBoardLikePublisher;
        private readonly InterationActionPublisher _interationActionPublisher;
        private readonly AuthContext _authContext;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly IUserService _userService;
        private readonly ITrainingService _trainingService;
        private readonly ICourseService _courseService;
        private readonly ICommentRepository _commentRepository;
        private readonly QuestBoardPublisher _questBoardPublisher;
        private readonly IPostRepository _postRepository;

        public CreateActionCommandHandler(IMapper mapper, IInteractionActionRepository interactionActionRepository, AuthContext authContext, DiscussionBoardLikePublisher discussionBoardLikePublisher, InterationActionPublisher interationActionPublisher, NotificationMessagePublisher notificationMessagePublisher, IUserService userService, ITrainingService trainingService, ICourseService courseService, ICommentRepository commentRepository, QuestBoardPublisher questBoardPublisher, IPostRepository postRepository)
        {
            _mapper = mapper;
            _interactionActionRepository = interactionActionRepository;
            _authContext = authContext;
            _discussionBoardLikePublisher = discussionBoardLikePublisher;
            _interationActionPublisher = interationActionPublisher;
            _notificationMessagePublisher = notificationMessagePublisher;
            _userService = userService;
            _trainingService = trainingService;
            _courseService = courseService;
            _commentRepository = commentRepository;
            _questBoardPublisher = questBoardPublisher;
            _postRepository = postRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateActionCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            #region
            /* if (action != null)
             {
                 if (request.Type == EnumInteractionActionType.Like)
                 {
                     await _interactionActionRepository.DeleteAsync(action);
                 }
                 else if (request.Type != EnumInteractionActionType.Like)
                 {
                     methodResult.Result = true;
                     return methodResult;
                 }
             }*/
            #endregion
            var action = await _interactionActionRepository.Queryable
                            .Where(x => x.UserId == _authContext.CurrentUserId &&
                                        x.ObjectId == request.ObjectId &&
                                        x.Type == request.Type)
                            .FirstOrDefaultAsync(cancellationToken);

            var userResult = await _userService.GetUserByIdAsync(_authContext.CurrentUserId.ToString());
            var user = userResult.Content?.Result;

            await _interactionActionRepository.ExecuteTransactionAsync(async () =>
            {
                if (action == null)
                {
                    action = _mapper.Map<InteractionAction>(request);
                    action.UserId = _authContext.CurrentUserId;
                    action.CourseId = user?.CourseId;

                    if (!action.IsValid())
                    {
                        methodResult.AddErrorBadRequest(action.ErrorMessages);
                        return methodResult;
                    }
                    action = _interactionActionRepository.Add(action);

                    var postResult = await _postRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.ObjectId);

                    // Discussion Board
                    if (postResult != null)
                    {
                        var userNameLiked = await CustomNotificationLikeMessage(action.ObjectId, _authContext.CurrentUserId);

                        InterationActionQueueModel model = new InterationActionQueueModel()
                        {
                            ObjectId = request.ObjectId,
                            UserIds = new List<Guid>() { postResult.CreatedUserId },
                            SenderId = _authContext.CurrentUserId,
                            ParamsMessage = new List<object> { userNameLiked ?? string.Empty },
                            ParamsLink = new List<object>(),
                            Type = EnumNotificationType.LinkPage,
                            Content = EnumNotificationContent.LikePost,
                            InterationType = action.Type,
                            PlatformCode = EnumPlatformCode.LMS
                        };
                        await _interationActionPublisher.Publish(model, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        //class forum
                        var businessType = EnumNotificationType.LinkPage;
                        var businessContent = EnumNotificationContent.LikeClassForum;

                        var classForumResultTemp = await GetClassForumResultModel(request.ObjectId);

                        var lesson = await _courseService.GetLessonResult(classForumResultTemp.LessonResultId ?? default);

                        var objectOwnerId = CustomDataForParamMessage(classForumResultTemp!);

                        FeatureModuleQuery query = new FeatureModuleQuery
                        {
                            FeatureModule = EnumFeatureModule.ClassForumResult,
                            ObjectId = classForumResultTemp?.Id ?? default,
                            UserId = _authContext.CurrentUserId,
                        };
                        var moduleResultReply = await _courseService.GetModuleModel(query);
                        var featureModuleReplyModel = moduleResultReply?.Content?.Result;

                        var paramLinksValue = new List<object>
                                     {
                                        featureModuleReplyModel?.CourseId ?? default,
                                        featureModuleReplyModel?.UnitId ?? default,
                                        featureModuleReplyModel?.LessonId ?? default,
                                        featureModuleReplyModel?.ClassForumDetailResultId ?? default,
                                     };

                        bool conditionCheckIsClassForum = await CheckObjecIsClassForum(request.ObjectId);
                        if (!conditionCheckIsClassForum)
                        {
                            var comment = await _commentRepository.GetByIdAsync(request.ObjectId);
                            classForumResultTemp = await GetClassForumResultModel(comment!.ObjectId);
                            objectOwnerId = CustomDataForParamMessage(comment!);

                            businessType = EnumNotificationType.LinkComment;
                            businessContent = EnumNotificationContent.LikeComment;
                        }

                        if (action.Type == EnumInteractionActionType.Like)
                        {
                            var postOwnerResult = await _courseService.GetClassForumResultByIdAsync(request.ObjectId).ConfigureAwait(false);
                            var postOwner = postOwnerResult.Content?.Result;

                            //Không tìm thấy postOwner và Không thông báo khi like bài viết của chính mình
                            if (postOwner != null && postOwner!.CreatedUserId != _authContext.CurrentUserId)
                            {
                                var userNameLiked = await CustomNotificationLikeMessage(action.ObjectId, _authContext.CurrentUserId);

                                InterationActionQueueModel model = new InterationActionQueueModel()
                                {
                                    ObjectId = request.ObjectId,
                                    UserIds = new List<Guid>() { objectOwnerId },
                                    SenderId = _authContext.CurrentUserId,
                                    ParamsMessage = new List<object> { userNameLiked ?? string.Empty },
                                    ParamsLink = paramLinksValue,
                                    Type = businessType,
                                    Content = businessContent,
                                    InterationType = action.Type,
                                    PlatformCode = EnumPlatformCode.LMS
                                };
                                await _interationActionPublisher.Publish(model, cancellationToken).ConfigureAwait(false);
                            }
                        }
                    }
                }
                else if (action.Type == EnumInteractionActionType.Like)
                {
                    await _interactionActionRepository.DeleteAsync(action);
                }
                else if (action.Type == EnumInteractionActionType.Disable)
                {
                    InterationActionQueueModel model = new InterationActionQueueModel()
                    {
                        ObjectId = action.ObjectId,
                        InterationType = action.Type,
                        Type = EnumNotificationType.LinkPage,
                        Content = EnumNotificationContent.FlagClassForum,
                        UserIds = new List<Guid> { action.CreatedUserId },
                        SenderId = _authContext.CurrentUserId
                    };

                    await _interationActionPublisher.Publish(model, cancellationToken).ConfigureAwait(false);
                }

                await _interactionActionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                //await DoDailyQuest(action.Id, cancellationToken);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }

        /// <summary>
        /// Laasy duwx lijeeu cuar Clas
        /// </summary>
        /// <param name="courseService"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<ClassForumResultModel> GetClassForumResultModel(Guid id)
        {
            if (_courseService == null)
            {
                return new ClassForumResultModel();
            }

            var response = await _courseService.GetClassForumResultByIdAsync(id) ?? default;

            return response?.Content?.Result ?? new ClassForumResultModel();
        }

        /// <summary>
        /// Kiểm tra xem ObjectId truyền vào có phải là ClassForum hay không
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="commentRepository"></param>
        /// <param name="courseService"></param>
        /// <returns></returns>
        public async Task<bool> CheckObjecIsClassForum(Guid objectId)
        {
            bool isClassForum = true;

            var comment = await _commentRepository.GetByIdAsync(objectId);

            var classForum = await _courseService.GetClassForumResultByIdAsync(objectId);

            if (comment != null && classForum?.Content?.Result == null)
            {
                isClassForum = false;
            }

            return isClassForum;
        }

        /// <summary>
        /// Lấy tham số để truyền vào link, message
        /// </summary>
        /// <param name="_courseService"></param>
        /// <param name="objectId"></param>
        /// <param name="userService"></param>
        /// <param name="trainingService"></param>
        /// <returns></returns>
        public Guid CustomDataForParamMessage(dynamic templateResult)
        {
            if (templateResult == null)
            {
                return Guid.Empty;
            }
            // param

            var ownerObjectId = templateResult?.CreatedUserId ?? default;

            return ownerObjectId;
        }

        /// <summary>
        /// Custom lại Message khi một tài khoản like bài viết, comment của một tài khoản khác.
        /// </summary>
        /// <returns></returns>
        public async Task<string> CustomNotificationLikeMessage(Guid objectId, Guid userId)
        {
            string result = "";
            List<Guid> listUserIdsLiked = new List<Guid>();
            List<string?> listNameUserLiked = new List<string?>();

            listUserIdsLiked = _interactionActionRepository.Queryable.OrderByDescending(x => x.CreatedDate).Where(x => x.Type == EnumInteractionActionType.Like && x.ObjectId == objectId).Select(x => x.UserId).ToList();
            listUserIdsLiked.Add(userId);

            GetUsersByIdsQueryModel model = new GetUsersByIdsQueryModel() { UserIds = listUserIdsLiked };
            var listUserQuery = await _userService.GetUsersByIdsAsync(model);
            var listUserQueryResult = listUserQuery?.Content?.Result!;

            listNameUserLiked = listUserQueryResult.Where(x => x.UserId != userId).Select(x => x.FullName).ToList() ?? new List<string?>();
            var userActionRecently = listUserQueryResult.Where(x => x.UserId == userId).Select(x => x.FullName).Single() ?? string.Empty;

            int totalLiked = listNameUserLiked.Count + 1; // 1 like của người vừa like bài viết "userActionRecently"

            switch (totalLiked)
            {
                case ValueSettings.CreateAction.NoOneAction:
                    break;

                case ValueSettings.CreateAction.OnePeopleAction:
                    result = userActionRecently;
                    break;

                case ValueSettings.CreateAction.TwoPeopleAction:
                    result = ValueSettings.CreateAction.TwoPeopleLike.Format(userActionRecently, listNameUserLiked[0]);
                    break;

                default:
                    result = ValueSettings.CreateAction.ThreePeopleOrMoreLike.Format(userActionRecently, listNameUserLiked.Count);
                    break;
            }
            return result;
        }
    }
}
