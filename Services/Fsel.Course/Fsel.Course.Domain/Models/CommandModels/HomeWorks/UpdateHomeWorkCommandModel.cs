// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Enums;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Models.CommandModels.Questions;

namespace Fsel.Course.Domain.Models.CommandModels.HomeWorks
{
    public class UpdateHomeWorkCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }

        public string? Code { get; set; }
        public string? MediaPost { get; set; }

        public bool IsActive { get; set; }
        public IList<UpdateQuestionCommandModel>? Questions { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
    }
}
