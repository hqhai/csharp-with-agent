// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;

    public static class HarmfulContentHelper
    {
        private class WordSetting
        {
            public const int Hate = 2;
            public const int Sexual = 0;
            public const int SelfHarm = 0;
            public const int Violence = 0;
        }

        private class ImageSetting
        {
            public const int Hate = 2;
            public const int Sexual = 2;
            public const int SelfHarm = 0;
            public const int Violence = 0;
        }

        public static bool CheckHarmfulContent(ICollection<CategoriesAnalysisModel>? categories, EnumHarmfulContentType contentType)
        {
            int settingHate = contentType == EnumHarmfulContentType.Word ? WordSetting.Hate : ImageSetting.Hate;
            int settingSexual = contentType == EnumHarmfulContentType.Word ? WordSetting.Sexual : ImageSetting.Sexual;
            int settingSelfHarm = contentType == EnumHarmfulContentType.Word ? WordSetting.SelfHarm : ImageSetting.SelfHarm;
            int settingViolence = contentType == EnumHarmfulContentType.Word ? WordSetting.Violence : ImageSetting.Violence;

            var hate = categories?.FirstOrDefault(p => p.Category == EnumHarmfulContent.Hate.ToString());
            if (hate != null && hate.Severity > settingHate)
            {
                return true;
            }
            var sexual = categories?.FirstOrDefault(p => p.Category == EnumHarmfulContent.Sexual.ToString());
            if (sexual != null && sexual.Severity > settingSexual)
            {
                return true;
            }
            var selfHarm = categories?.FirstOrDefault(p => p.Category == EnumHarmfulContent.SelfHarm.ToString());
            if (selfHarm != null && selfHarm.Severity > settingSelfHarm)
            {
                return true;
            }
            var violence = categories?.FirstOrDefault(p => p.Category == EnumHarmfulContent.Violence.ToString());
            if (violence != null && violence.Severity > settingViolence)
            {
                return true;
            }
            return false;
        }
    }
}
