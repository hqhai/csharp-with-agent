using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Sender.Common.Models.Commands
{
    public class SendEmailCommandModel
    {
        public List<string> ToEmails { get; set; } = new List<string>();
        public List<string> BccEmails { get; set; } = new List<string>();
        public List<string> CcEmails { get; set; } = new List<string>();
        public string? Subject { get; set; }
        public string? Content { get; set; }
    }
}