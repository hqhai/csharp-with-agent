// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.FlagCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Queues.Publishers;
    using Fsel.Interaction.Application.Services.CourseServices;
    using Fsel.Interaction.Application.Services.TrainingServices;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;
    using Fsel.Interaction.Domain.Models.CommandModels.Flags;
    using Fsel.Interaction.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Fsel.Interaction.Application.Services.UserServices;
    using Fsel.Interaction.Application.Services.CourseServices.Models;

    public class RateFlagCommand : RateFlagCommandModel, IRequest<MethodResult<FlagModel>>
    {
    }

    public class RateFlagCommandHandler : IRequestHandler<RateFlagCommand, MethodResult<FlagModel>>
    {
        private readonly IFlagRepository _flagRepository;
        private readonly IMapper _mapper;
        private readonly InterationActionPublisher _interationActionPublisher;
        private readonly AuthContext _authContext;
        private readonly ICommentRepository _commentRepository;
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;
        private readonly ITrainingService _trainingService;

        public RateFlagCommandHandler(IFlagRepository flagRepository, IMapper mapper, AuthContext authContext, ICommentRepository commentRepository, ICourseService courseService, IUserService userService, ITrainingService trainingService, InterationActionPublisher interationActionPublisher)
        {
            _flagRepository = flagRepository;
            _mapper = mapper;
            _authContext = authContext;
            _commentRepository = commentRepository;
            _courseService = courseService;
            _userService = userService;
            _trainingService = trainingService;
            _interationActionPublisher = interationActionPublisher;
        }

        public async Task<MethodResult<FlagModel>> Handle(RateFlagCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<FlagModel> methodResult = new MethodResult<FlagModel>();

            Flag flag = _mapper.Map<Flag>(request);

            var isExistFlag = await _flagRepository.Queryable.AnyAsync(x => x.CreatedUserId == _authContext.CurrentUserId && x.Type == request.Type && x.ObjectId == request.ObjectId, cancellationToken);
            if (isExistFlag)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(isExistFlag));
                return methodResult;
            }

            await _flagRepository.ExecuteTransactionAsync(async () =>
            {
                flag.Status = EnumFlagStatus.New;
                flag = _flagRepository.Add(flag);
                await _flagRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                var businessType = EnumNotificationType.LinkPage;
                var businessContent = EnumNotificationContent.FlagClassForum;

                //class forum
                var classForumResultTemp = await GetClassForumResultModel(_courseService, request.ObjectId);

                var (returnedParamsMessage, returnedParamsLink, objectOwnerId) = await CustomDataForParamMessage(request.ObjectId, _userService, _trainingService, classForumResultTemp!, classForumResultTemp?.CourseId, classForumResultTemp?.UnitId);

                if (!CheckObjecIsClassForum(request.ObjectId, _commentRepository, _courseService))
                {
                    //comment
                    var comment = await _commentRepository.GetByIdAsync(request.ObjectId);
                    classForumResultTemp = await GetClassForumResultModel(_courseService, comment!.ObjectId);
                    (returnedParamsMessage, returnedParamsLink, objectOwnerId) = await CustomDataForParamMessage(request.ObjectId, _userService, _trainingService, comment!, classForumResultTemp?.CourseId, classForumResultTemp?.UnitId);

                    businessType = EnumNotificationType.LinkComment;
                    businessContent = EnumNotificationContent.FlagComment;
                }

                InterationActionQueueModel model = new InterationActionQueueModel()
                {
                    ObjectId = request.ObjectId,
                    InterationType = EnumInteractionActionType.Flag,
                    ParamsMessage = returnedParamsMessage,
                    ParamsLink = returnedParamsLink,
                    Type = businessType,
                    Content = businessContent,
                    SenderId = _authContext.CurrentUserId,
                    Roles = new List<EnumRole> { EnumRole.CSO },
                    PlatformCode = EnumPlatformCode.LMSAdmin
                };

                await _interationActionPublisher.Publish(model, cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<FlagModel>(flag);
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
        public async Task<ClassForumResultModel> GetClassForumResultModel(ICourseService courseService, Guid id)
        {
            if (courseService == null)
            {
                return new ClassForumResultModel();
            }

            var response = await courseService.GetClassForumResultByIdAsync(id) ?? default;

            return response?.Content?.Result ?? new ClassForumResultModel();
        }

        /// <summary>
        /// Kiểm tra xem ObjectId truyền vào có phải là ClassForum hay không
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="commentRepository"></param>
        /// <param name="courseService"></param>
        /// <returns></returns>
        public static bool CheckObjecIsClassForum(Guid objectId, ICommentRepository commentRepository, ICourseService courseService)
        {
            bool isClassForum = true;

            var comment = commentRepository?.GetByIdAsync(objectId).Result;

            var classForum = courseService?.GetClassForumResultByIdAsync(objectId);

            if (comment != null && classForum?.Result?.Content == null)
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
        public async Task<(List<object> paramsMessage, List<object> paramsLink, Guid ownerObjectId)> CustomDataForParamMessage(Guid objectId, IUserService userService, ITrainingService trainingService, dynamic templateResult, Guid? courseId, Guid? unitId)
        {
            if (templateResult == null)
            {
                return (new List<object>(), new List<object>(), Guid.Empty);
            }

            //student info
            var studentInfo = templateResult?.CreatedUserId != null ? await userService.GetStudentByUserIdAsync(templateResult.CreatedUserId) : default;
            var studentInfoResult = studentInfo?.Content?.Result;

            //class info
            var classInfo = studentInfoResult?.Id != null ? await trainingService.GetClassToStudentId(studentInfoResult.Id) : default;
            var classInfoResult = classInfo?.Content?.Result;

            // param
            var paramsMessage = new List<object> { templateResult?.CreatedFullName! ?? string.Empty, classInfoResult?.Name ?? "" };
            var paramsLink = new List<object> { unitId?.ToString() ?? string.Empty, courseId?.ToString() ?? string.Empty, templateResult?.Id.ToString() ?? string.Empty, objectId };
            var ownerObjectId = templateResult?.CreatedUserId ?? default;

            return (paramsMessage, paramsLink, ownerObjectId);
        }
    }
}
