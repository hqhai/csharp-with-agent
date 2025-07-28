// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.QueryModels.Videos
{
    public class SearchVideoQueryModel : BaseQueryModel
    {
        public Guid? TeacherId { get; set; }
        public Guid? ProgramId { get; set; }
        public Guid? LevelId { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public EnumTimeCodeType? TimeCodeType { get; set; }
    }
}
