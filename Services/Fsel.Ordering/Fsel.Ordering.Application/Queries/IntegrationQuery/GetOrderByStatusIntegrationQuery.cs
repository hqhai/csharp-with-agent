// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.IntegrationQuery
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.CourseService;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetOrderByStatusIntegrationQuery : IRequest<MethodResult<IList<OrderSearchModel>>>
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool Status { get; set; }

        public IList<Guid>? UserIds { get; set; }
    }

    public class GetOrderByStatusIntegrationQueryHandler : IRequestHandler<GetOrderByStatusIntegrationQuery, MethodResult<IList<OrderSearchModel>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly ILmsCourseService _lmsCourseService;

        public GetOrderByStatusIntegrationQueryHandler(IOrderRepository orderRepository, IMapper mapper, ILmsCourseService lmsCourseService)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _lmsCourseService = lmsCourseService;
        }

        public async Task<MethodResult<IList<OrderSearchModel>>> Handle(GetOrderByStatusIntegrationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<OrderSearchModel>>();

            IQueryable<Order> orders;

            if (request.UserIds == null && request.StartDate.HasValue && request.EndDate.HasValue)
            {
                var orderInRangeQuerys = _orderRepository.Queryable
                                                         .Include(p => p.Package)
                                                         .Include(p => p.Voucher)
                                                         .Where(x => x.UpdatedDate == null ? (x.CreatedDate >= request.StartDate && x.CreatedDate <= request.EndDate) : (x.UpdatedDate.Value >= request.StartDate && x.UpdatedDate.Value <= request.EndDate));

                if (!request.Status)
                {
                    orderInRangeQuerys.Where(x => (x.ExpireDate >= request.StartDate) && (x.ExpireDate <= request.EndDate));
                }

                var orderInRanges = await orderInRangeQuerys.AsNoTracking().ToListAsync(cancellationToken);

                methodResult.Result = _mapper.Map(orderInRanges, methodResult.Result);
            }

            else if (request.UserIds != null && request.UserIds.Any())
            {
                orders = _orderRepository.Queryable
                                         .Include(p => p.Package)
                                         .Include(p => p.Voucher)
                                         .WhereBulkContains(request.UserIds, x => x.UserId);

                var checkLeadOrClient = await CheckLeadOrClient(orders, request.Status, cancellationToken);
                methodResult.Result = _mapper.Map(checkLeadOrClient.Result, methodResult.Result);

                if (methodResult.Result != null && methodResult.Result.Any())
                {
                    var userIds = methodResult.Result.Select(x => x.UserId).Distinct().ToList();
                    var courseResults = await _lmsCourseService.GetCourseResultsByUserIds(userIds);
                    if (courseResults.IsSuccessStatusCode)
                    {
                        foreach (var item in methodResult.Result)
                        {
                            item.CourseName = courseResults.Content?.Result?.FirstOrDefault(x => x.CourseId == item.CourseId)?.CourseLevel;
                            item.StatusCourseResult = courseResults.Content?.Result?.FirstOrDefault(x => x.CourseId == item.CourseId && x.UserId == item.UserId)?.Status;
                        }
                    }
                }
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static async Task<MethodResult<IList<Order>>> CheckLeadOrClient(IQueryable<Order> orderQueryable, bool status, CancellationToken cancellationToken)
        {
            MethodResult<IList<Order>> methodResult = new MethodResult<IList<Order>>();
            IList<Order> orders = new List<Order>();

            if (status)
            {
                orders = await orderQueryable.Where(x => x.ExpireDate.HasValue && x.ExpireDate.Value > DateTime.UtcNow && !x.IsTrial && x.RevenueType == EnumPaymentRevenueType.Revenue).AsNoTracking().ToListAsync(cancellationToken);
            }
            else
            {
                orders = await orderQueryable.Where(x => !(x.ExpireDate.HasValue && x.ExpireDate.Value > DateTime.UtcNow && !x.IsTrial && x.RevenueType == EnumPaymentRevenueType.Revenue)).AsNoTracking().ToListAsync(cancellationToken);
            }

            methodResult.Result = orders;
            return methodResult;
        }
    }
}
