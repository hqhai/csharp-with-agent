// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.V1i2
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Queries.OrderQuery;
    using Fsel.Ordering.Application.Queues.Publishers;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateOrderTrialCommand : IRequest<MethodResult<bool>>
    {
    }

    public class CreateOrderTrialCommandHandler : IRequestHandler<CreateOrderTrialCommand, MethodResult<bool>>
    {
        private readonly IUserService _userService;
        private readonly IOrderRepository _orderRepository;
        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;
        private readonly AddExpiredDateForStudentPublisher _addExpiredDateForStudentPublisher;
        private readonly AppSetting _appSetting;

        public CreateOrderTrialCommandHandler(IUserService userService, IOrderRepository orderRepository, AuthContext authContext, IMediator mediator, AddExpiredDateForStudentPublisher addExpiredDateForStudentPublisher, AppSetting appSetting)
        {
            _userService = userService;
            _orderRepository = orderRepository;
            _authContext = authContext;
            _mediator = mediator;
            _addExpiredDateForStudentPublisher = addExpiredDateForStudentPublisher;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<bool>> Handle(CreateOrderTrialCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            int? trialPeriod = _appSetting.OrderConfigs?.TrialPeriod;
            if (!trialPeriod.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(trialPeriod));
                return methodResult;
            }

            if (await _orderRepository.Queryable.AnyAsync(p => p.UserId == _authContext.CurrentUserId && p.IsTrial, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var student = studentResult.Content?.Result;

            if (student == null || string.IsNullOrEmpty(student.User?.FullName) || string.IsNullOrEmpty(student.User?.Email))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            var codeSend = await _mediator.Send(new GenerateRandomOrderQuery() { StudentCode = student.User?.Code }, cancellationToken).ConfigureAwait(false);
            string code = codeSend.Result ?? string.Empty;

            if (string.IsNullOrEmpty(code) || await _orderRepository.Queryable.AnyAsync(x => x.Code == code, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(code));
                return methodResult;
            }

            var orderTrial = new Order()
            {
                Code = code,
                FullName = student?.User?.FullName,
                Email = student?.User?.Email,
                PhoneNumber = student?.User?.PhoneNumber,
                Status = EnumOrderStatus.Payment,
                ExpireDate = DateTime.UtcNow.AddDays(trialPeriod.Value),
                IsTrial = true,
                IsInvoice = false,
                UserId = _authContext.CurrentUserId,
                RevenueType = EnumPaymentRevenueType.NotRevenue
            };

            if (!orderTrial.IsValid())
            {
                methodResult.AddErrorBadRequest(orderTrial.ErrorMessages);
                return methodResult;
            }

            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                var orders = new List<Order>() { orderTrial };

                await _orderRepository.BulkMergeAsync(orders, x =>
                {
                    x.ColumnPrimaryKeyExpression = c => new { c.UserId, c.IsTrial };
                });

                await _orderRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                await _userService.CreateStudentTrialRegistration();

                var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).AddDays(trialPeriod.Value);

                await _addExpiredDateForStudentPublisher.Publish(new AddExpiredDateForStudentQueueModel()
                {
                    StudentId = student.Id,
                    Month = null,
                    Day = null,
                    ExpiredDate = currentDate
                }, cancellationToken);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
