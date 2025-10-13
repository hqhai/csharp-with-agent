// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.PlacementTestModels
{
    using System.Text.Json.Serialization;
    using Fsel.Course.Domain.Enums;
    using Newtonsoft.Json;
    using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;

    public class PTStateModel
    {
        public Guid? FlowId { get; set; }

        public Guid? StudentId { get; set; }

        public Guid? TestGroupResultId { get; set; }

        public EnumResultStatus? Status { get; set; }

        public ICollection<BaseTestStateModel> TestStates { get; set; } = new List<BaseTestStateModel>();
    }

    [JsonDerivedType(typeof(TestStateModel))]
    [JsonDerivedType(typeof(SectionStateModel))]
    [JsonDerivedType(typeof(QuestionStateModel))]
    public class BaseTestStateModel
    {
        public EnumResultStatus Status { get; set; }
    }

    public class TestStateModel : BaseTestStateModel
    {
        [JsonProperty("ModuleId")]
        public Guid? StepFlowId { get; set; }

        public Guid? TestId { get; set; }

        public Guid? TestResultId { get; set; }

        public double PercentResult { get; set; }

        public List<BaseTestStateModel> Children { get; set; } = new List<BaseTestStateModel>();

        [JsonIgnore]
        public int StartPercent { get; set; }

        [JsonIgnore]
        public int ToPercent { get; set; }
    }

    public class SectionStateModel : BaseTestStateModel
    {
        public Guid? SectionId { get; set; }

        public Guid? SectionResultId { get; set; }

        public List<BaseTestStateModel> Children { get; set; } = new List<BaseTestStateModel>();
    }

    public class QuestionStateModel : BaseTestStateModel
    {
        public Guid? QuestionId { get; set; }

        public Guid? QuestionResultId { get; set; }

        public QuestionModel Question { get; set; }

        public AnswerModel Answer { get; set; }
    }
}
