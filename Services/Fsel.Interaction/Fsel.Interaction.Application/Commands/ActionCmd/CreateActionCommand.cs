// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.ActionCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Queues.Publishers;
    using Fsel.Interaction.Application.Services.CourseServices;
    using Fsel.Interaction.Application.Services.CourseServices.Models;
    using Fsel.Interaction.Application.Services.TrainingServices;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Actions;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Refit;

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
        private readonly ICommentRepository _commentRepository;
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;
        private readonly ITrainingService _trainingService;

        public CreateActionCommandHandler(IMapper mapper, IInteractionActionRepository interactionActionRepository, AuthContext authContext, DiscussionBoardLikePublisher discussionBoardLikePublisher, InterationActionPublisher interationActionPublisher, ICourseService courseService, ICommentRepository commentRepository, IUserService userService, ITrainingService trainingService)
        {
            _mapper = mapper;
            _interactionActionRepository = interactionActionRepository;
            _authContext = authContext;
            _discussionBoardLikePublisher = discussionBoardLikePublisher;
            _interationActionPublisher = interationActionPublisher;
            _courseService = courseService;
            _commentRepository = commentRepository;
            _userService = userService;
            _trainingService = trainingService;
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

                    if (action.Type == EnumInteractionActionType.Like)
                    {
                        await _discussionBoardLikePublisher.Publish(action.ObjectId, cancellationToken).ConfigureAwait(false);
                    }
                }
                else if (action.Type == EnumInteractionActionType.Like)
                {
                    await _interactionActionRepository.DeleteAsync(action);
                }
                else if (action.Type == EnumInteractionActionType.Disable || action.Type == EnumInteractionActionType.Flag)
                {
                    var classForumResult = await GetClassForumResultModel(_courseService, request!.ObjectId);
                    var classForumResultTemp = classForumResult?.Content?.Result;
                    var studentInfo = classForumResultTemp?.CreatedUserId != null ? await _userService.GetStudentByUserIdAsync(classForumResultTemp.CreatedUserId) : default;
                    var studentInfoResult = studentInfo?.Content?.Result;
                    var classInfo = studentInfoResult?.Id != null ? await _trainingService.GetClassByStudentId(studentInfoResult.Id) : default;
                    var classInfoResult  = classInfo?.Content?.Result;

                    var businessType = EnumNotificationType.LinkPage;
                    var businessContent = EnumNotificationContent.FlagClassForum;


                    var paramsMessage = new List<object> { classForumResultTemp?.CreatedFullName! ?? string.Empty, classInfoResult?.Name ?? "" };
                    var paramsLink = new List<object> { classForumResultTemp?.UnitId.ToString() ?? string.Empty, classForumResultTemp?.CourseId.ToString() ?? string.Empty, classForumResultTemp?.UnitId.ToString() ?? string.Empty, classForumResultTemp?.Id.ToString() ?? string.Empty, request.ObjectId };

                    if (!CheckObjecIsClassForum(request.ObjectId, _commentRepository, _courseService))
                    {
                        var comment = await _commentRepository.GetByIdAsync(request.ObjectId);
                        classForumResult = await GetClassForumResultModel(_courseService, comment!.ObjectId);
                        classForumResultTemp = classForumResult?.Content?.Result;
                        studentInfo = comment?.CreatedUserId != null ? await _userService.GetStudentByUserIdAsync(comment.CreatedUserId) : default;
                        studentInfoResult = studentInfo?.Content?.Result;
                        classInfo = studentInfoResult?.Id != null ? await _trainingService.GetClassByStudentId(studentInfoResult.Id) : default;
                        classInfoResult = classInfo?.Content?.Result;

                        businessType = EnumNotificationType.LinkComment;
                        businessContent = EnumNotificationContent.FlagComment;
                        paramsMessage = new List<object> { comment?.CreatedFullName ?? string.Empty, classInfoResult?.Name ?? string.Empty };
                        paramsLink = new List<object> { classForumResultTemp?.UnitId.ToString() ?? string.Empty, classForumResultTemp?.CourseId.ToString() ?? string.Empty, classForumResultTemp?.UnitId.ToString() ?? string.Empty, classForumResultTemp?.Id.ToString() ?? string.Empty, request.ObjectId };


                    }



                    InterationActionQueueModel model = new InterationActionQueueModel()
                    {
                        ObjectId = action.ObjectId,
                        InterationType = action.Type,
                        ParamsMessage = paramsMessage,
                        ParamsLink = paramsLink,
                        Type = businessType,
                        Content = businessContent,
                        UserId = action.CreatedUserId,
                        SenderId = _authContext.CurrentUserId
                    };

                    await _interationActionPublisher.Publish(model, cancellationToken).ConfigureAwait(false);
                }

                await _interactionActionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);


                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }

        public static bool CheckObjecIsClassForum(Guid objectId, ICommentRepository commentRepository, ICourseService courseService)
        {
            bool isClassForum = true;

            var comment = commentRepository.GetByIdAsync(objectId).Result;

            var classForum = courseService.GetClassForumResultByIdAsync(objectId);

            if (comment != null && classForum?.Result?.Content == null)
            {
                isClassForum = false;
            }

            return isClassForum;
        }

        public async Task<IApiResponse<MethodResult<ClassForumResultModel>>> GetClassForumResultModel(ICourseService courseService, Guid Id)
        {
            var result = await courseService.GetClassForumResultByIdAsync(Id);

            return result;
        }
    }
}
