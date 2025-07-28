// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Implementations
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Application.Handlers.Interfaces;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Constants;
    using Microsoft.AspNetCore.Identity;

    public class ForgotPasswordHandler : IForgotPasswordHandler
    {
        private readonly IOtpPipelineFactory _otpPipelineFactory;
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;

        public ForgotPasswordHandler(IOtpPipelineFactory otpPipelineFactory,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IUserRepository userRepository)
        {
            _otpPipelineFactory = otpPipelineFactory;
            _userManager = userManager;
            _userRepository = userRepository;
        }

        public async Task<string> GetResetPasswordToken(string identity)
        {
            var user = await _userRepository.GetUserByIdentity(identity);
            if (user == null)
            {
                return null;
            }

            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<(bool, KeyValuePair<string, string>?)> SendOtpAsync(string identity, OtpProviderType otpProviderType = OtpProviderType.Sms)
        {
            if (identity.IsValidEmail())
            {
                otpProviderType = OtpProviderType.Email;
            }

            var sendOtpPipeline = _otpPipelineFactory.CreatePipeline(OtpStep.SendOtp);
            var sendOtpContext = new OtpPipelineContext(
                identity,
                OtpPurpose.ForgotPassword,
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

        public async Task<(bool, KeyValuePair<string, string>?)> VerifyOtpAsync(string identity, string otpCode)
        {
            var sendOtpPipeline = _otpPipelineFactory.CreatePipeline(OtpStep.VerifyOtp);
            var sendOtpContext = new OtpPipelineContext(
                identity,
                OtpPurpose.ForgotPassword,
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
    }
}
