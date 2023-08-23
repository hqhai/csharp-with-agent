// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.CommandModels.Auths;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    public class ConFirmOTPResetPasswordCommand : ComfirmOTPResetPasswordCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class ConFirmOTPResetPasswordCommandHandler : IRequestHandler<ConFirmOTPResetPasswordCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly IMapper _mapper;
        private readonly IHumanRepository _humanRepository;
        private readonly IParentRepository _parentRepository;

        public ConFirmOTPResetPasswordCommandHandler(UserManager<User> userManager, IUserOtpCodeRepository userOtpCodeRepository, IMapper mapper, IHumanRepository humanRepository, IParentRepository parentRepository)
        {
            _userManager = userManager;
            _userOtpCodeRepository = userOtpCodeRepository;
            _mapper = mapper;
            _humanRepository = humanRepository;
            _parentRepository = parentRepository;
        }

        public async Task<MethodResult<bool>> Handle(ConFirmOTPResetPasswordCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            if (string.IsNullOrEmpty(request.NewPassword))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.NewPassword));
                return methodResult;
            }
            if (string.IsNullOrEmpty(request.Otp))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Otp));
                return methodResult;
            }

            var user = await _userManager.Users.Include(x => x.UserOtpCodes)
                                .FirstOrDefaultAsync(x => x.UserOtpCodes.Where(x => x.Status == EnumStatusUser.New).Select(x => x.OTPCode).Contains(request.Otp), cancellationToken);

            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Otp));
                return methodResult;
            }

            var userOtpCode = await _userOtpCodeRepository.Queryable
                       .FirstOrDefaultAsync(x => x.UserId == user!.Id && x.Status == EnumStatusUser.New && !x.IsDeleted && x.OTPCode == request.Otp, cancellationToken);
            if (userOtpCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Otp));
                return methodResult;
            }

            if (DateTime.Compare(DateTime.Now, userOtpCode.ExpiredTime) > 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Otp));
                return methodResult;
            }

            userOtpCode.Status = EnumStatusUser.Verified;
            _userOtpCodeRepository.Update(userOtpCode);
            await _userOtpCodeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            if (!user.EmailConfirmed)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _userManager.ConfirmEmailAsync(user, token);
                var roles = await _userManager.GetRolesAsync(user);

                var human = await CreateHuman(roles, user);
                await _humanRepository.ExecuteTransactionAsync(async () =>
                {
                    human = _humanRepository.Add(human);
                    await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    return methodResult;
                });
            }

            var hashPassword = _userManager.PasswordHasher.HashPassword(user, request.NewPassword);
            user.PasswordHash = hashPassword;
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }

        private async Task<Human> CreateHuman(IList<string> roles, User user)
        {
            Human human = _mapper.Map<Human>(user);
            human.UserId = user.Id;
            var currentDate = DateTime.Now;
            var weekNumber = (currentDate.DayOfYear - 1) / 7 + 1;

            if (roles.Contains(EnumRoleRegister.Student.ToString()))
            {
                human.Student = new Student
                {
                    HumanId = human.Id,
                    CreatedByParent = false,
                    Occupation = "Student"
                };
            }
            else if (roles.Contains(EnumRoleRegister.Parent.ToString()))
            {
                var stt = await _parentRepository.Queryable.CountAsync();
                human.Parent = new Parent
                {
                    HumanId = human.Id,
                };
                human.Code = $"PH_{weekNumber}{stt:0000}";
            }
            return human;
        }
    }
}
