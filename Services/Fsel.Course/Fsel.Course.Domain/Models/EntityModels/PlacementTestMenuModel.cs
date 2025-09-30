// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    // Model chính cho Tree structure theo business.md
    public class PlacementTestTreeModel : BaseModel
    {
        public Guid? StudentId { get; set; }
        public List<PlacementPartNode>? Parts { get; set; }
    }

    // Mỗi Part đại diện cho một PlacementTestResult
    public class PlacementPartNode
    {
        public int Part { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsExpanded { get; set; }

        // Dữ liệu từ PlacementTestResult entity (inherit từ BaseScoreResult)
        public double TotalQuestion { get; set; }
        public double CountQuestion { get; set; }
        public EnumPlacementTestLevel Level { get; set; }
        public Guid? PlacementTestId { get; set; }
        public Guid? PlacementTestGroupResultId { get; set; }
        public Guid PlacementTestResultId { get; set; }

        // Từ BaseResult
        public int CorrectCount { get; set; }
        public int CorrectTotal { get; set; }
        public double Percent { get; set; }
        public Guid StudentId { get; set; }

        // Từ BaseScoreResult - Raw SkillScores string
        public string? SkillScoresStr { get; set; }

        // SkillScores được parse từ SkillScoresStr theo chuẩn entity
        public IList<PlacementTestSkillScores>? SkillScores { get; set; }

        // Thông tin bổ sung
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool IsLock { get; set; }
    }

    // Model cho SkillScores JSON structure từ PlacementTestResult (theo format business.md)
    public class PlacementTestSkillScores
    {
        public EnumCourseSkill Skill { get; set; }
        public double Scores { get; set; }
        public int TotalCount { get; set; }
        public int CorrectCount { get; set; }
        public int TotalQuestion { get; set; }
        public int CountQuestion { get; set; }
        public double Percent { get; set; }
        public Guid? SectionGroupId { get; set; }
    }
}
