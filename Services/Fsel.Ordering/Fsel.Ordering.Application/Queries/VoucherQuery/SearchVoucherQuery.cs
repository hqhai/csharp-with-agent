// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.VoucherQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Domain.Models.QueryModels.Vouchers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchVoucherQuery : SearchVoucherQueryModel, IRequest<MethodResult<PagingItemsModel<VoucherModel>>>
    {
    }

    public class SearchVoucherQueryHandler : IRequestHandler<SearchVoucherQuery, MethodResult<PagingItemsModel<VoucherModel>>>
    {
        private readonly IVoucherRepository _voucherRepository;

        public SearchVoucherQueryHandler(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task<MethodResult<PagingItemsModel<VoucherModel>>> Handle(SearchVoucherQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<VoucherModel>> methodResult = new MethodResult<PagingItemsModel<VoucherModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var voucherQuery = _voucherRepository.Queryable
                            .Select(x => new VoucherModel
                            {
                                Id = x.Id,
                                Name = x.Name,
                                StartDate = x.StartDate,
                                EndDate = x.EndDate,
                                IsGlobal = x.IsGlobal,
                                CustomerType = x.CustomerType,
                                CreatedDate = x.CreatedDate,
                            });
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                voucherQuery = voucherQuery.Where(m => m.Id.ToString() == request.Keyword || (m.Name ?? string.Empty).Contains(request.Keyword));
            }
            int totalItem = await voucherQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await voucherQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<VoucherModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
