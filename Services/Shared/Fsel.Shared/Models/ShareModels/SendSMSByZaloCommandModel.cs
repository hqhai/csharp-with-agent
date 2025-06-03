namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class SendSMSByZaloCommandModel
    {
        public IList<string>? PhoneNumbers { get; set; }
        public int Type { get; set; }
        public EnumZaloTemplate Template { get; set; }
        public object? Params { get; set; }
        public int UseUnicode { get; set; }
    }
}
