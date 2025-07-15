// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.CommandModels.UserGroup;
using Fsel.Identity.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.UserGroupCmd
{
    public class UpdateUserGroupCommand : CreateUserGroupCommandModel, IRequest<MethodResult<UserGroupModel>>
    {
        public Guid Id { get; set; }

        public class Handler : IRequestHandler<UpdateUserGroupCommand, MethodResult<UserGroupModel>>
        {
            private readonly IUserGroupRepository _userGroupRepository;
            private readonly IMapper _mapper;

            public Handler(IUserGroupRepository userGroupRepository, IMapper mapper)
            {
                _userGroupRepository = userGroupRepository;
                _mapper = mapper;
            }

            public async Task<MethodResult<UserGroupModel>> Handle(UpdateUserGroupCommand request, CancellationToken cancellationToken)
            {
                var methodResult = new MethodResult<UserGroupModel>();

                // Lấy nhóm cần cập nhật
                var userGroup = await _userGroupRepository.GetByIdAsync(request.Id);
                if (userGroup == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(UserGroup));
                    return methodResult;
                }

                // Kiểm tra xem tên nhóm đã tồn tại chưa (nếu thay đổi tên)
                if (userGroup.GroupName != request.GroupName)
                {
                    var existingGroup = await _userGroupRepository.Queryable
                        .FirstOrDefaultAsync(x => x.GroupName == request.GroupName && x.Id != request.Id, cancellationToken);

                    if (existingGroup != null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.GroupName));
                        return methodResult;
                    }
                }

                // Lấy tất cả các nhóm để xử lý DisplayOrder
                var allGroups = await _userGroupRepository.Queryable
                    .Where(x => x.Id != request.Id) // Loại trừ nhóm hiện tại
                    .OrderBy(x => x.DisplayOrder)
                    .ToListAsync(cancellationToken);

                // Lưu lại DisplayOrder cũ của nhóm hiện tại
                var oldDisplayOrder = userGroup.DisplayOrder;
                var newDisplayOrder = request.DisplayOrder;

                // Nếu DisplayOrder không thay đổi hoặc là 0, không cần xử lý
                if (newDisplayOrder <= 0)
                {
                    // Nếu DisplayOrder = 0, đặt xuống cuối
                    if (newDisplayOrder == 0 && allGroups.Any())
                    {
                        newDisplayOrder = allGroups.Max(x => x.DisplayOrder) + 1;
                        request.DisplayOrder = newDisplayOrder;
                    }
                    else if (newDisplayOrder == 0)
                    {
                        // Nếu không có nhóm nào khác, đặt là 1
                        newDisplayOrder = 1;
                        request.DisplayOrder = newDisplayOrder;
                    }
                    else
                    {
                        // Giữ nguyên DisplayOrder cũ
                        newDisplayOrder = oldDisplayOrder;
                        request.DisplayOrder = oldDisplayOrder;
                    }
                }
                else if (newDisplayOrder != oldDisplayOrder)
                {
                    // Thay đổi DisplayOrder của các nhóm khác
                    if (newDisplayOrder < oldDisplayOrder)
                    {
                        // Di chuyển lên (DisplayOrder giảm): Các nhóm có DisplayOrder >= newDisplayOrder và < oldDisplayOrder sẽ tăng lên 1
                        var groupsToUpdate = allGroups.Where(x => x.DisplayOrder >= newDisplayOrder && x.DisplayOrder < oldDisplayOrder).ToList();
                        foreach (var group in groupsToUpdate)
                        {
                            group.DisplayOrder += 1;
                        }
                    }
                    else if (newDisplayOrder > oldDisplayOrder)
                    {
                        // Di chuyển xuống (DisplayOrder tăng): Các nhóm có DisplayOrder > oldDisplayOrder và <= newDisplayOrder sẽ giảm xuống 1
                        var groupsToUpdate = allGroups.Where(x => x.DisplayOrder > oldDisplayOrder && x.DisplayOrder <= newDisplayOrder).ToList();
                        foreach (var group in groupsToUpdate)
                        {
                            group.DisplayOrder -= 1;
                        }
                    }
                }

                // Cập nhật thông tin nhóm với AutoMapper
                _mapper.Map(request, userGroup);

                await _userGroupRepository.ExecuteTransactionAsync(async () =>
                {
                    // Nếu có sự thay đổi DisplayOrder, cập nhật các nhóm bị ảnh hưởng
                    if (newDisplayOrder != oldDisplayOrder)
                    {
                        if (newDisplayOrder < oldDisplayOrder)
                        {
                            var groupsToUpdate = allGroups.Where(x => x.DisplayOrder >= newDisplayOrder && x.DisplayOrder < oldDisplayOrder).ToList();
                            if (groupsToUpdate.Any())
                            {
                                _userGroupRepository.UpdateList(groupsToUpdate);
                            }
                        }
                        else if (newDisplayOrder > oldDisplayOrder)
                        {
                            var groupsToUpdate = allGroups.Where(x => x.DisplayOrder > oldDisplayOrder && x.DisplayOrder <= newDisplayOrder).ToList();
                            if (groupsToUpdate.Any())
                            {
                                _userGroupRepository.UpdateList(groupsToUpdate);
                            }
                        }
                    }

                    // Cập nhật nhóm hiện tại
                    _userGroupRepository.Update(userGroup);
                    await _userGroupRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

                    // Map trở lại model
                    var resultModel = _mapper.Map<UserGroupModel>(userGroup);

                    methodResult.StatusCode = StatusCodes.Status200OK;
                    methodResult.Result = resultModel;
                    return methodResult;
                });

                return methodResult;
            }
        }
    }
}
