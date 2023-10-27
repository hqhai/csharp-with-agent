// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.QueryModels.Units
{
    public class SearchUnitQueryModel : BaseQueryModel
    {
        public Guid? TeacherId { get; set; }

        public EnumCourseLevel? CourseLevel { get; set; }
    }
}
