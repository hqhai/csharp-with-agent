// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.TokenHistoryCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Queues.Publisher;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class AddCoinWhenCoursePurchasedCommand : AddCoinWhenCoursePurchasedCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class AddCoinWhenCoursePurchasedCommandHandler : IRequestHandler<AddCoinWhenCoursePurchasedCommand, MethodResult<bool>>
    {
        private readonly IMediator _mediator;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;

        public AddCoinWhenCoursePurchasedCommandHandler(IMediator mediator, NotificationMessagePublisher notificationMessagePublisher)
        {
            _mediator = mediator;
            _notificationMessagePublisher = notificationMessagePublisher;
        }

        public async Task<MethodResult<bool>> Handle(AddCoinWhenCoursePurchasedCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.UserIds);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            foreach (var userId in request.UserIds.Distinct())
            {
                var newTokenHistory = new List<TokenHistoryQueueModel>
                {
                    new TokenHistoryQueueModel
                    {
                        VolatileToken = request.Coins,
                        UserId = userId,
                        ObjectId = request.ObjectId,
                        Feature = EnumTokenFeature.Payment,
                        Type = EnumTokenHistoryType.Recevived,
                        TokenHistoryTranslations = new List<TokenHistoryTranslationModel>
                        {
                            new TokenHistoryTranslationModel
                            {
                                Language = "vi-VN",
                                Config = new List<object>
                                {
                                    new
                                    {
                                         Title = $"Mua gói học {request.Month} tháng"
                                    }
                                }
                            },
                            new TokenHistoryTranslationModel
                            {
                                Language = "en-US",
                                Config = new List<object>
                                {
                                    new
                                    {
                                         Title = $"Purchase {request.Month}-month course"
                                    }
                                }
                            },
                            new TokenHistoryTranslationModel
                            {
                                Language = "fr-FR",
                                Config = new List<object>
                                {
                                    new
                                    {
                                         Title = $"Acheter un cours de {request.Month} mois"
                                    }
                                }
                            }
                        }
                    }
                };

                await _mediator.Send(new CreateTokenHistoryCommand { TokenHistorys = newTokenHistory }, cancellationToken);
            }

            // gửi thông báo
            await _notificationMessagePublisher.Publish(new NotificationSendingQueueModel
            {
                UserIds = request.UserIds.Distinct().ToList(),
                ObjectId = Guid.Empty,
                Type = EnumNotificationType.Text,
                Content = EnumNotificationContent.AddCoinBuyCourse,
                ParamsMessage = new List<object> { request.Coins, request.Month },
                SenderId = Guid.Empty,
                PlatformCode = EnumPlatformCode.LMS
            }, cancellationToken);

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
