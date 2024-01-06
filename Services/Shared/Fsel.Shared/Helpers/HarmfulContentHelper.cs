// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;

    public static class HarmfulContentHelper
    {
        public static bool HarmfulContentWords(ICollection<CategoriesAnalysisModel> categories)
        {
            var hate = categories.FirstOrDefault(p => p.Category == EnumHarmfulContent.Hate.ToString());
            if (hate != null && hate.Severity >= 2)
            {
                return true;
            }
            var sexual = categories.FirstOrDefault(p => p.Category == EnumHarmfulContent.Sexual.ToString());
            if (sexual != null && sexual.Severity > 0)
            {
                return true;
            }
            var selfHarm = categories.FirstOrDefault(p => p.Category == EnumHarmfulContent.SelfHarm.ToString());
            if (selfHarm != null && selfHarm.Severity >= 2)
            {
                return true;
            }
            var violence = categories.FirstOrDefault(p => p.Category == EnumHarmfulContent.Violence.ToString());
            if (violence != null && violence.Severity >= 2)
            {
                return true;
            }
            return false;
        }

        public static bool HarmfulContentImage(ICollection<CategoriesAnalysisModel> categories)
        {
            var hate = categories.FirstOrDefault(p => p.Category == EnumHarmfulContent.Hate.ToString());
            if (hate != null && hate.Severity >= 2)
            {
                return true;
            }
            var sexual = categories.FirstOrDefault(p => p.Category == EnumHarmfulContent.Sexual.ToString());
            if (sexual != null && sexual.Severity >= 2)
            {
                return true;
            }
            var selfHarm = categories.FirstOrDefault(p => p.Category == EnumHarmfulContent.SelfHarm.ToString());
            if (selfHarm != null && selfHarm.Severity >= 3)
            {
                return true;
            }
            var violence = categories.FirstOrDefault(p => p.Category == EnumHarmfulContent.Violence.ToString());
            if (violence != null && violence.Severity >= 2)
            {
                return true;
            }
            return false;
        }
    }
}
