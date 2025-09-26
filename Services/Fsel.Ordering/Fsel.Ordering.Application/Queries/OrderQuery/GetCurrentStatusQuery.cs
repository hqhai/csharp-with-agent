// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Queries.OrderQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCurrentStatusQuery : IRequest<MethodResult<EnumTrialRegistrationStatus?>>
    {
        public Guid UserId { get; set; }
    }

    public class GetCurrentStatusQueryHandler : IRequestHandler<GetCurrentStatusQuery, MethodResult<EnumTrialRegistrationStatus?>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public GetCurrentStatusQueryHandler(IOrderRepository orderRepository, IUserService userService, AuthContext authContext)
        {
            _orderRepository = orderRepository;
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<EnumTrialRegistrationStatus?>> Handle(GetCurrentStatusQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<EnumTrialRegistrationStatus?> methodResult = new MethodResult<EnumTrialRegistrationStatus?>();

            var role = _authContext.Roles?.FirstOrDefault();
            if (!string.IsNullOrEmpty(role) && role == EnumRole.StudentCampus.ToString())
            {
                methodResult.Result = EnumTrialRegistrationStatus.Payment;
                return methodResult;
            }

            var currentStatus = EnumTrialRegistrationStatus.New;

            var query = await _orderRepository.Queryable.Where(x => x.UserId == request.UserId && x.Status == EnumOrderStatus.Payment).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(cancellationToken);

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

            DateTime currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
            var currentExpireDate = student.ExpiredDate ?? query.ExpireDate;

            if (!currentExpireDate.HasValue)
            {
                currentStatus = EnumTrialRegistrationStatus.New;
            }
            else if (currentExpireDate.Value < currentDate)
            {
                currentStatus = EnumTrialRegistrationStatus.Expired;
            }
            else if (await _orderRepository.Queryable.AnyAsync(p => p.UserId == request.UserId && !p.IsTrial && p.Status == EnumOrderStatus.Payment, cancellationToken))
            {
                currentStatus = EnumTrialRegistrationStatus.Payment;
            }
            else if (await _orderRepository.Queryable.AnyAsync(p => p.UserId == request.UserId && p.IsTrial && p.Status == EnumOrderStatus.Payment, cancellationToken))
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
