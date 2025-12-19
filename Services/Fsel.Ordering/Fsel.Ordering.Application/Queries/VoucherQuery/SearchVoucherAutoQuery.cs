// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.VoucherQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Domain.Models.QueryModels.Vouchers;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchVoucherAutoQuery : SearchVoucherQueryModel, IRequest<MethodResult<PagingItemsModel<VoucherModel>>>
    {
    }

    public class SearchVoucherAutoQueryHandler : IRequestHandler<SearchVoucherAutoQuery, MethodResult<PagingItemsModel<VoucherModel>>>
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IOrderRepository _orderRepository;

        public SearchVoucherAutoQueryHandler(IVoucherRepository voucherRepository, IOrderRepository orderRepository)
        {
            _voucherRepository = voucherRepository;
            _orderRepository = orderRepository;
        }

        public async Task<MethodResult<PagingItemsModel<VoucherModel>>> Handle(SearchVoucherAutoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<VoucherModel>>();

            var voucherQuery = await _voucherRepository.Queryable.Where(p => p.Source == EnumVoucherSource.Auto).GroupBy(p => p.CodePrefix).ToListAsync(cancellationToken);

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var voucherIds = voucherQuery.SelectMany(p => p.Select(x => x.Id)).ToList();

            var orders = await _orderRepository.Queryable.WhereBulkContains(voucherIds, p => p.VoucherId).Where(p => p.Status == EnumOrderStatus.New || p.Status == EnumOrderStatus.Payment).ToListAsync(cancellationToken);

            var voucherModels = voucherQuery.Select(x =>
            {
                {
                    var ids = x.Select(x => x.Id).ToList();
                    var quantityUsed = orders.Where(p => p.VoucherId.HasValue && ids.Contains(p.VoucherId.Value)).Count();
                    return new VoucherModel
                    {
                        Name = x.First().Name,
                        CodePrefix = x.First().CodePrefix,
                        CreatedDate = x.First().CreatedDate,
                        Quantity = x.Count(),
                        Value = x.First().Value,
                        IsActive = x.First().IsActive,
                        Source = x.First().Source,
                        QuantityUsed = quantityUsed,
                        RemainingQuantity = x.Count() - quantityUsed,
                        Status = quantityUsed < x.Count(),
                        Category = x.First().Category,
                        ItemStatus = quantityUsed < x.Count() ? "Đang còn" : "Đã hết",
                        Duration = currentDate < x.First().EndDate ? "Còn hiệu lực" : "Hết hiệu lực"
                    };
                }
            });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                voucherModels = voucherModels.Where(m => (!string.IsNullOrEmpty(m.Name) && m.Name.Contains(request.Keyword, StringComparison.InvariantCultureIgnoreCase)) || (!string.IsNullOrEmpty(m.Code) && m.Code.Contains(request.Keyword, StringComparison.InvariantCultureIgnoreCase)));
            }

            if (request.Status.HasValue)
            {
                voucherModels = voucherModels.Where(m => m.Status == request.Status).ToList();
            }

            if (request.CreatedDate.HasValue)
            {
                voucherModels = voucherModels.Where(m => m.CreatedDate.HasValue && m.CreatedDate.Value.Date == request.CreatedDate.Value.Date).ToList();
            }

            voucherModels = voucherModels.OrderByDescending(x => x.CreatedDate);

            int totalItem = voucherModels.Count();
            var lists = voucherModels.ApplyPaging(request).ToList();

            methodResult.Result = new PagingItemsModel<VoucherModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
