namespace Fsel.Shared.Models.ShareModels
{
    public class SendSMSByZaloCommandModel
    {
        public IList<string>? PhoneNumbers { get; set; }
        public int Type { get; set; }
        public string? TemplateId { get; set; }
        public object? Params { get; set; }
        public int UseUnicode { get; set; }
    }
}
