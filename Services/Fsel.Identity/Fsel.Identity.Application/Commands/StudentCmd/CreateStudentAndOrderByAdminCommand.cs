// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Commands.UserCmd;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CreateStudentAndOrderByAdminCommand : CreateStudentAndOrderByAdminCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateStudentAndOrderByAdminCommandHandler : IRequestHandler<CreateStudentAndOrderByAdminCommand, MethodResult<bool>>
    {
        private readonly IMediator _mediator;
        private readonly UserManager<User> _userManager;
        private readonly IOrderService _orderService;

        public CreateStudentAndOrderByAdminCommandHandler(IMediator mediator, UserManager<User> userManager, IOrderService orderService)
        {
            _mediator = mediator;
            _userManager = userManager;
            _orderService = orderService;
        }

        public async Task<MethodResult<bool>> Handle(CreateStudentAndOrderByAdminCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            request.Email ??= string.Empty;
            var queryByEmail = _userManager.Users.Where(p => p.Email == request.Email.Trim());
            var queryByUserName = _userManager.Users.Where(p => p.UserName == request.Email.Trim());

            var user = await queryByEmail
                .Union(queryByUserName)
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                var result = await _mediator.Send(new CreateStudentByAdminCommand()
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
