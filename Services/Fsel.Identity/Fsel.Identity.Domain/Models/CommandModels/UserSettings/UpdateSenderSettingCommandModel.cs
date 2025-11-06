namespace Fsel.Identity.Domain.Models.CommandModels.UserSettings
{
    using Fsel.Shared.Enums;

    public class UpdateSenderSettingCommandModel
    {
        public Guid UserId { get; set; }

        public EnumSenderTemplate Template { get; set; }
    }
}
