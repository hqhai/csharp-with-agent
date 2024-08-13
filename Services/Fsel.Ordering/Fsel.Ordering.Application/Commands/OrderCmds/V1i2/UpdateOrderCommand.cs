// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.V1i2
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Application.Commands.VoucherCmds;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i2;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateOrderCommand : UpdateOrderCommandModel, IRequest<MethodResult<OrderModel>>
    {
    }

    public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, MethodResult<OrderModel>>
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly IVoucherRepository _voucherRepository;
        private readonly IMediator _mediator;

        public UpdateOrderCommandHandler(IMapper mapper, IOrderRepository orderRepository, IVoucherRepository voucherRepository, MediatR.IMediator mediator)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _voucherRepository = voucherRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<OrderModel>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OrderModel>();

            if (request.Order == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Order));
                return methodResult;
            }

            if (request.Package == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Package));
                return methodResult;
            }
            var discountPercent = 0;

            if (!string.IsNullOrEmpty(request.VoucherCode))
            {
                var checkVoucher = await _mediator.Send(new CheckVoucherCommand()
                {
                    Code = request.VoucherCode,
                    PackageId = request.Package.Id,
                }, cancellationToken);
                if (!checkVoucher.IsOK)
                {
                    methodResult.AddError(checkVoucher.ErrorMessages);
                    return methodResult;
                }
                var voucher = await _voucherRepository.GetByIdAsync(checkVoucher.Result?.VoucherId ?? default);
                if (voucher == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumVoucherErrorCode.VoucherNotExist));
                    return methodResult;
                }
                discountPercent = voucher.Percent;
                request.Order.VoucherId = voucher.Id;
            }
            else
            {
                request.Order.VoucherId = null;
            }

            AddDataIntoOrder(request.Order, request.Code, request.Package.Price, discountPercent, request);

            if (!request.Order.IsValid())
            {
                methodResult.AddErrorBadRequest(request.Order.ErrorMessages);
                return methodResult;
            }

            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                request.Order = _orderRepository.Update(request.Order);
                await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<OrderModel>(request.Order);
                return methodResult;
            });

            return methodResult;
        }

        private static void AddDataIntoOrder(Order order, string? code, decimal price, int discountPercent, UpdateOrderCommandModel request)
        {
            order.Status = EnumOrderStatus.New;
            order.PaymentMethod = request.PaymentMethod;
            order.FullName = request.FullName;
            order.PhoneNumber = request.PhoneNumber;
            order.Email = request.Email;
            order.Address = request.Address;
            order.Code = code;
            order.Price = price;
            order.DiscountPercent = discountPercent;
            order.DiscountPrice = (decimal)NumberHelper.ConvertDoublePercent(Convert.ToDouble(order.Price * order.DiscountPercent));
            order.TotalPrice = order.Price - order.DiscountPrice;
            order.PackageId = request.Package?.Id;
            order.IsInvoice = request.IsInvoice;
            order.CompanyName = request.CompanyName;
            order.CompanyAddress = request.CompanyAddress;
            order.CompanyTaxCode = request.CompanyTaxCode;
            order.CompanyEmail = request.CompanyEmail;
            order.ReferralCode = request.ReferralCode;
            order.EventId = request.EventId;
        }
    }
}
