// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Implementations
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.Caching;
    using Fsel.Identity.Application.Handlers.Interfaces;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.OpenId;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Localization;

    public class UserRegisterHandler : IUserRegisterHandler
    {
        private readonly ICacheService<UserRegisterModel> _cacheService;
        private readonly IUserRepository _userRepository;
        private readonly IOtpPipelineFactory _otpPipelineFactory;
        private readonly Microsoft.AspNetCore.Identity.UserManager<User> _userManager;
        private readonly IUserOtpCodeRepository _userOtpRepository;
        private readonly IStringLocalizer _localizer;
        private readonly IMapper _mapper;

        public UserRegisterHandler(ICacheService<UserRegisterModel> cacheService,
            IUserRepository userRepository,
            IOtpPipelineFactory otpPipelineFactory,
            Microsoft.AspNetCore.Identity.UserManager<User> userManager,
            IUserOtpCodeRepository userOtpRepository,
            IStringLocalizer localizer,
            IMapper mapper)
        {
            _cacheService = cacheService;
            _userRepository = userRepository;
            _otpPipelineFactory = otpPipelineFactory;
            _userManager = userManager;
            _userOtpRepository = userOtpRepository;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<(bool, KeyValuePair<string, string>?)> TempRegisterUserAsync(UserRegisterModel userRegisterModel)
        {
            ArgumentNullException.ThrowIfNull(userRegisterModel, nameof(userRegisterModel));
            ArgumentNullException.ThrowIfNull(userRegisterModel.PhoneNumber, nameof(userRegisterModel.PhoneNumber));

            var isExist = await _userRepository.DbContext.Set<User>().AsQueryable().AsNoTracking()
               .AnyAsync(x => x.PhoneNumber == userRegisterModel.PhoneNumber);

            if (isExist)
            {
                return (false, new KeyValuePair<string, string>(nameof(EnumAuthUserErrorCode.DuplicatePhoneNumber),
                    _localizer[nameof(EnumAuthUserErrorCode.DuplicatePhoneNumber)]));
            }

            var cacheKey = GetKeyToCacheRegisterInfo(userRegisterModel.PhoneNumber);
            await _cacheService.SetAsync(cacheKey, userRegisterModel, TimeSpan.FromMinutes(30));

            return (true, default);
        }

        public async Task<(bool, KeyValuePair<string, string>?)> SendRegisterOtpAsync(string phoneNumber, OtpProviderType otpProviderType = OtpProviderType.Sms)
        {
            var sendOtpPipeline = _otpPipelineFactory.CreatePipeline(OtpStep.SendOtp);
            var sendOtpContext = new OtpPipelineContext(
                phoneNumber,
                OtpPurpose.Register,
                OtpStep.SendOtp)
            {
                OtpProviderType = otpProviderType,
                OtpBlockDuration = OtpSetting.OtpBlockDuration,
                OtpLifeTimeDuration = OtpSetting.OtpLifeTimeDuration,
                MinimumBetweenTwoSendsDuration = OtpSetting.MinimumBetweenTwoSendsDuration,
                BlockSendOtpDuration = OtpSetting.BlockSendOtpDuration,
                MaxCountOtpSend = OtpSetting.MaxCountOtpSend,
                MaxCountVerifyFail = OtpSetting.MaxCountVerifyFail,
            };
            await sendOtpPipeline.Handle(sendOtpContext);
            if (!sendOtpContext.Status)
            {
                return (false, sendOtpContext.ErrorMessage);
            }
            return (true, default);
        }

        public async Task<(bool, KeyValuePair<string, string>?)> VerifyUserAsync(string phoneNumber, string otpCode)
        {
            var sendOtpPipeline = _otpPipelineFactory.CreatePipeline(OtpStep.VerifyOtp);
            var sendOtpContext = new OtpPipelineContext(
                phoneNumber,
                OtpPurpose.Register,
                OtpStep.VerifyOtp)
            {
                OtpBlockDuration = OtpSetting.OtpBlockDuration,
                OtpLifeTimeDuration = OtpSetting.OtpLifeTimeDuration,
                MinimumBetweenTwoSendsDuration = OtpSetting.MinimumBetweenTwoSendsDuration,
                BlockSendOtpDuration = OtpSetting.BlockSendOtpDuration,
                MaxCountOtpSend = OtpSetting.MaxCountOtpSend,
                MaxCountVerifyFail = OtpSetting.MaxCountVerifyFail,
                RequestOtp = otpCode,
            };
            await sendOtpPipeline.Handle(sendOtpContext);
            if (!sendOtpContext.Status)
            {
                return (false, sendOtpContext.ErrorMessage);
            }
            return (true, default);
        }

        public async Task<(bool, KeyValuePair<string, string>?)> CreateUserAsync(string phoneNumber, string otp)
        {
            var cacheKey = GetKeyToCacheRegisterInfo(phoneNumber);
            var cachedRegisterInfo = await _cacheService.GetAsync(cacheKey);
            if (cachedRegisterInfo == null)
            {
                return (false, new KeyValuePair<string, string>(nameof(EnumAuthUserErrorCode.RegisterExpired),
                    _localizer[nameof(EnumAuthUserErrorCode.RegisterExpired)]));
            }

            var user = _mapper.Map<User>(cachedRegisterInfo);
            user.PhoneNumber = phoneNumber;
            user.UserName = phoneNumber;
            user.Id = Guid.NewGuid();
            user.Email = $"Emaildefault_{Guid.NewGuid()}@atlantic.edu.vn";
            user.PhoneNumberConfirmed = true;
            user.EmailConfirmed = true;
            await _userManager.CreateAsync(user, cachedRegisterInfo.Password ?? string.Empty);
            await _userManager.AddToRoleAsync(user, EnumRoleRegister.Student.ToString());

            var userOtpCode = new UserOtpCode
            {
                UserId = user.Id,
                OtpCode = otp,
                Type = EnumUserOtpCodeType.Zalo,
            };
            _userOtpRepository.Add(userOtpCode);
            await _userOtpRepository.UnitOfWork.SaveChangesAsync();

            return (true, default);
        }

        private static string GetKeyToCacheRegisterInfo(string indentity)
        {
            return $"Register_Info_{indentity}";
        }
    }
}
