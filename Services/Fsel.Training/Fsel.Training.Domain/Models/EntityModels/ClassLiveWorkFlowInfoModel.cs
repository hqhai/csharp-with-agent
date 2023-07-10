// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using Fsel.Training.Domain.Enums;

    public class ClassLiveWorkFlowInfoModel
    {
        public string? ClassName { get; set; }
        public string? TeacherName { get; set; }
        public string? Description { get; set; }
        public EnumWorkFlowType Type { get; set; }
        public IList<StudentInfoModel>? Students { get; set; }
    }
}
