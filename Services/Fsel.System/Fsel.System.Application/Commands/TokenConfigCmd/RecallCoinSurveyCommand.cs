// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.TokenConfigCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Commands.TokenHistoryCmd;
    using Fsel.System.Application.Queues.Publisher;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class RecallCoinSurveyCommand : IRequest<MethodResult<bool>>
    {
    }

    public class RecallCoinSurveyCommandHandler : IRequestHandler<RecallCoinSurveyCommand, MethodResult<bool>>
    {
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly ITokenHistoryRepository _tokenHistoryRepository;
        private readonly IMediator _mediator;

        public RecallCoinSurveyCommandHandler(NotificationMessagePublisher notificationMessagePublisher,
                                                              ITokenHistoryRepository tokenHistoryRepository,
                                                              IMediator mediator)
        {
            _notificationMessagePublisher = notificationMessagePublisher;
            _tokenHistoryRepository = tokenHistoryRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(RecallCoinSurveyCommand request, CancellationToken cancellationToken)
        {
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var userBugCoins = await _tokenHistoryRepository.Queryable
                                                            .Where(x => x.Feature == EnumTokenFeature.FselEvent && x.Mission == EnumTokenMission.SurveyEvent && x.Type == EnumTokenHistoryType.Recevived)
                                                            .GroupBy(x => x.UserId)
                                                            .Where(x => x.Count() > 1)
                                                            .Select(g => new { UserId = g.Key, Tokens = g.ToList() })
                                                            .ToListAsync(cancellationToken);

            if (userBugCoins == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            // cộng lại coin từ thu hồi quà
            var userKieuHoangHais = await _tokenHistoryRepository.Queryable.Where(x => x.Feature == EnumTokenFeature.MarketPlace &&
                                                                                       x.Mission == EnumTokenMission.FselStore &&
                                                                                       x.UserId == Guid.Parse("AC5DF11C-9E49-41F5-7DD8-08DD3C79EC86"))
                                                                           .OrderBy(x => x.CreatedDate)
                                                                           .ToListAsync(cancellationToken);

            if (userKieuHoangHais.Any())
            {
                foreach (var item in userKieuHoangHais)
                {
                    var reclaimGiftCoins = new List<TokenHistoryQueueModel>
                    {
                        new TokenHistoryQueueModel
                        {
                             VolatileToken = item.VolatileToken,
                             UserId = item.UserId,
                             Feature = EnumTokenFeature.ReclaimGift,
                             Mission = EnumTokenMission.ReclaimGiftCoins,
                             Type = EnumTokenHistoryType.Recevived,
                             Config = item.Config
                        }
                    };

                    await _mediator.Send(new CreateTokenHistoryCommand { TokenHistorys = reclaimGiftCoins }, cancellationToken);
                }
            }

            // thu hồi coin
            IList<TokenHistoryQueueModel> tokenHistories = new List<TokenHistoryQueueModel>();
            foreach (var user in userBugCoins)
            {
                if (user.UserId == Guid.Parse("AC5DF11C-9E49-41F5-7DD8-08DD3C79EC86"))
                {
                    Console.WriteLine();
                }
                var coin = user.Tokens.Sum(x => x.VolatileToken);
                var newTokenHistory = new List<TokenHistoryQueueModel>
                {
                    new TokenHistoryQueueModel
                    {
                        VolatileToken = coin - 80,
                        UserId = user.UserId,
                        Feature = EnumTokenFeature.RecallCoinBug,
                        Mission = EnumTokenMission.RecallCoinsSurveyEvent,
                        Type = EnumTokenHistoryType.Exchanged
                    }
                };

                await _mediator.Send(new CreateTokenHistoryCommand { TokenHistorys = newTokenHistory }, cancellationToken);
            }

            // gửi thông báo
            await _notificationMessagePublisher.Publish(new NotificationSendingQueueModel
            {
                UserIds = userBugCoins.Select(x => x.UserId).Distinct().ToList(),
                ObjectId = Guid.Empty,
                Type = EnumNotificationType.Text,
                Content = EnumNotificationContent.RecallCoinSurvey,
                SenderId = Guid.Empty,
                PlatformCode = EnumPlatformCode.LMS
            }, cancellationToken);

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
