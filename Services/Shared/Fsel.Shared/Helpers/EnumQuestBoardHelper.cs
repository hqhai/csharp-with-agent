// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using Fsel.Shared.Enums;

    public static class EnumQuestBoardHelper
    {
        private static IList<KeyValuePair<EnumQuestBoardType, EnumQuestBoardCategory>> s_questboardTypeCategory = new List<KeyValuePair<EnumQuestBoardType, EnumQuestBoardCategory>>
        {
            new KeyValuePair<EnumQuestBoardType, EnumQuestBoardCategory>(EnumQuestBoardType.MainQuests, EnumQuestBoardCategory.FinishOnelesson),
            new KeyValuePair<EnumQuestBoardType, EnumQuestBoardCategory>(EnumQuestBoardType.MainQuests, EnumQuestBoardCategory.FinishOneHomeworkMiniProject),
            new KeyValuePair<EnumQuestBoardType, EnumQuestBoardCategory>(EnumQuestBoardType.MainQuests, EnumQuestBoardCategory.FinishOneUnitTest),
            new KeyValuePair<EnumQuestBoardType, EnumQuestBoardCategory>(EnumQuestBoardType.MainQuests, EnumQuestBoardCategory.FinishOneUnit),
            new KeyValuePair<EnumQuestBoardType, EnumQuestBoardCategory>(EnumQuestBoardType.MainQuests, EnumQuestBoardCategory.FinishTheFirstFinalTest),
            new KeyValuePair<EnumQuestBoardType, EnumQuestBoardCategory>(EnumQuestBoardType.MainQuests, EnumQuestBoardCategory.FinishTheFirstLevelPass),
            new KeyValuePair<EnumQuestBoardType, EnumQuestBoardCategory>(EnumQuestBoardType.MainQuests, EnumQuestBoardCategory.FinishOneFinalTest),
            new KeyValuePair<EnumQuestBoardType, EnumQuestBoardCategory>(EnumQuestBoardType.MainQuests, EnumQuestBoardCategory.FinishOneLevelPass),
        };

        public static IList<EnumQuestBoardCategory> GetEnumQuestBoardCategorys(this EnumQuestBoardType? questBoardType)
        {
            var questBoardCategorys = new List<EnumQuestBoardCategory>();
            if (questBoardType == null)
            {
                questBoardCategorys = s_questboardTypeCategory.Select(x => x.Value).ToList();
            }
            else
            {
                questBoardCategorys = s_questboardTypeCategory.Where(x => x.Key == questBoardType).Select(x => x.Value).ToList();
            }

            return questBoardCategorys;
        }
    }
}
