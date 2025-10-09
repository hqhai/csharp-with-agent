// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.PlacementTestModels
{
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using Newtonsoft.Json;

    public class PTStateModel
    {
        public Guid? FlowId { get; set; }

        public Guid? StudentId { get; set; }

        public Guid? TestGroupResultId { get; set; }

        public int NumberOfModules { get; set; }

        public EnumResultStatus Status { get; set; }

        public List<ModuleStateModel> Modules { get; set; } = new List<ModuleStateModel>();
    }

    public class ModuleStateModel
    {
        [JsonProperty("ModuleId")]
        public Guid? StepFlowId { get; set; }

        public Guid? TestId { get; set; }

        public Guid? TestResultId { get; set; }

        public int PercentResult { get; set; }

        public EnumResultStatus Status { get; set; }

        public List<SectionStateModel> Skills { get; set; } = new List<SectionStateModel>();

        [JsonIgnore]
        public int StartPercent { get; set; }

        [JsonIgnore]
        public int ToPercent { get; set; }
    }

    public class SectionStateModel
    {
        public Guid? SectionId { get; set; }

        public Guid? SectionResultId { get; set; }

        public EnumResultStatus Status { get; set; }

        public List<SectionStateModel> ChildSections { get; set; } = new List<SectionStateModel>();

        public List<QuestionStateModel> Questions { get; set; } = new List<QuestionStateModel>();
    }

    public class QuestionStateModel
    {
        public Guid? QuestionId { get; set; }

        public Guid? QuestionResultId { get; set; }

        public QuestionModel Question { get; set; }

        public AnswerModel Answer { get; set; }

        public EnumAnswerStatus Status { get; set; }
    }
}
