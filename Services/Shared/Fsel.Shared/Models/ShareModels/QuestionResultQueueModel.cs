// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System.Text.Json.Serialization;
    using Fsel.Shared.Enums;

    public class QuestionResultQueueModel
    {
        public Guid TResultId { get; set; }

        public Guid QuestionId { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EnumQuestionResultType Type { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EnumQuestionType QuestionType { get; set; }

        public object? Config { get; set; }
    }
}
