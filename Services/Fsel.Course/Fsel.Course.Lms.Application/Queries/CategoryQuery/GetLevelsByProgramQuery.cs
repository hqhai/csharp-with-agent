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
    using Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Services.ApplicationServices;
    using Services.UserServices;
    using Shared.Helpers;

    public class GetLevelsByProgramQuery : IRequest<MethodResult<List<SelectionLevelModel>>>
    {
        public Guid SelectedProgramId { get; set; }

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

            var human = studentResult?.Content?.Result?.Human;

            if (human == null)
            {
                return new MethodResult<List<SelectionLevelModel>>();
            }


            var ptTestResult = await _testGroupResult.ReadQueryable
                .Where(x => x.StudentId == student.Id && x.TestType == EnumTestType.PlacementTest)
                .FirstOrDefaultAsync(cancellationToken);

            if (ptTestResult == null)
            {
                return new MethodResult<List<SelectionLevelModel>>();
            }


            var program = await _categoryRepository.Queryable
                .Include(x => x.Levels)
                .FirstOrDefaultAsync(x => x.Id == request.SelectedProgramId, cancellationToken);

            if (program == null)
            {
                return new MethodResult<List<SelectionLevelModel>>();
            }


            if (!ptTestResult.CurrentLevelId.HasValue)
            {
                return new MethodResult<List<SelectionLevelModel>> { Result = _mapper.Map<List<SelectionLevelModel>>(program.Levels), StatusCode = 200 };
            }


            var age = DateTimeHelper.GetYearOld(human.Birthday);
            var suggestCondition = await _subjectConditionRepository.ReadQueryable
                .Where(x => x.CategoryId == program.ParentId && x.Status && x.Type == EnumConditionType.CourseSuggest)
                .Include(x => x.SubjectConditionRules)
                .FirstOrDefaultAsync(cancellationToken);


            var matchestRule = suggestCondition?.SubjectConditionRules.Where(x => IsMatchRule(x, age, ptTestResult.CurrentLevelId.Value))
                .OrderBy(x =>
                {
                    var ageCondition = x?.ConditionRules?.FirstOrDefault(x => x.Type == EnumSubjectConditionRuleType.Age);
                    return ageCondition?.FromAge == null ? 999 : Math.Abs(age - ageCondition.FromAge.Value);
                })
                .FirstOrDefault();

            var additionalLevelIds = matchestRule?.ConditionValues?
                .SelectMany(x => x.LevelIds ?? new List<Guid>())
                .Distinct().ToList() ?? new List<Guid>();

            additionalLevelIds.Add(ptTestResult.CurrentLevelId.Value);

            var suggestLevels = program.Levels.Select(x =>
            {
                var selectionLevel = _mapper.Map<SelectionLevelModel>(x);
                selectionLevel.ProgramId = request.SelectedProgramId;
                selectionLevel.CanSelect = additionalLevelIds.Contains(x.Id);
                return selectionLevel;
            }).ToList();
            return new MethodResult<List<SelectionLevelModel>> { Result = suggestLevels, StatusCode = 200 };
        }


        public static bool IsMatchRule(SubjectConditionRule rule, int age, Guid levelId)
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
                    default:
                        return false;
                }
            }

            var levelCondition = rule?.ConditionRules?.FirstOrDefault(x => x.Type == EnumSubjectConditionRuleType.CurrentLevel);
            if (levelCondition == null)
            {
                return true;
            }

            return levelCondition.OperatorType switch
            {
                EnumOperatorType.Include => levelCondition.LevelIds != null && levelCondition.LevelIds.Contains(levelId),
                EnumOperatorType.Exclude => levelCondition.LevelIds == null || !levelCondition.LevelIds.Contains(levelId),
                _ => true
            };
        }
    }
}
