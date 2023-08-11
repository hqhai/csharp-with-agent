// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class CancelScheduleLiveInfoModel : BaseModel
    {
        public string? ClassName { get; set; }
        public string? TeacherName { get; set; }
        public string? AccessLink { get; set; }
        public string? Description { get; set; }
        public bool IsWaitVote { get; set; }
        public IList<StudentInfoModel>? Students { get; set; }
        public IList<ClassLiveWorkFlowPlanModel>? ClassLiveWorkFlowPlans { get; set; }
    }
}
