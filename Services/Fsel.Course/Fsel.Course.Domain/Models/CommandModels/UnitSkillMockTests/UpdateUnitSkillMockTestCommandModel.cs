using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Domain.Models.CommandModels.UnitSkillMockTests
{
    public class UpdateUnitSkillMockTestCommandModel : BaseCommandModel
    {
        public Guid MockTestId { get; set; }

        public EnumMockTestType MockTestType { get; set; }
    }
}
