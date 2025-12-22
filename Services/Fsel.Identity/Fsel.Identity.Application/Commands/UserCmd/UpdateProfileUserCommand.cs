// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserCmd
{
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Users;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateProfileUserCommand : UpdateProfileUserCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class UpdateProfileUserCommandHandler : IRequestHandler<UpdateProfileUserCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IUserGroupRepository _userGroupRepository;
        private readonly IUserRoleRepository _userRoleRepository;

        public UpdateProfileUserCommandHandler(UserManager<User> userManager,
                                               IMapper mapper,
                                               IUserGroupRepository userGroupRepository,
                                               IUserRoleRepository userRoleRepository)
        {
            _userManager = userManager;
            _mapper = mapper;
            _userGroupRepository = userGroupRepository;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<MethodResult<bool>> Handle(UpdateProfileUserCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Password);
            var methodResult = new MethodResult<bool>();

            var validateUser = await ValidateUser(request);
            if (!validateUser.IsOK)
            {
                methodResult.AddErrorBadRequest(validateUser.ErrorMessages.ToList());
                return methodResult;
            }

            var user = await _userManager.Users
                                         .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user), request.Id);
                return methodResult;
            }

            var checkUserGroup = await _userGroupRepository.Queryable.AnyAsync(x => x.Id == request.GroupId, cancellationToken);
            if (!checkUserGroup)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.GroupId), request.GroupId);
                return methodResult;
            }

            var passwordValidator = new Microsoft.AspNetCore.Identity.PasswordValidator<User>();
            var validPassword = await passwordValidator.ValidateAsync(_userManager, user, request.Password);
            if (!validPassword.Succeeded)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.PasswordIsNotValid));
                return methodResult;
            }

            _mapper.Map(request, user);

            if (!user.IsValid())
            {
                methodResult.AddErrorBadRequest(user.ErrorMessages);
                return methodResult;
            }

            user.UserName = request.UserName;
            var hashPassword = _userManager.PasswordHasher.HashPassword(user, request.Password);
            user.PasswordHash = hashPassword;

            if (request.GroupId.HasValue)
            {
                // Lấy UserRole hiện tại
                var currentUserRole = await _userRoleRepository.GetQuery()
                    .FirstOrDefaultAsync(x => x.UserId == user.Id, cancellationToken);

                if (currentUserRole != null)
                {
                    // Nếu RoleId khác với GroupId mới, cần xóa UserRole cũ và tạo mới
                    // Vì RoleId là part of composite key, không thể update trực tiếp
                    if (currentUserRole.RoleId != request.GroupId.Value)
                    {
                        // Xóa UserRole cũ
                        await _userRoleRepository.DeleteAsync(currentUserRole);

                        // Tạo UserRole mới với RoleId mới
                        var newUserRole = new UserRole
                        {
                            UserId = user.Id,
                            RoleId = request.GroupId.Value,
                            IsActive = currentUserRole.IsActive // Giữ nguyên trạng thái IsActive
                        };
                        await _userRoleRepository.AddAsync(newUserRole);
                    }
                }
                else
                {
                    // Nếu chưa có UserRole, tạo mới
                    var newUserRole = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = request.GroupId.Value,
                        IsActive = true
                    };
                    await _userRoleRepository.AddAsync(newUserRole);
                }
            }

            await _userManager.UpdateAsync(user);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }

        public async Task<MethodResult<bool>> ValidateUser(UpdateProfileUserCommandModel user)
        {
            ArgumentNullException.ThrowIfNull(user);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            // Kiểm tra UserName
            if (await _userManager.Users.AnyAsync(x => x.Id != user.Id && x.UserName == user.UserName!.Trim()))
            {
                methodResult.AddErrorBadRequest("Tên đăng nhập không được trùng.");
                return methodResult;
            }

            // Kiểm tra FullName
            if (string.IsNullOrEmpty(user.FullName))
            {
                methodResult.AddErrorBadRequest("FullName là bắt buộc.");
                return methodResult;
            }

            // Kiểm tra Email
            if (string.IsNullOrEmpty(user.Email))
            {
                methodResult.AddErrorBadRequest("Email là bắt buộc.");
                return methodResult;
            }
            else if (user.Email.Length > 254)
            {
                methodResult.AddErrorBadRequest("Email không được vượt quá 254 ký tự.");
                return methodResult;
            }
            else if (!Regex.IsMatch(user.Email, @"^[a-zA-Z0-9!#$%&'*+\-/=?^_`{|}~]+(\.[a-zA-Z0-9!#$%&'*+\-/=?^_`{|}~]+)*@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                methodResult.AddErrorBadRequest("Email không hợp lệ.");
                return methodResult;
            }

            // Kiểm tra số điện thoại
            if (string.IsNullOrEmpty(user.PhoneNumber))
            {
                methodResult.AddErrorBadRequest("Số điện thoại là bắt buộc.");
                return methodResult;
            }
            else if (!Regex.IsMatch(user.PhoneNumber, @"^\+?[0-9]{7,15}$"))
            {
                methodResult.AddErrorBadRequest("Số điện thoại không hợp lệ.");
                return methodResult;
            }

            // Kiểm tra ngày sinh
            var today = DateTimeHelper.ConvertTimeFromUtc(DateTime.UtcNow, EnumCountryKey.Vietnam);
            var age = today.Year - user.Birthday.Year;
            if (age < 5 || age > 120)
            {
                methodResult.AddErrorBadRequest("Độ tuổi phải từ 5 đến 120.");
                return methodResult;
            }

            // Kiểm tra chức vụ
            if (string.IsNullOrEmpty(user.Position))
            {
                methodResult.AddErrorBadRequest("Bạn chưa điền chức vụ.");
                return methodResult;
            }

            // Kiểm tra tên tài khoản
            if (string.IsNullOrEmpty(user.UserName))
            {
                methodResult.AddErrorBadRequest("Tên tài khoản là bắt buộc.");
                return methodResult;
            }
            else if (user.UserName.Length > 50)
            {
                methodResult.AddErrorBadRequest("Tên tài khoản không được vượt quá 50 ký tự.");
                return methodResult;
            }
            else if (!Regex.IsMatch(user.UserName, @"^[A-Za-z\s'-]+$"))
            {
                methodResult.AddErrorBadRequest("Tên tài khoản chỉ chứa chữ cái, khoảng trắng, dấu nháy đơn và dấu gạch ngang.");
                return methodResult;
            }

            // Kiểm tra mật khẩu
            if (string.IsNullOrEmpty(user.Password))
            {
                methodResult.AddErrorBadRequest("Mật khẩu là bắt buộc.");
                return methodResult;
            }
            else if (user.Password.Length < 8 || user.Password.Length > 30)
            {
                methodResult.AddErrorBadRequest("Mật khẩu phải có từ 8 đến 30 ký tự.");
                return methodResult;
            }
            else if (!Regex.IsMatch(user.Password, @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?!.*(.)\1{2,}).{8,30}$"))
            {
                methodResult.AddErrorBadRequest("Mật khẩu phải chứa ít nhất 1 chữ hoa, 1 chữ thường, 1 số, và không có ký tự trùng lặp liên tiếp quá 2 lần.");
                return methodResult;
            }

            methodResult.Result = true;
            return methodResult;
        }
    }
}
