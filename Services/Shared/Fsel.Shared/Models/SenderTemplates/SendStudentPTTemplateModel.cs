// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.SenderTemplates
{
    using Fsel.Shared.Enums;

    public class SendStudentPTTemplateModel
    {
        public EnumCourseLevel CourseLevel { get; set; }

        public string? StudentName { get; set; }
    }
}
