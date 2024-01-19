// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;

    public static class HarmfulContentHelper
    {
        private static Dictionary<EnumHarmfulContent, int> s_wordSetting = new Dictionary<EnumHarmfulContent, int>()
        {
            { EnumHarmfulContent.Hate, 2 },
        };

        public static bool CheckHarmfulWords(ICollection<CategoriesAnalysisModel>? categories)
        {
            return CheckHarmfulContents(categories, s_wordSetting);
        }

        private static bool CheckHarmfulContents(ICollection<CategoriesAnalysisModel>? categories, Dictionary<EnumHarmfulContent, int> harmfulSetting)
        {
            return categories?.Any(p => p.Severity > (harmfulSetting.Any(x => x.Key.ToString() == p.Category) ? harmfulSetting.FirstOrDefault(x => x.Key.ToString() == p.Category).Value : default)) ?? default;
        }
    }
}
