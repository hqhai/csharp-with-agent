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

        public Guid? LevelId { get; set; }

        public EnumResultStatus? Status { get; set; }

        public Guid? SelectedProgramId { get; set; }

        public Guid? SelectedPtProgramId { get; set; }

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

        public EnumScoringFormulaType ScoringFormulaType { get; set; }
        public double? Score { get; set; }

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
            ScoringFormulaType = test.ScoringFormulaType;
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

            if (Children != null && Children.Any() && Children.All(c => c is SectionStateModel o && o.Order.HasValue))
            {
                Children = Children
                    .Cast<SectionStateModel>()
                    .OrderBy(s => s.Order)
                    .ToList<BaseTestStateModel>();
            }
        }
    }

    public class SectionStateModel : BaseTestStateModel
    {
        public string? Name { get; set; }
        public string? FilePath { get; set; }
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
        public double? ScoreModule { get; set; }
        public Guid? CurrentSectionTimeCodeId { get; set; }
        public IList<BaseTestStateModel> Children { get; set; } = new List<BaseTestStateModel>();
        public IList<SkillScores>? SkillScores { get; set; }

        public Guid? TestAnswerId { get; set; }
        public AnswerModel? Answer { get; set; }

        public int? Order { get; set; }

        public void UpdateDetailInfo(TestSection? section)
        {
            if (section == null)
            {
                return;
            }

            Order = section.DisplayOrder;
            Name = section.Name ?? section.Skill?.Name;
            FilePath = section.Skill?.FilePath;
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

            if (Children != null && Children.Any() && Children.All(c => c is SectionStateModel o && o.Order.HasValue))
            {
                Children = Children
                    .Cast<SectionStateModel>()
                    .OrderBy(s => s.Order)
                    .ToList<BaseTestStateModel>();
            }
        }
    }

    public class QuestionStateModel : BaseTestStateModel
    {
        public Guid? TestSectionId { get; set; }
        public Guid? QuestionId { get; set; }
        public Guid? TestAnswerId { get; set; }
        public int DisplayOrder { get; set; }
        public QuestionModel? Question { get; set; }
        public AnswerModel? Answer { get; set; }
    }
}
