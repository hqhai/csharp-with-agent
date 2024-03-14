// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.v1i1
{
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Application.Services.CourseService;
    using Fsel.Ordering.Application.Services.InAppPurchase;
    using Fsel.Ordering.Application.Services.InAppPurchase.Models;
    using Fsel.Ordering.Application.Services.TrainingService;
    using Fsel.Ordering.Application.Services.TrainingService.CommandModels;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Jose;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class GetInfoTransactionFromAppStoreCommand : IRequest<MethodResult<bool>>
    {
        public string? TransactionId { get; set; }
    }

    public class GetInfoTransactionFromAppStoreCommandHandler : IRequestHandler<GetInfoTransactionFromAppStoreCommand, MethodResult<bool>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly AppSetting _appSetting;
        private readonly IAppStoreService _appStoreService;
        private readonly INotificationProcessor _notificationProcessor;
        private readonly ILogger<GetInfoTransactionFromAppStoreCommand> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IPackageRepository _packageRepository;
        private readonly ITrainingService _trainingService;
        private readonly ILmsCourseService _courseService;

        public GetInfoTransactionFromAppStoreCommandHandler(IOrderRepository orderRepository, IAppStoreService appStoreService, AppSetting appSetting, INotificationProcessor notificationProcessor, ILogger<GetInfoTransactionFromAppStoreCommand> logger, IHostEnvironment hostEnvironment, IPackageRepository packageRepository, ITrainingService trainingService, ILmsCourseService courseService)
        {
            _orderRepository = orderRepository;
            _appStoreService = appStoreService;
            _appSetting = appSetting;
            _notificationProcessor = notificationProcessor;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
            _packageRepository = packageRepository;
            _trainingService = trainingService;
            _courseService = courseService;
        }

        public async Task<MethodResult<bool>> Handle(GetInfoTransactionFromAppStoreCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (string.IsNullOrEmpty(request.TransactionId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var issuer = _appSetting.PurchaseSettings?.AppStore?.Issuer;
            var bundleId = _appSetting.PurchaseSettings?.AppStore?.BundleId;
            var keyId = _appSetting.PurchaseSettings?.AppStore?.KeyId;
            var audience = _appSetting.PurchaseSettings?.AppStore?.Audience;
            var iat = ConvertToUnixTimestamp(DateTimeOffset.UtcNow);
            var exp = ConvertToUnixTimestamp(DateTimeOffset.UtcNow.AddMinutes(60));

            var header = new Dictionary<string, object>()
            {
                { "alg", "ES256" },
                { "kid", keyId ?? string.Empty },
                { "typ", "JWT" }
            };

            var payload = new
            {
                iss = issuer,
                iat = iat,
                exp = exp,
                aud = audience,
                bid = bundleId
            };

            string privateKey = File.ReadAllText(ResourceSettings.AppStore);

            byte[] privateKeyBytes = Convert.FromBase64String(ExtractBase64FromPem(privateKey));

            CngKey key = CngKey.Import(privateKeyBytes, CngKeyBlobFormat.Pkcs8PrivateBlob);

            string token = JWT.Encode(payload, key, JwsAlgorithm.ES256, header);

            var signedTransactionInfoResult = await _appStoreService.GetInfoTransaction(token, request.TransactionId);
            if (!signedTransactionInfoResult.IsSuccessStatusCode)
            {
                _logger.LogError("Get info transaction not success");
                methodResult.AddError(signedTransactionInfoResult.Error);
                return methodResult;
            }

            var transactionInfo = _notificationProcessor.TransactionInfo(signedTransactionInfoResult.Content?.SignedTransactionInfo ?? string.Empty);
            if (transactionInfo == null)
            {
                _logger.LogError("SignedTransactionInfo is not valid");
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            #region Validate

            if (transactionInfo.BundleId != _appSetting.PurchaseSettings?.AppStore?.BundleId)
            {
                _logger.LogError("BundleId is wrong");
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            if (transactionInfo.TransactionReason != "PURCHASE")
            {
                _logger.LogError("Transaction in not PURCHASE");
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            #region Check environment

            if ((_hostEnvironment.IsDevelopment() || _hostEnvironment.IsStaging() || _hostEnvironment.IsEnvironment(Settings.Environments.Testing)) && transactionInfo.Environment != EnumAppStoreEnvironment.Sandbox)
            {
                _logger.LogError("Invalid Environment");
                return methodResult;
            }

            if (_hostEnvironment.IsProduction() && transactionInfo.Environment != EnumAppStoreEnvironment.Production)
            {
                _logger.LogError("Invalid Environment");
                return methodResult;
            }

            #endregion Check environment

            #endregion Validate

            var package = await _packageRepository.Queryable.FirstOrDefaultAsync(p => p.Name == transactionInfo.ProductId, cancellationToken);

            if (package == null)
            {
                _logger.LogError($"Package is null");
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var order = await _orderRepository.Queryable.Include(p => p.OrderTransactions).FirstOrDefaultAsync(p => p.UserId.ToString() == transactionInfo.AppAccountToken && p.PackageId == package.Id && p.Status == EnumOrderStatus.New, cancellationToken);
            if (order == null)
            {
                _logger.LogError($"Order is null");
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                var numberOfShield = (package != null && package.Code.HasValue) ? (int)package.Code.Value : default;

                var addStudentIntoClassResult = await _trainingService.AddStudentIntoClass(new AddStudentIntoClassCommandModel() { UserId = order.CreatedUserId, CourseId = order.CourseId, PackageId = order.PackageId ?? default, NumberOfShield = numberOfShield });

                if (!addStudentIntoClassResult.IsSuccessStatusCode)
                {
                    _logger.LogError(addStudentIntoClassResult.Error.Content);
                    methodResult.AddError(addStudentIntoClassResult.Error);
                    return methodResult;
                }

                var updateNextUnitResult = await _courseService.UpdateNextUnit(order.UserId);
                if (!updateNextUnitResult.IsSuccessStatusCode)
                {
                    _logger.LogError(addStudentIntoClassResult.Error.Content);
                    methodResult.AddError(updateNextUnitResult.Error);
                    return methodResult;
                }

                order.ExpireDate = DateTime.UtcNow.AddMonths(package!.MonthNumber);
                order.Status = EnumOrderStatus.Payment;

                order.OrderTransactions.Add(new OrderTransaction()
                {
                    ResponseBody = transactionInfo,
                    Type = EnumOrderTransactionType.AppStore,
                    Status = EnumOrderTransactionStatus.Success
                });

                order = _orderRepository.Update(order);
                await _orderRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }

        private static long ConvertToUnixTimestamp(DateTimeOffset dateTime)
        {
            DateTimeOffset epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

            TimeSpan timeDifference = dateTime - epoch;

            return (long)timeDifference.TotalSeconds;
        }

        private static string ExtractBase64FromPem(string pemContent)
        {
            const string beginMarker = "-----BEGIN PRIVATE KEY-----";
            const string endMarker = "-----END PRIVATE KEY-----";

            int startIndex = pemContent.IndexOf(beginMarker);
            int endIndex = pemContent.IndexOf(endMarker);

            if (startIndex < 0 || endIndex < 0)
            {
                throw new InvalidOperationException("Invalid PEM format.");
            }

            startIndex += beginMarker.Length;
            int base64Length = endIndex - startIndex;

            string base64Content = pemContent.Substring(startIndex, base64Length).Trim();

            return base64Content;
        }
    }
}
