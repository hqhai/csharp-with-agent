using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Core.Base.BaseModels
{
    public class BaseEntityModel
    {
        public Guid Id { get; set; }

        public Guid CreatedUserId { get; set; }

        public Guid? UpdatedUserId { get; set; }

        public string? CreatedUserName { get; set; }

        public string? UpdatedUserName { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
