// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    using System.ComponentModel.DataAnnotations;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class ConfirmOtpSignUpCommand : IRequest<MethodResult<ConfirmOtpModel>>
    {
        [Required]
        public string? OTP { get; set; }
    }

    public class ConfirmOtpSignUpCommandHandler : IRequestHandler<ConfirmOtpSignUpCommand, MethodResult<ConfirmOtpModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMediator _mediator;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly AppSetting _appSetting;
        private readonly IHumanRepository _humanRepository;
        private readonly IMapper _mapper;
        private readonly IParentRepository _parentRepository;

        public ConfirmOtpSignUpCommandHandler(UserManager<User> userManager
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

        public async Task<MethodResult<ConfirmOtpModel>> Handle(ConfirmOtpSignUpCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(_appSetting.Otp);
            MethodResult<ConfirmOtpModel> methodResult = new MethodResult<ConfirmOtpModel>();

            var userOtpCode = await _userOtpCodeRepository.Queryable.Include(x => x.User)
                        .FirstOrDefaultAsync(x => x.Status == EnumOtpCodeStatus.New && x.OTPCode == request.OTP, cancellationToken);
            if (userOtpCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.OTP));
                return methodResult;
            }

            if (DateTime.Compare(DateTime.UtcNow, userOtpCode.ExpiredTime) > 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.OTPExpired), nameof(request.OTP), request.OTP);
                return methodResult;
            }
            var user = userOtpCode.User;
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }

            userOtpCode.Status = EnumOtpCodeStatus.Verified;
            _userOtpCodeRepository.Update(userOtpCode);
            await _userOtpCodeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _userManager.ConfirmEmailAsync(user, token);
            var roles = await _userManager.GetRolesAsync(user);

            var human = await CreateHuman(roles, user);
            if (!human.IsValid())
            {
                methodResult.AddError(human.ErrorMessages);
                return methodResult;
            }
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

        private async Task<Human> CreateHuman(IList<string> roles, User user)
        {
            Human human = _mapper.Map<Human>(user);
            human.UserId = user.Id;
            var currentDate = DateTime.UtcNow;
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
