// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteListUsersCommand : IRequest<MethodResult<bool>>
    {
        public IList<Guid>? UserIds { get; set; }
    }

    public class DeleteListUsersCommandHandler : IRequestHandler<DeleteListUsersCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly AuthContext _authContext;
        private readonly IUserGroupMemberShipRepository _userGroupMemberShipRepository;

        public DeleteListUsersCommandHandler(UserManager<User> userManager, IUserGroupMemberShipRepository userGroupMemberShipRepository, AuthContext authContext)
        {
            _userManager = userManager;
            _userGroupMemberShipRepository = userGroupMemberShipRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(DeleteListUsersCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.UserIds == null || !request.UserIds.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.UserIds));
                return methodResult;
            }

            var usersToDelete = await _userManager.Users
                .WhereBulkContains(request.UserIds, x => x.Id)
                .ToListAsync(cancellationToken);


            if (!usersToDelete.Any() || usersToDelete.Count != request.UserIds.Distinct().Count())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.UserIds));
                return methodResult;
            }

            await _userGroupMemberShipRepository.ExecuteTransactionAsync(async () =>
            {
                await _userManager.Users
                    .Where(u => request.UserIds.Contains(u.Id))
                    .ExecuteUpdateAsync(setter => setter
                        .SetProperty(u => u.IsDeleted, true)
                        .SetProperty(u => u.DeletedFullName, _authContext.CurrentFullName)
                        .SetProperty(u => u.DeletedUserId, _authContext.CurrentUserId)
                        .SetProperty(u => u.DeletedDate, DateTime.UtcNow)
                    , cancellationToken);

                await _userGroupMemberShipRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }
    }
}
