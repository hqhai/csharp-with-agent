// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base.Managers;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.CommandModels.UserGroup;
using Fsel.Identity.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UserManager = Fsel.Core.Base.Managers.UserManager<Fsel.Identity.Domain.Entities.User>;


namespace Fsel.Identity.Application.Commands.UserGroupCmd
{
    public class AddUserToGroupCommand : AddUserToGroupCommandModel, IRequest<MethodResult<List<UserGroupMemberShipModel>>>
    {
        public class Handler : IRequestHandler<AddUserToGroupCommand, MethodResult<List<UserGroupMemberShipModel>>>
        {
            private readonly RoleManager<Role> _roleManager;
            private readonly IUserRoleRepository _userRoleRepository;
            private readonly IMapper _mapper;
            private readonly UserManager _userManager;

            public Handler(
                IMapper mapper,
                UserManager userManager,
                RoleManager<Role> roleManager,
                IUserRoleRepository userRoleRepository)
            {
                _mapper = mapper;
                _userManager = userManager;
                _roleManager = roleManager;
                _userRoleRepository = userRoleRepository;
            }

            public async Task<MethodResult<List<UserGroupMemberShipModel>>> Handle(AddUserToGroupCommand request, CancellationToken cancellationToken)
            {
                var methodResult = new MethodResult<List<UserGroupMemberShipModel>>();

                // Kiểm tra đầu vào
                if (request.UserIds == null || !request.UserIds.Any())
                {
                    methodResult.AddErrorBadRequest(nameof(EnumUserGroupErrorCode.ModifyAtLeastOneUser));
                    return methodResult;
                }

                // Kiểm tra nhóm tồn tại
                var userGroup = await _roleManager.FindByIdAsync(request.GroupId.ToString());
                if (userGroup == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(UserGroup));
                    return methodResult;
                }

                // Danh sách kết quả
                var resultList = new List<UserGroupMemberShipModel>();


                    foreach (var userId in request.UserIds)
                    {
                        // Kiểm tra user tồn tại
                        var user = await _userManager.FindByIdAsync(userId.ToString());
                        if (user == null)
                        {
                            // Skip nếu user không tồn tại
                            continue;
                        }

                    // Kiểm tra xem user đã thuộc nhóm chưa
                    var existingMembership = await _userRoleRepository.GetQuery()
                        .FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == request.GroupId, cancellationToken);

                        if (existingMembership != null)
                        {
                            // Nếu tài khoản đã bị vô hiệu hóa trước đó, kích hoạt lại
                            if (!existingMembership.IsActive)
                            {
                                existingMembership.IsActive = true;
                            await _userRoleRepository.UpdateAsync(existingMembership);

                                // Thêm vào danh sách kết quả
                                resultList.Add(_mapper.Map<UserGroupMemberShipModel>(existingMembership));
                            }
                            // Không thêm vào kết quả nếu đã là thành viên active
                            continue;
                        }

                        // Tạo membership mới cho mỗi user
                    var membership = new UserRole
                        {
                        RoleId = request.GroupId,
                            UserId = userId,
                        };

                    await _userRoleRepository.AddAsync(membership);

                        // Thêm vào danh sách kết quả
                        resultList.Add(_mapper.Map<UserGroupMemberShipModel>(membership));
                    }

                    methodResult.StatusCode = StatusCodes.Status200OK;
                    methodResult.Result = resultList;
                    return methodResult;
            }
        }
    }
}
