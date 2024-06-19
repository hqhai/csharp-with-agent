// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.TokenConfigs
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class UpdateTokenConfigsCommandModel : BaseCommandModel
    {
        public IList<UpdateTokenConfigCommandModel>? TokenConfigs { get; set; }
    }

    public class UpdateTokenConfigCommandModel
    {
        public Guid Id { get; set; }
        public EnumTokenFeature Feature { get; set; }
        public EnumTokenMission Mission { get; set; }
        public EnumCourseType? CourseType { get; set; }
        public object? Config { get; set; }
    }
}
