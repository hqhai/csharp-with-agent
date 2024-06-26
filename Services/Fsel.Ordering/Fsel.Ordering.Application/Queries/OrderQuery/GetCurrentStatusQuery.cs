// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Services.UserService;
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
        private readonly IUserService _userService;

        public GetCurrentStatusQueryHandler(IOrderRepository orderRepository, IUserService userService)
        {
            _orderRepository = orderRepository;
            _userService = userService;
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

            var studentResult = await _userService.GetStudentByUserIdAsync(request.UserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var student = studentResult.Content?.Result;

            if (student == null)
            {
                methodResult.Result = currentStatus;
                return methodResult;
            }

            DateTime currentDate = DateTime.UtcNow;
            var currentExpireDate = query.ExpireDate;
            var checkTrial = query.IsTrial;

            if (!student.ExpiredDate.HasValue)
            {
                currentStatus = EnumTrialRegistrationStatus.New;
            }
            else if (student.ExpiredDate.Value.Date < currentDate.Date)
            {
                currentStatus = EnumTrialRegistrationStatus.Expired;
            }
            else if (checkTrial)
            {
                currentStatus = EnumTrialRegistrationStatus.Trial;
            }
            else
            {
                currentStatus = EnumTrialRegistrationStatus.Payment;
            }

            methodResult.Result = currentStatus;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
