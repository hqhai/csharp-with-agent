// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateReferralCodeStudentCommand : UpdateReferralCodeStudentCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class UpdateReferralCodeStudentCommandHandler : IRequestHandler<UpdateReferralCodeStudentCommand, MethodResult<bool>>
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<User> _userManager;

        public UpdateReferralCodeStudentCommandHandler(IOrderService orderService, UserManager<User> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        public async Task<MethodResult<bool>> Handle(UpdateReferralCodeStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (string.IsNullOrEmpty(request.ReferralCode))
            {
                methodResult.AddErrorBadRequest(nameof(Common.Enums.ErrorCodes.EnumSystemErrorCode.Required));
                return methodResult;
            }

            var userReferral = await _userManager.Users.Include(x => x.Human).FirstOrDefaultAsync(x => x.Human!.Code == request.ReferralCode, cancellationToken);
            if (userReferral == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.UserNotExistByCode));
                return methodResult;
            }
            var userReferralResult = await _orderService.CreateUserReferralAsync(new CreateUserReferralCommandModel { SenderId = new Guid(userReferral.Id), ReceiverId = request.UserId });
            if (!userReferralResult.IsSuccessStatusCode)
            {
                methodResult.AddError(userReferralResult.Error);
                return methodResult;
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
