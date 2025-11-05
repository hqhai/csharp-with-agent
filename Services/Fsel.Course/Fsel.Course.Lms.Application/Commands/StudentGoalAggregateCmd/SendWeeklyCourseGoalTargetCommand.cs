// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.StudentGoalAggregateCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Commands.LessonCmd;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SendWeeklyCourseGoalTargetCommand : IRequest<MethodResult<bool>>
    {
    }

    public class SendWeeklyCourseGoalTargetCommandHandler : IRequestHandler<SendWeeklyCourseGoalTargetCommand, MethodResult<bool>>
    {
        private readonly IStudentGoalSummaryRepository _studentGoalSummaryRepository;
        private readonly IStudentGoalAggregateRepository _studentGoalAggregateRepository;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly IUserService _userService;

        public SendWeeklyCourseGoalTargetCommandHandler(IStudentGoalSummaryRepository studentGoalSummaryRepository,
        IStudentGoalAggregateRepository studentGoalAggregateRepository,
        NotificationMessagePublisher notificationMessagePublisher,
        IUserService userService)
        {
            _studentGoalSummaryRepository = studentGoalSummaryRepository;
            _studentGoalAggregateRepository = studentGoalAggregateRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(SendWeeklyCourseGoalTargetCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var startDate = DateTime.UtcNow.Date;

            var studentGoalSummaries = await (from baseQ in _studentGoalSummaryRepository.Queryable.Where(x => x.StartDate.Date <= startDate.Date && x.EndDate.Date >= startDate.Date)
                                              join sga in _studentGoalAggregateRepository.Queryable on baseQ.StudentGoalAggregateId equals sga.Id
                                              select new
                                              {
                                                  StudentGoalSummary = baseQ,
                                                  StudentId = sga.StudentId,
                                              }).ToListAsync(cancellationToken);
            var studentIds = studentGoalSummaries.Select(x => x.StudentId).ToList();
            var studentResults = await _userService.GetStudentsByStudentIdsAsync(studentIds);
            var students = studentResults?.Content?.Result ?? new List<StudentModel>();
            var userIds = students.Where(x => x.Human != null).Select(x => x.Human!)
                                   .Where(x => x.UserId.HasValue)
                                   .Select(x => x.UserId!.Value).ToList();

            foreach (var student in students)
            {
                var studentGoalSummary = studentGoalSummaries.FirstOrDefault(x => x.StudentId == student.Id);
                if (studentGoalSummary == null)
                {
                    continue;
                }
                var userId = student.Human?.UserId ?? default;
                var sgs = studentGoalSummary.StudentGoalSummary;
                var paramsMessage = new List<object> { sgs.LessonsPerWeek };
                await SendNotificationAsync(new List<Guid> { userId }, paramsMessage, EnumNotificationType.LinkPage, EnumNotificationContent.CourseGoalStudent);
            }

            return methodResult;
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
