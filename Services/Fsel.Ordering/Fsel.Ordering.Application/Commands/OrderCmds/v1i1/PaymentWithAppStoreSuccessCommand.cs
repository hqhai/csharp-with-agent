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
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class PaymentWithAppStoreSuccessCommand : PaymentWithAppStoreSuccessCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class PaymentWithAppStoreSuccessCommandHandler : IRequestHandler<PaymentWithAppStoreSuccessCommand, MethodResult<bool>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<PaymentWithAppStoreSuccessCommand> _logger;
        private readonly AppSetting _appSetting;
        private readonly IPackageRepository _packageRepository;
        private readonly ITrainingService _trainingService;
        private readonly ILmsCourseService _courseService;
        private readonly IHostEnvironment _hostEnvironment;

        public PaymentWithAppStoreSuccessCommandHandler(IOrderRepository orderRepository, ILogger<PaymentWithAppStoreSuccessCommand> logger, AppSetting appSetting, IPackageRepository packageRepository, ITrainingService trainingService, ILmsCourseService courseService, IHostEnvironment hostEnvironment)
        {
            _orderRepository = orderRepository;
            _logger = logger;
            _appSetting = appSetting;
            _packageRepository = packageRepository;
            _trainingService = trainingService;
            _courseService = courseService;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<MethodResult<bool>> Handle(PaymentWithAppStoreSuccessCommand request, CancellationToken cancellationToken)
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

            var order = await _orderRepository.Queryable.FirstOrDefaultAsync(p => p.UserId.ToString() == request.TransactionInfo.AppAccountToken && p.PackageId == package.Id && p.Status == EnumOrderStatus.New, cancellationToken);

            if (order == null)
            {
                _logger.LogError($"Order is null, userId: {request.TransactionInfo.AppAccountToken}");
                return methodResult;
            }

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
            return methodResult;
        }
    }
}
