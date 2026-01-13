// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Ais
{
    using System.Text.Json;
    using System.Text.Json.Serialization;


    public class AiJsonResponseModel
    {
        [JsonPropertyName("feedback")] public JsonElement Feedback { get; set; }
    }


}
