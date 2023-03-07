namespace Fsel.User.Common.Models.Entities
{
    public class SendEmailModel
    {
        public List<string> To { get; set; }
        public string? Subject { get; set; }
        public string? Content { get; set; }

        public SendEmailModel(List<string> to, string? subject, string? content)
        {
            To = to;
            Subject = subject;
            Content = content;
        }
    }
}