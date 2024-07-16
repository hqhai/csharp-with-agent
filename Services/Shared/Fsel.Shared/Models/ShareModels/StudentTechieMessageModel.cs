// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System.Text.Json.Serialization;
    using Fsel.Shared.Enums;

    public class StudentTechieMessageModel
    {
        public Guid StudentId { get; set; }

        public string? Message { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EnumTechieFeature Feature { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EnumTechieAction Action { get; set; }

        public int Priority { get; set; }

    }

    public class StudentTechieActionModel
    {
        public TechieConfig? Config { get; set; }

        public EnumTechieFeature Feature { get; set; }

        public EnumTechieAction Action { get; set; }
    }


    public class TechieConfig
    {
        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public string? Value { get; set; }
    }
}
