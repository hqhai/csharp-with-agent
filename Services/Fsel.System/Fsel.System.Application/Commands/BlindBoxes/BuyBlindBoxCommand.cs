namespace Fsel.System.Application.Commands.BlindBoxes
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Commands.TokenHistoryCmd;
    using Fsel.System.Application.Queries.BlindBoxes;
    using Fsel.System.Application.Queues.Publisher;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Entities.BlindBoxs;
    using Fsel.System.Domain.IRepositories.BlindBoxes;
    using Fsel.System.Domain.Models.CommandModels.BlindBoxes;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Threading;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

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
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly SendNotifyBuyBlindBoxPublisher _sendNotifyBuyBlindBox;
        private const int MaxPercent = 100;
        private readonly ILogger<BuyBlindBoxCommand> _logger;

        public BuyBlindBoxCommandHandler(IMediator mediator, IBlindBoxUserRepository blindBoxUserRepository, AuthContext authContext, IBlindBoxChestConfigRepository blindBoxChestConfigRepository, IBlindBoxHistoryRepository blindBoxHistoryRepository, IUserService userService, IMapper mapper, SendNotifyBuyBlindBoxPublisher sendNotifyBuyBlindBox, ILogger<BuyBlindBoxCommand> logger)
        {
            _mediator = mediator;
            _blindBoxUserRepository = blindBoxUserRepository;
            _authContext = authContext;
            _blindBoxChestConfigRepository = blindBoxChestConfigRepository;
            _blindBoxHistoryRepository = blindBoxHistoryRepository;
            _userService = userService;
            _mapper = mapper;
            _sendNotifyBuyBlindBox = sendNotifyBuyBlindBox;
            _logger = logger;
        }

        public async Task<MethodResult<bool>> Handle(BuyBlindBoxCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var blindBoxUser = await _blindBoxUserRepository.Queryable.FirstOrDefaultAsync(p => p.UserId == _authContext.CurrentUserId, cancellationToken);

            if (blindBoxUser == null)
            {
                await SendNotify(StatusCodes.Status400BadRequest, EnumBuyBlindBoxErrorCode.NotIncludedInTheEvent.ToString(), null, null, null, cancellationToken);
                methodResult.AddErrorBadRequest(nameof(EnumBuyBlindBoxErrorCode.NotIncludedInTheEvent));
                return methodResult;
            }

            var blindBoxResult = await _mediator.Send(new GetBlindBoxByUserQuery(), cancellationToken);
            if (!blindBoxResult.IsOK)
            {
                await SendNotify(StatusCodes.Status400BadRequest, EnumBuyBlindBoxErrorCode.DataNotExist.ToString(), null, null, null, cancellationToken);
                methodResult.AddError(blindBoxResult.ErrorMessages);
                return methodResult;
            }

            var blindBox = blindBoxResult.Result;
            var blindBoxChestActive = blindBox?.BlindBoxChests?.FirstOrDefault(p => p.IsActive && p.Id == request.BlindBoxChestId);
            if (blindBoxChestActive == null)
            {
                await SendNotify(StatusCodes.Status400BadRequest, EnumBuyBlindBoxErrorCode.WrongChestReceived.ToString(), null, null, null, cancellationToken);
                methodResult.AddErrorBadRequest(nameof(EnumBuyBlindBoxErrorCode.WrongChestReceived));
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }

            var student = studentResult.Content?.Result;
            if (student?.NumberOfToken < blindBoxChestActive.OpenPrice)
            {
                await SendNotify(StatusCodes.Status400BadRequest, EnumBuyBlindBoxErrorCode.NotEnoughTokens.ToString(), null, null, null, cancellationToken);
                methodResult.AddErrorBadRequest(nameof(EnumBuyBlindBoxErrorCode.NotEnoughTokens));
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

                    var blindBoxChestConfigModels = _mapper.Map<List<BlindBoxChestConfigModel>>(blindBoxChestConfigs);

                    if (blindBoxHistories.Count < blindBoxUser.NumberOpen - 1)
                    {
                        await BuyBlindBox(methodResult, blindBoxChestConfigModels, blindBoxHistories, blindBoxChestActive, null, cancellationToken);
                    }
                    else
                    {
                        var blindBoxChestConfig = blindBoxChestConfigs.FirstOrDefault(p => p.ConfigType == EnumBlindBoxConfigType.Piece);
                        var blindBoxChestConfigModel = _mapper.Map<BlindBoxChestConfigModel>(blindBoxChestConfig);
                        await BuyBlindBox(methodResult, blindBoxChestConfigModels, blindBoxHistories, blindBoxChestActive, blindBoxChestConfigModel, cancellationToken);
                    }
                }
            }

            return methodResult;
        }

        private async Task BuyBlindBox(MethodResult<bool> methodResult, List<BlindBoxChestConfigModel>? blindBoxChestConfigs, List<BlindBoxHistory>? blindBoxHistories, BlindBoxChestModel blindBoxChest, BlindBoxChestConfigModel? blindBoxChestConfig, CancellationToken cancellationToken)
        {
            if (blindBoxChestConfigs == null || blindBoxChestConfigs.Count == 0)
            {
                var blindBoxChestConfigEntities = await _blindBoxChestConfigRepository.Queryable
               .Where(p => p.BlindBoxChestId == blindBoxChest.Id)
               .ToListAsync(cancellationToken);
                blindBoxChestConfigs = _mapper.Map<List<BlindBoxChestConfigModel>>(blindBoxChestConfigEntities);
            }

            if (!blindBoxChestConfigs.Any())
            {
                await SendNotify(StatusCodes.Status400BadRequest, EnumBuyBlindBoxErrorCode.DataNotExist.ToString(), null, null, null, cancellationToken);
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
                await SendNotify(StatusCodes.Status400BadRequest, EnumBuyBlindBoxErrorCode.DataNotExist.ToString(), null, null, null, cancellationToken);
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return;
            }

            var blindBoxHistory = new BlindBoxHistory
            {
                BlindBoxChestConfigId = blindBoxChestConfig.Id,
                IsPiece = blindBoxChestConfig.ConfigType == EnumBlindBoxConfigType.Piece,
                Coin = blindBoxChestConfig.ConfigType == EnumBlindBoxConfigType.Coin ? blindBoxChestConfig.Coin : null
            };

            await _blindBoxHistoryRepository.ExecuteTransactionAsync(async () =>
            {
                blindBoxHistory = _blindBoxHistoryRepository.Add(blindBoxHistory);
                await _blindBoxHistoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                if (blindBoxChestConfig.ConfigType == EnumBlindBoxConfigType.Coin)
                {
                    try
                    {
                        var plusTokens = await ProcessTokenTransactionAsync(methodResult, EnumTokenFeature.ReceiveTokensBlindBox, blindBoxChestConfig.Coin ?? 0, blindBoxHistory.Id, EnumTokenMission.OpenChestCoins, EnumTokenHistoryType.Recevived, cancellationToken);

                        if (!plusTokens)
                        {
                            return methodResult;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                }

                try
                {
                    var minusTokens = await ProcessTokenTransactionAsync(methodResult, EnumTokenFeature.BuyBlindBox, blindBoxChest.OpenPrice, blindBoxHistory.Id, GetEnumMission(blindBoxChest), EnumTokenHistoryType.Exchanged, cancellationToken);

                    if (!minusTokens)
                    {
                        return methodResult;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                if (blindBoxChestConfig.ConfigType == EnumBlindBoxConfigType.Coin)
                {
                    await SendNotify(StatusCodes.Status200OK, null, EnumBlindBoxConfigType.Coin.ToString(), null, blindBoxChestConfig.Coin ?? 0, cancellationToken);
                }
                else if (blindBoxChestConfig.ConfigType == EnumBlindBoxConfigType.GoodLuck)
                {
                    await SendNotify(StatusCodes.Status200OK, null, EnumBlindBoxConfigType.GoodLuck.ToString(), null, null, cancellationToken);
                }
                else if (blindBoxChestConfig.ConfigType == EnumBlindBoxConfigType.Piece)
                {
                    await SendNotify(StatusCodes.Status200OK, null, EnumBlindBoxConfigType.Piece.ToString(), blindBoxChestConfig.ImagePath, null, cancellationToken);
                }

                return methodResult;
            });
        }

        private static EnumTokenMission GetEnumMission(BlindBoxChestModel blindBoxChest)
        {
            if (blindBoxChest.Index == 1)
            {
                return EnumTokenMission.PurchaseChestFirst;
            }
            else if (blindBoxChest.Index == 2)
            {
                return EnumTokenMission.PurchaseChestSecond;
            }
            else if (blindBoxChest.Index == 3)
            {
                return EnumTokenMission.PurchaseChestThird;
            }
            else if (blindBoxChest.Index == 4)
            {
                return EnumTokenMission.PurchaseChestFourth;
            }
            else if (blindBoxChest.Index == 5)
            {
                return EnumTokenMission.PurchaseChestFifth;
            }
            else
            {
                return EnumTokenMission.PurchaseChestSixth;
            }
        }

        private async Task SendNotify(int statusCode, string? errorCode, string? configType, string? imagePath, int? coin, CancellationToken cancellationToken)
        {
            await _sendNotifyBuyBlindBox.Publish(new SendNotifyBuyBlindBoxModel
            {
                StatusCode = statusCode,
                ErrorCode = errorCode,
                ConfigType = configType,
                ImagePath = imagePath,
                Coin = coin
            }, cancellationToken);
        }

        private async Task<bool> ProcessTokenTransactionAsync(MethodResult<bool> methodResult, EnumTokenFeature tokenFeature, int tokens, Guid objectId, EnumTokenMission mission, EnumTokenHistoryType historyType, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CreateTokenHistoryCommand
            {
                TokenHistorys = new List<TokenHistoryQueueModel>
                                        {
                                            new TokenHistoryQueueModel
                                            {
                                                VolatileToken = tokens,
                                                Feature = tokenFeature,
                                                Mission = mission,
                                                UserId = _authContext.CurrentUserId,
                                                Type = historyType,
                                                ObjectId = objectId
                                            }
                                        }
            }, cancellationToken);
            if (!result.IsOK)
            {
                _logger.LogError(result.ErrorMessages.Serialize());
                methodResult.AddError(result.ErrorMessages);
                return false;
            }
            return true;
        }

        public static BlindBoxChestConfigModel? GetRandomBlindBoxChestConfig(IList<BlindBoxChestConfigModel> blindBoxChestConfigs)
        {
            ArgumentNullException.ThrowIfNull(blindBoxChestConfigs);
            Random random = new Random();
            int totalWeight = blindBoxChestConfigs.Sum(g => g.Percentage);
            if (totalWeight < MaxPercent)
            {
                NormalizeGiftProbabilities(blindBoxChestConfigs);
            }
            int randomValue = random.Next(0, totalWeight);
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

        public static void NormalizeGiftProbabilities(IList<BlindBoxChestConfigModel> blindBoxChestConfigs)
        {
            ArgumentNullException.ThrowIfNull(blindBoxChestConfigs);
            int totalWeight = blindBoxChestConfigs.Sum(g => g.Percentage);
            if (totalWeight < MaxPercent)
            {
                int missingWeight = MaxPercent - totalWeight;
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
