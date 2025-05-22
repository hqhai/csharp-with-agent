// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.UrBoxs
{
    using System;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Queries.UrBoxQuery;
    using Fsel.Ordering.Application.Queues.Publishers;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Request;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Response;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.UrBox;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.Extensions.Hosting;

    public class CreateRedemptionRequestCommand : CreateRedemptionRequestCommandModel, IRequest<MethodResult<RedemptionResponseModel>>
    {
    }

    public class CreateRedemptionRequestCommandHandler : IRequestHandler<CreateRedemptionRequestCommand, MethodResult<RedemptionResponseModel>>
    {
        private readonly IUrBoxService _urBoxService;
        private readonly CreateTokenHistoryPublisher _createTokenHistoryPublisher;
        private readonly AppSetting _appSetting;
        private readonly IMediator _mediator;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IOrderTransactionRepository _orderTransactionRepository;
        private readonly IHostEnvironment _hostEnvironment;

        public CreateRedemptionRequestCommandHandler(IUrBoxService urBoxService, CreateTokenHistoryPublisher createTokenHistoryPublisher, AppSetting appSetting, IMediator mediator, AuthContext authContext, IUserService userService, IOrderTransactionRepository orderTransactionRepository, IHostEnvironment hostEnvironment)
        {
            _urBoxService = urBoxService;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
            _appSetting = appSetting;
            _mediator = mediator;
            _authContext = authContext;
            _userService = userService;
            _orderTransactionRepository = orderTransactionRepository;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<MethodResult<RedemptionResponseModel>> Handle(CreateRedemptionRequestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<RedemptionResponseModel>();

            if (request.DataBuy == null || request.DataBuy.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            foreach (var item in request.DataBuy)
            {
                if (string.IsNullOrEmpty(item.PriceId) || string.IsNullOrEmpty(item.Quantity))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
            }

            if (string.IsNullOrEmpty(request.PhoneNumber))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            var token = student?.NumberOfToken ?? default;

            var gift = await _mediator.Send(new GetGiftQuery { Id = request.DataBuy.FirstOrDefault()?.PriceId }, cancellationToken).ConfigureAwait(false);
            if (!gift.IsOK)

            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            if (!long.TryParse(gift.Result?.Price, out long price))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            if (!long.TryParse(request.DataBuy.FirstOrDefault()?.Quantity, out long quantity))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var totalPrice = quantity * price;
            if (token < totalPrice)
            {
                methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.NotEnoughTokens), nameof(token), token);
                return methodResult;
            }

            await _orderTransactionRepository.ExecuteTransactionAsync(async () =>
            {
                var orderTransaction = _orderTransactionRepository.Add(new OrderTransaction() { Type = EnumOrderTransactionType.UrBox });
                await _orderTransactionRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                var redemptionRequest = new CreateRedemptionRequestModel(_appSetting);

                redemptionRequest.SiteUserId = _authContext.CurrentUserId.ToString();
                redemptionRequest.TransactionId = orderTransaction.Id.ToString();
                redemptionRequest.PhoneNumber = request.PhoneNumber;
                redemptionRequest.IsSendSms = 0;
                redemptionRequest.DataBuy = request.DataBuy.Select(p => new DataBuy { PriceId = p.PriceId, Quantity = p.Quantity }).ToList();

                if (!int.TryParse(gift.Result?.Type, out int type))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }

                if (type == (int)EnumGiftType.Physics)
                {
                    //redemptionRequest.Language = "vi";
                    redemptionRequest.ShippingInfoAvailable = 2;
                    redemptionRequest.CityId = request.CityId;
                    redemptionRequest.DistrictId = request.DistrictId;
                    redemptionRequest.WardId = request.WardId;
                    redemptionRequest.TtAddress = request.Address;
                    redemptionRequest.DeliveryNote = request.Note;
                }

                var urBoxSignature = new UrBoxSignatureModel(_appSetting)
                {
                    DataBuy = redemptionRequest.DataBuy,
                    IsSendSms = redemptionRequest.IsSendSms,
                    SiteUserId = redemptionRequest.SiteUserId,
                    TransactionId = redemptionRequest.TransactionId
                };

                string requestBody = urBoxSignature.Serialize();
                string? privateKeyPath;

                if (_hostEnvironment.IsProduction() || _hostEnvironment.IsStaging())
                {
                    privateKeyPath = ResourceSettings.PrivateKeyProdUrBox;
                }
                else
                {
                    privateKeyPath = ResourceSettings.PrivateKeyDevUrBox;
                }

                var signature = EncodeHelper.CreateDigitalSignature(requestBody, privateKeyPath, HashAlgorithmName.SHA256);

                if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(redemptionRequest.AppId) || string.IsNullOrEmpty(redemptionRequest.AppSecret))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }

                var createRedemptionRequest = await _urBoxService.CreateRedemptionRequest(redemptionRequest, signature);
                if (createRedemptionRequest.Content?.Status == 200)
                {
                    methodResult.Result = createRedemptionRequest.Content;
                }
                else
                {
                    methodResult.AddErrorBadRequest(createRedemptionRequest.Content?.Msg);
                }

                orderTransaction.RequestBody = redemptionRequest;
                orderTransaction.ResponseBody = createRedemptionRequest.Content;
                orderTransaction.Status = createRedemptionRequest.Content?.Status == 200 ? EnumOrderTransactionStatus.Success : EnumOrderTransactionStatus.Fail;

                _orderTransactionRepository.Update(orderTransaction);
                await _orderTransactionRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                if (orderTransaction.Status == EnumOrderTransactionStatus.Success)
                {
                    var configs = new List<object>();

                    for (int i = 0; i < quantity; i++)
                    {
                        var data = new
                        {
                            Id = gift.Result.Id,
                            Title = gift.Result.Title,
                            Price = price
                        };
                        configs.Add(data);
                    }
                    var tokenHistorys = new List<TokenHistoryQueueModel>
                    {
                        new TokenHistoryQueueModel
                        {
                            ObjectId = orderTransaction.Id,
                            VolatileToken = totalPrice,
                            Feature = EnumTokenFeature.MarketPlace,
                            Type = EnumTokenHistoryType.Exchanged,
                            UserId = student?.UserId ?? default,
                            Mission = EnumTokenMission.UrBox,
                            Config = configs
                        }
                    };
                    await _createTokenHistoryPublisher.Publish(tokenHistorys, cancellationToken).ConfigureAwait(false);
                }
                return methodResult;
            });

            return methodResult;
        }
    }
}
