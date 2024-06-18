// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Application.Services.CourseService;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MassTransit.Initializers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetOrderByStatusQuery : IRequest<MethodResult<IList<OrderSearchModel>>>
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool Status { get; set; }
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

            var orders = await _orderRepository.Queryable
                                               .Include(p => p.Package)
                                               .Where(x => x.UpdatedDate == null ? (x.CreatedDate >= request.StartDate && x.CreatedDate <= request.EndDate) : (x.UpdatedDate.Value >= request.StartDate && x.UpdatedDate.Value <= request.EndDate))
                                               .Where(p => request.Status == false ? p.Status == EnumOrderStatus.New || p.IsTrial : p.Status == EnumOrderStatus.Payment && !p.IsDeleted)
                                               .Select(x => new OrderSearchModel
                                               {
                                                   Id = x.Id,
                                                   UserId = x.UserId,
                                                   Code = x.Code,
                                                   CourseId = x.CourseId,
                                                   CreatedDate = x.CreatedDate,
                                                   CreatedFullName = x.CreatedFullName,
                                                   PackageName = x.Package != null ? x.Package.Code.ToString() : string.Empty,
                                                   Status = x.Status,
                                                   PaymentMethod = x.PaymentMethod,
                                                   PackageId = x.PackageId ?? default,
                                                   FullName = x.FullName,
                                                   IsTrial = x.IsTrial,
                                                   ExpireDate = x.ExpireDate,
                                                   MonthNumber = x.Package != null ? x.Package.MonthNumber : 0,
                                                   UpdatedDate = x.UpdatedDate,
                                                   TotalPrice = x.TotalPrice,
                                                   DiscountPrice = x.DiscountPrice
                                               })
                                               .ToListAsync(cancellationToken);

            if (orders == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(orders));
                return methodResult;
            }

            var userIds = orders.Select(x => x.UserId).Distinct().ToList();
            var courseResults = await _lmsCourseService.GetCourseResultsByUserIds(userIds);
            if (courseResults.IsSuccessStatusCode)
            {
                foreach (var item in orders)
                {
                    item.CourseName = courseResults.Content?.Result?.FirstOrDefault(x => x.CourseId == item.CourseId)?.CourseLevel;
                    item.StatusCourseResult = courseResults.Content?.Result?.FirstOrDefault(x => x.CourseId == item.CourseId && x.UserId == item.UserId)?.Status;
                }
            }

            methodResult.Result = _mapper.Map(orders, methodResult.Result);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
