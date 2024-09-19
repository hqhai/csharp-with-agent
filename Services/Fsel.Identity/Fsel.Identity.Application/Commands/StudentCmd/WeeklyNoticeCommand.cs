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

            var studentsAbsentIds = await _studentDailyStreakRepository.Queryable
                .GroupBy(x => x.CreatedUserId)
                .Where(g => g.All(x => x.CreatedUserId != Guid.Empty)
                    && g.Any(x => x.DailyDate.Date >= startOfWeek
                    && x.DailyDate.Date <= endOfWeek))
                .Select(g => g.Key)
                .ToListAsync(cancellationToken);

            await SendNotificationMessage(studentsAbsentIds, content, cancellationToken);
        }

        private async Task SendNotificationMessage(List<Guid>? userIds, EnumNotificationContent content, CancellationToken cancellationToken)
        {

            NotificationSendingQueueModel model = new NotificationSendingQueueModel()
            {
                ObjectId = Guid.Empty,
                UserIds = userIds,
                SenderId = Guid.Empty,
                Type = EnumNotificationType.LinkPage,
                Content = content
            };

            await _notificationMessagePublisherl.Publish(model, cancellationToken).ConfigureAwait(false);
        }
    }
}
