// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.TestConfigQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.SkillModels;
    using Fsel.Course.Domain.Models.EntityModels.TestConfig;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTestConfigQuery : IRequest<MethodResult<TestConfigModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetTestConfigQueryHandler : IRequestHandler<GetTestConfigQuery, MethodResult<TestConfigModel>>
    {
        private readonly ITestConfigRepository _testConfigRepository;
        private readonly ITestLayoutRepository _testLayoutRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISkillLevelRepository _skillLevelRepository;
        private readonly IStepFlowRepository _stepFlowRepository;
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly ILevelRepository _levelRepository;

        public GetTestConfigQueryHandler(ITestConfigRepository testConfigRepository, ISkillRepository skillRepository,
            ICategoryRepository categoryRepository, ITestLayoutRepository testLayoutRepository, ISkillLevelRepository skillLevelRepository,
            IStepFlowRepository stepFlowRepository, IPlacementTestRepository placementTestRepository, ILevelRepository levelRepository)
        {
            _testConfigRepository = testConfigRepository;
            _skillRepository = skillRepository;
            _categoryRepository = categoryRepository;
            _testLayoutRepository = testLayoutRepository;
            _skillLevelRepository = skillLevelRepository;
            _stepFlowRepository = stepFlowRepository;
            _placementTestRepository = placementTestRepository;
            _levelRepository = levelRepository;
        }

        public async Task<MethodResult<TestConfigModel>> Handle(GetTestConfigQuery request, CancellationToken cancellationToken)
        {
            MethodResult<TestConfigModel> methodResult = new MethodResult<TestConfigModel>();
            ArgumentNullException.ThrowIfNull(request);
            var testConfig = _testConfigRepository.Queryable;
            var skill = _skillRepository.Queryable;
            var category = _categoryRepository.Queryable;
            var testLayout = _testLayoutRepository.Queryable;
            var testConfigResult = await _testConfigRepository.Queryable
                                .Where(tc => tc.Id == request.Id)
                                .Select(tc => new TestConfigModel
                                {
                                    Id = tc.Id,
                                    Name = tc.Name,
                                    Skills = tc.Skills.Select(s => new SkillModel //-------------> CODE
                                    {
                                        Id = s.Id,
                                        Name = s.Name,
                                        Code = s.Code,
                                    }).ToList(),
                                    TestLayouts = tc.TestLayouts != null //--------------> TESTLAYOUT
                                    ? tc.TestLayouts
                                    .Select(tl => new TestLayoutModel
                                    {
                                        Name = tl.Name,
                                        TotalScore = tl.TotalScore,
                                        ExcutionTime = tl.ExcutionTime,
                                        Section = tl.Section != null ? new SectionModel
                                        {
                                            Id = tl.Section.Id,
                                            Name = tl.Section.Name,
                                            MediaPost = tl.Section.MediaPost,

                                            TargetWord = tl.Section.TargetWord,
                                            VideoFilePath = tl.Section.VideoFilePath,
                                            SubFilePath = tl.Section.SubFilePath,
                                            DisplayOrder = tl.Section.DisplayOrder,
                                            Questions = tl.Section.SectionQuestions
                                            .Select(sq => sq.Question != null ? new QuestionModel
                                            {
                                                Id = sq.Question.Id,
                                                QuestionType = sq.Question.QuestionType,
                                                Ungraded = sq.Question.Ungraded,
                                                Explanation = sq.Question.Explanation,
                                                CorrectTotal = sq.Question.CorrectTotal,
                                                Description = sq.Question.Description,
                                                Config = sq.Question.Config,
                                                SubQuestionIndexs = sq.Question.SubQuestionIndexs,
                                            } : new QuestionModel())
                                            .ToList(),
                                        } : null
                                    }).ToList() : new List<TestLayoutModel>(),
                                    Program = tc.Program != null ? new CategoryModel //----------> PROGRAM
                                    {
                                        Id = tc.Program.Id,
                                        Name = tc.Program.Name,
                                        Code = tc.Program.Code,
                                        Levels = tc.Program.Levels.Select(lv => new LevelModel
                                        {
                                            Id = lv.Id,
                                            Name = lv.Name,
                                            Code = lv.Code,
                                            Description = lv.Description,
                                            LevelOrder = lv.LevelOrder,
                                        }).ToList(),
                                    } : new CategoryModel(),
                                })
                             .FirstOrDefaultAsync(cancellationToken);

            if (testConfigResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            methodResult.Result = testConfigResult;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
