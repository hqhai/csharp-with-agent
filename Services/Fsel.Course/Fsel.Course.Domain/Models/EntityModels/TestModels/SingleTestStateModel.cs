// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.TestModels
{
    using Entities.TestConfigs;
    using Enums;
    using PlacementTestModels;

    public class SingleTestStateModel
    {
        public Guid? StudentId { get; set; }

        public Guid? TestGroupResultId { get; set; }

        public string? Level { get; set; }

        public EnumResultStatus? Status { get; set; }

        public List<BaseTestStateModel> TestStates { get; set; } = new List<BaseTestStateModel>();
    }

    public class TestStateModel : BaseTestStateModel
    {
        public string? Name { get; set; }

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

        public TestSectionConfig? Config { get; set; }

        public Guid? SectionResultId { get; set; }

        public int CorrectCount { get; set; }

        public double TotalCount { get; set; }

        public List<BaseTestStateModel> Children { get; set; } = new List<BaseTestStateModel>();

        public double? WorkingTime { get; set; }

        public double? PercentResult { get; set; }

        public void UpdateDetailInfo(TestSection? section)
        {
            if (section == null)
            {
                return;
            }
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

        public AnswerModel Answer { get; set; }
    }
}
