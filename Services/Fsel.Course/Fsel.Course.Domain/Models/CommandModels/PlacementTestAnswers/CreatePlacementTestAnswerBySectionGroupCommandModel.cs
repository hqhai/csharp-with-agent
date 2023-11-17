// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.PlacementTestAnswers
{
    using System.Text.Json.Serialization;

    public class CreatePlacementTestAnswerBySectionGroupCommandModel
    {
        public double TotalQuestion { get; set; }
        public double CountQuestion { get; set; }
        public Guid PlacementTestResultId { get; set; }
        public Guid SectionGroupId { get; set; }
        public bool IsSubmit { get; set; }
        [JsonIgnore]
        public Guid? StudentId { get; set; }
        public IList<PlacementTestAnswerQuestionModel>? Answers { get; set; }
    }
}
