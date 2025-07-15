// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.CommandModels.UserGroup;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.UserGroupCmd
{
    public class RemoveUserFromGroupCommand : RemoveUserFromGroupCommandModel, IRequest<MethodResult<bool>>
    {
        public class Handler : IRequestHandler<RemoveUserFromGroupCommand, MethodResult<bool>>
        {
            private readonly IUserRoleRepository _userRoleRepository;

            public Handler(IUserRoleRepository userRoleRepository)
            {
                _userRoleRepository = userRoleRepository;
            }

            public async Task<MethodResult<bool>> Handle(RemoveUserFromGroupCommand request, CancellationToken cancellationToken)
            {
                var methodResult = new MethodResult<bool>();

                // Kiểm tra đầu vào
                if (request.UserIds == null || !request.UserIds.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumUserGroupErrorCode.ModifyAtLeastOneUser));
                    return methodResult;
                }

                // Xử lý trong transaction để đảm bảo tính nhất quán

                    foreach (var userId in request.UserIds)
                    {
                        // Kiểm tra xem user có thuộc nhóm không
                    var membership = await _userRoleRepository.GetQuery()
                        .FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == request.GroupId, cancellationToken);

                        if (membership == null)
                        {
                            // Skip nếu không tìm thấy thành viên
                            continue;
                        }
                    await _userRoleRepository.DeleteAsync(membership);
                    }

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            }
        }
    }
}
