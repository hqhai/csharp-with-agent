// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Commands.AuthCmd;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Users;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class ComfirmOTPWithAdminCommand : ConfirmOTPWithAdminCommandModel, IRequest<MethodResult<ConfirmOtpModel>>
    {
    }

    public class ComfirmOTPWithAdminCommandHandler : IRequestHandler<ComfirmOTPWithAdminCommand, MethodResult<ConfirmOtpModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMediator _mediator;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly AppSetting _appSetting;
        private readonly IHumanRepository _humanRepository;
        private readonly IMapper _mapper;
        private readonly ICSORepository _cSORepository;
        private readonly ITeacherRepository _teacherRepository;

        public ComfirmOTPWithAdminCommandHandler(UserManager<User> userManager
            , IMediator mediator
            , IUserOtpCodeRepository userOtpCodeRepository
            , AppSetting appSetting
            , IHumanRepository humanRepository
            , IMapper mapper
            , ICSORepository cSORepository
            , ITeacherRepository teacherRepository)
        {
            _userManager = userManager;
            _mediator = mediator;
            _userOtpCodeRepository = userOtpCodeRepository;
            _appSetting = appSetting;
            _humanRepository = humanRepository;
            _mapper = mapper;
            _cSORepository = cSORepository;
            _teacherRepository = teacherRepository;
        }

        public async Task<MethodResult<ConfirmOtpModel>> Handle(ComfirmOTPWithAdminCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(_appSetting.Otp);
            var methodResult = new MethodResult<ConfirmOtpModel>();
            var user = new User();
            if (request.Email != null)
            {
                user = await _userManager.FindByEmailAsync(request.Email);
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

            if (request.Role == EnumRoleRegisterWithAdmin.Teacher || request.Role == EnumRoleRegisterWithAdmin.CSO)
            {
                var human = await CreateHuman(request, user);
                await _humanRepository.ExecuteTransactionAsync(async () =>
                {
                    human = _humanRepository.Add(human);
                    await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    return methodResult;
                });
            }

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
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<Human> CreateHuman(ComfirmOTPWithAdminCommand request, User user)
        {
            var human = _mapper.Map<Human>(request);
            human.UserId = user.Id;
            if (request.Role == EnumRoleRegisterWithAdmin.Teacher)
            {
                var stt = await _teacherRepository.Queryable.CountAsync();
                human.Teacher = new Teacher
                {
                    HumanId = human.Id,
                    CourseLevels = request.CourseLevels,
                    CourseTypes = request.CourseTypes
                };
                human.Code = $"TC_{stt:0000}";
            }
            else if (request.Role == EnumRoleRegisterWithAdmin.CSO)
            {
                var stt = await _cSORepository.Queryable.CountAsync();
                human.CSO = new CSO
                {
                    HumanId = human.Id,
                    CourseLevels = request.CourseLevels,
                    CourseTypes = request.CourseTypes
                };
                human.Code = $"CSO_{stt:0000}";
            }
            return human;
        }
    }
}
