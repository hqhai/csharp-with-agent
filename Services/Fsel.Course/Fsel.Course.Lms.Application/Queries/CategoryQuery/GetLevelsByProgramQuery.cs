// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CategoryQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Core.Base.Interfaces;
    using Domain.Entities.TestConfigs;
    using Domain.Enums;
    using Domain.IRepositories;
    using Common.ActionResults;
    using Domain.Entities;
    using Domain.Entities.SubjectConditionRuleConfigs;
    using Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Services.ApplicationServices;
    using Services.UserServices;
    using Shared.Helpers;

    public class GetLevelsByProgramQuery : IRequest<MethodResult<List<SelectionLevelModel>>>
    {
        public Guid UserId { get; set; }
    }

    public class GetLevelsByProgramQueryHandler : IRequestHandler<GetLevelsByProgramQuery, MethodResult<List<SelectionLevelModel>>>
    {
        private readonly IMapper _mapper;
        private readonly ICategoryService _categoryService;
        private readonly IRepository<TestGroupResult> _testGroupResult;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISubjectConditionRepository _subjectConditionRepository;
        private readonly IUserService _userService;


        public GetLevelsByProgramQueryHandler(
            ICategoryService categoryService,
            ICategoryRepository categoryRepository,
            IRepository<TestGroupResult> testGroupResult,
            ISubjectConditionRepository subjectConditionRepository,
            IUserService userService,
            IMapper mapper)
        {
            _categoryService = categoryService;
            _categoryRepository = categoryRepository;
            _testGroupResult = testGroupResult;
            _subjectConditionRepository = subjectConditionRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<List<SelectionLevelModel>>> Handle(GetLevelsByProgramQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var studentResult = await _userService.GetStudentByUserIdAsync(request.UserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                return new MethodResult<List<SelectionLevelModel>>();
            }

            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                return new MethodResult<List<SelectionLevelModel>>();
            }

            var user = studentResult?.Content?.Result?.User;

            if (user == null)
            {
                return new MethodResult<List<SelectionLevelModel>>();
            }


            var ptTestResult = await _testGroupResult.ReadQueryable
                .Where(x => x.StudentId == student.Id && x.TestType == EnumTestType.PlacementTest)
                .FirstOrDefaultAsync(cancellationToken);

            if (!IsDonePt(ptTestResult))
            {
                return new MethodResult<List<SelectionLevelModel>>();
            }


            var program = await _categoryRepository.Queryable
                .Include(x => x.Levels)
                .FirstOrDefaultAsync(x => x.Id == ptTestResult.ProgramId, cancellationToken);

            if (program == null)
            {
                return new MethodResult<List<SelectionLevelModel>>();
            }

            var age = DateTimeHelper.GetYearOld(user.Birthday);
            var suggestCondition = await _subjectConditionRepository.ReadQueryable
                .Where(x => x.CategoryId == program.ParentId && x.Status && x.Type == EnumConditionType.CourseSuggest)
                .Include(x => x.SubjectConditionRules)
                .FirstOrDefaultAsync(cancellationToken);


            var matchestRule = suggestCondition?.SubjectConditionRules.Where(x => IsMatchRule(x, age, ptTestResult.CurrentLevelId))
                .OrderBy(x =>
                {
                    var ageCondition = x?.ConditionRules?.FirstOrDefault(x => x.Type == EnumSubjectConditionRuleType.Age);
                    return ageCondition?.FromAge == null ? 999 : Math.Abs(age - ageCondition.FromAge.Value);
                })
                .FirstOrDefault();


            var suggestLevels = program.Levels.Select(x =>
            {
                var selectionLevel = _mapper.Map<SelectionLevelModel>(x);
                selectionLevel.ProgramId = ptTestResult.ProgramId.Value;

                var matchCondition = GetMatchConditionValue(matchestRule?.ConditionValues, x.Id);
                if (matchCondition != null)
                {
                    selectionLevel.CanSelect = true;
                    selectionLevel.CourseType = matchCondition.Type.ToString();
                }

                if (!selectionLevel.CanSelect)
                {
                    selectionLevel.CanSelect = !ptTestResult.CurrentLevelId.HasValue ? true : ptTestResult.CurrentLevelId.Value == x.Id;
                }

                selectionLevel.IsCurrentLevel = ptTestResult.CurrentLevelId == x.Id;

                return selectionLevel;
            }).ToList();
            return new MethodResult<List<SelectionLevelModel>> { Result = suggestLevels, StatusCode = 200 };
        }

        public static ConditionValue? GetMatchConditionValue(IList<ConditionValue>? conditionValues, Guid levelId)
        {
            if (conditionValues == null)
            {
                return null;
            }

            return conditionValues.FirstOrDefault(x => x.LevelIds != null && x.LevelIds.Contains(levelId));
        }

        public static bool IsDonePt(TestGroupResult? ptTestResult)
        {
            return ptTestResult?.Status == EnumResultStatus.Done || ptTestResult?.Status == EnumResultStatus.ByPass;
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
