// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.PlacementTestModels
{
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

        public List<SkillStateModel> Skills { get; set; } = new List<SkillStateModel>();

        [JsonIgnore]
        public int StartPercent { get; set; }

        [JsonIgnore]
        public int ToPercent { get; set; }
    }

    public class SkillStateModel
    {
        [JsonProperty("SkillId")]
        public Guid? SectionId { get; set; }

        [JsonProperty("SkillResultId")]
        public Guid? SectionResultId { get; set; }

        public EnumResultStatus Status { get; set; }

        public List<ExcerciseStateModel> Exercises { get; set; } = new List<ExcerciseStateModel>();
    }

    public class ExcerciseStateModel
    {
        [JsonProperty("ExerciseId")]
        public Guid? SectionId { get; set; }

        [JsonProperty("ExerciseResultId")]
        public Guid? SectionResultId { get; set; }

        public EnumResultStatus Status { get; set; }

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
