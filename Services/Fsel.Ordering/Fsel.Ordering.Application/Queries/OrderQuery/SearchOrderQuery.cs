// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Application.Services.CourseService;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Domain.Models.QueryModels.Oders;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchOrderQuery : SearchOrderQueryModel, IRequest<MethodResult<PagingItemsModel<OrderSearchModel>>>
    {
    }

    public class SearchOrderQueryHandler : IRequestHandler<SearchOrderQuery, MethodResult<PagingItemsModel<OrderSearchModel>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILmsCourseService _lmsCourseService;

        public SearchOrderQueryHandler(IOrderRepository orderRepository, ILmsCourseService lmsCourseService)
        {
            _orderRepository = orderRepository;
            _lmsCourseService = lmsCourseService;
        }

        public async Task<MethodResult<PagingItemsModel<OrderSearchModel>>> Handle(SearchOrderQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<OrderSearchModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var query = _orderRepository.Queryable.Include(p => p.Package).Select(x => new OrderSearchModel
            {
                Id = x.Id,
                UserId = x.UserId,
                Code = x.Code,
                CreatedDate = x.CreatedDate,
                CreatedFullName = x.CreatedFullName,
                PackageName = x.Package!.Code.ToString(),
                Status = x.Status,
                PaymentMethod = x.PaymentMethod,
                PackageId = x.PackageId ?? default,
                FullName = x.FullName,
                IsTrial = x.IsTrial,
                ExpireDate = x.ExpireDate,
                Email = x.Email,
            });

            if (request.Status.HasValue)
            {
                query = query.Where(p => request.Status == false ? p.Status == EnumOrderStatus.New : p.Status != EnumOrderStatus.New);
                if (!request.Status.Value)
                {
                    query = query.Where(p => p.PaymentMethod == EnumPaymentMethodStatus.BankTransfer || p.PaymentMethod == EnumPaymentMethodStatus.Card);
                }
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (request.Keyword.IsValidEmail())
                {
                    query = query.Where(x => x.Email != null).Where(m => (m.Email ?? string.Empty).Trim().ToLower().Contains(request.Keyword.Trim().ToLower()));
                }
                else
                {
                    query = query.Where(x => x.Code != null).Where(m => (m.Code ?? string.Empty).Trim().ToLower().Contains(request.Keyword.Trim().ToLower()));
                }
            }
            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            //var courses = await _lmsCourseService.GetCoursesByIdsAsync(lists.Select(p => p.CourseId).ToList()!);
            //if (courses.IsSuccessStatusCode)
            //{
            //    foreach (var item in lists)
            //    {
            //        item.CourseName = courses.Content?.Result?.FirstOrDefault(x => item.CourseId == x.Id)?.CourseLevel;
            //    }
            //}

            methodResult.Result = new PagingItemsModel<OrderSearchModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
