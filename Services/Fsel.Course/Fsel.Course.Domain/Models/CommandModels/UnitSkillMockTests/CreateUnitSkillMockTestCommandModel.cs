// Copyright (c) Atlantic. All rights reserved.

using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Models.CommandModels.UnitSkillMockTests
{
    public class CreateUnitSkillMockTestCommandModel
    {
        public Guid MockTestId { get; set; }

        public EnumMockTestType MockTestType { get; set; }
    }
}
