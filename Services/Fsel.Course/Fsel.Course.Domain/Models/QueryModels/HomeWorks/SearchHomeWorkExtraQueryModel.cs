// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.HomeWorks
{
    using Fsel.Core.Base.BaseModels;

    public class SearchHomeWorkExtraQueryModel : BaseQueryModel
    {
        public string? CourseLevelStr { get; set; }
        public string? CourseSkillStr { get; set; }
        public string? TopicIdStr { get; set; }
        public string? WorkFilterStr { get; set; }
    }
}
