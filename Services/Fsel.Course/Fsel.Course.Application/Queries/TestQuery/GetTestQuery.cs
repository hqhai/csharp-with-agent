// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.TestQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.TestModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTestQuery : IRequest<MethodResult<TestModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetTestQueryHandler : IRequestHandler<GetTestQuery, MethodResult<TestModel>>
    {
        private readonly ITestRepository _testRepository;
        private readonly ITestSectionRepository _testSectionRepository;
        private readonly IMapper _mapper;
        private readonly ITestSectionQuestionRepository _testSectionQuestionRepository;
        private readonly ITestAISettingRepository _testAISettingRepository;

        public GetTestQueryHandler(
            ITestRepository testRepository,
            ITestSectionRepository testSectionRepository,
            IMapper mapper,
            ITestSectionQuestionRepository testSectionQuestionRepository,
            ITestAISettingRepository testAISettingRepository)
        {
            _testRepository = testRepository;
            _testSectionRepository = testSectionRepository;
            _mapper = mapper;
            _testSectionQuestionRepository = testSectionQuestionRepository;
            _testAISettingRepository = testAISettingRepository;
        }

        public async Task<MethodResult<TestModel>> Handle(GetTestQuery request, CancellationToken cancellationToken)
        {
            MethodResult<TestModel> methodResult = new MethodResult<TestModel>();
            ArgumentNullException.ThrowIfNull(request);

            var test = await _testRepository.Queryable.Include(x => x.Level).Include(x => x.Program).Where(tc => tc.Id == request.Id).AsNoTracking().FirstOrDefaultAsync(cancellationToken);
            if (test == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var allSections = await _testSectionRepository.Queryable
                                     .Include(x => x.Skill)
                                     .Where(x => x.TestId == request.Id)
                                     .AsNoTracking()
                                     .ToListAsync(cancellationToken);

            var allTestAISettings = await _testAISettingRepository.Queryable.Include(x => x.TestAICriteriaSettings)
                                    .WhereBulkContains(allSections.Select(x => x.Id), x => x.TestSectionId)
                                    .ToListAsync(cancellationToken);

            var allQuestions = await _testSectionQuestionRepository.Queryable
                                .WhereBulkContains(allSections.Select(x => x.Id), x => x.TestSectionId)
                                .Select(x => new
                                {
                                    TestSectionId = x.TestSectionId,
                                    Question = x.Question,
                                })
                                .AsNoTracking()
                                .ToListAsync(cancellationToken);

            var testAISettingDict = allTestAISettings.GroupBy(x => x.TestSectionId)
                              .ToDictionary(
                                  g => g.Key,
                                  g => g.Select(x => x).OrderBy(x => x.CreatedDate).ToList()
                              );

            var questionDict = allQuestions.GroupBy(x => x.TestSectionId)
                                    .ToDictionary(
                                        g => g.Key,
                                        g => g.Select(x => x.Question).OrderBy(x => x.CreatedDate).ToList()
                                    );

            // Mapping Test
            var testModel = _mapper.Map<TestModel>(test);
            testModel.TestSections = BuildSectionTree(allSections, questionDict, testAISettingDict);

            methodResult.Result = testModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private List<TestSectionModel> BuildSectionTree(List<TestSection> allSections, Dictionary<Guid, List<Question>> questionDict, Dictionary<Guid, List<TestAISetting>> testAISettingDict, Guid? parentId = null)
        {
            var currentSections = allSections
                .Where(s => s.ParentId == parentId)
                .OrderBy(s => s.DisplayOrder)
                .ToList();

            var result = new List<TestSectionModel>();
            foreach (var entity in currentSections)
            {
                var model = _mapper.Map<TestSectionModel>(entity);
                if (questionDict.TryGetValue(entity.Id, out var questions))
                {
                    model.Questions = _mapper.Map<IList<Domain.Models.EntityModels.QuestionModel>>(questions);
                }
                if (testAISettingDict.TryGetValue(entity.Id, out var testAISettings))
                {
                    model.TestAISettings = _mapper.Map<IList<TestAISettingModel>>(testAISettings);
                }
                model.Childrens = BuildSectionTree(allSections, questionDict, testAISettingDict, entity.Id);
                result.Add(model);
            }
            return result;
        }
    }
}
