// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
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

        public GetOrderByStatusQueryHandler(IOrderRepository orderRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<OrderSearchModel>>> Handle(GetOrderByStatusQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<OrderSearchModel>>();

            var orders = await _orderRepository.Queryable
                                               .Include(p => p.Package)
                                               .Where(x => x.UpdatedDate == null ? (x.CreatedDate >= request.StartDate && x.CreatedDate <= request.EndDate) : (x.UpdatedDate >= request.StartDate && x.UpdatedDate <= request.EndDate))
                                               .Where(p => request.Status == false ? p.Status == EnumOrderStatus.New : p.Status != EnumOrderStatus.New)
                                               .Select(x => new OrderSearchModel
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
                                                   PackageId = x.PackageId ?? default,
                                                   FullName = x.FullName,
                                                   IsTrial = x.IsTrial,
                                                   ExpireDate = x.ExpireDate,
                                                   MonthNumber = x.Package!.MonthNumber
                                               })
                                               .ToListAsync(cancellationToken);

            if (orders == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(orders));
                return methodResult;
            }

            methodResult.Result = _mapper.Map(orders, methodResult.Result);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
