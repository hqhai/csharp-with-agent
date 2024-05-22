// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.Auths;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using MediatR;

    public class LockUserCommand : LockUserCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class LockUserCommandHandler : IRequestHandler<LockUserCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;

        public LockUserCommandHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<MethodResult<bool>> Handle(LockUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var userName = GetUserName(request);

            if (string.IsNullOrEmpty(userName))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            if (request.IsLock)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            methodResult.Result = true;
            return methodResult;
        }

        private static string? GetUserName(CreateOrdersFromCRMCommandModel model)
        {
            if (!string.IsNullOrEmpty(model.Email))
            {
                return model.Email;
            }
            else if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                return model.PhoneNumber;
            }
            else if (!string.IsNullOrEmpty(model.FatherEmail))
            {
                return model.FatherEmail;
            }
            else if (!string.IsNullOrEmpty(model.FatherPhoneNumber))
            {
                return model.FatherPhoneNumber;
            }
            else if (!string.IsNullOrEmpty(model.MotherEmail))
            {
                return model.MotherEmail;
            }
            else if (!string.IsNullOrEmpty(model.MotherPhoneNumber))
            {
                return model.MotherPhoneNumber;
            }
            return null;
        }
    }
}
