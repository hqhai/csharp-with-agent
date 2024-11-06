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

    public class SearchVoucherFSELQuery : SearchVoucherQueryModel, IRequest<MethodResult<PagingItemsModel<VoucherModel>>>
    {
    }

    public class SearchVoucherFSELQueryHandler : IRequestHandler<SearchVoucherFSELQuery, MethodResult<PagingItemsModel<VoucherModel>>>
    {
        private readonly IVoucherRepository _voucherRepository;

        public SearchVoucherFSELQueryHandler(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task<MethodResult<PagingItemsModel<VoucherModel>>> Handle(SearchVoucherFSELQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<VoucherModel>>();

            var voucherQuery = await _voucherRepository.Queryable.Where(p => p.Source == EnumVoucherSource.Admin).Include(x => x.Orders).ToListAsync(cancellationToken);

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var voucherModels = voucherQuery.Select(x =>
            {
                {
                    var quantityUsed = x.Orders.Where(p => p.Status == EnumOrderStatus.New || p.Status == EnumOrderStatus.Payment).Count();
                    return new VoucherModel
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Name = x.Name,
                        CreatedDate = x.CreatedDate,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        Quantity = x.Quantity,
                        Value = x.Value,
                        IsActive = x.IsActive,
                        Source = x.Source,
                        QuantityUsed = quantityUsed,
                        RemainingQuantity = x.Quantity - quantityUsed,
                        Status = quantityUsed < x.Quantity,
                        Category = x.Category,
                        ItemStatus = quantityUsed < x.Quantity ? "Đang còn" : "Đã hết",
                        Duration = currentDate > x.EndDate ? "Còn hiệu lực" : "Hết hiệu lực"
                    };
                }
            });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                voucherModels = voucherModels.Where(m => (m.Name ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim()) || (m.Code ?? string.Empty).ToLower().Trim().Contains(request.Keyword.ToLower().Trim())).ToList();
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
