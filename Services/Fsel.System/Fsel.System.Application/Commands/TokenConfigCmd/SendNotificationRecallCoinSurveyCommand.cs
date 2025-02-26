// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.TokenConfigCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Queues.Publisher;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SendNotificationRecallCoinSurveyCommand : IRequest<MethodResult<bool>>
    {
    }

    public class SendNotificationRecallCoinSurveyCommandHandler : IRequestHandler<SendNotificationRecallCoinSurveyCommand, MethodResult<bool>>
    {
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly ITokenHistoryRepository _tokenHistoryRepository;

        public SendNotificationRecallCoinSurveyCommandHandler(NotificationMessagePublisher notificationMessagePublisher, ITokenHistoryRepository tokenHistoryRepository)
        {
            _notificationMessagePublisher = notificationMessagePublisher;
            _tokenHistoryRepository = tokenHistoryRepository;
        }

        public async Task<MethodResult<bool>> Handle(SendNotificationRecallCoinSurveyCommand request, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var userBugCoins = await _tokenHistoryRepository.Queryable
                                                            .Where(x => x.Feature == EnumTokenFeature.FselEvent && x.Mission == EnumTokenMission.SurveyEvent && x.Type == EnumTokenHistoryType.Recevived)
                                                            .GroupBy(x => x.UserId)
                                                            .Where(x => x.Count() > 1)
                                                            .Select(g => new { UserId = g.Key, Count = g.Count() })
                                                            .ToListAsync(cancellationToken);

            if (userBugCoins == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            await _notificationMessagePublisher.Publish(new NotificationSendingQueueModel
            {
                UserIds = userBugCoins.Select(x => x.UserId).ToList(),
                ObjectId = Guid.Empty,
                Type = EnumNotificationType.Text,
                Content = EnumNotificationContent.RecallCoinSurvey,
                SenderId = Guid.Empty,
                PlatformCode = EnumPlatformCode.LMS
            }, cancellationToken);

            methodResult.Result = true;
            return methodResult;
        }
    }
}
