namespace Fsel.Sender.Common.Models.Entities
{
    public class SendEmailModel
    {
        public List<string> ToEmails { get; set; } = new List<string>();
        public List<string> BccEmails { get; set; } = new List<string>();
        public List<string> CcEmails { get; set; } = new List<string>();
        public string? Subject { get; set; }
        public string? Content { get; set; }
    }
}