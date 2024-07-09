// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.v1i1
{
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Application.Queues.Publishers;
    using Fsel.Ordering.Application.Services.CourseService;
    using Fsel.Ordering.Application.Services.InAppPurchase.IOS;
    using Fsel.Ordering.Application.Services.SystemService;
    using Fsel.Ordering.Application.Services.SystemService.Models;
    using Fsel.Ordering.Application.Services.TrainingService;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using JWT.Algorithms;
    using JWT.Builder;
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
        private readonly AddExpiredDateForStudentPublisher _addExpiredDateForStudentPublisher;
        private readonly IUserService _userService;
        private readonly ISystemService _systemService;
        private readonly IMediator _mediator;
        private readonly IPackageEventRepository _packageEventRepository;

        public GetInfoTransactionFromAppStoreCommandHandler(IOrderRepository orderRepository, IAppStoreService appStoreService, AppSetting appSetting, INotificationProcessor notificationProcessor, ILogger<GetInfoTransactionFromAppStoreCommand> logger, IHostEnvironment hostEnvironment, IPackageRepository packageRepository, ITrainingService trainingService, ILmsCourseService courseService, AddExpiredDateForStudentPublisher addExpiredDateForStudentPublisher, IUserService userService, ISystemService systemService, IMediator mediator, IPackageEventRepository packageEventRepository)
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
            _addExpiredDateForStudentPublisher = addExpiredDateForStudentPublisher;
            _userService = userService;
            _systemService = systemService;
            _mediator = mediator;
            _packageEventRepository = packageEventRepository;
        }

        public async Task<MethodResult<bool>> Handle(GetInfoTransactionFromAppStoreCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            _logger.LogError(_appSetting.Services.AppStoreApiUrl ?? "AppStoreApiUrl");

            if (string.IsNullOrEmpty(request.TransactionId))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var issuer = _appSetting.PurchaseSettings?.AppStore?.Issuer;
            var bundleId = _appSetting.PurchaseSettings?.AppStore?.BundleId;
            var keyId = _appSetting.PurchaseSettings?.AppStore?.KeyId;
            var audience = _appSetting.PurchaseSettings?.AppStore?.Audience;
            var iat = ConvertToUnixTimestamp(DateTime.UtcNow);
            var exp = ConvertToUnixTimestamp(DateTime.UtcNow.AddMinutes(60));

            string privateKey = File.ReadAllText(ResourceSettings.AppStore);

            byte[] privateKeyBytes = Convert.FromBase64String(ExtractBase64FromPem(privateKey));

            string token;

            using (ECDsa prvKey = ECDsa.Create())
            {
                prvKey.ImportPkcs8PrivateKey(privateKeyBytes, out var read);
                var jwtBuilder = new JwtBuilder()
                    .WithAlgorithm(new ES256Algorithm(prvKey, prvKey))
                    .AddHeader("kid", keyId)
                    .ExpirationTime(exp)
                    .IssuedAt(iat)
                    .Issuer(issuer)
                    .Audience(audience)
                    .AddClaim("bid", bundleId);
                token = jwtBuilder.Encode();
            }

            _logger.LogError(token);

            var purchaseSetting = _appSetting.PurchaseSettings.Serialize();
            if (purchaseSetting == "null")
            {
                _logger.LogError("PurchaseSettings");
            }
            else
            {
                _logger.LogError(purchaseSetting);
            }

            var signedTransactionInfoResult = await _appStoreService.GetInfoTransaction(token, request.TransactionId);

            if (!signedTransactionInfoResult.IsSuccessStatusCode)
            {
                _logger.LogError("Get info transaction not success");
                var message = signedTransactionInfoResult.Error?.Message;
                _logger.LogError(message);
                methodResult.AddErrorBadRequest(signedTransactionInfoResult.Error?.Message);
                methodResult.StatusCode = (int)signedTransactionInfoResult.StatusCode;
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

            #region Check environment

            //if ((_hostEnvironment.IsDevelopment() || _hostEnvironment.IsStaging() || _hostEnvironment.IsEnvironment(Settings.Environments.Testing)) && transactionInfo.Environment != EnumAppStoreEnvironment.Sandbox)
            //{
            //    _logger.LogError("Invalid Environment");
            //    return methodResult;
            //}

            //if (_hostEnvironment.IsProduction() && transactionInfo.Environment != EnumAppStoreEnvironment.Production)
            //{
            //    _logger.LogError("Invalid Environment");
            //    return methodResult;
            //}

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
                var changeStatusResult = _mediator.Send(new ChangeStatusOrderCommand()
                {
                    OrderId = order.Id,
                    OrderStatus = EnumOrderStatus.Payment,
                    Receipt = transactionInfo.Serialize(),
                    Type = EnumOrderTransactionType.AppStore
                });

                var studentResult = await _userService.GetStudentByUserIdAsync(order.UserId);
                if (!studentResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(studentResult.Error);
                    return methodResult;
                }
                var student = studentResult.Content?.Result;

                if (order.IsInvoice)
                {
                    await _systemService.AddPaymentInfoToGoogleSheet(new AddPaymentInfoToGoogleSheetModel()
                    {
                        Code = order.Code,
                        CreatedDate = order.CreatedDate.ToString("dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture),
                        Price = order.Price.ToString(CultureInfo.InvariantCulture),
                        FullName = order.FullName,
                        StudentEmail = student?.Human?.Email,
                        BillingEmail = order.Email,
                        CompanyTaxCode = order.CompanyTaxCode,
                        CompanyAddress = order.CompanyAddress,
                        CompanyName = order.CompanyName,
                    });
                }

                #region Gửi mail thanh toán

                if (order.Status == EnumOrderStatus.Payment)
                {
                    await _mediator.Send(new SendMailPaymentCommand() { OrderId = order.Id });
                }

                #endregion Gửi mail thanh toán

                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }

        private static long ConvertToUnixTimestamp(DateTime dateTime)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            TimeSpan timeDifference = dateTime.ToUniversalTime() - epoch;

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
