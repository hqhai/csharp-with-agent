// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.UserGroupCmd
{
    public class DeleteUserGroupCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }

        public class Handler : IRequestHandler<DeleteUserGroupCommand, MethodResult<bool>>
        {
            private readonly IUserGroupRepository _userGroupRepository;
            private readonly IUserGroupMemberShipRepository _userGroupMemberShipRepository;

            public Handler(IUserGroupRepository userGroupRepository, IUserGroupMemberShipRepository userGroupMemberShipRepository)
            {
                _userGroupRepository = userGroupRepository;
                _userGroupMemberShipRepository = userGroupMemberShipRepository;
            }

            public async Task<MethodResult<bool>> Handle(DeleteUserGroupCommand request, CancellationToken cancellationToken)
            {
                ArgumentNullException.ThrowIfNull(request);
                var methodResult = new MethodResult<bool>();

                // Lấy nhóm cần xóa
                var userGroup = await _userGroupRepository.GetByIdAsync(request.Id);
                if (userGroup == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(UserGroup));
                    return methodResult;
                }

                if (userGroup.IsDefault)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumUserGroupErrorCode.DoNotDeleteTheDefaultUserGroup), nameof(userGroup));
                    return methodResult;
                }

                // Lấy tất cả thành viên trong nhóm
                var memberships = await _userGroupMemberShipRepository.Queryable
                    .Where(x => x.GroupId == request.Id)
                    .ToListAsync(cancellationToken);

                if (memberships.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumUserGroupErrorCode.SomeoneIsInTheUserGroup), nameof(userGroup));
                    return methodResult;
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