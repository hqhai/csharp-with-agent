// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public static class CovertJsonHelper
    {
        public static JsonSerializerOptions CreateDefault()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            };

            options.Converters.Add(new JsonStringEnumConverter());

            return options;
        }
    }
}
