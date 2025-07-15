// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
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
            private readonly IUserGroupRepository _userGroupRepository;
            private readonly IUserGroupMemberShipRepository _userGroupMemberShipRepository;
            private readonly IMapper _mapper;
            private readonly UserManager _userManager;

            public Handler(
                IUserGroupRepository userGroupRepository,
                IUserGroupMemberShipRepository userGroupMemberShipRepository,
                IMapper mapper,
                UserManager userManager)
            {
                _userGroupRepository = userGroupRepository;
                _userGroupMemberShipRepository = userGroupMemberShipRepository;
                _mapper = mapper;
                _userManager = userManager;
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
                var userGroup = await _userGroupRepository.GetByIdAsync(request.GroupId);
                if (userGroup == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(UserGroup));
                    return methodResult;
                }

                // Danh sách kết quả
                var resultList = new List<UserGroupMemberShipModel>();

                // Xử lý trong transaction để đảm bảo tính nhất quán
                await _userGroupMemberShipRepository.ExecuteTransactionAsync(async () =>
                {
                    foreach (var userId in request.UserIds)
                    {
                        // Kiểm tra user tồn tại
                        var user = await _userManager.FindByIdAsync(userId.ToString());
                        if (user == null)
                        {
                            // Skip nếu user không tồn tại
                            continue;
                        }

                        // Kiểm tra xem user đã thuộc 1 nhóm khác hay chưa
                        var existingMembershipOfOtherGroup = await _userGroupMemberShipRepository.Queryable
                            .FirstOrDefaultAsync(x => x.UserId == userId && x.GroupId != request.GroupId, cancellationToken);

                        // Nếu có thì xóa bản ghi đã tồn tại để thỏa mãn rule 1 user chỉ thuộc 1 nhóm người dùng
                        if (existingMembershipOfOtherGroup != null)
                        {
                            await _userGroupMemberShipRepository.DeleteAsync(existingMembershipOfOtherGroup);
                        }

                        var existingMembership = await _userGroupMemberShipRepository.Queryable
                            .FirstOrDefaultAsync(x => x.UserId == userId && x.GroupId == request.GroupId, cancellationToken);

                        if (existingMembership != null)
                        {
                            // Nếu tài khoản đã bị vô hiệu hóa trước đó, kích hoạt lại
                            if (!existingMembership.IsActive)
                            {
                                existingMembership.IsActive = true;
                                _userGroupMemberShipRepository.Update(existingMembership);

                                // Thêm vào danh sách kết quả
                                resultList.Add(_mapper.Map<UserGroupMemberShipModel>(existingMembership));
                            }
                            // Không thêm vào kết quả nếu đã là thành viên active
                            continue;
                        }

                        // Tạo membership mới cho mỗi user
                        var membership = new UserGroupMemberShip
                        {
                            GroupId = request.GroupId,
                            UserId = userId,
                        };

                        _userGroupMemberShipRepository.Add(membership);

                        // Thêm vào danh sách kết quả
                        resultList.Add(_mapper.Map<UserGroupMemberShipModel>(membership));
                    }

                    // Lưu tất cả thay đổi
                    await _userGroupMemberShipRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

                    methodResult.StatusCode = StatusCodes.Status200OK;
                    methodResult.Result = resultList;
                    return methodResult;

                });

                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
        }
    }
}
