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
        private readonly IUserService _userService;
        private readonly ITrainingService _trainingService;
        private readonly INotificationService _notificationService;
        private readonly IInteractionService _interactionService;
        private readonly AuthContext _authContext;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;

        public SearchRelevantClassForumsQueryHandler(IMapper mapper, IClassForumResultRepository classForumResultRepository, IUserService userService, ITrainingService trainingService, INotificationService notificationService, IInteractionService interactionService, AuthContext authContext, ICourseResultRepository courseResultRepository, ILessonResultRepository lessonResultRepository)
        {
            _mapper = mapper;
            _classForumResultRepository = classForumResultRepository;
            _userService = userService;
            _trainingService = trainingService;
            _notificationService = notificationService;
            _interactionService = interactionService;
            _authContext = authContext;
            _courseResultRepository = courseResultRepository;
            _lessonResultRepository = lessonResultRepository;
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
            var query = from cfr in _classForumResultRepository.Queryable
                        join lr in _lessonResultRepository.Queryable on cfr.LessonResultId equals lr.Id
                        join cr in _courseResultRepository.Queryable on new { lr.CourseId, lr.StudentId } equals new { cr.CourseId, cr.StudentId }
                        where cfr.Status == EnumClassForumResultStatus.Graded && cfr.ClassForumId == classForumResult.ClassForumId && cfr.Id != request.ClassForumResultId && cr.CourseId == student.CourseId
                        select cfr;

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query.ApplySortAndPaging(request)
                                               .AsNoTracking()
                                               .ToListAsync(cancellationToken: cancellationToken)
                                               .ConfigureAwait(false);

            var classForumResults = await _classForumResultRepository.Queryable
                                                .Include(x => x.ClassForumResultFiles)
                                                .Include(x => x.ClassForumScores)
                                                .Where(x => x.ClassForumId == classForumResult.ClassForumId
                                                && x.Status == EnumClassForumResultStatus.Graded
                                                && x.Id != request.ClassForumResultId
                                                && lists.Select(x => x.Id).Contains(x.Id)).ToListAsync(cancellationToken);

            var actionsResult = await _interactionService.GetsActionAsync(new InteractionActionCommandModel { ObjectIds = lists.Select(x => x.Id).ToList(), UserId = _authContext.CurrentUserId });
            var actions = actionsResult.Content?.Result;

            GetListNotificationRemindQuery queryNotify = new GetListNotificationRemindQuery
            {
                ObjectIds = lists.Select(x => x.Id).ToList(),
                Status = EnumNotificationRemindStatus.Off
            };

            var notificationRemind = await _notificationService.GetListNotificationRemind(queryNotify);
            var notificationTurnOff = notificationRemind.Content?.Result;
            var classForumResultModels = _mapper.Map<List<ClassForumResultModel>>(classForumResults);
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(classForumResults.Select(x => x.StudentId).ToList());
            var students = studentResults?.Content?.Result;
            if (actions != null)
            {
                classForumResultModels = classForumResultModels.Where(x => !actions.Any(n => n.IsDisable && n.ObjectId == x.Id)).ToList();
                classForumResultModels.ForEach(item =>
                {
                    var studentDto = students?.FirstOrDefault(x => x.Id == item.StudentId);
                    var action = actions.FirstOrDefault(x => x.ObjectId == item.Id);
                    item.CommentNumber = action?.CommentNumber;
                    item.LikeNumber = action?.LikeNumber;
                    item.IsLiked = action?.IsLiked;
                    item.CreatedFullName = studentDto?.Human?.FullName;
                    item.IsTurnedOffNotification = notificationTurnOff?.Any(x => x.ObjectId == item.Id) ?? default;
                    item.CourseLevel = student.CourseLevel;
                });
            }
            methodResult.Result = new PagingItemsModel<ClassForumResultModel>(classForumResultModels, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
