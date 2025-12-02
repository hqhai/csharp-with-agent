// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.LessonCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.SystemService.QueryModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SendWeeklyLessonCourseTargetCommand : IRequest<MethodResult<bool>>
    {
    }

    public class SendWeeklyLessonCourseTargetCommandHandler : IRequestHandler<SendWeeklyLessonCourseTargetCommand, MethodResult<bool>>
    {
        private readonly IStudentGoalSummaryRepository _studentGoalSummaryRepository;
        private readonly IStudentGoalAggregateRepository _studentGoalAggregateRepository;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;
        private readonly ICourseResultRepository _courseResultRepository;

        public SendWeeklyLessonCourseTargetCommandHandler(IStudentGoalSummaryRepository studentGoalSummaryRepository,
        IStudentGoalAggregateRepository studentGoalAggregateRepository,
        NotificationMessagePublisher notificationMessagePublisher,
        ISystemService systemService,
        IUserService userService,
        ICourseResultRepository courseResultRepository)
        {
            _studentGoalSummaryRepository = studentGoalSummaryRepository;
            _studentGoalAggregateRepository = studentGoalAggregateRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
            _systemService = systemService;
            _userService = userService;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(SendWeeklyLessonCourseTargetCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var (weekStartUtc, weekEndUtc) = DateTimeHelper.GetCurrentWeekRangeNow(DateTime.UtcNow.AddDays(-7));

            var querySum = _studentGoalSummaryRepository.Queryable.Where(x => x.StartDate.Date <= weekStartUtc && x.EndDate.Date >= weekEndUtc);
            var studentGoalSummaries = await (from baseQ in querySum
                                              join sga in _studentGoalAggregateRepository.Queryable.Where(x => x.IsActive) on baseQ.StudentGoalAggregateId equals sga.Id
                                              select new
                                              {
                                                  StudentGoalSummary = baseQ,
                                                  StudentId = sga.StudentId,
                                              }).ToListAsync(cancellationToken);

            var studentIds = studentGoalSummaries.Select(x => x.StudentId).ToList();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            var students = studentResults?.Content?.Result ?? new List<StudentModel>();
            var userIds = students.Where(x => x.UserId != Guid.Empty)
                                   .Select(x => x.UserId).ToList();

            var featureAccessTimeResults = await _systemService.GetFeatureAccessTimeRangeByUserIds(new GetFeatureAccessTimesByUserIdsQueryModel
            {
                UserIds = userIds,
                StartDate = weekStartUtc,
                EndDate = weekEndUtc
            });
            if (!featureAccessTimeResults.IsSuccessStatusCode)
            {
                methodResult.AddError(featureAccessTimeResults.Error);
                return methodResult;
            }
            var featureAccessTimes = featureAccessTimeResults.Content?.Result?.ToList();

            foreach (var student in students)
            {
                var studentGoalSummary = studentGoalSummaries.FirstOrDefault(x => x.StudentId == student.Id);
                if (studentGoalSummary == null)
                {
                    continue;
                }

                var userId = student?.UserId ?? default;
                var sgs = studentGoalSummary.StudentGoalSummary;
                var featureAccessTime = featureAccessTimes?.FirstOrDefault(x => x.CreatedUserId == userId);
                var (hours, minutes) = (featureAccessTime?.AccessTime ?? default).ConvertHoursAndMinutesBySeconds();
                var paramsMessage = new List<object> { hours, minutes, sgs.CompletedLessons, sgs.LessonsPerWeek };
                await SendNotificationAsync(new List<Guid> { userId }, paramsMessage, EnumNotificationType.LinkPage, GetProgressStatus(sgs.ProgressStatus));
            }

            return methodResult;
        }

        private static EnumNotificationContent GetProgressStatus(EnumProgressStatus progressStatus)
        {
            if (progressStatus == EnumProgressStatus.Behind)
            {
                return EnumNotificationContent.BelowTargetCourseGoal;
            }
            if (progressStatus == EnumProgressStatus.OnTrack)
            {
                return EnumNotificationContent.AchievedCourseGoal;
            }
            return EnumNotificationContent.ExceededCourseGoal;
        }

        private async Task SendNotificationAsync(IList<Guid> userIds, IList<object>? paramsMessage, EnumNotificationType type, EnumNotificationContent content)
        {
            NotificationSendingQueueModel notificationQueue = new NotificationSendingQueueModel()
            {
                ObjectId = Guid.Empty,
                UserIds = userIds,
                Type = type,
                Content = content,
                PlatformCode = EnumPlatformCode.LMS,
                ParamsMessage = paramsMessage
            };
            await _notificationMessagePublisher.Publish(notificationQueue, CancellationToken.None);
        }
    }
}
