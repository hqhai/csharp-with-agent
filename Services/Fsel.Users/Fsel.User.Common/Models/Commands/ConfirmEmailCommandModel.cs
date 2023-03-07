using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.User.Common.Models.Commands
{
    public class ConfirmEmailCommandModel
    {
        public string? Token { get; set; }
        public string? Email { get; set; }
    }
}