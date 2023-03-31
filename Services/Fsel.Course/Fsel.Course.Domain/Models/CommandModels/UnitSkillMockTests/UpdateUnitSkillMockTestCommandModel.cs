// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Models.CommandModels.UnitSkillMockTests
{
    public class UpdateUnitSkillMockTestCommandModel : BaseCommandModel
    {
        public Guid MockTestId { get; set; }

        public EnumMockTestType MockTestType { get; set; }
    }
}
