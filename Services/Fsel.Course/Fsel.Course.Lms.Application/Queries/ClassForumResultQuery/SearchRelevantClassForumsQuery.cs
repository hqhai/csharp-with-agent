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
        private readonly INotificationService _notificationService;
        private readonly IInteractionService _interactionService;
        private readonly AuthContext _authContext;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IClassForumResultFileRepository _classForumResultFileRepository;

        public SearchRelevantClassForumsQueryHandler(IMapper mapper, IClassForumResultRepository classForumResultRepository, IUserService userService, INotificationService notificationService, IInteractionService interactionService, AuthContext authContext, ILessonResultRepository lessonResultRepository, IClassForumResultFileRepository classForumResultFileRepository)
        {
            _mapper = mapper;
            _classForumResultRepository = classForumResultRepository;
            _userService = userService;
            _notificationService = notificationService;
            _interactionService = interactionService;
            _authContext = authContext;
            _lessonResultRepository = lessonResultRepository;
            _classForumResultFileRepository = classForumResultFileRepository;
        }

        public async Task<MethodResult<PagingItemsModel<ClassForumResultModel>>> Handle(SearchRelevantClassForumsQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<PagingItemsModel<ClassForumResultModel>>();
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

            var classForumResult = await _classForumResultRepository.ReadQueryable
                                                                    .Include(x => x.ClassForumDetailResults)
                                                                    .Include(x => x.LessonResult)
                                                                    .Where(x => x.Id == request.ClassForumResultId)
                                                                    .FirstOrDefaultAsync(cancellationToken);

            var lessonResult = classForumResult?.LessonResult;
            if (classForumResult == null || lessonResult == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            if (classForumResult.ClassForumDetailResults.Any(x => x.SubmissionCount == EnumSubmissionCount.FirstSubmit && x.Status == EnumClassForumResultStatus.Draft))
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var query = from cfr in _classForumResultRepository.ReadQueryable
                        join lr in _lessonResultRepository.ReadQueryable on cfr.LessonResultId equals lr.Id
                        where cfr.Status == EnumClassForumResultStatus.Graded
                        && cfr.ClassForumId == classForumResult.ClassForumId
                        && cfr.Id != request.ClassForumResultId
                        && lr.CourseId == lessonResult.CourseId
                        && lr.UnitId == lessonResult.UnitId
                        && cfr.StudentId != student.Id
                        select cfr;

            int totalItem = await query.CountAsync(cancellationToken).ConfigureAwait(false);
            var lists = await query.ApplySortAndPaging(request)
                                   .AsNoTracking()
                                   .ToListAsync(cancellationToken)
                                   .ConfigureAwait(false);

            var classForumResultIds = lists.Select(x => x.Id).ToList();

            if (lists.Any())
            {
                var classForumResultFileEntities = await _classForumResultFileRepository.ReadQueryable
                                                                                        .WhereBulkContains(classForumResultIds, p => p.ClassForumResultId)
                                                                                        .ToListAsync(cancellationToken);
                lists.ForEach(p =>
                {
                    var classForumResultFiles = classForumResultFileEntities.Where(x => x.ClassForumResultId == p.Id).ToList();
                    p.ClassForumResultFiles = classForumResultFiles;
                });
            }

            var actionsTask = _interactionService.GetsActionAsync(new InteractionActionCommandModel
            {
                ObjectIds = classForumResultIds,
                UserId = _authContext.CurrentUserId
            });

            var notificationTask = _notificationService.GetListNotificationRemind(new GetListNotificationRemindQuery
            {
                ObjectIds = classForumResultIds,
                Status = EnumNotificationRemindStatus.Off
            });

            var studentsTask = _userService.GetStudentsByStudentIdsAsync(lists.Select(x => x.StudentId).ToList());

            await Task.WhenAll(actionsTask, notificationTask, studentsTask);

            var actionsResponse = await actionsTask;
            var notificationResponse = await notificationTask;
            var studentsResponse = await studentsTask;

            var actions = actionsResponse.Content?.Result;
            var notificationTurnOff = notificationResponse.Content?.Result;
            var students = studentsResponse.Content?.Result?.ToDictionary(x => x.Id, x => x);

            var classForumResultModels = _mapper.Map<List<ClassForumResultModel>>(lists);

            if (actions != null)
            {
                var disabledActionIds = new HashSet<Guid>(actions.Where(n => n.IsDisable).Select(n => n.ObjectId));

                classForumResultModels = classForumResultModels
                    .Where(x => !disabledActionIds.Contains(x.Id))
                    .ToList();

                classForumResultModels.ForEach(item =>
               {
                   if (students != null && students.TryGetValue(item.StudentId, out var studentDto))
                   {
                       item.CreatedFullName = studentDto.User?.FullName;
                   }

                   var action = actions.FirstOrDefault(x => x.ObjectId == item.Id);
                   if (action != null)
                   {
                       item.CommentNumber = action.CommentNumber;
                       item.LikeNumber = action.LikeNumber;
                       item.IsLiked = action.IsLiked;
                   }

                   item.IsTurnedOffNotification = notificationTurnOff?.Any(x => x.ObjectId == item.Id) ?? false;
                   item.CourseLevel = student.CourseLevel;
               });
            }

            methodResult.Result = new PagingItemsModel<ClassForumResultModel>(classForumResultModels, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
