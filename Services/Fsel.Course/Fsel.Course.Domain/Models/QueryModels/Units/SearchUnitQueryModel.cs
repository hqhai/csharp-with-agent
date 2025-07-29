// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;

namespace Fsel.Course.Domain.Models.QueryModels.Units
{
    public class SearchUnitQueryModel : BaseQueryModel
    {
        public Guid? TeacherId { get; set; }

        public Guid? CourseLevel { get; set; }
    }
}
