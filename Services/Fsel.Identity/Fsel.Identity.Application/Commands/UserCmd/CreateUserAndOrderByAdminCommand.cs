// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.Users;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CreateUserAndOrderByAdminCommand : CreateUserAndOrderByAdminCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateUserAndOrderByAdminCommandHandler : IRequestHandler<CreateUserAndOrderByAdminCommand, MethodResult<bool>>
    {
        private readonly IMediator _mediator;
        private readonly UserManager<User> _userManager;
        private readonly IOrderService _orderService;

        public CreateUserAndOrderByAdminCommandHandler(IMediator mediator, UserManager<User> userManager, IOrderService orderService)
        {
            _mediator = mediator;
            _userManager = userManager;
            _orderService = orderService;
        }

        public async Task<MethodResult<bool>> Handle(CreateUserAndOrderByAdminCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var user = await _userManager.Users.FirstOrDefaultAsync(p => p.Email.Trim().ToLower() == request.Email.Trim().ToLower() || p.UserName.Trim().ToLower() == request.Email.Trim().ToLower(), cancellationToken);

            if (user == null)
            {
                var result = await _mediator.Send(new CreateUserByAdminCommand()
                {
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    School = request.School,
                    DateOfBirth = request.DateOfBirth,
                    ReferralCode = request.ReferralCode,
                }, cancellationToken);
                if (!result.IsOK)
                {
                    methodResult.AddError(result.ErrorMessages);
                    return methodResult;
                }

                user = result.Result;
            }

            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var createOrderResult = await _orderService.CreateOrderPayment(new CreateOrderPaymentCommandModel()
            {
                UserId = user.Id,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                MonthNumber = request.MonthNumber,
                ExpireDate = request.ExpireDate,
                IsRevenue = request.IsRevenue,
                IsSendMail = request.IsSendMail,
                VoucherCode = request.VoucherCode,
                EventCode = request.EventCode,
            });

            if (!createOrderResult.IsSuccessStatusCode)
            {
                methodResult.AddError(createOrderResult.Error);
                return methodResult;
            }
            return methodResult;
        }
    }
}
