// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.Payoo
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Application.Commands.OrderCmds;
    using Fsel.Ordering.Application.Services.CourseService;
    using Fsel.Ordering.Application.Services.CourseService.Model;
    using Fsel.Ordering.Application.Services.PayooService.Models;
    using Fsel.Ordering.Application.Services.TrainingService;
    using Fsel.Ordering.Application.Services.TrainingService.CommandModels;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
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
        private readonly ILmsCourseService _courseService;
        private readonly IMediator _mediator;

        public NotifyUrlCommandHandler(AppSetting appSetting, ILogger<NotifyUrlCommand> logger, IOrderTransactionRepository orderTransactionRepository, IOrderRepository orderRepository, ITrainingService trainingService, IPackageRepository packageRepository, ILmsCourseService courseService, IMediator mediator)
        {
            _appSetting = appSetting;
            _logger = logger;
            _orderTransactionRepository = orderTransactionRepository;
            _orderRepository = orderRepository;
            _trainingService = trainingService;
            _packageRepository = packageRepository;
            _courseService = courseService;
            _mediator = mediator;
        }

        public async Task<MethodResult<NotifyUrlModel>> Handle(NotifyUrlCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<NotifyUrlModel>();

            var payooResponse = request.Serialize();

            _logger.LogError($"payoo notify: {payooResponse}");

            methodResult.Result = new NotifyUrlModel { ReturnCode = 1, Description = string.Empty };

            var secureHash = EncodeHelper.SecureHash(_appSetting.PayooConfig?.Key + request.ResponseData + _appSetting.PayooConfig?.PayooIP);
            if (secureHash.ToLower(CultureInfo.CurrentCulture) != request.SecureHash?.ToLower(CultureInfo.CurrentCulture))
            {
                _logger.LogError($"SecureHash wrong: {request.SecureHash}");
                return methodResult;
            }

            var paymentInfo = request.ResponseData.Deserialize<PaymentInfoResponseModel>();
            var paymentInfoStr = paymentInfo.Serialize();
            _logger.LogError($"Payment Info: {paymentInfoStr}");
            if (paymentInfo == null || string.IsNullOrEmpty(paymentInfo.OrderNo))
            {
                _logger.LogError($"Payment Info Null: {paymentInfoStr}");
                return methodResult;
            }

            var order = await _orderRepository.Queryable.FirstOrDefaultAsync(p => p.Code.ToLower() == paymentInfo.OrderNo.ToLower(), cancellationToken);
            if (order == null)
            {
                _logger.LogError($"Order Null: {paymentInfo.OrderNo}");
                return methodResult;
            }

            await _mediator.Send(new ChangeStatusOrderCommand()
            {
                OrderId = order.Id,
                OrderStatus = paymentInfo.PaymentStatus == 1 ? EnumOrderStatus.Payment : EnumOrderStatus.Fail,
                Type = EnumOrderTransactionType.Payoo,
                Receipt = paymentInfo.Serialize(),
                RevenueType = EnumPaymentRevenueType.Revenue
            }, cancellationToken);

            methodResult.Result = new NotifyUrlModel { ReturnCode = 0, Description = string.Empty };

            return methodResult;
        }
    }
}
