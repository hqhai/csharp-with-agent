// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Ordering.Application.Services.CourseService;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Ordering.Domain.Models.QueryModels.Oders;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchOrderQuery : SearchOderQueryModel, IRequest<MethodResult<PagingItemsModel<SearchOrderModel>>>
    {
    }

    public class SearchOrderQueryHandler : IRequestHandler<SearchOrderQuery, MethodResult<PagingItemsModel<SearchOrderModel>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILmsCourseService _lmsCourseService;

        public SearchOrderQueryHandler(IOrderRepository orderRepository, ILmsCourseService lmsCourseService)
        {
            _orderRepository = orderRepository;
            _lmsCourseService = lmsCourseService;
        }

        public async Task<MethodResult<PagingItemsModel<SearchOrderModel>>> Handle(SearchOrderQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<SearchOrderModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }
            var orders = _orderRepository.Queryable.Include(p => p.Package).Select(x => new SearchOrderModel
            {
                Id = x.Id,
                UserId = x.UserId,
                Code = x.Code,
                CourseId = x.CourseId,
                CreatedDate = x.CreatedDate,
                CreatedFullName = x.CreatedFullName,
                PackageName = x.Package!.Code.ToString(),
                Status = x.Status,
                PaymentMethod = x.PaymentMethod,
                PackageId = x.PackageId,
            });
            if (request.Status.HasValue)
            {
                orders = orders.Where(p => request.Status == false ? p.Status == EnumOrderStatus.New : p.Status != EnumOrderStatus.New);
            }
            int totalItem = await orders.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await orders
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            var courses = await _lmsCourseService.GetCoursesByIdsAsync(lists.Select(p => p.CourseId).ToList()!);
            if (courses.IsSuccessStatusCode)
            {
                foreach (var item in lists)
                {
                    item.CourseName = courses.Content?.Result?.FirstOrDefault(x => item.CourseId == x.Id)?.CourseLevel;
                }
            }

            methodResult.Result = new PagingItemsModel<SearchOrderModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
