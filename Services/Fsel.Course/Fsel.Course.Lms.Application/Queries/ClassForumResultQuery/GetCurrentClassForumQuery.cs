// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumResultQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.InteractionService;
    using Fsel.Course.Lms.Application.Services.InteractionService.Models;
    using Fsel.Course.Lms.Application.Services.NotificationServices;
    using Fsel.Course.Lms.Application.Services.NotificationServices.Models;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCurrentClassForumQuery : IRequest<MethodResult<ClassForumByStudentModel>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class GetCurrentClassForumQueryHandler : IRequestHandler<GetCurrentClassForumQuery, MethodResult<ClassForumByStudentModel>>
    {
        private readonly IMapper _mapper;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUserService _userService;
        private readonly ITrainingService _trainingService;
        private readonly INotificationService _notificationService;
        private readonly IInteractionService _interactionService;
        private readonly AuthContext _authContext;
        private readonly IStudentFeedbackRepository _studentFeedbackRepository;

        public GetCurrentClassForumQueryHandler(IMapper mapper,
            IClassForumResultRepository classForumResultRepository,
            IClassForumRepository classForumRepository,
            ILessonResultRepository lessonResultRepository,
            IUserService userService,
            ITrainingService trainingService,
            INotificationService notificationService,
            IInteractionService interactionService,
            AuthContext authContext,
            IStudentFeedbackRepository studentFeedbackRepository)
        {
            _mapper = mapper;
            _classForumResultRepository = classForumResultRepository;
            _classForumRepository = classForumRepository;
            _lessonResultRepository = lessonResultRepository;
            _userService = userService;
            _trainingService = trainingService;
            _notificationService = notificationService;
            _interactionService = interactionService;
            _authContext = authContext;
            _studentFeedbackRepository = studentFeedbackRepository;
        }

        public async Task<MethodResult<ClassForumByStudentModel>> Handle(GetCurrentClassForumQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassForumByStudentModel> methodResult = new MethodResult<ClassForumByStudentModel>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var student = studentResult?.Content?.Result;

            if (studentResult == null || student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }

            var isLessonResult = await _lessonResultRepository.AnyAsync(request.LessonResultId);
            if (!isLessonResult)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(isLessonResult));
                return methodResult;
            }

            var lessonResult = await _lessonResultRepository.GetByIdAsync(request.LessonResultId);

            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonResult));
                return methodResult;
            }

            var classForum = await _classForumRepository.Queryable.Include(x => x.ClassForumFiles).Where(x => x.LessonId == lessonResult.LessonId).FirstOrDefaultAsync(cancellationToken);

            var classForumByStudentModel = _mapper.Map<ClassForumByStudentModel>(classForum);

            var classForumResult = await _classForumResultRepository.Queryable
                .Include(x => x.ClassForumResultFiles)
                .Include(x => x.ClassForumScores)
                .Where(x => x.LessonResultId == request.LessonResultId && x.ClassForumId == classForum!.Id && x.Status != EnumClassForumResultStatus.Draft)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            classForumByStudentModel.ClassForumResultCurrentStudent = _mapper.Map<ClassForumResultModel>(classForumResult);
            var classForumResultModel = classForumByStudentModel.ClassForumResultCurrentStudent;

            if (classForumResultModel != null)
            {
                classForumResultModel.IsTeacherFeedBack = await _studentFeedbackRepository.Queryable.AnyAsync(x => x.ObjectId == classForumResultModel.Id && x.Type == EnumStudentFeedBackType.Teacher, cancellationToken);
                classForumResultModel.IsAIFeedBack = await _studentFeedbackRepository.Queryable.AnyAsync(x => x.ObjectId == classForumResultModel.Id && x.Type == EnumStudentFeedBackType.AI, cancellationToken);

                IList<Guid> classForumResultIds = new List<Guid>()
                {
                    classForumResult!.Id
                };
                var actionsResult = await _interactionService.GetsActionAsync(new InteractionActionCommandModel { ObjectIds = classForumResultIds.ToList(), UserId = _authContext.CurrentUserId });
                var actions = actionsResult.Content?.Result;

                if (actions != null)
                {
                    GetListNotificationRemindQuery query = new GetListNotificationRemindQuery
                    {
                        ObjectIds = classForumResultIds.ToList(),
                        Status = EnumNotificationRemindStatus.Off
                    };

                    var notificationRemind = await _notificationService.GetListNotificationRemind(query);
                    var notificationTurnOff = notificationRemind.Content?.Result;

                    var action = actions.FirstOrDefault(x => x.ObjectId == classForumResultModel.Id);
                    classForumResultModel.CommentNumber = action?.CommentNumber;
                    classForumResultModel.LikeNumber = action?.LikeNumber;
                    classForumResultModel.IsLiked = action?.IsLiked;
                    classForumResultModel.IsTurnedOffNotification = notificationTurnOff!.Any(x => x.ObjectId == classForumResultModel.Id);
                    classForumResultModel.CourseLevel = student.CourseLevel;
                }
            }

            methodResult.Result = classForumByStudentModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
