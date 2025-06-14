// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.UrBoxs
{
    using System;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Caching;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Commands.MarketplacePremiumCmd;
    using Fsel.Ordering.Application.Queries.UrBoxQuery;
    using Fsel.Ordering.Application.Queues.Publishers;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Request;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Response;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Application.Services.UserService.Models;
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
    using Microsoft.Extensions.Logging;

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

        private readonly ICacheService<StudentModel> _cacheService;
        private readonly ILogger<RedeemProductPremiumCommandHandler> _logger;

        private const string LanguageVN = "vi";
        private const string LanguageEN = "en";
        private const string CultureCodeVN = "vi-VN";
        private const string CultureCodeEN = "en-US";

        public CreateRedemptionRequestCommandHandler(IUrBoxService urBoxService, CreateTokenHistoryPublisher createTokenHistoryPublisher, AppSetting appSetting, IMediator mediator, AuthContext authContext, IUserService userService, IOrderTransactionRepository orderTransactionRepository, IHostEnvironment hostEnvironment, ICacheService<StudentModel> cacheService, ILogger<RedeemProductPremiumCommandHandler> logger)
        {
            _urBoxService = urBoxService;
            _createTokenHistoryPublisher = createTokenHistoryPublisher;
            _appSetting = appSetting;
            _mediator = mediator;
            _authContext = authContext;
            _userService = userService;
            _orderTransactionRepository = orderTransactionRepository;
            _hostEnvironment = hostEnvironment;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<MethodResult<RedemptionResponseModel>> Handle(CreateRedemptionRequestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<RedemptionResponseModel>();

            if (string.IsNullOrEmpty(request.PhoneNumber) || !request.PhoneNumber.IsValidPhoneNumber())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            if (request.DataBuy == null || request.DataBuy.Count == 0 || request.DataBuy.Count != 1)
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

            var gift = await _mediator.Send(new GetGiftQuery { Id = request.DataBuy.FirstOrDefault()?.PriceId }, cancellationToken).ConfigureAwait(false);
            if (!gift.IsOK)

            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            string? nameOther = string.Empty;

            var language = _authContext.CurrentCountryInfo?.CultureCode;
            if (language?.Substring(0, 2) == LanguageVN)
            {
                var giftOther = await _mediator.Send(new GetGiftWithLanguageQuery { Id = request.DataBuy.FirstOrDefault()?.PriceId, Language = LanguageEN }, cancellationToken).ConfigureAwait(false);
                if (!giftOther.IsOK)

                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
                language = CultureCodeEN;
                nameOther = giftOther.Result?.Title;
            }
            else
            {
                var giftOther = await _mediator.Send(new GetGiftWithLanguageQuery { Id = request.DataBuy.FirstOrDefault()?.PriceId, Language = LanguageVN }, cancellationToken).ConfigureAwait(false);
                if (!giftOther.IsOK)

                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }
                language = CultureCodeVN;
                nameOther = giftOther.Result?.Title;
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

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }

            var student = studentResult.Content?.Result;

            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var key = $"StudentRedeemUrBox_{_authContext.CurrentUserId}";

            var studentRedeemUrBoxCache = await _cacheService.GetAsync(key);
            if (studentRedeemUrBoxCache != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.TransactionInProgress));
                return methodResult;
            }

            await _cacheService.SetAsync(key, student, TimeSpan.FromSeconds(10));

            var totalPrice = quantity * price;

            if (student.NumberOfToken < totalPrice)
            {
                methodResult.AddErrorBadRequest(nameof(EnumProductErrorCode.NotEnoughTokens), nameof(student.NumberOfToken), student.NumberOfToken);
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

                    var result = await _userService.DeductCoinOfStudent(new DeductCoinOfStudentCommandModel()
                    {
                        UserId = student?.Human?.UserId ?? default,
                        NumberOfCoinsDeducted = totalPrice,
                        Feature = EnumTokenFeature.MarketPlace,
                        Mission = EnumTokenMission.UrBox,
                        ObjectId = orderTransaction.Id,
                        Translations = new List<TokenHistoryTranslationModel>()
                    {
                        new TokenHistoryTranslationModel()
                        {
                            Language = _authContext.CurrentCountryInfo?.CultureCode,
                            Config = new List<object>
                                {
                                        new { Title = gift.Result.Title }
                                }
                        },new TokenHistoryTranslationModel()
                        {
                            Language = language,
                            Config = new List<object>
                                {
                                        new { Title = nameOther }
                                }
                        },
                    }
                    });

                    if (!result.IsSuccessStatusCode)
                    {
                        methodResult.AddError(result.Error);
                        return methodResult;
                    }
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

                return methodResult;
            });

            return methodResult;
        }
    }
}
