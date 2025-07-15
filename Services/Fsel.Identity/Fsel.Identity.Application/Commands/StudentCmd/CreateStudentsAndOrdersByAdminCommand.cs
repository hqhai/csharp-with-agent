// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CreateStudentsAndOrdersByAdminCommandResultModel
    {
        public IList<string> CreateUserError { get; set; } = new List<string>();
        public IList<string> CreateOrderError { get; set; } = new List<string>();
    }

    public class CreateStudentsAndOrdersByAdminCommand : CreateStudentsAndOrdersByAdminCommandModel, IRequest<MethodResult<CreateStudentsAndOrdersByAdminCommandResultModel>>
    {
    }

    public class CreateStudentsAndOrdersByAdminCommandHandler : IRequestHandler<CreateStudentsAndOrdersByAdminCommand, MethodResult<CreateStudentsAndOrdersByAdminCommandResultModel>>
    {
        private readonly IMediator _mediator;
        private readonly UserManager<User> _userManager;
        private readonly IOrderService _orderService;

        public CreateStudentsAndOrdersByAdminCommandHandler(IMediator mediator, UserManager<User> userManager, IOrderService orderService)
        {
            _mediator = mediator;
            _userManager = userManager;
            _orderService = orderService;
        }

        public async Task<MethodResult<CreateStudentsAndOrdersByAdminCommandResultModel>> Handle(CreateStudentsAndOrdersByAdminCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CreateStudentsAndOrdersByAdminCommandResultModel>();

            if (request.Users == null || request.Users.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            var result = new CreateStudentsAndOrdersByAdminCommandResultModel();

            foreach (var item in request.Users)
            {
                item.Email ??= string.Empty;
                var queryByEmail = _userManager.Users.Where(p => p.Email == item.Email.Trim());
                var queryByUserName = _userManager.Users.Where(p => p.UserName == item.Email.Trim());

                var user = await queryByEmail
                    .Union(queryByUserName)
                    .FirstOrDefaultAsync(cancellationToken);

                if (user == null)
                {
                    var createUserResult = await _mediator.Send(new CreateStudentByAdminCommand()
                    {
                        FullName = item.FullName,
                        PhoneNumber = item.PhoneNumber,
                        Email = item.Email,
                        School = item.School,
                        SchoolId = item.SchoolId,
                        DateOfBirth = item.DateOfBirth,
                        ReferralCode = item.ReferralCode,
                    }, cancellationToken);
                    if (!createUserResult.IsOK)
                    {
                        result.CreateUserError.Add(item.Email ?? string.Empty);
                        methodResult.AddError(createUserResult.ErrorMessages);
                    }

                    user = createUserResult.Result;
                }

                if (user == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                    return methodResult;
                }

                var createOrderResult = await _orderService.CreateOrderPayment(new CreateOrderPaymentCommandModel()
                {
                    UserId = user.Id,
                    FullName = item.FullName,
                    PhoneNumber = item.PhoneNumber,
                    Email = item.Email,
                    MonthNumber = item.MonthNumber,
                    ExpireDate = item.ExpireDate,
                    IsRevenue = item.IsRevenue,
                    IsSendMail = item.IsSendMail,
                    VoucherCode = item.VoucherCode,
                    EventCode = item.EventCode,
                });

                if (!createOrderResult.IsSuccessStatusCode)
                {
                    result.CreateOrderError.Add(item.Email ?? string.Empty);
                    methodResult.AddError(createOrderResult.Error);
                }
            }
            methodResult.Result = result;
            return methodResult;
        }
    }
}
