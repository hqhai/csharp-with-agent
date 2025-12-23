// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.PlacementTestModels
{
    using System.Text.Json.Serialization;
    using Entities.TestConfigs;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Newtonsoft.Json;
    using Shared.Enums;

    public class PtStateModel
    {
        public Guid? FlowId { get; set; }

        public Guid? StudentId { get; set; }

        public Guid? TestGroupResultId { get; set; }

        public string? Level { get; set; }

        public EnumResultStatus? Status { get; set; }

        public ICollection<BaseTestStateModel> TestStates { get; set; } = new List<BaseTestStateModel>();
    }

    [JsonDerivedType(typeof(TestStateModel))]
    [JsonDerivedType(typeof(SectionStateModel))]
    [JsonDerivedType(typeof(QuestionStateModel))]
    public class BaseTestStateModel
    {
        public EnumResultStatus Status { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class TestStateModel : BaseTestStateModel
    {
        public string? Name { get; set; }

        [JsonProperty("ModuleId")] public Guid? StepFlowId { get; set; }

        public Guid? TestId { get; set; }

        public Guid? TestResultId { get; set; }

        public double PercentResult { get; set; }

        public List<BaseTestStateModel> Children { get; set; } = new List<BaseTestStateModel>();

        public void UpdateDetailInfo(Test? test)
        {
            if (test == null)
            {
                return;
            }

            Name = test.Name;

            foreach (var sectionResult in Children)
            {
                if (sectionResult is not SectionStateModel sectionStateModel)
                {
                    continue;
                }

                var section = test.TestSections.FirstOrDefault(s => s.Id == sectionStateModel.SectionId);
                if (section == null)
                {
                    continue;
                }

                sectionStateModel.UpdateDetailInfo(section);
            }
        }
    }

    public class SectionStateModel : BaseTestStateModel
    {
        public string? Name { get; set; }
        public Guid? SectionId { get; set; }
        public EnumTestLayoutType? TestLayoutType { get; set; }
        public TestSectionConfig? Config { get; set; }
        public Guid? SectionResultId { get; set; }
        public int CorrectCount { get; set; }
        public int CorrectTotal { get; set; }
        public double TotalCount { get; set; }
        public double? WorkingTime { get; set; }
        public double? PercentResult { get; set; }
        public int? HighestStreak { get; set; }
        public IList<BaseTestStateModel> Children { get; set; } = new List<BaseTestStateModel>();
        public IList<SkillScores>? SkillScores { get; set; }

        public int? Order { get; set; }

        public void UpdateDetailInfo(TestSection? section)
        {
            if (section == null)
            {
                return;
            }

            Order = section?.DisplayOrder;
            Name = section.Name ?? section.Skill?.Name;
            Config = section.Config;
            foreach (var sectionResult in Children)
            {
                if (sectionResult is not SectionStateModel sectionStateModel)
                {
                    continue;
                }

                var sectionMatch = section.TestSections.FirstOrDefault(s => s.Id == sectionStateModel.SectionId);
                if (sectionMatch == null)
                {
                    continue;
                }

                sectionStateModel.UpdateDetailInfo(sectionMatch);
            }
        }
    }

    public class QuestionStateModel : BaseTestStateModel
    {
        public Guid? QuestionId { get; set; }
        public Guid? TestAnswerId { get; set; }
        public QuestionModel? Question { get; set; }
        public AnswerModel? Answer { get; set; }
    }
}
