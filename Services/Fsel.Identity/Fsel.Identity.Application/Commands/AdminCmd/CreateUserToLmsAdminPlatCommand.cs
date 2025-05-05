// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Commands.UserGroupCmd;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Users;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateUserToLmsAdminPlatCommand : CreateUserToLmsAdminPlatCommandModel, IRequest<MethodResult<UserModel>>
    {
    }

    public class CreateUserToLmsAdminPlatCommandHandler : IRequestHandler<CreateUserToLmsAdminPlatCommand, MethodResult<UserModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IPlatformRepository _platformRepository;
        private readonly IHumanRepository _humanRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly AppSetting _appSetting;
        private const string DEFAULT_PASSWORD = "Fsel@2025";

        public CreateUserToLmsAdminPlatCommandHandler(UserManager<User> userManager, IPlatformRepository platformRepository, IHumanRepository humanRepository, IMapper mapper, IMediator mediator, AppSetting appSetting)
        {
            _userManager = userManager;
            _platformRepository = platformRepository;
            _humanRepository = humanRepository;
            _mapper = mapper;
            _mediator = mediator;
            _appSetting = appSetting;
        }

        public async Task<MethodResult<UserModel>> Handle(CreateUserToLmsAdminPlatCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserModel>();

            #region Validate user
            User? user = null;

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                user = await _userManager.FindByEmailAsync(request.Email);
                if (user != null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicateEmail), nameof(request.Email), request.Email);
                    return methodResult;
                }
            }

            user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == request.UserName, cancellationToken: cancellationToken);
            if (user != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicateUsername), nameof(request.UserName), request.UserName);
                return methodResult;
            }

            user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber, cancellationToken: cancellationToken);
            if (user != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicatePhoneNumber), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            #endregion

            user = new();
            _mapper.Map(request, user);
            user.EmailConfirmed = true;

            // Gắn user vào LMS Admin platform
            var platform = await _platformRepository.GetPlatformAsync(EnumPlatformCode.LMSAdmin, cancellationToken);
            if (platform != null)
            {
                user.UserPlatforms.Add(new UserPlatform
                {
                    PlatformId = platform.Id
                });
            }

            await _humanRepository.ExecuteTransactionAsync(async () =>
            {
                Microsoft.AspNetCore.Identity.IdentityResult result;
                request.Password = request.Password ?? DEFAULT_PASSWORD;

                // Thêm user
                result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate), nameof(request.Password), request.Password);
                    return methodResult;
                }

                // Tạo Human
                var human = _mapper.Map<Human>(request);
                human.UserId = user.Id;
                human = _humanRepository.Add(human);

                if (request.UserGroupId.HasValue)
                {
                    var userGroupResult = await _mediator.Send(new AddUserToGroupCommand()
                    {
                        GroupId = request.UserGroupId.Value,
                        UserIds = new List<Guid> { user.Id }
                    }, cancellationToken);

                    if (!userGroupResult.IsOK)
                    {
                        methodResult.AddErrorBadRequest(userGroupResult.ErrorMessages);
                        return methodResult;
                    }
                }

                await _humanRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                // Có lẽ không cần thiết phải gửi mã OTP cho người dùng mới tạo

                //#region Send Code OTP

                //var userOtpCode = await _mediator.Send(new SaveUserOtpCodeCommand { Id = user.Id }, cancellationToken);
                //var param = new SendOtpTemplateModel
                //{
                //    OtpCode = userOtpCode.Result,
                //    AccessLink = string.Format(CultureInfo.InvariantCulture, _appSetting.ConstantUrl!.ConfirmOtpUrl!, userOtpCode.Result, user.Id),
                //    OtpValidTime = string.Format(CultureInfo.InvariantCulture, SenderSettings.OtpValidDay, _appSetting!.Otp!.StepDayWithAdmin)
                //};
                //var subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendOtpSubjectFullName, user.FullName);
                //var sendResult = new MethodResult<bool>();
                //if (!string.IsNullOrEmpty(request.Email))
                //{
                //    sendResult = await _mediator.Send(new SenderCommand { Email = user.Email, Subject = subject, Params = param, Template = EnumSenderTemplate.SendOtpAndLink }, cancellationToken).ConfigureAwait(false);
                //}

                //if (!sendResult.IsOK)
                //{
                //    methodResult.AddErrorBadRequest(sendResult?.ErrorMessages);
                //    return methodResult;
                //}

                //#endregion Send Code OTP

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<UserModel>(user);
                return methodResult;
            });

            return methodResult;
        }
    }
}
