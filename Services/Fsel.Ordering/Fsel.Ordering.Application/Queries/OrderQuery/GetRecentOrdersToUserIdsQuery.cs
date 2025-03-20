// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;

    public class GetRecentOrdersToUserIdsQuery : IRequest<MethodResult<IList<OrderModel>>>
    {
        public IList<Guid>? UserIds { get; set; }
        public DateTime? StartDate { get; set; }
    }

    public class GetRecentOrdersToUserIdsQueryHandler : IRequestHandler<GetRecentOrdersToUserIdsQuery, MethodResult<IList<OrderModel>>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IPackageRepository _packageRepository;

        public GetRecentOrdersToUserIdsQueryHandler(IOrderRepository orderRepository, IMapper mapper,
            IPackageRepository packageRepository)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _packageRepository = packageRepository;
        }

        public async Task<MethodResult<IList<OrderModel>>> Handle(GetRecentOrdersToUserIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<OrderModel>>();
            if (request.UserIds == null || !request.UserIds.Any())
            {
                return methodResult;
            }

            var orders = (await (from baseQ in _orderRepository.Queryable.WhereBulkContains(request.UserIds, x => x.UserId)
                                                                         .Where(x => x.Status == EnumOrderStatus.Payment)
                                                                         .Where(x => !request.StartDate.HasValue || (x.UpdatedDate ?? x.CreatedDate) >= request.StartDate)
                                 join p in _packageRepository.Queryable on baseQ.PackageId equals p.Id
                                 where p.MonthNumber == ExtendMonth.TwentyFourMonth || p.MonthNumber == ExtendMonth.TwelveMonth
                                 select baseQ).ToListAsync(cancellationToken))
                                .GroupBy(x => x.UserId)
                                .Select(x => x.OrderByDescending(x => x.CreatedDate).FirstOrDefault())
                                .ToList();

            methodResult.Result = _mapper.Map<IList<OrderModel>>(orders);
            return methodResult;
        }
    }
}
