// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class LearningProgressWarningModel
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? SkillScore { get; set; }
        public int TotalPercent { get; set; }
        public Guid? UserId { get; set; }
    }
}
