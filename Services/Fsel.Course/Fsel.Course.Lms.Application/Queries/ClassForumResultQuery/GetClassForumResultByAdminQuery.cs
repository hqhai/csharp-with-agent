// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumResultQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.InteractionService;
    using Fsel.Course.Lms.Application.Services.InteractionService.Models;
    using Fsel.Course.Lms.Application.Services.NotificationServices;
    using Fsel.Course.Lms.Application.Services.NotificationServices.Models;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetClassForumResultByAdminQuery : IRequest<MethodResult<ClassForumByStudentModel>>
    {
        public Guid ClassForumResultId { get; set; }
    }

    public class GetClassForumResultByAdminQueryHandler : IRequestHandler<GetClassForumResultByAdminQuery, MethodResult<ClassForumByStudentModel>>
    {
        private readonly IMapper _mapper;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        private readonly IInteractionService _interactionService;
        private readonly AuthContext _authContext;
        private readonly IStudentFeedbackRepository _studentFeedbackRepository;

        public GetClassForumResultByAdminQueryHandler(IMapper mapper, IClassForumResultRepository classForumResultRepository, IClassForumRepository classForumRepository, IUserService userService, INotificationService notificationService, IInteractionService interactionService, AuthContext authContext, IStudentFeedbackRepository studentFeedbackRepository)
        {
            _mapper = mapper;
            _classForumResultRepository = classForumResultRepository;
            _classForumRepository = classForumRepository;
            _userService = userService;
            _notificationService = notificationService;
            _interactionService = interactionService;
            _authContext = authContext;
            _studentFeedbackRepository = studentFeedbackRepository;
        }

        public async Task<MethodResult<ClassForumByStudentModel>> Handle(GetClassForumResultByAdminQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ClassForumByStudentModel>();

            var classForumResult = await _classForumResultRepository.Queryable
             .Include(x => x.ClassForumDetailResults)
             .ThenInclude(x => x.ClassForumResultFiles)
             .Include(x => x.ClassForumResultFiles)
             .Include(x => x.ClassForumScores)
             .Where(x => x.Id == request.ClassForumResultId)
             .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (classForumResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(classForumResult), request.ClassForumResultId);
                return methodResult;
            }

            var classForum = await _classForumRepository.ReadQueryable.Include(x => x.Skill)
                                                        .Include(x => x.ClassForumFiles)
                                                        .Where(x => x.Id == classForumResult.ClassForumId)
                                                        .FirstOrDefaultAsync(cancellationToken);

            if (classForum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classForum));
                return methodResult;
            }

            var classForumByStudentModel = _mapper.Map<ClassForumByStudentModel>(classForum);

            classForumByStudentModel.ClassForumResultCurrentStudent = _mapper.Map<ClassForumResultModel>(classForumResult);
            var classForumResultModel = classForumByStudentModel.ClassForumResultCurrentStudent;

            if (classForumResultModel != null)
            {
                classForumResultModel.ClassForumDetailResults = classForumResult.ClassForumDetailResults.Select(x =>
                {
                    x.Score = GetTargetCount(x, classForumResult);
                    return _mapper.Map<ClassForumDetailResultModel>(x);
                }).ToList();

                classForumResultModel.IsTeacherFeedBack = await _studentFeedbackRepository.Queryable.AnyAsync(x => x.ObjectId == classForumResultModel.Id && x.Type == EnumStudentFeedBackType.Teacher, cancellationToken);
                classForumResultModel.IsAIFeedBack = await _studentFeedbackRepository.Queryable.AnyAsync(x => x.ObjectId == classForumResultModel.Id && x.Type == EnumStudentFeedBackType.AI, cancellationToken);

                IList<Guid> classForumResultIds = new List<Guid>()
                {
                    classForumResult.Id
                };
                var actionsResult = await _interactionService.GetsActionAsync(new InteractionActionCommandModel
                {
                    ObjectIds = classForumResultIds.ToList(),
                    UserId = _authContext.CurrentUserId
                });
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
                    //classForumResultModel.CourseLevel = student.CourseLevel;
                }
            }

            methodResult.Result = classForumByStudentModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static int GetTargetCount(ClassForumDetailResult classForumDetailResult, ClassForumResult classForumResult)
        {
            int targetScore = default;
            var classForum = classForumResult.ClassForum;
            if (classForum?.Layout == EnumClassForumLayout.Writing && classForum?.TaggetWordLimit <= classForumDetailResult.WordCount)
            {
                ++targetScore;
            }
            if (classForum?.Layout == EnumClassForumLayout.Speaking && classForum?.TaggetTimeLimit <= classForumDetailResult.TimeCount)
            {
                ++targetScore;
            }
            return targetScore;
        }
    }
}
