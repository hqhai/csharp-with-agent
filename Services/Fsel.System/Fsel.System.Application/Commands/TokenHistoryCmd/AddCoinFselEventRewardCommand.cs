// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.TokenHistoryCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Domain.Models.CommandModels.TokenHistorys;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class AddCoinFselEventRewardCommand : IRequest<MethodResult<bool>>
    {
        public IList<AddCoinFselEventRewardCommandModel>? Values { get; set; }
    }

    public class AddCoinFselEventRewardCommandHandler : IRequestHandler<AddCoinFselEventRewardCommand, MethodResult<bool>>
    {
        private readonly IMediator _mediator;

        public AddCoinFselEventRewardCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(AddCoinFselEventRewardCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Values);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            foreach (var value in request.Values)
            {
                var newTokenHistory = new List<TokenHistoryQueueModel>
                {
                    new TokenHistoryQueueModel
                    {
                        VolatileToken = value.Coin,
                        UserId = value.UserId,
                        Feature = EnumTokenFeature.FselEvent,
                        Mission = EnumTokenMission.FselEventReward,
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
                                         Title = $"Thưởng sự kiện của FSEL"
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
                                         Title = $"FSEL Event Reward"
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
                                         Title = $"Récompense d'événement FSEL"
                                    }
                                }
                            }
                        }
                    }
                };

                await _mediator.Send(new CreateTokenHistoryCommand { TokenHistorys = newTokenHistory }, cancellationToken);
            }
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
