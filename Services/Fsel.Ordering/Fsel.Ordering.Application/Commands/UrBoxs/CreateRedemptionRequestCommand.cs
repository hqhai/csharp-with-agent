// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.UrBoxs
{
    using System;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Queries.UrBoxQuery;
    using Fsel.Ordering.Application.Services.UrBoxService;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Request;
    using Fsel.Ordering.Application.Services.UrBoxService.Models.Response;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.UrBox;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CreateRedemptionRequestCommand : CreateRedemptionRequestCommandModel, IRequest<MethodResult<RedemptionResponseModel>>
    {
    }

    public class CreateRedemptionRequestCommandHandler : IRequestHandler<CreateRedemptionRequestCommand, MethodResult<RedemptionResponseModel>>
    {
        private readonly IUrBoxService _urBoxService;
        private readonly AppSetting _appSetting;
        private readonly IMediator _mediator;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IUrBoxTransactionRepository _urBoxTransactionRepository;

        public CreateRedemptionRequestCommandHandler(IUrBoxService urBoxService, AppSetting appSetting, IMediator mediator, AuthContext authContext, IUserService userService, IUrBoxTransactionRepository urBoxTransactionRepository)
        {
            _urBoxService = urBoxService;
            _appSetting = appSetting;
            _mediator = mediator;
            _authContext = authContext;
            _userService = userService;
            _urBoxTransactionRepository = urBoxTransactionRepository;
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

            var token = studentResult.Content?.Result?.NumberOfToken ?? 0;

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

            if (token < price)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Min));
                return methodResult;
            }

            await _urBoxTransactionRepository.ExecuteTransactionAsync(async () =>
            {
                string transactionId;
                while (true)
                {
                    transactionId = NumberHelper.GenerateCodeNumber(11);
                    if (!await _urBoxTransactionRepository.Queryable.AnyAsync(p => p.TransactionId == transactionId))
                    {
                        break;
                    }
                }

                var redemptionRequest = new CreateRedemptionRequestModel(_appSetting);

                redemptionRequest.SiteUserId = _authContext.CurrentUserId.ToString();
                redemptionRequest.TransactionId = transactionId;
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

                var signature = EncodeHelper.CreateDigitalSignature(requestBody, ResourceSettings.PrivateKeyUrBox, HashAlgorithmName.SHA256);

                if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(redemptionRequest.AppId) || string.IsNullOrEmpty(redemptionRequest.AppSecret))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }

                var createRedemptionRequest = await _urBoxService.CreateRedemptionRequest(redemptionRequest, signature);
                if (createRedemptionRequest.Content?.Status == 200)
                {
                    var updateTokenResult = await _userService.UpdateStudentByTokenAsync(new Application.Services.UserService.Models.UpdateStudentByTokenModel { StudentId = studentResult.Content?.Result?.Id ?? default, NumberOfToken = token - price });
                    if (!updateTokenResult.IsSuccessStatusCode)
                    {
                        methodResult.AddError(updateTokenResult.Error);
                        return methodResult;
                    }
                    methodResult.Result = createRedemptionRequest.Content;
                }
                else
                {
                    methodResult.AddErrorBadRequest(createRedemptionRequest.Content?.Msg);
                }

                _urBoxTransactionRepository.Add(new UrBoxTransaction()
                {
                    TransactionId = transactionId,
                    RequestBody = redemptionRequest,
                    ResponseBody = createRedemptionRequest.Content,
                    Status = createRedemptionRequest.Content?.Status == 200 ? EnumUrBoxTransactionStatus.Success : EnumUrBoxTransactionStatus.Unsuccessful
                });
                await _urBoxTransactionRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                return methodResult;
            });
            return methodResult;
        }
    }
}
