// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Implementations
{
    using System.Threading.Tasks;
    using System.Transactions;
    using AutoMapper;
    using Fsel.Common.Caching;
    using Fsel.Identity.Application.Handlers.Interfaces;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.OpenId;
    using Fsel.Shared.Enums;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class UserRegisterHandler : IUserRegisterHandler
    {
        private readonly ICacheService<UserRegisterModel> _cacheService;
        private readonly IUserRepository _userRepository;
        private readonly IOtpPipelineFactory _otpPipelineFactory;
        private readonly Microsoft.AspNetCore.Identity.UserManager<User> _userManager;
        private readonly IUserOtpCodeRepository _userOtpRepository;
        private readonly IMapper _mapper;

        public UserRegisterHandler(ICacheService<UserRegisterModel> cacheService,
            IUserRepository userRepository,
            IOtpPipelineFactory otpPipelineFactory,
            Microsoft.AspNetCore.Identity.UserManager<User> userManager,
            IUserOtpCodeRepository userOtpRepository,
            IMapper mapper)
        {
            _cacheService = cacheService;
            _userRepository = userRepository;
            _otpPipelineFactory = otpPipelineFactory;
            _userManager = userManager;
            _userOtpRepository = userOtpRepository;
            _mapper = mapper;
        }

        public async Task<bool> TempRegisterUserAsync(UserRegisterModel userRegisterModel)
        {
            ArgumentNullException.ThrowIfNull(userRegisterModel, nameof(userRegisterModel));
            ArgumentNullException.ThrowIfNull(userRegisterModel.PhoneNumber, nameof(userRegisterModel.PhoneNumber));

            var isExist = await _userRepository.DbContext.Set<User>().AsQueryable().AsNoTracking()
               .AnyAsync(x => (x.PhoneNumber == userRegisterModel.PhoneNumber && x.PhoneNumberConfirmed)
               || (x.UserName == userRegisterModel.PhoneNumber && x.PhoneNumberConfirmed));

            if (isExist)
            {
                return false;
            }

            var cacheKey = GetKeyToCacheRegisterInfo(userRegisterModel.PhoneNumber);
            await _cacheService.SetAsync(cacheKey, userRegisterModel, TimeSpan.FromMinutes(30));

            return true;
        }

        public async Task<(bool, OtpSessionInfo)> SendRegisterOtpAsync(string phoneNumber, OtpProviderType otpProviderType = OtpProviderType.Zalo)
        {
            var sendOtpPipeline = _otpPipelineFactory.CreatePipeline(OtpStep.SendOtp);
            var sendOtpContext = new OtpPipelineContext(
                phoneNumber,
                OtpPurpose.Register,
                OtpStep.SendOtp)
            {
                OtpProviderType = otpProviderType
            };
            await sendOtpPipeline.Handle(sendOtpContext);
            if (sendOtpContext.OtpSessionInfo?.SendInfo != null)
            {
                sendOtpContext.OtpSessionInfo.SendInfo.IsJustSendLastTime = sendOtpContext.Status;
            }
            return (sendOtpContext.Status, sendOtpContext.OtpSessionInfo);
        }

        public async Task<(bool, OtpSessionInfo)> VerifyUserAsync(string phoneNumber, string otpCode)
        {
            var sendOtpPipeline = _otpPipelineFactory.CreatePipeline(OtpStep.VerifyOtp);
            var sendOtpContext = new OtpPipelineContext(
                phoneNumber,
                OtpPurpose.Register,
                OtpStep.VerifyOtp)
            {
                RequestOtp = otpCode
            };
            await sendOtpPipeline.Handle(sendOtpContext);
            return (sendOtpContext.Status, sendOtpContext.OtpSessionInfo);
        }

        public async Task<bool> CreateUserAsync(string phoneNumber, string otp)
        {
            var cacheKey = GetKeyToCacheRegisterInfo(phoneNumber);
            var cachedRegisterInfo = await _cacheService.GetAsync(cacheKey);
            if (cachedRegisterInfo == null)
            {
                return false;
            }
            var user = await _userRepository.DbContext.Set<User>().AsQueryable().FirstOrDefaultAsync(x => x.UserName == cachedRegisterInfo.PhoneNumber);
            await _userRepository.DbContext.Database.CreateExecutionStrategy().ExecuteAsync<IActionResult>(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                if (user != null)
                {
                    user.PhoneNumber = phoneNumber;
                    user.PhoneNumberConfirmed = true;
                    user.EmailConfirmed = true;
                    user.FirstName = cachedRegisterInfo.FirstName;
                    user.LastName = cachedRegisterInfo.LastName;
                    await _userManager.UpdateAsync(user);
                    await _userManager.AddPasswordAsync(user, cachedRegisterInfo.Password);
                }
                else
                {
                    user = _mapper.Map<User>(cachedRegisterInfo);
                    user.PhoneNumber = phoneNumber;
                    user.UserName = phoneNumber;
                    user.Id = Guid.NewGuid();
                    user.Email = $"Emaildefault_{Guid.NewGuid()}@fsel.openid";
                    user.PhoneNumberConfirmed = true;
                    user.EmailConfirmed = true;

                    user = await _userRepository.GenerateUserDataAsync(user, EnumRoleRegister.Student);
                    var result = await _userManager.CreateAsync(user, cachedRegisterInfo.Password ?? string.Empty);
                    result = await _userManager.AddToRoleAsync(user, EnumRoleRegister.Student.ToString());
                }

                var userOtpCode = new UserOtpCode
                {
                    UserId = user.Id,
                    OtpCode = otp,
                    Type = EnumUserOtpCodeType.Zalo,
                };
                _userOtpRepository.Add(userOtpCode);
                scope.Complete();
                return new ViewResult();
            });

            return true;
        }

        private static string GetKeyToCacheRegisterInfo(string indentity)
        {
            return $"Register_Info_{indentity}";
        }
    }
}
