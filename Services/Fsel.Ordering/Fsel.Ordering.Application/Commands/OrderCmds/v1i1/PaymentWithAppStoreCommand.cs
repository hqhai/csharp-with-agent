// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.v1i1
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Constants;
    using Fsel.Ordering.Application.Services.CourseService;
    using Fsel.Ordering.Application.Services.InAppPurchase.Models;
    using Fsel.Ordering.Application.Services.TrainingService;
    using Fsel.Ordering.Application.Services.TrainingService.CommandModels;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class PaymentWithAppStoreCommand : PaymentWithAppStoreCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class PaymentWithAppStoreCommandHandler : IRequestHandler<PaymentWithAppStoreCommand, MethodResult<bool>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<PaymentWithAppStoreCommand> _logger;
        private readonly AppSetting _appSetting;
        private readonly IPackageRepository _packageRepository;
        private readonly ITrainingService _trainingService;
        private readonly ILmsCourseService _courseService;
        private readonly IHostEnvironment _hostEnvironment;

        public PaymentWithAppStoreCommandHandler(IOrderRepository orderRepository, ILogger<PaymentWithAppStoreCommand> logger, AppSetting appSetting, IPackageRepository packageRepository, ITrainingService trainingService, ILmsCourseService courseService, IHostEnvironment hostEnvironment)
        {
            _orderRepository = orderRepository;
            _logger = logger;
            _appSetting = appSetting;
            _packageRepository = packageRepository;
            _trainingService = trainingService;
            _courseService = courseService;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<MethodResult<bool>> Handle(PaymentWithAppStoreCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            methodResult.Result = false;

            #region Validate

            if (string.IsNullOrEmpty(request.DecodedPayload?.Data?.AppAppleId) || string.IsNullOrEmpty(request.DecodedPayload?.Data?.BundleId))
            {
                _logger.LogError("AppAppleId is null or BundleId is null");
                return methodResult;
            }
            if (request.DecodedPayload?.Data?.AppAppleId != _appSetting.PurchaseSettings?.AppStore?.AppId || request.DecodedPayload?.Data?.BundleId != _appSetting.PurchaseSettings?.AppStore?.BundleId)
            {
                _logger.LogError("AppAppleId is wrong or BundleId is wrong");
                return methodResult;
            }
            if (string.IsNullOrEmpty(request.TransactionInfo?.ProductId))
            {
                _logger.LogError("ProductId is null");
                return methodResult;
            }
            if (string.IsNullOrEmpty(request.TransactionInfo?.AppAccountToken))
            {
                _logger.LogError("UserId is null");
                return methodResult;
            }

            #endregion Validate

            #region Check environment

            if ((_hostEnvironment.IsDevelopment() || _hostEnvironment.IsStaging() || _hostEnvironment.IsEnvironment(Settings.Environments.Testing)) && request.DecodedPayload?.Data.Environment != EnumAppStoreEnvironment.Sandbox)
            {
                _logger.LogError("Invalid Environment");
                return methodResult;
            }

            if (_hostEnvironment.IsProduction() && request.DecodedPayload?.Data.Environment != EnumAppStoreEnvironment.Production)
            {
                _logger.LogError("Invalid Environment");
                return methodResult;
            }

            #endregion Check environment

            var package = await _packageRepository.Queryable.FirstOrDefaultAsync(p => p.Name == request.TransactionInfo.ProductId, cancellationToken);

            if (package == null)
            {
                _logger.LogError($"Package is null, name: {request.TransactionInfo.ProductId}");
                return methodResult;
            }

            var order = await _orderRepository.Queryable.Include(p => p.OrderTransactions).FirstOrDefaultAsync(p => p.UserId.ToString() == request.TransactionInfo.AppAccountToken && p.PackageId == package.Id && p.Status == EnumOrderStatus.New, cancellationToken);

            if (order == null)
            {
                _logger.LogError($"Order is null, userId: {request.TransactionInfo.AppAccountToken}");
                return methodResult;
            }

            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                if (!string.IsNullOrEmpty(request.DecodedPayload?.NotificationType.ToString()) && !string.IsNullOrEmpty(request.DecodedPayload.Subtype.ToString()) && IsPaymentSuccess(request.DecodedPayload.NotificationType, request.DecodedPayload.Subtype))
                {
                    var numberOfShield = (package != null && package.Code.HasValue) ? (int)package.Code.Value : default;

                    var addStudentIntoClassResult = await _trainingService.AddStudentIntoClass(new AddStudentIntoClassCommandModel() { UserId = order.CreatedUserId, CourseId = order.CourseId, PackageId = order.PackageId ?? default, NumberOfShield = numberOfShield });

                    if (!addStudentIntoClassResult.IsSuccessStatusCode)
                    {
                        _logger.LogError(addStudentIntoClassResult.Error.Content);
                        return methodResult;
                    }

                    var updateNextUnitResult = await _courseService.UpdateNextUnit(order.UserId);
                    if (!updateNextUnitResult.IsSuccessStatusCode)
                    {
                        _logger.LogError(addStudentIntoClassResult.Error.Content);
                        return methodResult;
                    }
                    order.ExpireDate = DateTime.UtcNow.AddMonths(package!.MonthNumber);
                    order.Status = EnumOrderStatus.Payment;
                }

                order.OrderTransactions.Add(new OrderTransaction()
                {
                    ResponseBody = new AppStoreResponseModel()
                    {
                        NotificationType = request.DecodedPayload?.NotificationType,
                        Subtype = request.DecodedPayload?.Subtype,
                        NotificationUUID = request.DecodedPayload?.NotificationUUID,
                        NotificationVersion = request.DecodedPayload?.NotificationVersion,
                        TransactionInfo = request.TransactionInfo,
                        RenewalInfoV2 = request.RenewalInfo,
                        SignedDate = request.DecodedPayload?.SignedDate
                    },
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

        public bool IsPaymentSuccess(Application.Services.InAppPurchase.Models.EnumNotificationType notificationType, EnumNotificationSubtype notificationSubtype)
        {
            if (notificationType == Application.Services.InAppPurchase.Models.EnumNotificationType.SUBSCRIBED && (notificationSubtype == EnumNotificationSubtype.INITIAL_BUY || notificationSubtype == EnumNotificationSubtype.RESUBSCRIBE))
            {
                return true;
            }
            else if (notificationType == Application.Services.InAppPurchase.Models.EnumNotificationType.DID_CHANGE_RENEWAL_PREF && (notificationSubtype == EnumNotificationSubtype.DOWNGRADE || notificationSubtype == EnumNotificationSubtype.UPGRADE))
            {
                return true;
            }
            else if (notificationType == Application.Services.InAppPurchase.Models.EnumNotificationType.DID_RENEW && notificationSubtype == EnumNotificationSubtype.BILLING_RECOVERY)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
