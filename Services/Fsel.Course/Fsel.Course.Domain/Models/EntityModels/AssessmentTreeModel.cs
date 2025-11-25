// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;
    using Fsel.Core.Base.BaseModels;

    public enum PartType
    {
        Part1 = 1,
        Part2 = 2,
        Part3 = 3
    }

    public enum SkillType
    {
        Listening,
        Reading,
        Grammar,
        Vocabulary
    }

    public class SkillItem : BaseModel
    {
        public SkillType Skill { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Score must be non-negative")]
        public int Score { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "MaxScore must be positive")]
        public int MaxScore { get; set; }

        public bool IsCompleted { get; set; }
    }

    public class PartNode : BaseModel
    {
        public PartType Part { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsExpanded { get; set; }
        public List<SkillItem> Skills { get; set; } = new();

        [JsonIgnore]
        public int TotalScore => Skills.Sum(s => s.Score);

        [JsonIgnore]
        public int TotalMaxScore => Skills.Sum(s => s.MaxScore);

        [JsonIgnore]
        public bool AllCompleted => Skills.All(s => s.IsCompleted);
    }

    public class AssessmentTreeModel : BaseModel
    {
        public List<PartNode> Parts { get; set; } = new();

        /// <summary>
        /// Creates sample data for testing (Part 1 with 15/20 scores and completed status)
        /// This is a simple factory method for testing purposes only
        /// </summary>
        public static AssessmentTreeModel CreateSampleForTesting()
        {
            var parts = new List<PartNode>();

            foreach (var partType in new[] { PartType.Part1, PartType.Part2, PartType.Part3 })
            {
                var isFirstPart = partType == PartType.Part1;
                parts.Add(new PartNode
                {
                    Id = Guid.NewGuid(),
                    Part = partType,
                    Title = $"Phần {(int)partType}",
                    IsExpanded = isFirstPart,
                    Skills = new List<SkillItem>
                    {
                        new() { Id = Guid.NewGuid(), Skill = SkillType.Listening, Score = isFirstPart ? 15 : 0, MaxScore = 20, IsCompleted = isFirstPart },
                        new() { Id = Guid.NewGuid(), Skill = SkillType.Reading, Score = isFirstPart ? 15 : 0, MaxScore = 20, IsCompleted = isFirstPart },
                        new() { Id = Guid.NewGuid(), Skill = SkillType.Grammar, Score = isFirstPart ? 15 : 0, MaxScore = 20, IsCompleted = isFirstPart },
                        new() { Id = Guid.NewGuid(), Skill = SkillType.Vocabulary, Score = isFirstPart ? 15 : 0, MaxScore = 20, IsCompleted = isFirstPart }
                    }
                });
            }

            return new AssessmentTreeModel
            {
                Id = Guid.NewGuid(),
                Parts = parts
            };
        }
    }
}