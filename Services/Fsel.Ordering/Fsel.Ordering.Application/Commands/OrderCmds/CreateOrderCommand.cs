// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateOrderCommand : CreateOrderCommandModel, IRequest<MethodResult<OrderModel>>
    {
    }

    public class CreateClassForumCommandHandler : IRequestHandler<CreateOrderCommand, MethodResult<OrderModel>>
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly AuthContext _authContext;
        private readonly IPackageRepository _packageRepository;

        public CreateClassForumCommandHandler(IMapper mapper,
            IOrderRepository orderRepository,
            AuthContext authContext,
            IPackageRepository packageRepository)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _authContext = authContext;
            _packageRepository = packageRepository;
        }

        public async Task<MethodResult<OrderModel>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<OrderModel> methodResult = new MethodResult<OrderModel>();

            var package = await _packageRepository.GetByIdAsync(request.PackageId);
            if (package == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOrderErrorCode.PackageNotExist));
                return methodResult;
            }
            Order order = _mapper.Map<Order>(request);
            order.UserId = _authContext.CurrentUserId;
            if (!order.IsValid())
            {
                methodResult.AddErrorBadRequest(order.ErrorMessages);
                return methodResult;
            }
            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                order = _orderRepository.Add(order);
                await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<OrderModel>(order);
                return methodResult;
            });
            return methodResult;
        }
    }
}
