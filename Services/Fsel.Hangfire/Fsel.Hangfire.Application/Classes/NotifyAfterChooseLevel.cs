using Fsel.Shared.Enums;

namespace Fsel.Hangfire.Application.Classes
{
    public class NotifyAfterChooseLevel
    {
        public EnumNotifyAfterChooseLevelType AfterChooseLevelType { get; set; }

        public NotifyAfterChooseLevel(EnumNotifyAfterChooseLevelType afterChooseLevelType)
        {
            AfterChooseLevelType = afterChooseLevelType;
        }
    }
}
