// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base.Managers;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Identity.Application.Commands.UserGroupCmd
{
    public class DeleteUserGroupCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }

        public class Handler : IRequestHandler<DeleteUserGroupCommand, MethodResult<bool>>
        {
            private readonly IUserRoleRepository _userRoleRepository;
            private readonly RoleManager<Role> _roleManager;

            public Handler(RoleManager<Role> roleManager, IUserRoleRepository userRoleRepository)
            {
                _roleManager = roleManager;
                _userRoleRepository = userRoleRepository;
            }

            public async Task<MethodResult<bool>> Handle(DeleteUserGroupCommand request, CancellationToken cancellationToken)
            {
                var methodResult = new MethodResult<bool>();

                // Lấy nhóm cần xóa
                var userGroup = await _roleManager.FindByIdAsync(request.Id.ToString());
                if (userGroup == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(UserGroup));
                    return methodResult;
                }

                if (userGroup.IsDefault)
                {
                    methodResult.AddErrorBadRequest(nameof(Domain.Enums.ErrorCodes.EnumUserGroupErrorCode.DoNotDeleteTheDefaultUserGroup), nameof(userGroup));
                    return methodResult;
                }

                // Lấy tất cả thành viên trong nhóm
                var memberships = _userRoleRepository.GetQuery()
                    .Where(x => x.RoleId == request.Id)
                    .ToList();

                // Xóa nhóm và tất cả thành viên trong transaction
                // Xóa tất cả thành viên trong nhóm
                foreach (var membership in memberships)
                {
                    await _userRoleRepository.DeleteAsync(membership);
                }

                // Xóa nhóm
                await _roleManager.DeleteAsync(userGroup);

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            }
        }
    }
}
