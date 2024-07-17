// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery.V1i2
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels.V1i2;
    using Fsel.Ordering.Domain.Models.QueryModels.Oders.V1i2;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchOrderQuery : SearchOrderQueryModel, IRequest<MethodResult<PagingItemsModel<SearchOrderModel>>>
    {
    }

    public class SearchOrderQueryHandler : IRequestHandler<SearchOrderQuery, MethodResult<PagingItemsModel<SearchOrderModel>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly AuthContext _authContext;

        public SearchOrderQueryHandler(IOrderRepository orderRepository, AuthContext authContext)
        {
            _orderRepository = orderRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<PagingItemsModel<SearchOrderModel>>> Handle(SearchOrderQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<SearchOrderModel>>();

            var query = _orderRepository.Queryable.Include(p => p.Package).Select(x => new SearchOrderModel
            {
                Id = x.Id,
                Code = x.Code,
                FullName = x.FullName,
                Email = x.Email,
                UserId = x.UserId,
                CreatedDate = x.CreatedDate,
                UpdatedDate = x.UpdatedDate,
                CreatedFullName = x.CreatedFullName,
                Status = x.Status,
                PaymentMethod = x.PaymentMethod,
                PackageId = x.PackageId,
                MonthNumber = x.Package == null ? null : x.Package.MonthNumber,
                RevenueType = x.RevenueType,
                TotalPrice = x.TotalPrice,
            });

            if (request.Status.HasValue)
            {
                query = query.Where(p => p.Status == request.Status);
            }

            if (_authContext.Roles?.FirstOrDefault() == EnumRole.Student.ToString())
            {
                query = query.Where(p => p.UserId == _authContext.CurrentUserId);
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (request.Keyword.IsValidEmail())
                {
                    query = query.Where(p => !string.IsNullOrEmpty(p.Email) && p.Email.Contains(request.Keyword));
                }
                else
                {
                    query = query.Where(p => (!string.IsNullOrEmpty(p.Code) && p.Code.Contains(request.Keyword)) || (!string.IsNullOrEmpty(p.FullName) && p.FullName.Contains(request.Keyword)));
                }
            }

            if (request.PackageIds != null && request.PackageIds.Count > 0)
            {
                query = query.Where(p => p.PackageId.HasValue && request.PackageIds.Contains(p.PackageId.Value));
            }

            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                query = query.Where(p => p.CreatedDate.HasValue && request.StartDate.Value.Date <= p.CreatedDate.Value.Date && request.EndDate.Value.Date >= p.CreatedDate.Value.Date);
            }
            else if (request.StartDate.HasValue)
            {
                query = query.Where(p => p.CreatedDate.HasValue && request.StartDate.Value.Date <= p.CreatedDate.Value.Date);
            }
            else if (request.EndDate.HasValue)
            {
                query = query.Where(p => p.CreatedDate.HasValue && request.EndDate.Value.Date >= p.CreatedDate.Value.Date);
            }

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<SearchOrderModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
