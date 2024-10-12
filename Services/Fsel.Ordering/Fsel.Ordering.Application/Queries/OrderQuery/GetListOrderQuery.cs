// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetListOrderQuery : IRequest<MethodResult<IList<OrderModel>>>
    {
        public Guid? UserId { get; set; }
    }

    public class GetListOrderQueryHandler : IRequestHandler<GetListOrderQuery, MethodResult<IList<OrderModel>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public GetListOrderQueryHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<OrderModel>>> Handle(GetListOrderQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<OrderModel>>();
            var orders = _orderRepository.Queryable.Where(x => x.UserId == request.UserId);

            methodResult.Result = _mapper.Map<IList<OrderModel>>(orders);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
