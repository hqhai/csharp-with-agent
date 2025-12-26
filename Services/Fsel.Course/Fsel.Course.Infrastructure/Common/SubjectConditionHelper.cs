namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Course.Domain.Entities.SubjectConditionRuleConfigs;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;

    public class SubjectConditionHelper
    {
        public SubjectConditionHelper()
        {
        }

        public IList<SubjectConditionRule> SubjectConditionRuleHandler(IList<SubjectConditionRule> subjectConditionRules, Guid levelId, int age)
        {
            ArgumentNullException.ThrowIfNull(subjectConditionRules);
            IList<SubjectConditionRule> subjectConditionRuleResults = new List<SubjectConditionRule>();

            foreach (var subjectConditionRule in subjectConditionRules)
            {
                if (subjectConditionRule.ConditionRules == null || !subjectConditionRule.ConditionRules.Any() || subjectConditionRule.ConditionValues == null || !subjectConditionRule.ConditionValues.Any())
                {
                    continue;
                }

                var checkRule = CheckRule(subjectConditionRule.ConditionRules, levelId, age);
                if (!checkRule)
                {
                    continue;
                }

                subjectConditionRuleResults.Add(subjectConditionRule);
            }

            return subjectConditionRuleResults;
        }

        private static bool CheckRule(IList<ConditionRule> conditionRules, Guid levelId, int age)
        {
            bool isCheck = false;

            foreach (var conditionRule in conditionRules)
            {
                switch (conditionRule.OperatorType)
                {
                    case EnumOperatorType.LessThan:
                        isCheck = age < conditionRule.FromAge;
                        break;

                    case EnumOperatorType.GreaterThan:
                        isCheck = age > conditionRule.FromAge;
                        break;

                    case EnumOperatorType.GreaterThanEqual:
                        isCheck = age >= conditionRule.FromAge;
                        break;

                    case EnumOperatorType.LessThanEqual:
                        isCheck = age <= conditionRule.FromAge;
                        break;

                    case EnumOperatorType.Equal:
                        isCheck = age == conditionRule.FromAge;
                        break;

                    case EnumOperatorType.Between:
                        var minAge = Math.Min(conditionRule.FromAge ?? default, conditionRule.ToAge ?? default);
                        var maxAge = Math.Max(conditionRule.FromAge ?? default, conditionRule.ToAge ?? default);
                        isCheck = age >= minAge && age <= maxAge;
                        break;

                    case EnumOperatorType.Include:
                        isCheck = conditionRule.LevelIds != null && conditionRule.LevelIds.Any(x => x == levelId);
                        break;

                    case EnumOperatorType.Exclude:
                        isCheck = conditionRule.LevelIds != null && !conditionRule.LevelIds.Any(x => x == levelId);
                        break;
                }

                if (!isCheck)
                {
                    break;
                }
            }

            return isCheck;
        }
    }
}
