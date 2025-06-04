using Fsel.Shared.Enums;

namespace Fsel.Hangfire.Application.Classes
{
    public class NotifyTime
    {
        public EnumTimeNotifyType TimeNotifyType { get; set; }

        public NotifyTime(EnumTimeNotifyType timeNotifyType)
        {
            TimeNotifyType = timeNotifyType;
        }
    }
}
