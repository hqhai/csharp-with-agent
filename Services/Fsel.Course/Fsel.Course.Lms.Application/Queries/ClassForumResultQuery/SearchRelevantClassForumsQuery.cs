// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ClassForumResultQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumResults;
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

    public class SearchRelevantClassForumsQuery : SearchRelevantClassForumsQueryModel, IRequest<MethodResult<PagingItemsModel<ClassForumResultModel>>>
    {
    }

    public class SearchRelevantClassForumsQueryHandler : IRequestHandler<SearchRelevantClassForumsQuery, MethodResult<PagingItemsModel<ClassForumResultModel>>>
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

        public SearchRelevantClassForumsQueryHandler(IMapper mapper, IClassForumResultRepository classForumResultRepository, IClassForumRepository classForumRepository, ILessonResultRepository lessonResultRepository, IUserService userService, ITrainingService trainingService, INotificationService notificationService, IInteractionService interactionService, AuthContext authContext)
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
        }

        public async Task<MethodResult<PagingItemsModel<ClassForumResultModel>>> Handle(SearchRelevantClassForumsQuery request, CancellationToken cancellationToken)
        {
            MethodResult<PagingItemsModel<ClassForumResultModel>> methodResult = new MethodResult<PagingItemsModel<ClassForumResultModel>>();
            ArgumentNullException.ThrowIfNull(request);
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var currentClass = await _trainingService.GetClassByStudentId(student.Id);
            var classStudentIds = currentClass.Content?.Result?.ClassStudents?.Select(x => x.StudentId).ToList();
            if (classStudentIds == null || classStudentIds.Count == 0)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var lessonResult = await _lessonResultRepository.GetByIdAsync(request.LessonResultId);
            if (lessonResult == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var classForumResult = await _classForumResultRepository.Queryable.Include(x => x.ClassForumDetailResults).FirstOrDefaultAsync(x => x.Id == request.ClassForumResultId, cancellationToken);
            if (classForumResult == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            if (classForumResult.ClassForumDetailResults.Any() && classForumResult.ClassForumDetailResults.Any(x => x.SubmissionCount == EnumSubmissionCount.FirstSubmit && x.Status == EnumClassForumResultStatus.Draft))
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var classForum = await _classForumRepository.Queryable.Where(x => x.LessonId == lessonResult.LessonId).FirstOrDefaultAsync(cancellationToken);
            if (classForum == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var classForumResults = _classForumResultRepository.Queryable
                .Include(x => x.ClassForumResultFiles)
                .Include(x => x.ClassForumScores)
                .Where(x => x.ClassForumId == classForum.Id
                && x.Status == EnumClassForumResultStatus.Graded
                && x.Id != request.ClassForumResultId
                && classStudentIds.Contains(x.StudentId))
                .Select(x => new ClassForumResultModel
                {
                    Id = x.Id,
                    Status = x.Status,
                    CheckCsoId = x.CheckCsoId,
                    CheckStartDate = x.CheckStartDate,
                    ClassForumId = x.ClassForumId,
                    Content = x.Content,
                    CorrectCount = x.CorrectCount,
                    CorrectTotal = x.CorrectTotal,
                    SkillScores = x.SkillScores,
                    WordCount = x.WordCount,
                    WordContent = x.WordContent,
                    UpdatedUserId = x.UpdatedUserId,
                    UpdatedFullName = x.UpdatedFullName,
                    UpdatedDate = x.UpdatedDate,
                    TimeCount = x.TimeCount,
                    StudentId = x.StudentId,
                    LessonResultId = x.LessonResultId,
                    GradingStartDate = x.GradingStartDate,
                    CreatedDate = x.CreatedDate,
                    CreatedFullName = x.CreatedFullName,
                    CreatedUserId = x.CreatedUserId,
                    ClassForumScores = _mapper.Map<IList<ClassForumScoreModel>>(x.ClassForumScores.OrderBy(x => x.CreatedDate)),
                    ClassForumResultFiles = _mapper.Map<IList<ClassForumResultFileModel>>(x.ClassForumResultFiles),
                    TokenLastTime = x.TokenLastTime,
                    TokenFirstTime = x.TokenFirstTime,
                    IsViewed = x.IsViewed,
                });

            int totalItem = await classForumResults.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await classForumResults
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            var actionsResult = await _interactionService.GetsActionAsync(new InteractionActionCommandModel { ObjectIds = lists.Select(x => x.Id).ToList(), UserId = _authContext.CurrentUserId });
            var actions = actionsResult.Content?.Result;

            GetListNotificationRemindQuery query = new GetListNotificationRemindQuery
            {
                ObjectIds = lists.Select(x => x.Id).ToList(),
                Status = EnumNotificationRemindStatus.Off
            };

            var notificationRemind = await _notificationService.GetListNotificationRemind(query);
            var notificationTurnOff = notificationRemind.Content?.Result;
            List<ClassForumResultModel> classForumResultModels = new List<ClassForumResultModel>();

            var userResults = await _userService.GetUsersByUserIdsAsync(lists.Select(x => x.CreatedUserId).ToList());
            var users = userResults?.Content?.Result;
            if (actions != null)
            {
                classForumResultModels = lists.Where(x => !actions.Any(n => n.IsDisable && n.ObjectId == x.Id)).ToList();
                foreach (var item in classForumResultModels)
                {
                    var user = users?.FirstOrDefault(x => x.Id == item.CreatedUserId);
                    var action = actions.FirstOrDefault(x => x.ObjectId == item.Id);
                    item.CommentNumber = action?.CommentNumber;
                    item.LikeNumber = action?.LikeNumber;
                    item.IsLiked = action?.IsLiked;
                    item.CreatedFullName = user?.FullName;
                    item.IsTurnedOffNotification = notificationTurnOff!.Any(x => x.ObjectId == item.Id);
                    item.CourseLevel = student.CourseLevel;
                }
            }

            methodResult.Result = new PagingItemsModel<ClassForumResultModel>(classForumResultModels, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
