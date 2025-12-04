// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.NoticeAccessFeatureCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Queues.Publisher;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Application.Services.UserServices.Models;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class NoticeFeatureAccessCommand : IRequest<MethodResult<bool>>
    {
    }

    public class NoticeFeatureAccessCommandHandler : IRequestHandler<NoticeFeatureAccessCommand, MethodResult<bool>>
    {
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;
        private readonly IUserService _userService;

        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private const int One_Day_Off = 1;
        private const int Two_Days_Off = 2;
        private const int Three_Days_Off = 3;
        private const int Four_Days_Off = 4;
        private const int Five_Days_Off = 5;
        private const int Fifteen_Days_Off = 15;

        public NoticeFeatureAccessCommandHandler(IFeatureAccessTimeRepository featureAccessTimeRepository, NotificationMessagePublisher notificationMessagePublisher, IUserService userService)
        {
            _featureAccessTimeRepository = featureAccessTimeRepository;
            _notificationMessagePublisher = notificationMessagePublisher;
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(NoticeFeatureAccessCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var notifications = new Dictionary<int, EnumNotificationContent>
                                {
                                    { One_Day_Off, EnumNotificationContent.FirstStudyNotice },
                                    { Two_Days_Off, EnumNotificationContent.SecondStudyNotice },
                                    { Three_Days_Off, EnumNotificationContent.ThirdStudyNotice },
                                    { Four_Days_Off, EnumNotificationContent.FourthStudyNotice },
                                    { Five_Days_Off, EnumNotificationContent.FifthStudyNotice },
                                    { Fifteen_Days_Off, EnumNotificationContent.StopBothering }
                                };

            try
            {
                foreach (var notifiaction in notifications)
                {
                    await CheckStudentsNoActionForSomeDays(notifiaction.Key, notifiaction.Value, cancellationToken);
                }
            }
            catch (Exception ex)
            {
            }

            return methodResult;
        }


        private async Task CheckStudentsNoActionForSomeDays(int dayAbsent, EnumNotificationContent content, CancellationToken cancellationToken)
        {
            DateTime now = DateTime.Now;
            int currentHour = now.Hour;
            DateTime targetDate = now.AddDays(-dayAbsent).Date;

            var studentsAbsentIds = await _featureAccessTimeRepository.Queryable
                                            .Where(x => x.CreatedUserId != Guid.Empty)
                                            .GroupBy(x => x.CreatedUserId)
                                            .Select(g => new
                                            {
                                                UserId = g.Key,
                                                LastVisit = g.Max(x => x.LastVisited)
                                            })
                                            .Where(x => x.LastVisit.HasValue &&
                                                        x.LastVisit.Value.Date == targetDate &&
                                                        x.LastVisit.Value.Hour == currentHour)
                                            .Select(x => x.UserId)
                                            .ToListAsync(cancellationToken);

            await SendNotificationMessage(studentsAbsentIds, content, cancellationToken);

        }

        private async Task SendNotificationMessage(List<Guid>? userIds, EnumNotificationContent content, CancellationToken cancellationToken)
        {
            var users = await GetParamMessage(userIds);

            if (users != null)
            {
                foreach (var item in users)
                {
                    NotificationSendingQueueModel model = new NotificationSendingQueueModel()
                    {
                        ObjectId = Guid.Empty,
                        UserIds = new List<Guid> { item.Id },
                        SenderId = Guid.Empty,
                        Type = EnumNotificationType.LinkPage,
                        Content = content,
                        ParamsMessage = new List<object> { item?.FullName ?? string.Empty }
                    };

                    await _notificationMessagePublisher.Publish(model, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task<IList<UserModel>> GetParamMessage(List<Guid>? userIds)
        {
            var usersResult = await _userService.GetUsersByUserIdsAsync(userIds);
            var user = usersResult?.Content?.Result;
            return user ?? new List<UserModel>();
        }
    }
}
