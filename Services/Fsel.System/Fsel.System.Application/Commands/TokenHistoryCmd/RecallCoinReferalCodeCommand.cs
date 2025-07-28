// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.TokenHistoryCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Queues.Publisher;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class RecallCoinReferalCodeCommand : IRequest<MethodResult<bool>>
    {
        public IList<Guid>? UserIds { get; set; }
    }

    public class RecallCoinReferalCodeCommandHandler : IRequestHandler<RecallCoinReferalCodeCommand, MethodResult<bool>>
    {
        private readonly ITokenHistoryRepository _tokenHistoryRepository;
        private readonly IMediator _mediator;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;

        public RecallCoinReferalCodeCommandHandler(ITokenHistoryRepository tokenHistoryRepository, IMediator mediator, NotificationMessagePublisher notificationMessagePublisher)
        {
            _tokenHistoryRepository = tokenHistoryRepository;
            _mediator = mediator;
            _notificationMessagePublisher = notificationMessagePublisher;
        }

        public async Task<MethodResult<bool>> Handle(RecallCoinReferalCodeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.UserIds);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var tokenHistories = await _tokenHistoryRepository.Queryable
                                                              .Where(x => request.UserIds.Contains(x.UserId) && x.Feature == EnumTokenFeature.FriendMission && (x.Mission == EnumTokenMission.FriendCompletePT || x.Mission == EnumTokenMission.FriendCompleteUnit1 || x.Mission == EnumTokenMission.FriendCompletePayment))
                                                              .OrderBy(x => x.UserId)
                                                              .ThenBy(x => x.CreatedDate)
                                                              .ToListAsync(cancellationToken);
            if (tokenHistories == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return methodResult;
            }

            foreach (var tokenHistory in tokenHistories)
            {
                var reclaimGiftCoins = new List<TokenHistoryQueueModel>
                {
                    new TokenHistoryQueueModel
                    {
                        VolatileToken = tokenHistory.VolatileToken,
                        UserId = tokenHistory.UserId,
                        Feature = EnumTokenFeature.FriendMission,
                        Mission = EnumTokenMission.RecallCoinsSurveyEvent,
                        Type = EnumTokenHistoryType.Exchanged,
                        Config = tokenHistory.Config
                    }
                };

                await _mediator.Send(new CreateTokenHistoryCommand { TokenHistorys = reclaimGiftCoins }, cancellationToken);
            }

            // gửi thông báo
            await _notificationMessagePublisher.Publish(new NotificationSendingQueueModel
            {
                UserIds = request.UserIds.Distinct().ToList(),
                ObjectId = Guid.Empty,
                Type = EnumNotificationType.Text,
                Content = EnumNotificationContent.IllegalCoinRecall,
                SenderId = Guid.Empty,
                PlatformCode = EnumPlatformCode.LMS
            }, cancellationToken);

            methodResult.Result = true;
            return methodResult;
        }
    }
}
