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
            { EnumHarmfulContent.Sexual, 0 },
            { EnumHarmfulContent.SelfHarm, 0 },
            { EnumHarmfulContent.Violence, 0 }
        };

        private static Dictionary<EnumHarmfulContent, int> s_imageSetting = new Dictionary<EnumHarmfulContent, int>()
        {
            { EnumHarmfulContent.Hate, 2 },
            { EnumHarmfulContent.Sexual, 2 },
            { EnumHarmfulContent.SelfHarm, 0 },
            { EnumHarmfulContent.Violence, 0 }
        };

        public static bool CheckHarmfulWords(ICollection<CategoriesAnalysisModel>? categories)
        {
            return CheckHarmfulContents(categories, s_wordSetting);
        }

        public static bool CheckHarmfulImages(ICollection<CategoriesAnalysisModel>? categories)
        {
            return CheckHarmfulContents(categories, s_imageSetting);
        }

        private static bool CheckHarmfulContents(ICollection<CategoriesAnalysisModel>? categories, Dictionary<EnumHarmfulContent, int> harmfulSetting)
        {
            return categories?.Any(p => p.Severity > harmfulSetting.FirstOrDefault(x => x.Key.ToString() == p.Category).Value) ?? default;
        }
    }
}
