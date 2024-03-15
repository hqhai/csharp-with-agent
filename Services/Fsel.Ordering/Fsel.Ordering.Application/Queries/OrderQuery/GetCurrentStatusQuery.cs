// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetCurrentStatusQuery : IRequest<MethodResult<EnumTrialRegistrationStatus?>>
    {
        public Guid UserId { get; set; }
    }

    public class GetCurrentStatusQueryHandler : IRequestHandler<GetCurrentStatusQuery, MethodResult<EnumTrialRegistrationStatus?>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetCurrentStatusQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<MethodResult<EnumTrialRegistrationStatus?>> Handle(GetCurrentStatusQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<EnumTrialRegistrationStatus?> methodResult = new MethodResult<EnumTrialRegistrationStatus?>();
            var currentStatus = EnumTrialRegistrationStatus.New;

            var query = _orderRepository.Queryable.OrderByDescending(x => x.CreatedDate).FirstOrDefault(x => (x.UserId == request.UserId));

            if (query == null)
            {
                methodResult.Result = currentStatus;
                return methodResult;
            }

            DateTime currentDate = DateTime.UtcNow;
            var currentExpireDate = query.ExpireDate;
            var checkTrial = query.IsTrial;

            if (currentDate.Date > currentExpireDate?.Date && currentDate.Month >= currentExpireDate?.Month && currentDate.Year >= currentExpireDate?.Year)
            {
                currentStatus = EnumTrialRegistrationStatus.Expired;
            }
            else if (checkTrial)
            {
                currentStatus = EnumTrialRegistrationStatus.Trial;
            }
            else if (!checkTrial && query.Status == EnumOrderStatus.Payment)
            {
                currentStatus = EnumTrialRegistrationStatus.Payment;
            }

            methodResult.Result = currentStatus;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
