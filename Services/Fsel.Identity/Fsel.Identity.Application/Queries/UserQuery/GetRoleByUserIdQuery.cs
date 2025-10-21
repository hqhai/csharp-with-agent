// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.UserQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetRoleByUserIdQuery : IRequest<MethodResult<string?>>
    {
        public string? UserId { get; set; }
    }

    public class GetRoleByUserIdQueryHandler : IRequestHandler<GetRoleByUserIdQuery, MethodResult<string?>>
    {
        private readonly UserManager<User> _userManager;

        public GetRoleByUserIdQueryHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<MethodResult<string?>> Handle(GetRoleByUserIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<string?>();

            var user = await _userManager.Users.FirstOrDefaultAsync(p => p.Id.ToString() == request.UserId, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.UserId), request.UserId);
                return methodResult;
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            methodResult.Result = userRoles.FirstOrDefault() ?? null;

            return methodResult;
        }
    }
}
