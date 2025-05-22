// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.TokenHistoryCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Queues.Publisher;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class AddCoinSurveyRewardCommand : IRequest<MethodResult<bool>>
    {
        public IList<Guid>? UserIds { get; set; }

        public double Coin { get; set; }
    }

    public class AddCoinSurveyRewardCommandHandler : IRequestHandler<AddCoinSurveyRewardCommand, MethodResult<bool>>
    {
        private readonly IMediator _mediator;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;

        public AddCoinSurveyRewardCommandHandler(IMediator mediator, NotificationMessagePublisher notificationMessagePublisher)
        {
            _mediator = mediator;
            _notificationMessagePublisher = notificationMessagePublisher;
        }

        public async Task<MethodResult<bool>> Handle(AddCoinSurveyRewardCommand request, CancellationToken cancellationToken)
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
                        VolatileToken = request.Coin,
                        UserId = userId,
                        Feature = EnumTokenFeature.FselEvent,
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
                                         Title = "đánh giá FSEL 5 sao"
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
                                         Title = "rating FSEL 5 stars"
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
                                         Title = "Évaluez FSEL 5 étoiles"
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
                Content = EnumNotificationContent.SurveyReward,
                ParamsMessage = new List<object> { $"{request.Coin}" },
                SenderId = Guid.Empty,
                PlatformCode = EnumPlatformCode.LMS
            }, cancellationToken);

            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
