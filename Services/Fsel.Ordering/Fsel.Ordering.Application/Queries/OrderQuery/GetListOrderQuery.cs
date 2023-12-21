// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.Runtime.Internal;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Infrastructure.Repositories;
    using Fsel.Shared.Enums;
    using MassTransit.Initializers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetListOrderQuery : IRequest<MethodResult<Guid>>
    {
        public Guid? UserId { get; set; }
    }
    public class GetListOrderQueryHandler : IRequestHandler<GetListOrderQuery, MethodResult<Guid>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetListOrderQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<MethodResult<Guid>> Handle(GetListOrderQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request); 
           var methodResult = new MethodResult<Guid>();
            var order = await _orderRepository.Queryable.FirstOrDefaultAsync(x => x.UserId == request.UserId);

            methodResult.Result =order.PackageId;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;

        }
    }
}
