// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Commands.UserGroupCmd;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Users;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateUserInLmsAdminCommand : UpdateUserInLmsAdminPlatCommandModel, IRequest<MethodResult<UserModel>>
    {
    }

    public class UpdateUserInLmsAdminCommandHandler : IRequestHandler<UpdateUserInLmsAdminCommand, MethodResult<UserModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateUserInLmsAdminCommandHandler(UserManager<User> userManager, IMapper mapper, IMediator mediator)
        {
            _userManager = userManager;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<MethodResult<UserModel>> Handle(UpdateUserInLmsAdminCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserModel>();

            #region Validate user
            // Kiểm tra user có tồn tại không
            var userEntity = await _userManager.Users
                .Include(x => x.UserGroups)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);

            if (userEntity == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id));
                return methodResult;
            }

            User? user = null;
            //Kiểm tra email
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                user = await _userManager.FindByEmailAsync(request.Email);
                if (user != null && user.Id != userEntity.Id)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicateEmail), nameof(request.Email), request.Email);
                    return methodResult;
                }
            }

            // Kiểm tra tên đăng nhập
            user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == request.UserName, cancellationToken: cancellationToken);
            if (user != null && user.Id != userEntity.Id)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicateUsername), nameof(request.UserName), request.UserName);
                return methodResult;
            }

            // Kiểm tra SĐT
            user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber, cancellationToken: cancellationToken);
            if (user != null && user.Id != userEntity.Id)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicatePhoneNumber), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }
            #endregion

            // Map dữ liệu
            _mapper.Map(request, userEntity);

            bool isSuccessChangePassword = true;
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                // Xóa mật khẩu hiện tại
                var result = await _userManager.RemovePasswordAsync(userEntity).ConfigureAwait(false);

                // Nếu xóa thành công thì thêm mật khẩu mới
                if (result.Succeeded)
                {
                    // Thêm mật khẩu mới
                    result = await _userManager.AddPasswordAsync(userEntity, request.Password).ConfigureAwait(false);
                }

                isSuccessChangePassword = result.Succeeded;
            }

            if (!isSuccessChangePassword)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.ErrorResetPassword));
                return methodResult;
            }

            await _userManager.UpdateAsync(userEntity).ConfigureAwait(false);

            var groupId = userEntity.UserGroups.FirstOrDefault()?.GroupId;

            // Cập nhật dữ liệu nhóm người dùng
            // Nếu có Id nhóm người dùng mới, thêm vào nhóm
            if (request.UserGroupId.HasValue)
            {
                var userGroupResult = await _mediator.Send(new AddUserToGroupCommand()
                {
                    GroupId = request.UserGroupId.Value,
                    UserIds = new List<Guid> { userEntity.Id }
                }, cancellationToken);

                if (!userGroupResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(userGroupResult.ErrorMessages);
                    return methodResult;
                }
            }
            else // Nếu không có Id nhóm người dùng mới, xóa khỏi nhóm hiện tại (nếu có)
            {
                if (groupId.HasValue)
                {
                    var userGroupResult = await _mediator.Send(new RemoveUserFromGroupCommand()
                    {
                        GroupId = groupId.Value,
                        UserIds = new List<Guid> { userEntity.Id }
                    }, cancellationToken);

                    if (!userGroupResult.IsOK)
                    {
                        methodResult.AddErrorBadRequest(userGroupResult.ErrorMessages);
                        return methodResult;
                    }
                }
            }

            methodResult.Result = _mapper.Map<UserModel>(userEntity);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
