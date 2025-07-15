// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.V1i2
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Application.Queries.OrderQuery;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i2;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateOrderCommand : UpdateOrderCommandModel, IRequest<MethodResult<OrderModel>>
    {
    }

    public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, MethodResult<OrderModel>>
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly IMediator _mediator;

        public UpdateOrderCommandHandler(IMapper mapper, IOrderRepository orderRepository, IMediator mediator)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;

            _mediator = mediator;
        }

        public async Task<MethodResult<OrderModel>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OrderModel>();

            if (request.Order == null || request.Request == null || string.IsNullOrEmpty(request.StudentCode))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required), nameof(request.Order));
                return methodResult;
            }

            _mapper.Map(request.Request, request.Order);

            //var codeSend = await _mediator.Send(new GenerateRandomOrderQuery() { StudentCode = request.StudentCode }, cancellationToken).ConfigureAwait(false);
            //string code = codeSend.Result ?? string.Empty;

            //if (string.IsNullOrEmpty(code) || await _orderRepository.Queryable.AnyAsync(x => x.Code == code, cancellationToken))
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(code));
            //    return methodResult;
            //}

            request.Order.Code = request.Order.Code;
            request.Order.DiscountPrice = request.DiscountPrice;
            request.Order.DiscountPercent = request.DiscountPercent;
            request.Order.TotalPrice = request.TotalPrice;
            request.Order.Price = request.Price;
            request.Order.VoucherId = request.VoucherId;

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

            if (request.Order.TotalPrice == 0)
            {
                var changeStatusOrdersResult = await _mediator.Send(new ChangeStatusOrderCommand()
                {
                    OrderIds = new[] { request.Order.Id },
                    RevenueType = EnumPaymentRevenueType.NotRevenue,
                    Status = EnumOrderStatus.Payment
                }, cancellationToken);

                if (!changeStatusOrdersResult.IsOK)
                {
                    methodResult.AddError(changeStatusOrdersResult.ErrorMessages);
                    return methodResult;
                }
            }

            return methodResult;
        }
    }
}
