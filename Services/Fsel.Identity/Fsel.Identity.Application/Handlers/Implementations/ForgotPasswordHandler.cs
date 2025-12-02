// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Implementations
{
    using System.Threading.Tasks;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Application.Handlers.Interfaces;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Microsoft.AspNetCore.Identity;

    public class ForgotPasswordHandler : IForgotPasswordHandler
    {
        private readonly IOtpPipelineFactory _otpPipelineFactory;
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;

        public ForgotPasswordHandler(IOtpPipelineFactory otpPipelineFactory,
            UserManager<User> userManager,
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

        public async Task<(bool, OtpSessionInfo)> SendOtpAsync(string identity, OtpProviderType otpProviderType)
        {
            if (identity.IsValidEmail())
            {
                otpProviderType = OtpProviderType.Email;
            }

            var sendOtpPipeline = _otpPipelineFactory.CreatePipeline(OtpStep.SendOtp);
            var sendOtpContext = new OtpPipelineContext(
                identity,
                OtpPurpose.Forgot,
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

        public async Task<(bool, OtpSessionInfo)> VerifyOtpAsync(string identity, string otpCode)
        {
            var sendOtpPipeline = _otpPipelineFactory.CreatePipeline(OtpStep.VerifyOtp);
            var sendOtpContext = new OtpPipelineContext(
                identity,
                OtpPurpose.Forgot,
                OtpStep.VerifyOtp)
            {
                RequestOtp = otpCode,
            };
            await sendOtpPipeline.Handle(sendOtpContext);
            return (sendOtpContext.Status, sendOtpContext.OtpSessionInfo);
        }
    }
}
