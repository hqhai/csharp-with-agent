// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetOrderQuery : IRequest<MethodResult<OrderModel>>
    {
        public Guid PackageId { get; set; }
    }

    public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, MethodResult<OrderModel>>
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IMapper _mapper;

        public GetOrderQueryHandler(IPackageRepository packageRepository, IMapper mapper)
        {
            _packageRepository = packageRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<OrderModel>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<OrderModel> methodResult = new MethodResult<OrderModel>(); 

            OrderModel order = new OrderModel();
            var package = await _packageRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.PackageId, cancellationToken);
            if (package == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOrderErrorCode.PackageNotExist));
                return methodResult;
            }
            order.Price = package.Price;
            order.Code = NumberHelper.GenerateOrderCode(8);
            order.Package = _mapper.Map<PackageModel>(package);
            methodResult.Result = order;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
