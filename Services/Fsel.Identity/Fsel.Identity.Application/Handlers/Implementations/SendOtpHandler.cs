// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Implementations
{
    using System.Globalization;
    using System.Threading.Tasks;
    using Fsel.Common.Caching;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Application.Handlers.Interfaces;
    using Fsel.Identity.Application.Services.SenderService;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.SenderTemplates;
    using Microsoft.Extensions.Localization;

    public class SendOtpHandler : BaseOtpHandlerPipeline, IOtpHandlerPipeline<SendOtpHandler>
    {
        private readonly ICacheService<string> _cache;
        private readonly ISenderService _senderService;
        private readonly IStringLocalizer _stringLocalizer;
        private readonly AppSetting _appSetting;

        public SendOtpHandler(ICacheService<string> cache,
            ISenderService senderService,
            IStringLocalizer stringLocalizer,
            AppSetting appSetting)
        {
            _cache = cache;
            _senderService = senderService;
            _stringLocalizer = stringLocalizer;
            _appSetting = appSetting;
        }

        public override async Task Handle(OtpPipelineContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(context.Step, nameof(context.Step));
            ArgumentNullException.ThrowIfNull(context.Identity, nameof(context.Identity));
            ArgumentNullException.ThrowIfNull(context.OtpCacheKey, nameof(context.OtpCacheKey));
            ArgumentNullException.ThrowIfNull(context.OtpLifeTimeDuration, nameof(context.OtpLifeTimeDuration));

            var otp = GenerateHelper.GetOtp();
            context.Otp = otp;
            await _cache.SetAsync(context.OtpCacheKey, otp, context.OtpLifeTimeDuration);
            await SendOtpAsync(context.OtpProviderType, otp, context.Identity, context.OtpLifeTimeDuration);
            context.Status = true;
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
                    Params = new
                    {
                        CountOTP = 1,
                        OTP = otp,
                    },
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
                var param = new SendOtpTemplateModel
                {
                    OtpCode = otp,
                    OtpValidTime = string.Format(CultureInfo.InvariantCulture, SenderSettings.OtpValidDay, _appSetting!.Otp!.StepDayWithAdmin)
                };

                if (!string.IsNullOrEmpty(identity))
                {
                    var senderCommandModel = new SendEmailByTemplateCommandModel
                    {
                        Subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendOtpSubject),
                        Params = param,
                        Template = EnumSenderTemplate.SendOtp,
                        IsCCEmailDefault = false,
                        ToEmails = new List<string> { $"{identity}" }
                    };

                    await _senderService.SendEmailAsync(senderCommandModel);
                }
            }
        }
    }
}
