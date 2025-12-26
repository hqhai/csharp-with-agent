// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.Test
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchTestConfigQueryModel : BaseQueryModel
    {
        public Guid? ProgramId { get; set; }
        public Guid? LevelId { get; set; }
        public EnumTestLayoutType? LayoutType { get; set; }
        public EnumVersion? Version { get; set; }
    }
}
