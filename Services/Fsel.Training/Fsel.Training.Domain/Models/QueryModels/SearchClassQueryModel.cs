// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.QueryModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchClassQueryModel : BaseQueryModel
    {
        public EnumCourseLevel? Level { get; set; }
        public EnumClassStatus? Status { get; set; }
    }
}
