// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Constants
{
    public static class OtpSetting
    {
        public static TimeSpan OtpBlockDuration = TimeSpan.FromMinutes(15);

        public static TimeSpan OtpLifeTimeDuration = TimeSpan.FromMinutes(3);

        public static TimeSpan MinimumBetweenTwoSendsDuration = TimeSpan.FromSeconds(60);

        public static TimeSpan BlockSendOtpDuration = TimeSpan.FromMinutes(15);

        public static int MaxCountOtpSend = 3;

        public static int MaxCountVerifyFail = 5;
    }
}
