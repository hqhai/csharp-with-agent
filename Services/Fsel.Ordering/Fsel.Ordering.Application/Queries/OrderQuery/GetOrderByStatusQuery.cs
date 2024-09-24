// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.CourseService;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using MassTransit.Initializers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetOrderByStatusQuery : IRequest<MethodResult<IList<OrderSearchModel>>>
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool Status { get; set; }

        public IList<Guid>? UserIds { get; set; }
    }

    public class GetOrderByStatusQueryHandler : IRequestHandler<GetOrderByStatusQuery, MethodResult<IList<OrderSearchModel>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly ILmsCourseService _lmsCourseService;

        public GetOrderByStatusQueryHandler(IOrderRepository orderRepository, IMapper mapper, ILmsCourseService lmsCourseService)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _lmsCourseService = lmsCourseService;
        }

        public async Task<MethodResult<IList<OrderSearchModel>>> Handle(GetOrderByStatusQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<OrderSearchModel>>();

            var orderInTimePeriods = await _orderRepository.Queryable
                                                           .Include(p => p.Package)
                                                           .Where(x => x.UpdatedDate == null ? (x.CreatedDate >= request.StartDate && x.CreatedDate <= request.EndDate) : (x.UpdatedDate.Value >= request.StartDate && x.UpdatedDate.Value <= request.EndDate))
                                                           .ToListAsync(cancellationToken);

            var orderInTimePeriodDistincts = orderInTimePeriods.Select(x => x.UserId).Distinct().ToList();

            var orderCombines = new List<Guid>();
            if (request.UserIds != null && request.UserIds.Any())
            {
                orderCombines = (orderInTimePeriodDistincts.Concat(request.UserIds)).Distinct().ToList();
            }

            var orders = await _orderRepository.Queryable
                                               .Include(p => p.Package)
                                               .Where(x => orderCombines.Contains(x.UserId))
                                               .ToListAsync(cancellationToken);

            var orderClients = orders.Where(x => x.ExpireDate.HasValue && x.ExpireDate.Value > DateTime.UtcNow && !x.IsTrial).Select(x => x.UserId).ToList();

            if (request.Status)
            {
                orders = orders.Where(x => orderClients.Contains(x.UserId)).ToList();
            }
            else
            {
                orders = orders.Where(x => !orderClients.Contains(x.UserId)).ToList();
            }

            methodResult.Result = _mapper.Map(orders, methodResult.Result);

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

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
