// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Implementations
{
    using System.Threading.Tasks;
    using Fsel.Common.Caching;
    using Fsel.Common.Helpers;
    using Fsel.Core.Localization;
    using Fsel.Identity.Application.Handlers.Interfaces;
    using Fsel.Identity.Application.Services.SenderService;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Microsoft.Extensions.Localization;

    public class SendOtpHandler : BaseOtpHandlerPipeline, IOtpHandlerPipeline<SendOtpHandler>
    {
        private readonly ICacheService<string> _cache;
        private readonly ISenderService _senderService;
        private readonly IStringLocalizer _stringLocalizer;

        public SendOtpHandler(ICacheService<string> cache,
            ISenderService senderService,
            IStringLocalizer stringLocalizer)
        {
            _cache = cache;
            _senderService = senderService;
            _stringLocalizer = stringLocalizer;
        }

        public override async Task Handle(OtpPipelineContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(context.Step, nameof(context.Step));

            if (context.Step == OtpStep.SendOtp)
            {
                ArgumentNullException.ThrowIfNull(context.PhoneNumber, nameof(context.PhoneNumber));
                ArgumentNullException.ThrowIfNull(context.OtpCacheKey, nameof(context.OtpCacheKey));
                ArgumentNullException.ThrowIfNull(context.OtpLifeTimeDuration, nameof(context.OtpLifeTimeDuration));

                var otp = GenerateHelper.GetOtp();
                context.Otp = otp;
                await _cache.SetAsync(context.OtpCacheKey, otp, context.OtpLifeTimeDuration);
                await SendOtpAsync(context.OtpProviderType, otp, context.PhoneNumber, context.OtpLifeTimeDuration);
                context.Status = true;
            }

            if (Next != null)
            {
                await Next.Handle(context);
            }
        }

        private async Task SendOtpAsync(OtpProviderType otpProviderType,
            string otp,
            string identity,
            TimeSpan expiredTime)
        {
            if (otpProviderType == OtpProviderType.Sms)
            {
                await _senderService.SendSMSAsync(new Shared.Models.ShareModels.SendSMSCommandModel
                {
                    PhoneNumbers = new List<string> { identity },
                    Template = EnumSendSMSTemplate.SendOTP,
                    Content = _stringLocalizer["i18n_OTP_notify"].Value.InjectParam(otp, expiredTime.Minutes.ToString()),
                });
            }
            else if (otpProviderType == OtpProviderType.Zalo)
            {
                await _senderService.SendSMSWithZaloAsync(new Shared.Models.ShareModels.SendSMSByZaloCommandModel
                {
                    PhoneNumbers = new List<string> { identity },
                    Type = 1,
                    Template = EnumZaloTemplate.OTP,
                    Params = new
                    {
                        otp = otp
                    },
                    UseUnicode = 0
                });
            }
            else
            {
                throw new NotSupportedException($"OTP provider type '{otpProviderType}' is not supported.");
            }
        }
    }
}
