// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.Payoo
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Application.Services.PayooService.Models;
    using Fsel.Ordering.Application.Services.TrainingService;
    using Fsel.Ordering.Application.Services.TrainingService.CommandModels;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class NotifyUrlCommand : NotifyUrlCommandModel, IRequest<MethodResult<NotifyUrlModel>>
    {
    }

    public class NotifyUrlCommandHandler : IRequestHandler<NotifyUrlCommand, MethodResult<NotifyUrlModel>>
    {
        private readonly AppSetting _appSetting;
        private readonly ILogger<NotifyUrlCommand> _logger;
        private readonly IOrderTransactionRepository _orderTransactionRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ITrainingService _trainingService;
        private readonly IPackageRepository _packageRepository;

        public NotifyUrlCommandHandler(AppSetting appSetting, ILogger<NotifyUrlCommand> logger, IOrderTransactionRepository orderTransactionRepository, IOrderRepository orderRepository, ITrainingService trainingService, IPackageRepository packageRepository)
        {
            _appSetting = appSetting;
            _logger = logger;
            _orderTransactionRepository = orderTransactionRepository;
            _orderRepository = orderRepository;
            _trainingService = trainingService;
            _packageRepository = packageRepository;
        }

        public async Task<MethodResult<NotifyUrlModel>> Handle(NotifyUrlCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<NotifyUrlModel>();

            methodResult.Result = new NotifyUrlModel { ReturnCode = 1, Description = string.Empty };
            bool checkFlow = true;

            var secureHash = EncodeHelper.SecureHash(_appSetting.PayooConfig?.Key + request.ResponseData + _appSetting.PayooConfig?.PayooIP);
            if (secureHash.ToLower(CultureInfo.CurrentCulture) != request.SecureHash?.ToLower(CultureInfo.CurrentCulture))
            {
                checkFlow = false;
                _logger.LogError("SecureHash wrong");
            }

            var paymentInfo = request.ResponseData.Deserialize<PaymentInfoResponseModel>();

            if (paymentInfo == null || string.IsNullOrEmpty(paymentInfo.OrderNo))
            {
                checkFlow = false;
                _logger.LogError("Payment Info Null");
            }

            Guid orderTransactionId = default;
            try
            {
                orderTransactionId = Guid.Parse(paymentInfo.OrderNo);
            }
            catch (FormatException)
            {
                _logger.LogError($"OrderNo Malformed: {paymentInfo.OrderNo}");
                return methodResult;
            }

            var orderTransaction = await _orderTransactionRepository.GetByIdAsync(orderTransactionId);
            if (orderTransaction == null)
            {
                _logger.LogError($"OrderTransaction Null: {paymentInfo.OrderNo}");
                return methodResult;
            }

            orderTransaction.Status = paymentInfo.PaymentStatus != 1 ? EnumOrderTransactionStatus.Fail : EnumOrderTransactionStatus.Success;
            orderTransaction.ResponseBody = paymentInfo;

            try
            {
                _orderTransactionRepository.Update(orderTransaction);
                await _orderTransactionRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (FormatException)
            {
                checkFlow = false;
                _logger.LogError($"Update OrderTransaction Error: {orderTransaction.Id}");
            }

            var order = await _orderRepository.GetByIdAsync(orderTransaction.OrderId ?? default);
            if (order == null)
            {
                _logger.LogError($"Order Null: {paymentInfo.OrderNo}");
                return methodResult;
            }

            var addStudentIntoClassResult = await _trainingService.AddStudentIntoClass(new AddStudentIntoClassCommandModel() { UserId = order.CreatedUserId, CourseId = order.CourseId, PackageId = order.PackageId ?? default });
            if (!addStudentIntoClassResult.IsSuccessStatusCode)
            {
                checkFlow = false;
                _logger.LogError($"Add Student into class error: {addStudentIntoClassResult.StatusCode}, OrderId: {order.Id}");
            }

            var package = await _packageRepository.GetByIdAsync(order.PackageId ?? default);
            if (package == null)
            {
                checkFlow = false;
                _logger.LogError($"Package not exist: OrderId: {order.Id}");
            }

            order.Status = checkFlow ? EnumOrderStatus.Payment : EnumOrderStatus.Fail;
            order.ExpireDate = checkFlow ? DateTime.UtcNow.AddMonths(package!.MonthNumber) : null;
            order.ClassId = checkFlow ? addStudentIntoClassResult.Content?.Result ?? default : default;

            try
            {
                _orderRepository.Update(order);
                await _orderRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (FormatException)
            {
                _logger.LogError($"Update Order Error: {orderTransaction.Id}");
                return methodResult;
            }

            methodResult.Result = new NotifyUrlModel { ReturnCode = 0, Description = string.Empty };

            return methodResult;
        }
    }
}
