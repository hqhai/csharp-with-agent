namespace Fsel.Sender.Domain.Models.Entities
{
    public class SendEmailModel
    {
        public List<string>? ToEmails { get; set; }
        public List<string>? BccEmails { get; set; }
        public List<string>? CcEmails { get; set; }
        public string? Subject { get; set; }
        public string? Content { get; set; }
    }
}
