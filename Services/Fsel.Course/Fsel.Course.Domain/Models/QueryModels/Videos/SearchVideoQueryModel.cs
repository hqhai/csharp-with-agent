// Copyright (c) Atlantic. All rights reserved.

using Fsel.Shared.Enums;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Models.QueryModels.Videos
{
    public class SearchVideoQueryModel : BaseQueryModel
    {
        public Guid? TeacherId { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public EnumTimeCodeType? TimeCodeType { get; set; }
    }
}
