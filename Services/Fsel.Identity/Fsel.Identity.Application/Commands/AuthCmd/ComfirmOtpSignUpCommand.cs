// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Auths;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class ComfirmOTPSignUpCommand : ConfirmOTPCommandModel, IRequest<MethodResult<ConfirmOtpModel>>
    {
    }

    public class ComfirmOTPSignUpCommandHandler : IRequestHandler<ComfirmOTPSignUpCommand, MethodResult<ConfirmOtpModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMediator _mediator;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly AppSetting _appSetting;
        private readonly IHumanRepository _humanRepository;
        private readonly IMapper _mapper;
        private readonly IParentRepository _parentRepository;

        public ComfirmOTPSignUpCommandHandler(UserManager<User> userManager
            , IMediator mediator
            , IUserOtpCodeRepository userOtpCodeRepository
            , AppSetting appSetting
            , IHumanRepository humanRepository
            , IMapper mapper
            , IParentRepository parentRepository)
        {
            _userManager = userManager;
            _mediator = mediator;
            _userOtpCodeRepository = userOtpCodeRepository;
            _appSetting = appSetting;
            _humanRepository = humanRepository;
            _mapper = mapper;
            _parentRepository = parentRepository;
        }

        public async Task<MethodResult<ConfirmOtpModel>> Handle(ComfirmOTPSignUpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(_appSetting.Otp);
            MethodResult<ConfirmOtpModel> methodResult = new MethodResult<ConfirmOtpModel>();
            User? user = new User();
            if (!string.IsNullOrEmpty(request.Email))
            {
                user = await _userManager.FindByEmailAsync(request.Email);
            }
            else
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.EmailNull), nameof(request.Email), request.Email);
                return methodResult;
            }

            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.EmailNotExist), nameof(request.Email), request.Email);
                return methodResult;
            }
            var userOtpCode = await _userOtpCodeRepository.Queryable
                        .FirstOrDefaultAsync(x => x.UserId == user.Id && x.Status == EnumStatusUser.New && !x.IsDeleted && x.OTPCode == request.OTP, cancellationToken);
            if (userOtpCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.InvalidOTP), nameof(request.Email), request.Email);
                return methodResult;
            }

            if (DateTime.Compare(DateTime.Now, userOtpCode.ExpiredTime) > 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthErrorCode.OTPExpired), nameof(request.OTP), request.OTP);
                return methodResult;
            }

            userOtpCode.Status = EnumStatusUser.Verified;
            _userOtpCodeRepository.Update(userOtpCode);
            await _userOtpCodeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _userManager.ConfirmEmailAsync(user, token);
            var roles = await _userManager.GetRolesAsync(user);

            var human = await CreateHuman(request, roles, user);
            await _humanRepository.ExecuteTransactionAsync(async () =>
            {
                human = _humanRepository.Add(human);
                await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                return methodResult;
            });
            var generateToken = await _mediator.Send(new GenerateTokenCommand { Id = user.Id }, cancellationToken).ConfigureAwait(false);
            var confirmOtp = new ConfirmOtpModel
            {
                AccessToken = generateToken!.Result!.AccessToken,
                Expiration = generateToken.Result.Expiration,
                FullName = generateToken.Result.FullName,
                RefreshToken = generateToken.Result.RefreshToken,
                Roles = generateToken.Result.Roles,
                UserId = user.Id
            };
            methodResult.Result = confirmOtp;
            return methodResult;
        }

        private async Task<Human> CreateHuman(ComfirmOTPSignUpCommand request, IList<string> roles, User user)
        {
            Human human = _mapper.Map<Human>(request);
            human.UserId = user.Id;
            var currentDate = DateTime.Now;
            var weekNumber = (currentDate.DayOfYear - 1) / 7 + 1;

            if (roles.Contains(EnumRoleRegister.Student.ToString()))
            {
                human.Student = new Student
                {
                    HumanId = human.Id,
                    CreatedByParent = false,
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
