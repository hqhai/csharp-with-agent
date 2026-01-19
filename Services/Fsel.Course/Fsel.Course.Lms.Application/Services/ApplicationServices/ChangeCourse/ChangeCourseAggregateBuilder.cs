// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.ChangeCourse
{
    using Domain.Entities;
    using Domain.Entities.SubjectConditionRuleConfigs;
    using Domain.Entities.TestConfigs;
    using Domain.Enums;
    using Domain.Models.EntityModels.ChangeCourseModels;
    using UserServices.Models;
    using Shared.Enums;
    using Shared.Helpers;

    public class ChangeCourseAggregateBuilder
    {
        private readonly ICollection<Category> _subjects;
        private readonly CourseResult? _currentCourseResult;
        private ChangeCourseAggregate? _changeCourseAggregate;
        private ICollection<TestGroupResult>? _testGroupResults;
        private IList<CourseResult>? _courseResults;
        private readonly ICollection<CourseChangingHistory>? _courseChangingHistories;
        private readonly UserModel? _user;

        public ChangeCourseAggregateBuilder(ICollection<Category> subjects,
            ICollection<TestGroupResult>? testGroupResults,
            ICollection<CourseChangingHistory>? courseChangingHistories,
            CourseResult? currentCourseResult,
            UserModel? user,
            IList<CourseResult>? courseResults = null)
        {
            _subjects = subjects;
            _currentCourseResult = currentCourseResult;
            _testGroupResults = testGroupResults ?? new List<TestGroupResult>();
            _courseChangingHistories = courseChangingHistories;
            _courseResults = courseResults;
            _user = user;
        }

        public ChangeCourseAggregateBuilder BuildTree(Guid? targetLevelId)
        {
            var subjectComponents = new List<SubjectChangeCourse>();
            foreach (var subject in _subjects)
            {
                if (BuildItem(subject) is SubjectChangeCourse component)
                {
                    subjectComponents.Add(component);
                }
            }

            if (targetLevelId.HasValue)
            {
                subjectComponents = subjectComponents.Where(s => s.Contain(targetLevelId.Value)).ToList();
                if (!subjectComponents.Any())
                {
                    throw new ArgumentException("Target level not found in subjects");
                }

                subjectComponents.ForEach(component => component.TrimBranchNotRelated(targetLevelId.Value));
            }

            _changeCourseAggregate = new ChangeCourseAggregate(subjectComponents);
            return this;
        }

        public ChangeCourseAggregateBuilder BuildProgramInfo()
        {
            ArgumentNullException.ThrowIfNull(_changeCourseAggregate);

            var promgramComponents = _changeCourseAggregate.RootSubjects.SelectMany(x => x.GetComponentsByType<ProgramChangeCourse>());
            foreach (var program in promgramComponents)
            {
                if (program.TestMode == EnumTestMode.Not)
                {
                    var ptTestResult = _testGroupResults?.Where(h => h.ProgramId == program.Id)
                                                           .OrderByDescending(x => x.CreatedDate)
                                                           .FirstOrDefault();
                    program.PtResultId = ptTestResult?.Id;
                }
                else
                {
                    var ptTestResult = _testGroupResults?.Where(h => h.ProgramId == program.Id)
                                                           .OrderByDescending(x => x.CurrentLevel?.LevelOrder ?? 0)
                                                           .FirstOrDefault();
                    program.PtResultId = ptTestResult?.Id;
                }
            }

            return this;
        }

        public ChangeCourseAggregateBuilder BuildLevelInfomation()
        {
            ArgumentNullException.ThrowIfNull(_changeCourseAggregate);

            var levelComponents = _changeCourseAggregate.RootSubjects.SelectMany(x => x.GetComponentsByType<LevelChangeCourse>()).ToList();
            var currentLearningLevel = _currentCourseResult?.Course?.LevelId;

            foreach (var levelComponent in levelComponents)
            {
                if (currentLearningLevel == levelComponent.LevelId)
                {
                    levelComponent.IsCurrentLearningLevel = true;
                    levelComponent.CourseResultId = _currentCourseResult?.Id;
                }

                var latestHistoryMatch = _courseChangingHistories?.Where(h => h.SelectedLevelId == levelComponent.LevelId)
                    .OrderByDescending(x => x.CreatedDate)
                    .FirstOrDefault();

                if (latestHistoryMatch != null)
                {
                    levelComponent.CourseResultId ??= latestHistoryMatch.ToCourseResultId;
                }

                if (levelComponent.CourseResultId == null)
                {
                    levelComponent.CourseResultId = _courseResults?.Where(cr => cr.Course?.LevelId == levelComponent.LevelId)
                        .OrderByDescending(x => x.CreatedDate)
                        .Select(x => x.Id)
                        .FirstOrDefault();
                }
            }

            return this;
        }

        public async Task BuildLevelAccess(Func<Guid, Task<ICollection<SubjectConditionRule>>> getSubjectConditionRules)
        {
            ArgumentNullException.ThrowIfNull(_changeCourseAggregate);

            var projects = _changeCourseAggregate.RootSubjects.SelectMany(x => x.GetComponentsByType<SubjectChangeCourse>())
                .Where(x => x.Children.All(c => c is ProgramChangeCourse));

            foreach (var project in projects)
            {
                if (project == null)
                {
                    return;
                }

                var rules = getSubjectConditionRules != null ? await getSubjectConditionRules.Invoke(project.Id) : new List<SubjectConditionRule>();

                var conditionValues = new List<ConditionValue>();

                var ptResultWithHighestLevel = _testGroupResults.Where(x => x.CurrentLevel != null)
                    .GroupBy(g => g.ProgramId)
                    .Select(g => g.OrderByDescending(t => t.CurrentLevel?.LevelOrder ?? 0)
                    .First())
                    .ToList();

                foreach (var ptResult in ptResultWithHighestLevel)
                {
                    conditionValues = conditionValues.Concat(rules.GetMatchConditionRules(DateTimeHelper.GetYearOld(_user.Birthday), ptResult.CurrentLevelId.Value)
                        .SelectMany(x => x.ConditionValues ?? new List<ConditionValue>())).ToList();
                }

                foreach (var levelComponent in _changeCourseAggregate.RootSubjects.SelectMany(x => x.GetComponentsByType<LevelChangeCourse>()).ToList())
                {
                    if (!levelComponent.CanSelect)
                    {
                        levelComponent.CanAccess = conditionValues.GetMatchConditionValue(levelComponent.LevelId) != null;
                    }
                }

                var promgramComponents = project.GetComponentsByType<ProgramChangeCourse>();
                foreach (var program in promgramComponents)
                {
                    var levelComponents = program.GetComponentsByType<LevelChangeCourse>();
                    foreach (var levelComponent in levelComponents)
                    {
                        var existHigherLevelCanSelect = levelComponents.Any(l => l.LevelId != levelComponent.LevelId && l.CanSelect && levelComponent.LevelOrder <= l.LevelOrder);
                        if (existHigherLevelCanSelect)
                        {
                            levelComponent.CanAccess = true;
                        }
                    }
                }
            }
        }

        public ChangeCourseAggregate? Build()
        {
            return _changeCourseAggregate;
        }

        private static ChangeCourseComponent BuildItem(Category category)
        {
            if (category.Type == EnumTypeCategory.Subject)
            {
                var subjectComponent = new SubjectChangeCourse { Id = category.Id, Children = new List<ChangeCourseComponent>(), Name = category.Name };

                foreach (var subject in category.Categorys)
                {
                    subjectComponent.Children.Add(BuildItem(subject));
                }

                return subjectComponent;
            }
            else
            {
                var programComponent = new ProgramChangeCourse
                {
                    Id = category.Id,
                    Name = category?.Name,
                    TestMode = category.TestMode ?? EnumTestMode.Not,
                    IsDefaultForTest = category.IsTestDefault,
                    Children = new List<ChangeCourseComponent>()
                };

                foreach (var level in category.Levels.OrderBy(x => x.LevelOrder))
                {
                    programComponent.Children.Add(new LevelChangeCourse { Name = level?.Name, LevelId = level.Id, LevelOrder = level.LevelOrder, CanAccess = category.TestMode == EnumTestMode.Not });
                }

                return programComponent;
            }
        }
    }

    public static class ChangeCourseAggregateBuilderExtensions
    {
        public static void TrimBranchNotRelated(this SubjectChangeCourse subject, Guid targetLevelId)
        {
            ArgumentNullException.ThrowIfNull(subject);

            var childSubjects = subject.Children.OfType<SubjectChangeCourse>().ToList();
            foreach (var childSubject in childSubjects)
            {
                childSubject.TrimBranchNotRelated(targetLevelId);
                if (!childSubject.Contain(targetLevelId))
                {
                    subject.Children.Remove(childSubject);
                }
            }
        }

        public static ConditionValue? GetMatchConditionValue(this IList<ConditionValue>? conditionValues, Guid levelId)
        {
            if (conditionValues == null)
            {
                return null;
            }

            return conditionValues.FirstOrDefault(x => x.LevelIds != null && x.LevelIds.Contains(levelId));
        }

        public static IEnumerable<SubjectConditionRule> GetMatchConditionRules(this ICollection<SubjectConditionRule> subjectConditionRules, int age, Guid targetLevelId)
        {
            return subjectConditionRules.Where(x => IsMatchRule(x, age, targetLevelId))
                .OrderBy(x =>
                {
                    var ageCondition = x?.ConditionRules?.FirstOrDefault(x => x.Type == EnumSubjectConditionRuleType.Age);
                    return ageCondition?.FromAge == null ? 999 : Math.Abs(age - ageCondition.FromAge.Value);
                });
        }

        public static bool IsMatchRule(SubjectConditionRule rule, int age, Guid? levelId)
        {
            var ageCondition = rule?.ConditionRules?.FirstOrDefault(x => x.Type == EnumSubjectConditionRuleType.Age);
            if (ageCondition != null)
            {
                if (!ageCondition.FromAge.HasValue)
                {
                    return false;
                }

                switch (ageCondition.OperatorType)
                {
                    case EnumOperatorType.Include:
                    case EnumOperatorType.Exclude:
                        break;

                    case EnumOperatorType.Equal when ageCondition.FromAge != age:
                    case EnumOperatorType.GreaterThan when age <= ageCondition.FromAge.Value:
                    case EnumOperatorType.LessThan when age >= ageCondition.FromAge.Value:
                    case EnumOperatorType.GreaterThanEqual when age < ageCondition.FromAge.Value:
                    case EnumOperatorType.LessThanEqual when age > ageCondition.FromAge.Value:
                    case EnumOperatorType.Between when !ageCondition.ToAge.HasValue || age > ageCondition.ToAge.Value ||
                                                       age < ageCondition.FromAge.Value:
                        return false;
                }
            }

            var levelCondition = rule?.ConditionRules?.FirstOrDefault(x => x.Type == EnumSubjectConditionRuleType.CurrentLevel);
            if (levelCondition == null)
            {
                return true;
            }

            if (!levelId.HasValue)
            {
                return false;
            }

            return levelCondition.OperatorType switch
            {
                EnumOperatorType.Include => levelCondition.LevelIds != null && levelCondition.LevelIds.Contains(levelId.Value),
                EnumOperatorType.Exclude => levelCondition.LevelIds == null || !levelCondition.LevelIds.Contains(levelId.Value),
                _ => true
            };
        }
    }
}
