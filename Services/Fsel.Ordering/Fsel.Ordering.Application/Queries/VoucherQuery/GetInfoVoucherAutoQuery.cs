// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.VoucherQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetInfoVoucherAutoQuery : IRequest<MethodResult<VoucherModel>>
    {
        public string? CodePrefix { get; set; }
    }

    public class GetInfoVoucherAutoQueryHandler : IRequestHandler<GetInfoVoucherAutoQuery, MethodResult<VoucherModel>>
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public GetInfoVoucherAutoQueryHandler(IVoucherRepository voucherRepository, IOrderRepository orderRepository, IMapper mapper)
        {
            _voucherRepository = voucherRepository;
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<VoucherModel>> Handle(GetInfoVoucherAutoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VoucherModel>();

            var vouchers = await _voucherRepository.Queryable.Include(p => p.Orders.Where(x => x.Status == Shared.Enums.EnumOrderStatus.New || x.Status == Shared.Enums.EnumOrderStatus.Payment)).Where(p => !string.IsNullOrEmpty(p.CodePrefix) && p.CodePrefix.ToLower() == request.CodePrefix.ToLower()).ToListAsync(cancellationToken);
            if (vouchers == null || vouchers.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(vouchers));
                return methodResult;
            }
            var voucher = vouchers.First();
            var quantityUsed = vouchers.Where(p => p.Orders.Any()).Count();
            var voucherModel = _mapper.Map<VoucherModel>(voucher);
            voucherModel.QuantityUsed = quantityUsed;
            voucherModel.RemainingQuantity = voucherModel.Quantity - voucherModel.QuantityUsed;
            methodResult.Result = voucherModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
