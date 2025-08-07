// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base.Managers;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.CommandModels.UserGroup;
using Fsel.Identity.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.UserGroupCmd
{
    public class CreateUserGroupCommand : CreateUserGroupCommandModel, IRequest<MethodResult<UserGroupModel>>
    {
        public class Handler : IRequestHandler<CreateUserGroupCommand, MethodResult<UserGroupModel>>
        {
            private readonly RoleManager<Role> _roleManager;
            private readonly IMapper _mapper;

            public Handler(IMapper mapper, RoleManager<Role> roleManager)
            {
                _mapper = mapper;
                _roleManager = roleManager;
            }

            public async Task<MethodResult<UserGroupModel>> Handle(CreateUserGroupCommand request, CancellationToken cancellationToken)
            {
                var methodResult = new MethodResult<UserGroupModel>();

                // Kiểm tra xem tên nhóm đã tồn tại chưa
                var existingGroup = await _roleManager.Roles
                    .FirstOrDefaultAsync(x => x.Name == request.GroupName, cancellationToken);

                if (existingGroup != null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.GroupName));
                    return methodResult;
                }

                // Xử lý DisplayOrder
                var allGroups = await _roleManager.Roles
                    .OrderBy(x => x.DisplayOrder)
                    .ToListAsync(cancellationToken);

                // Nếu DisplayOrder chưa được chỉ định hoặc là 0, tự động đặt là lớn nhất + 1
                if (request != null && (request.DisplayOrder == null || request.DisplayOrder <= 0))
                {
                    var maxDisplayOrder = allGroups.Any() ? allGroups.Max(x => x.DisplayOrder) : 0;
                    request.DisplayOrder = maxDisplayOrder + 1;
                }
                else
                {
                    // Các nhóm có thứ tự hiển thị >= request.DisplayOrder sẽ bị đẩy xuống 1 bậc
                    var affectedGroups = allGroups.Where(x => x.DisplayOrder >= (request?.DisplayOrder ?? 0)).ToList();

                    foreach (var group in affectedGroups)
                    {
                        group.DisplayOrder += 1;
                    }
                }

                // Tạo nhóm mới sử dụng AutoMapper
                var userGroup = _mapper.Map<Role>(request);

                    // Cập nhật DisplayOrder của các nhóm khác trước khi thêm nhóm mới
                    var groupsToUpdate = allGroups.Where(x => x.DisplayOrder >= request.DisplayOrder).ToList();

                    if (groupsToUpdate.Any())
                    {
                    foreach (var item in groupsToUpdate)
                    {
                        await _roleManager.UpdateAsync(item);
                    }
                    }

                    // Thêm nhóm mới
                await _roleManager.CreateAsync(userGroup);

                    // Map trở lại model
                    var resultModel = _mapper.Map<UserGroupModel>(userGroup);

                    methodResult.StatusCode = StatusCodes.Status200OK;
                    methodResult.Result = resultModel;
                    return methodResult;
            }
        }
    }
}
