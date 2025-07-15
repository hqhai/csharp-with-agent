// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStatusUsersCommand : IRequest<MethodResult<bool>>
    {
        public IList<Guid>? Ids { get; set; }
        public EnumUserStatus Status { get; set; }
    }

    public class UpdateStatusUsersCommandHandler : IRequestHandler<UpdateStatusUsersCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;

        public UpdateStatusUsersCommandHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<MethodResult<bool>> Handle(UpdateStatusUsersCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Ids);

            var methodResult = new MethodResult<bool>();

            var users = await _userManager.Users.WhereBulkContains(request.Ids, x => x.Id).ToListAsync(cancellationToken);
            if (users == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(users));
                return methodResult;
            }

            foreach (var user in users)
            {
                user.Status = request.Status;
                await _userManager.UpdateAsync(user);
            }


            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
