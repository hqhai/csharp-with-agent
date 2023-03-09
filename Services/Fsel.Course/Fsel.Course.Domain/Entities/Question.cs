using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Entities
{
    public class Question : Entity
    {
        public bool IsSave { get; set; }

        public EnumQuestion Type { get; set; }

        public string? Config { get; set; }
    }
}