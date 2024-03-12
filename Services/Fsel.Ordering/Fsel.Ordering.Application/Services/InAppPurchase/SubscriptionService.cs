// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.InAppPurchase
{
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.CourseService;
    using Fsel.Ordering.Application.Services.InAppPurchase.Models;
    using Fsel.Ordering.Application.Services.TrainingService;
    using Fsel.Ordering.Application.Services.TrainingService.CommandModels;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Microsoft.Extensions.Logging;

    public class SubscriptionService : ISubscriptionService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<SubscriptionService> _logger;
        private readonly AppSetting _appSetting;
        private readonly IPackageRepository _packageRepository;
        private readonly ITrainingService _trainingService;
        private readonly ILmsCourseService _courseService;

        public SubscriptionService(IOrderRepository orderRepository, ILogger<SubscriptionService> logger, AppSetting appSetting, IPackageRepository packageRepository, ITrainingService trainingService, ILmsCourseService courseService)
        {
            _orderRepository = orderRepository;
            _logger = logger;
            _appSetting = appSetting;
            _packageRepository = packageRepository;
            _trainingService = trainingService;
            _courseService = courseService;
        }

        public async Task<bool> Update(NotificationV2 decodedPayload, RenewalInfoV2? renewalInfo, TransactionInfoV2? transactionInfo)
        {
            if (string.IsNullOrEmpty(decodedPayload?.Data?.AppAppleId) || string.IsNullOrEmpty(decodedPayload?.Data?.BundleId))
            {
                _logger.LogError("AppAppleId is null hoặc BundleId is null");
                return false;
            }
            if (decodedPayload?.Data?.AppAppleId != _appSetting.PurchaseSettings?.AppStore?.AppId || decodedPayload?.Data?.BundleId != _appSetting.PurchaseSettings?.AppStore?.BundleId)
            {
                _logger.LogError("AppAppleId is wrong hoặc BundleId is wrong");
                return false;
            }
            if (string.IsNullOrEmpty(transactionInfo?.ProductId))
            {
                _logger.LogError("ProductId is null");
                return false;
            }
            if (string.IsNullOrEmpty(transactionInfo?.AppAccountToken))
            {
                _logger.LogError("UserId is null");
                return false;
            }

            var package = _packageRepository.Queryable.FirstOrDefault(p => p.Name == transactionInfo.ProductId);
            if (package == null)
            {
                _logger.LogError("Package is null");
                return false;
            }

            var order = _orderRepository.Queryable.FirstOrDefault(p => p.UserId.ToString() == transactionInfo.AppAccountToken && p.PackageId == package.Id && p.Status == EnumOrderStatus.New);

            if (order == null)
            {
                _logger.LogError("Order is null");
                return false;
            }
            var methodResult = new MethodResult<bool>();
            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                var numberOfShield = (package != null && package.Code.HasValue) ? (int)package.Code.Value : default;

                var addStudentIntoClassResult = await _trainingService.AddStudentIntoClass(new AddStudentIntoClassCommandModel() { UserId = order.CreatedUserId, CourseId = order.CourseId, PackageId = order.PackageId ?? default, NumberOfShield = numberOfShield });
                if (!addStudentIntoClassResult.IsSuccessStatusCode)
                {
                    _logger.LogError(addStudentIntoClassResult.Error.Content);
                    methodResult.Result = false;
                    return methodResult;
                }

                var updateNextUnitResult = await _courseService.UpdateNextUnit();
                if (!updateNextUnitResult.IsSuccessStatusCode)
                {
                    _logger.LogError(addStudentIntoClassResult.Error.Content);
                    methodResult.Result = false;
                    return methodResult;
                }

                order.Status = EnumOrderStatus.Payment;
                order = _orderRepository.Update(order);

                await _orderRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);

                methodResult.Result = true;
                return methodResult;
            });
            return true;
        }
    }
}
