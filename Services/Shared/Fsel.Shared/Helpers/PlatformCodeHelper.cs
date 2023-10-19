// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Fsel.Shared.Enums;

    public static class PlatformCodeHelper
    {
        private static IList<KeyValuePair<EnumGameVocabType, EnumPlatformCode>> s_gameVocabType = new List<KeyValuePair<EnumGameVocabType, EnumPlatformCode>>
        {
            new KeyValuePair<EnumGameVocabType, EnumPlatformCode>(EnumGameVocabType.Hint, EnumPlatformCode.PlanetDefender),
            new KeyValuePair<EnumGameVocabType, EnumPlatformCode>(EnumGameVocabType.Definition, EnumPlatformCode.PlanetDefender),
            new KeyValuePair<EnumGameVocabType, EnumPlatformCode>(EnumGameVocabType.Audio, EnumPlatformCode.PlanetDefender),
            new KeyValuePair<EnumGameVocabType, EnumPlatformCode>(EnumGameVocabType.Image, EnumPlatformCode.PlanetDefender),
            new KeyValuePair<EnumGameVocabType, EnumPlatformCode>(EnumGameVocabType.JumbledSpelling, EnumPlatformCode.PlanetDefender),
        };

        public static IList<EnumPlatformCode>? GetEnumPlatformCodes(this List<EnumGameVocabType> gameVocabType)
        {
            var results = new List<EnumPlatformCode>();
            foreach (var item in s_gameVocabType.Where(x => gameVocabType.Contains(x.Key)))
            {
                results.Add(item.Value);
            }
            return results.Distinct().ToList();
        }
    }
}
