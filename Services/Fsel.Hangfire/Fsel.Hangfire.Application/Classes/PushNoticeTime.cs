namespace Fsel.Hangfire.Application.Classes
{
    using Fsel.Shared.Enums;

    public class PushNoticeTime
    {
        public EnumPushNoticeTimeType TimeNotifyType { get; set; }

        public PushNoticeTime(EnumPushNoticeTimeType timeNotifyType)
        {
            TimeNotifyType = timeNotifyType;
        }
    }
}
