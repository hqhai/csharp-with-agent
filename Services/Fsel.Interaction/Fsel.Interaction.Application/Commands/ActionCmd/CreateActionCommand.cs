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

        public CreateActionCommandHandler(IMapper mapper, IInteractionActionRepository interactionActionRepository, AuthContext authContext, DiscussionBoardLikePublisher discussionBoardLikePublisher, InterationActionPublisher interationActionPublisher, NotificationMessagePublisher notificationMessagePublisher, IUserService userService, ITrainingService trainingService, ICourseService courseService, ICommentRepository commentRepository, QuestBoardPublisher questBoardPublisher)
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

            await _interactionActionRepository.ExecuteTransactionAsync(async () =>
            {
                if (action == null)
                {
                    action = _mapper.Map<InteractionAction>(request);
                    action.UserId = _authContext.CurrentUserId;

                    if (!action.IsValid())
                    {
                        methodResult.AddErrorBadRequest(action.ErrorMessages);
                        return methodResult;
                    }

                    action = _interactionActionRepository.Add(action);

                    var businessType = EnumNotificationType.LinkPage;
                    var businessContent = EnumNotificationContent.LikeClassForum;

                    //class forum
                    var classForumResultTemp = await GetClassForumResultModel(request.ObjectId);

                    var (returnedParamsLink, objectOwnerId) = CustomDataForParamMessage(request.ObjectId, classForumResultTemp!, classForumResultTemp?.CourseId, classForumResultTemp?.UnitId);
                    bool conditionCheckIsClassForum = await CheckObjecIsClassForum(request.ObjectId);
                    if (!conditionCheckIsClassForum)
                    {
                        var comment = await _commentRepository.GetByIdAsync(request.ObjectId);
                        classForumResultTemp = await GetClassForumResultModel(comment!.ObjectId);
                        (returnedParamsLink, objectOwnerId) = CustomDataForParamMessage(request.ObjectId, comment!, classForumResultTemp?.CourseId, classForumResultTemp?.UnitId);

                        businessType = EnumNotificationType.LinkComment;
                        businessContent = EnumNotificationContent.LikeComment;
                    }

                    if (action.Type == EnumInteractionActionType.Like)
                    {
                        NotificationSendingQueueModel model = new NotificationSendingQueueModel()
                        {
                            ObjectId = request.ObjectId,
                            UserIds = new List<Guid>() { objectOwnerId },
                            SenderId = _authContext.CurrentUserId,
                            ParamsMessage = new List<object> { _authContext.CurrentFullName! ?? string.Empty, },
                            ParamsLink = returnedParamsLink,
                            Type = businessType,
                            Content = businessContent,
                            PlatformCode = EnumPlatformCode.LMS
                        };
                        await _notificationMessagePublisher.Publish(model, cancellationToken).ConfigureAwait(false);
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

                await DoDailyQuest(action.Id, cancellationToken);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }

        public async Task DoDailyQuest(Guid interationId, CancellationToken cancellationToken)
        {
            IList<EnumQuestBoardCategory> categories = new List<EnumQuestBoardCategory>() { EnumQuestBoardCategory.CommentOnNewLessonOfTwoClassMate };
            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentResult = student?.Content?.Result;
            var isInteractDiscussionBoard = _interactionActionRepository.Queryable.Any(c => c.CreatedUserId == _authContext.CurrentUserId &&
                                                                          c.Type == EnumInteractionActionType.Like &&
                                                                          c.BusinessType == EnumInteractionType.DiscussionBoard &&
                                                                          c.CreatedDate.Date == DateTime.UtcNow.Date &&
                                                                          c.CreatedDate.Month == DateTime.UtcNow.Month &&
                                                                          c.CreatedDate.Year == DateTime.UtcNow.Year);

            var studentClassInfo = await _trainingService.GetClassByStudentId(studentResult!.Id);
            var courseId = studentClassInfo?.Content?.Result?.CourseId;

            if (isInteractDiscussionBoard && studentResult != null && courseId != null)
            {
                QuestBoardQueueModel questBoardModel = new QuestBoardQueueModel()
                {
                    StudentId = studentResult.Id,
                    Categories = categories,
                    AchievedPoint = ValueSettings.QuestBoardPoint.Achieved_Point,
                    ObjectId = interationId,
                    CourseId = (Guid)courseId!,
                };

                await _questBoardPublisher.Publish(questBoardModel, cancellationToken);
            }
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
        public (List<object> paramsLink, Guid ownerObjectId) CustomDataForParamMessage(Guid objectId, dynamic templateResult, Guid? courseId, Guid? unitId)
        {
            if (templateResult == null)
            {
                return (new List<object>(), Guid.Empty);
            }

            // param
            var paramsLink = new List<object> { unitId?.ToString() ?? string.Empty, courseId?.ToString() ?? string.Empty, templateResult?.Id.ToString() ?? string.Empty, objectId };
            var ownerObjectId = templateResult?.CreatedUserId ?? default;


            return (paramsLink, ownerObjectId);
        }
    }
}
