// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;

    public static class EnumReviewTypeHelper
    {
        private static IList<KeyValuePair<EnumReviewType, EnumReviewQuestionType>> s_reviewType = new List<KeyValuePair<EnumReviewType, EnumReviewQuestionType>>
        {
            new KeyValuePair<EnumReviewType, EnumReviewQuestionType>(EnumReviewType.Course, EnumReviewQuestionType.SATISFIEDTEACHER),
            new KeyValuePair<EnumReviewType, EnumReviewQuestionType>(EnumReviewType.Course, EnumReviewQuestionType.LEVELCHALLENGE),
            new KeyValuePair<EnumReviewType, EnumReviewQuestionType>(EnumReviewType.Course, EnumReviewQuestionType.COURSERELEVANT),
            new KeyValuePair<EnumReviewType, EnumReviewQuestionType>(EnumReviewType.Course, EnumReviewQuestionType.GAINEDKNOWLEDGE),
            new KeyValuePair<EnumReviewType, EnumReviewQuestionType>(EnumReviewType.Course, EnumReviewQuestionType.QUALITYPICTURE),
            new KeyValuePair<EnumReviewType, EnumReviewQuestionType>(EnumReviewType.Platform, EnumReviewQuestionType.INTERFACEFRIENDLY),
            new KeyValuePair<EnumReviewType, EnumReviewQuestionType>(EnumReviewType.Platform, EnumReviewQuestionType.FEATURESACCESSIBLE),
            new KeyValuePair<EnumReviewType, EnumReviewQuestionType>(EnumReviewType.Platform, EnumReviewQuestionType.PROCESSINGPLATFORM)
        };

        public static IList<object> GetEnumReviewTypes(this EnumReviewType? reviewType)
        {
            var results = new List<object>();
            foreach (var item in s_reviewType.Where(x => x.Key == reviewType))
            {
                var result = new
                {
                    Value = item.Value,
                    Name = item.Value.GetDescription()
                };

                results.Add(result);
            }
            return results;
        }
    }
}
