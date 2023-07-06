// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class ChangeLiveSessionModel : BaseModel
    {
        public string? ClassName { get; set; }
        public Guid TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public Guid LiveTimeFrameId { get; set; }
        public double? StartTime { get; set; }
        public double? EndTime { get; set; }
        public DateTime LiveDate { get; set; }
        public string? Status { get; set; }
    }
}
