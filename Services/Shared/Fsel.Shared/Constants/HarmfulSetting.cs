// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Constants
{
    using Fsel.Shared.Enums;

    public static class HarmfulSetting
    {
        public const string OutputType = "FourSeverityLevels";
        public const string ContentType = "application/json";
        public static readonly IList<string> Categories = new List<string> { EnumHarmfulContent.Hate.ToString(), EnumHarmfulContent.Sexual.ToString(), EnumHarmfulContent.SelfHarm.ToString(), EnumHarmfulContent.Violence.ToString() };
    }
}
