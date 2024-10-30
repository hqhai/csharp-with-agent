// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Queues.Publishers;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class WeeklyNoticeCommand : IRequest<MethodResult<bool>>
    {
        public EnumWeeklyNoticeType WeeklyNoticeType { get; set; }
    }

    public class WeeklyNoticeCommandHandler : IRequestHandler<WeeklyNoticeCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentDailyStreakRepository _studentDailyStreakRepository;
        private readonly NotificationMessagePublisher _notificationMessagePublisherl;

        public WeeklyNoticeCommandHandler(IStudentRepository studentRepository, NotificationMessagePublisher notificationMessagePublisherl, IStudentDailyStreakRepository studentDailyStreakRepository)
        {
            _studentRepository = studentRepository;
            _notificationMessagePublisherl = notificationMessagePublisherl;
            _studentDailyStreakRepository = studentDailyStreakRepository;
        }

        public async Task<MethodResult<bool>> Handle(WeeklyNoticeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var noticeMapping = new Dictionary<EnumWeeklyNoticeType, EnumNotificationContent>
            {
                { EnumWeeklyNoticeType.EnergyOfPlanet, EnumNotificationContent.EnergyCollecting },
                { EnumWeeklyNoticeType.TreasureZMatter, EnumNotificationContent.TreasureExploration }
            };

            if (noticeMapping.TryGetValue(request.WeeklyNoticeType, out var content))
            {
                await WeeklyNotice(content, cancellationToken);
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;

        }

        private async Task WeeklyNotice(EnumNotificationContent content, CancellationToken cancellationToken)
        {
            var today = DateTime.Now.Date;
            var dayOfWeek = (int)today.DayOfWeek; // 0 = Sunday, 1 = Monday, ..., 6 = Saturday
            var startOfWeek = today.AddDays(-dayOfWeek + (int)DayOfWeek.Monday);
            var endOfWeek = startOfWeek.AddDays(6);

            var studentsAbsent = await _studentDailyStreakRepository.Queryable
                                .Include(x => x.Student)
                                .ThenInclude(x => x.User)
                                .GroupBy(x => new
                                {
                                    x.CreatedUserId,
                                    FullName = x.Student != null && x.Student.User != null ? x.Student.User.FullName : string.Empty
                                })
                                .Where(g => g.All(x => x.CreatedUserId != Guid.Empty)
                                    && g.Any(x => x.DailyDate.Date >= startOfWeek
                                    && x.DailyDate.Date <= endOfWeek
                                    && x.Student != null
                                    && x.Student.User != null))
                                .Select(g => new
                                {
                                    Id = g.Key.CreatedUserId,
                                    FullName = g.Key.FullName
                                })
                                .ToListAsync(cancellationToken);

            foreach (var student in studentsAbsent)
            {
                await SendNotificationMessage(student.Id, student.FullName ?? string.Empty, content, cancellationToken);
            }
        }

        private async Task SendNotificationMessage(Guid userId, string userName, EnumNotificationContent content, CancellationToken cancellationToken)
        {

            NotificationSendingQueueModel model = new NotificationSendingQueueModel()
            {
                ObjectId = Guid.Empty,
                UserIds = new List<Guid> { userId },
                SenderId = Guid.Empty,
                Type = EnumNotificationType.LinkPage,
                ParamsMessage = new List<object> { userName },
                Content = content
            };

            await _notificationMessagePublisherl.Publish(model, cancellationToken).ConfigureAwait(false);
        }
    }
}
