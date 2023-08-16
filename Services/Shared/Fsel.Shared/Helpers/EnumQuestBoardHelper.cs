// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using Fsel.Common.Helpers;
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

        public static IList<object> GetEnumQuestBoardTypes(this EnumQuestBoardType? questBoardType)
        {
            var results = new List<object>();
            foreach (var item in s_questboardTypeCategory.Where(x => x.Key == questBoardType))
            {
                var result = new
                {
                    Name = item.Value.GetDescription(),
                    Value = item.Value
                };

                results.Add(result);
            }
            return results;
        }
    }
}
