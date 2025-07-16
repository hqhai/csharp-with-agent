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

            List<Order> orders = new List<Order>();

            if (request.UserIds == null && request.StartDate.HasValue && request.EndDate.HasValue)
            {
                orders = await _orderRepository.Queryable
                                               .Include(p => p.Package)
                                               .Where(x => x.UpdatedDate == null ? (x.CreatedDate >= request.StartDate && x.CreatedDate <= request.EndDate) : (x.UpdatedDate.Value >= request.StartDate && x.UpdatedDate.Value <= request.EndDate))
                                               .ToListAsync(cancellationToken);

                var checkLeadOrClient = await CheckLeadOrClient(orders, request, cancellationToken);
                methodResult.Result = _mapper.Map(checkLeadOrClient.Result, methodResult.Result);
            }

            else if (request.UserIds != null && request.UserIds.Any())
            {
                orders = await _orderRepository.Queryable
                                               .Include(p => p.Package)
                                               .Where(x => request.UserIds.Contains(x.UserId))
                                               .ToListAsync(cancellationToken);

                var checkLeadOrClient = await CheckLeadOrClient(orders, request, cancellationToken);
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

        private async Task<MethodResult<IList<Order>>> CheckLeadOrClient(IList<Order> orders, GetOrderByStatusIntegrationQuery request, CancellationToken cancellationToken)
        {
            MethodResult<IList<Order>> methodResult = new MethodResult<IList<Order>>();

            var orderClients = orders.Where(x => x.ExpireDate.HasValue && x.ExpireDate.Value > DateTime.UtcNow && !x.IsTrial).Select(x => x.UserId).ToList();

            if (request.Status)
            {
                orders = orders.Where(x => orderClients.Contains(x.UserId) && x.RevenueType == EnumPaymentRevenueType.Revenue).ToList();
            }
            else
            {
                var userExpire = await _orderRepository.Queryable
                                                       .Include(p => p.Package)
                                                       .Where(x => (x.ExpireDate >= request.StartDate) && (x.ExpireDate <= request.EndDate))
                                                       .ToListAsync(cancellationToken);

                orders = orders.Where(x => !orderClients.Contains(x.UserId)).ToList();

                if (userExpire == null || !userExpire.Any())
                {
                    return methodResult;
                }

                // lấy nhưng order hết hạn đưa vào leads
                foreach (var item in userExpire)
                {
                    if (!orders.Any(x => x.Id == item.Id))
                    {
                        orders.Add(item);
                    }
                }
            }

            methodResult.Result = orders;
            return methodResult;
        }
    }
}
