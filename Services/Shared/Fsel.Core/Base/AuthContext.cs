using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Core.Base
{
    public class AuthContext
    {
        public Guid CurrentUserId { get; set; }
        public string? CurrentUsername { get; set; }
        public string? CurrentFullName { get; set; }
        public string? Email { get; set; }
    }
}