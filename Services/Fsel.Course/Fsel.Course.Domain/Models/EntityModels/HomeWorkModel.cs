// Copyright (c) Atlantic. All rights reserved.

using Fsel.Shared.Enums;
using Fsel.Core.Base.BaseModels;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class HomeWorkModel : BaseModel
    {
        public string? Name { get; set; }

        public string? Code { get; set; }

        public string? MediaPost { get; set; }

        public IList<QuestionModel>? Questions { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }

        public IList<HomeWorkResultModel>? HomeWorkResults { get; set; }
    }
}
