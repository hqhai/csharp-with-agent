namespace Fsel.System.Application.Commands.BlindBoxes
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Commands.TokenHistoryCmd;
    using Fsel.System.Application.Queries.BlindBoxes;
    using Fsel.System.Domain.Entities.BlindBoxs;
    using Fsel.System.Domain.IRepositories.BlindBoxes;
    using Fsel.System.Domain.Models.CommandModels.BlindBoxes;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Threading;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class BuyBlindBoxCommand : BuyBlindBoxCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class BuyBlindBoxCommandHandler : IRequestHandler<BuyBlindBoxCommand, MethodResult<bool>>
    {
        private readonly IMediator _mediator;
        private readonly IBlindBoxUserRepository _blindBoxUserRepository;
        private readonly AuthContext _authContext;
        private readonly IBlindBoxChestConfigRepository _blindBoxChestConfigRepository;
        private readonly IBlindBoxHistoryRepository _blindBoxHistoryRepository;

        public BuyBlindBoxCommandHandler(IMediator mediator, IBlindBoxUserRepository blindBoxUserRepository, AuthContext authContext, IBlindBoxChestConfigRepository blindBoxChestConfigRepository, IBlindBoxHistoryRepository blindBoxHistoryRepository)
        {
            _mediator = mediator;
            _blindBoxUserRepository = blindBoxUserRepository;
            _authContext = authContext;
            _blindBoxChestConfigRepository = blindBoxChestConfigRepository;
            _blindBoxHistoryRepository = blindBoxHistoryRepository;
        }

        public async Task<MethodResult<bool>> Handle(BuyBlindBoxCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var blindBoxUser = await _blindBoxUserRepository.Queryable.FirstOrDefaultAsync(p => p.UserId == _authContext.CurrentUserId, cancellationToken);

            if (blindBoxUser == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var blindBoxResult = await _mediator.Send(new GetBlindBoxesQuery(), cancellationToken);
            if (!blindBoxResult.IsOK)
            {
                methodResult.AddError(blindBoxResult.ErrorMessages);
                return methodResult;
            }

            var blindBox = blindBoxResult.Result;
            var blindBoxChestActive = blindBox?.BlindBoxChests?.FirstOrDefault(p => p.IsActive);
            if (blindBoxChestActive == null || blindBoxChestActive.Id != request.BlindBoxChestId)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            if (!blindBoxChestActive.IsLast)
            {
                await BuyBlindBox(methodResult, null, null, blindBoxChestActive, null, cancellationToken);
            }
            else
            {
                if (!blindBoxUser.IsWin)
                {
                    await BuyBlindBox(methodResult, null, null, blindBoxChestActive, null, cancellationToken);
                }
                else
                {
                    var blindBoxChestConfigs = await _blindBoxChestConfigRepository.Queryable.Where(p => p.BlindBoxChestId == blindBoxChestActive.Id).ToListAsync(cancellationToken);
                    if (!blindBoxChestConfigs.Any())
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                        return methodResult;
                    }
                    var blindBoxChestConfigIds = blindBoxChestConfigs.Select(p => p.Id).ToList();
                    var blindBoxHistories = await _blindBoxHistoryRepository.Queryable
                        .WhereBulkContains(blindBoxChestConfigIds, p => p.BlindBoxChestConfigId)
                        .Where(p => p.CreatedUserId == _authContext.CurrentUserId)
                        .ToListAsync(cancellationToken);

                    if (blindBoxHistories.Count < blindBoxUser.NumberOpen - 1)
                    {
                        await BuyBlindBox(methodResult, blindBoxChestConfigs, blindBoxHistories, blindBoxChestActive, null, cancellationToken);
                    }
                    else
                    {
                        var blindBoxChestConfig = blindBoxChestConfigs.FirstOrDefault(p => p.ConfigType == EnumBlindBoxConfigType.Coin);
                        await BuyBlindBox(methodResult, blindBoxChestConfigs, blindBoxHistories, blindBoxChestActive, blindBoxChestConfig, cancellationToken);
                    }
                }
            }

            return methodResult;
        }

        private async Task BuyBlindBox(MethodResult<bool> methodResult, List<BlindBoxChestConfig>? blindBoxChestConfigs, List<BlindBoxHistory>? blindBoxHistories, BlindBoxChestModel blindBoxChest, BlindBoxChestConfig? blindBoxChestConfig, CancellationToken cancellationToken)
        {
            if (blindBoxChestConfigs == null || blindBoxChestConfigs.Count == 0)
            {
                blindBoxChestConfigs = await _blindBoxChestConfigRepository.Queryable
              .Where(p => p.BlindBoxChestId == blindBoxChest.Id)
              .ToListAsync(cancellationToken);
            }

            if (!blindBoxChestConfigs.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return;
            }

            var blindBoxChestConfigIds = blindBoxChestConfigs.Select(p => p.Id).ToList();

            if (blindBoxHistories == null || blindBoxHistories.Count == 0)
            {
                blindBoxHistories = await _blindBoxHistoryRepository.Queryable
               .WhereBulkContains(blindBoxChestConfigIds, p => p.BlindBoxChestConfigId)
               .Where(p => p.CreatedUserId == _authContext.CurrentUserId)
               .ToListAsync(cancellationToken);
            }

            if (blindBoxChestConfig == null)
            {
                blindBoxChestConfig = blindBoxChest.IsLast
                                      ? GetRandomBlindBoxChestConfig(blindBoxChestConfigs.Where(p => p.ConfigType != EnumBlindBoxConfigType.Piece).ToList())
                                      : (blindBoxChest.MaxOpenCount.HasValue && blindBoxHistories.Count == (blindBoxChest.MaxOpenCount - 1))
                                      ? blindBoxChestConfigs.FirstOrDefault(p => p.ConfigType == EnumBlindBoxConfigType.Piece)
                                      : GetRandomBlindBoxChestConfig(blindBoxChestConfigs);
            }

            if (blindBoxChestConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return;
            }

            var blindBoxHistory = new BlindBoxHistory
            {
                BlindBoxChestConfigId = blindBoxChestConfig.Id,
                IsPiece = blindBoxChestConfig.ConfigType == EnumBlindBoxConfigType.Piece,
                Coin = blindBoxChestConfig.ConfigType == EnumBlindBoxConfigType.Coin ? blindBoxChestConfig.Coin : null,
                Code = blindBoxChest.IsLast ? "ABC" : null
            };

            await _blindBoxHistoryRepository.ExecuteTransactionAsync(async () =>
            {
                _blindBoxHistoryRepository.Add(blindBoxHistory);
                await _blindBoxHistoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                if (blindBoxChestConfig.ConfigType == EnumBlindBoxConfigType.Coin)
                {
                    var updateTokenStudent = await _mediator.Send(new CreateTokenHistoryCommand
                    {
                        TokenHistorys = new List<TokenHistoryQueueModel>
                                        {
                                            new TokenHistoryQueueModel
                                            {
                                                VolatileToken = blindBoxChestConfig.Coin ?? 0,
                                                Feature = EnumTokenFeature.BlindBox,
                                                Mission = EnumTokenMission.BuyBlindBox,
                                                UserId = _authContext.CurrentUserId,
                                                Type = EnumTokenHistoryType.Recevived,
                                                ObjectId = blindBoxChestConfig.Id
                                            }
                                        }
                    });

                    if (!updateTokenStudent.IsOK)
                    {
                        methodResult.AddError(updateTokenStudent.ErrorMessages);
                    }
                }
                return methodResult;
            });
        }

        public static BlindBoxChestConfig? GetRandomBlindBoxChestConfig(IList<BlindBoxChestConfig> blindBoxChestConfigs)
        {
            ArgumentNullException.ThrowIfNull(blindBoxChestConfigs);
            Random random = new Random();
            int totalWeight = blindBoxChestConfigs.Sum(g => g.Percentage);
            if (totalWeight < 100)
            {
                NormalizeGiftProbabilities(blindBoxChestConfigs);
            }
            int randomValue = random.Next() * totalWeight;
            int cumulative = 0;

            foreach (var blindBoxChestConfig in blindBoxChestConfigs.OrderBy(p => p.Percentage).ToList())
            {
                cumulative += blindBoxChestConfig.Percentage;
                if (randomValue < cumulative)
                {
                    return blindBoxChestConfig;
                }
            }
            return null;
        }

        public static void NormalizeGiftProbabilities(IList<BlindBoxChestConfig> blindBoxChestConfigs)
        {
            ArgumentNullException.ThrowIfNull(blindBoxChestConfigs);
            int totalWeight = blindBoxChestConfigs.Sum(g => g.Percentage);
            if (totalWeight < 100)
            {
                int missingWeight = 100 - totalWeight;
                int additionalWeightPerGift = missingWeight / blindBoxChestConfigs.Count;
                int remainder = missingWeight % blindBoxChestConfigs.Count;

                for (int i = 0; i < blindBoxChestConfigs.Count; i++)
                {
                    blindBoxChestConfigs[i].Percentage += additionalWeightPerGift;
                    if (remainder > 0)
                    {
                        blindBoxChestConfigs[i].Percentage += 1;
                        remainder--;
                    }
                }
            }
        }
    }
}
