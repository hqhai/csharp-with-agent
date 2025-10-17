// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.CourseServices.QueryModel
{
    using Fsel.Shared.Enums;

    public class FeatureModuleQuery
    {
        public EnumFeatureModule FeatureModule { get; set; }

        public Guid ObjectId { get; set; }
        public Guid? UserId { get; set; }
    }
}
