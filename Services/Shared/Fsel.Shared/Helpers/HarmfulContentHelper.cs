// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;

    public static class HarmfulContentHelper
    {
        private class Setting
        {
            public int Hate { get; }
            public int Sexual { get; }
            public int SelfHarm { get; }
            public int Violence { get; }

            public Setting(int hate, int sexual, int selfHarm, int violence)
            {
                Hate = hate;
                Sexual = sexual;
                SelfHarm = selfHarm;
                Violence = violence;
            }
        }

        private static readonly Setting s_wordSetting = new Setting(2, 0, 0, 0);
        private static readonly Setting s_imageSetting = new Setting(2, 2, 0, 0);

        public static bool CheckHarmfulContent(ICollection<CategoriesAnalysisModel>? categories, EnumHarmfulContentType contentType)
        {
            var setting = contentType == EnumHarmfulContentType.Word ? s_wordSetting : s_imageSetting;

            return CheckCategory(categories, EnumHarmfulContent.Hate, setting.Hate)
                || CheckCategory(categories, EnumHarmfulContent.Sexual, setting.Sexual)
                || CheckCategory(categories, EnumHarmfulContent.SelfHarm, setting.SelfHarm)
                || CheckCategory(categories, EnumHarmfulContent.Violence, setting.Violence);
        }

        private static bool CheckCategory(ICollection<CategoriesAnalysisModel>? categories, EnumHarmfulContent harmfulContent, int settingValue)
        {
            var category = categories?.FirstOrDefault(p => p.Category == harmfulContent.ToString());
            return category != null && category.Severity > settingValue;
        }
    }
}
