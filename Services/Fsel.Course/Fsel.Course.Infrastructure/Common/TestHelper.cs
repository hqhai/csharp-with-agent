// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Questions;
    using Fsel.Course.Domain.Models.CommandModels.TestAiSettings;
    using Fsel.Course.Domain.Models.CommandModels.TestSections;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    public class TestHelper
    {
        private readonly ITestAISettingRepository _testAISettingRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ITestSectionRepository _testSectionRepository;
        private readonly ITestSectionQuestionRepository _testSectionQuestionRepository;
        private readonly IMapper _mapper;
        private readonly QuestionConverter _questionConverter;
        private List<Question> DeleteQuestions = new List<Question>();
        private List<TestSection> DeleteTestSections = new List<TestSection>();
        private List<TestAISetting> DeleteTestAISettings = new List<TestAISetting>();
        private List<TestAICriteriaSetting> DeleteTestAICriteriaSettings = new List<TestAICriteriaSetting>();

        public TestHelper(ITestAISettingRepository testAISettingRepository,
            IQuestionRepository questionRepository,
            ITestSectionRepository testSectionRepository,
            ITestSectionQuestionRepository testSectionQuestionRepository,
            IMapper mapper,
            QuestionConverter questionConverter)
        {
            _testAISettingRepository = testAISettingRepository;
            _questionRepository = questionRepository;
            _testSectionRepository = testSectionRepository;
            _testSectionQuestionRepository = testSectionQuestionRepository;
            _mapper = mapper;
            _questionConverter = questionConverter;
        }

        #region Insert

        public VoidMethodResult InsertSectionRecursive(IList<CreateTestSectionCommandModel>? testSectionRequests, TestSection? testSection = null, Test? test = null)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            if (testSectionRequests == null || !testSectionRequests.Any())
            {
                return methodResult;
            }

            foreach (var testSectionRequest in testSectionRequests)
            {
                if (testSectionRequest.LayoutType.HasValue)
                {
                    var isLayOutBasicError = testSectionRequest.LayoutType == EnumTestLayoutType.Basic && testSectionRequest.TestAISettings.Any();
                    var isLayOutError = testSectionRequest.LayoutType != EnumTestLayoutType.Basic && testSectionRequest.Questions.Any();
                    if (isLayOutError || isLayOutBasicError)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(testSectionRequest), testSectionRequest.Name);
                        return methodResult;
                    }
                }

                var testSectionCreated = _mapper.Map<TestSection>(testSectionRequest);
                if (!testSectionCreated.IsValid())
                {
                    methodResult.AddErrorBadRequest(testSectionCreated.ErrorMessages);
                    return methodResult;
                }
                testSectionCreated.DisplayOrder = testSectionRequests.IndexOf(testSectionRequest);
                if (testSection != null)
                {
                    testSectionCreated.Test = testSection.Test;
                    testSection.TestSections.Add(testSectionCreated);
                }
                if (test != null)
                {
                    testSectionCreated.Test = test;
                    test.TestSections.Add(testSectionCreated);
                }
                if (testSectionRequest.Childrens.Any())
                {
                    var methodResultCreated = InsertSectionRecursive(testSectionRequest.Childrens, testSectionCreated);
                    if (!methodResultCreated.IsOK)
                    {
                        methodResult.AddErrorBadRequest(methodResultCreated.ErrorMessages);
                        return methodResult;
                    }
                }
                if (testSectionRequest.TestAISettings.Any())
                {
                    var methodResultCreated = InsertTestAISettings(testSectionRequest.TestAISettings, testSectionCreated);
                    if (!methodResultCreated.IsOK)
                    {
                        methodResult.AddErrorBadRequest(methodResultCreated.ErrorMessages);
                        return methodResult;
                    }
                }

                if (testSectionRequest.Questions.Any())
                {
                    var methodResultCreated = InsertQuestions(testSectionRequest.Questions, testSectionCreated);
                    if (!methodResultCreated.IsOK)
                    {
                        methodResult.AddErrorBadRequest(methodResultCreated.ErrorMessages);
                        return methodResult;
                    }
                }
            }
            return methodResult;
        }

        private VoidMethodResult InsertQuestions(IList<CreateQuestionCommandModel>? questionRequests, TestSection testSection)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            if (questionRequests == null || !questionRequests.Any())
            {
                return methodResult;
            }

            foreach (var questionRequest in questionRequests)
            {
                if (questionRequest == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questionRequest));
                    return methodResult;
                }
                var question = _mapper.Map<Question>(questionRequest);
                var method = _questionConverter.HandleQuestion(question);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                testSection.TestSectionQuestions.Add(new TestSectionQuestion
                {
                    Question = method.Result ?? question,
                });
            }
            return methodResult;
        }

        private VoidMethodResult InsertTestAISettings(IList<CreateTestAISettingCommandModel>? testAISettingRequests, TestSection testSection)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            if (testAISettingRequests == null || !testAISettingRequests.Any())
            {
                return methodResult;
            }

            foreach (var testAISettingRequest in testAISettingRequests)
            {
                if (testAISettingRequest == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(testAISettingRequest));
                    return methodResult;
                }
                var testAISetting = _mapper.Map<TestAISetting>(testAISettingRequest);
                if (!testAISetting.IsValid())
                {
                    methodResult.AddErrorBadRequest(testAISetting.ErrorMessages);
                    return methodResult;
                }
                foreach (var testAICriteriaSetting in testAISetting.TestAICriteriaSettings)
                {
                    if (!testAICriteriaSetting.IsValid())
                    {
                        methodResult.AddErrorBadRequest(testAICriteriaSetting.ErrorMessages);
                        return methodResult;
                    }
                }
                testSection.TestAISettings.Add(testAISetting);
            }
            return methodResult;
        }

        #endregion Insert

        #region Update

        public async Task DeleteDataAsync(Test test)
        {
            await _testAISettingRepository.DeleteListAsync(DeleteTestAISettings);
            await _testAISettingRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

            await _questionRepository.DeleteListAsync(DeleteQuestions);
            await _questionRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

            if (DeleteTestSections.Any())
            {
                var allSectionChilrens = await _testSectionRepository.Queryable.Where(x => x.ParentId.HasValue && x.TestId == test.Id)
                                                                     .WhereBulkContains(DeleteTestSections.Select(x => x.Id), x => x.ParentId).ToListAsync();
                if (allSectionChilrens.Any())
                {
                    await _testSectionRepository.DeleteListAsync(allSectionChilrens);
                    await _testSectionRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
                }
                var questions = await _testSectionQuestionRepository.Queryable.WhereBulkContains(allSectionChilrens.Select(x => x.Id), x => x.TestSectionId).Select(x => x.Question).ToListAsync();
                if (questions.Any())
                {
                    await _questionRepository.DeleteListAsync(questions);
                    await _questionRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
                }
                var testAISettings = await _testAISettingRepository.Queryable.Include(x => x.TestAICriteriaSettings).WhereBulkContains(allSectionChilrens.Select(x => x.Id), x => x.TestSectionId).ToListAsync();
                if (testAISettings.Any())
                {
                    await _testAISettingRepository.DeleteListAsync(testAISettings);
                    await _testAISettingRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
                }

                await _testSectionRepository.DeleteListAsync(DeleteTestSections);
                await _testSectionRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task<MethodResult<Test>> UpdateSectionRecursive(IList<UpdateTestSectionCommandModel>? testSectionRequests, TestSection? testSectionParent = null, Test? test = null)
        {
            MethodResult<Test> methodResult = new MethodResult<Test>();
            if (testSectionRequests == null || !testSectionRequests.Any())
            {
                return methodResult;
            }
            var testSections = new List<TestSection>();
            if (test != null)
            {
                testSections = await _testSectionRepository.Queryable.Where(x => x.TestId == test.Id && !x.ParentId.HasValue).ToListAsync();
            }
            else if (testSectionParent != null)
            {
                testSections = await _testSectionRepository.Queryable.Where(x => x.ParentId == testSectionParent.Id).ToListAsync();
            }

            foreach (var testSectionRequest in testSectionRequests)
            {
                if (testSectionRequest.LayoutType.HasValue)
                {
                    var isLayOutBasicError = testSectionRequest.LayoutType == EnumTestLayoutType.Basic && testSectionRequest.TestAISettings.Any();
                    var isLayOutError = testSectionRequest.LayoutType != EnumTestLayoutType.Basic && testSectionRequest.Questions.Any();
                    if (isLayOutError || isLayOutBasicError)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(testSectionRequest), testSectionRequest.Name);
                        return methodResult;
                    }
                }

                if (testSectionRequest == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(testSectionRequest));
                    return methodResult;
                }

                TestSection? testSection = null;
                if (testSectionRequest.Id.HasValue)
                {
                    testSection = testSections.FirstOrDefault(x => x.Id == testSectionRequest.Id);
                    if (testSection == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(testSection), testSectionRequest.Id);
                        return methodResult;
                    }
                    _mapper.Map(testSectionRequest, testSection);
                }
                else
                {
                    testSection = _mapper.Map<TestSection>(testSectionRequest);
                }

                if (!testSection.IsValid())
                {
                    methodResult.AddErrorBadRequest(testSection.ErrorMessages);
                    return methodResult;
                }
                testSection.DisplayOrder = testSectionRequests.IndexOf(testSectionRequest);
                if (testSection.Id == Guid.Empty)
                {
                    if (testSectionParent != null)
                    {
                        testSection.Test = testSectionParent.Test;
                        testSectionParent.TestSections.Add(testSection);
                    }
                    if (test != null)
                    {
                        testSection.Test = test;
                        test.TestSections.Add(testSection);
                    }
                }
                if (testSectionRequest.Childrens.Any())
                {
                    var methodResultCreated = await UpdateSectionRecursive(testSectionRequest.Childrens, testSection);
                    if (!methodResultCreated.IsOK)
                    {
                        methodResult.AddErrorBadRequest(methodResultCreated.ErrorMessages);
                        return methodResult;
                    }
                }
                if (testSectionRequest.TestAISettings.Any())
                {
                    var methodResultCreated = await UpdateTestAISettings(testSectionRequest.TestAISettings, testSection);
                    if (!methodResultCreated.IsOK)
                    {
                        methodResult.AddErrorBadRequest(methodResultCreated.ErrorMessages);
                        return methodResult;
                    }
                }

                if (testSectionRequest.Questions.Any())
                {
                    var methodResultCreated = await UpdateQuestions(testSectionRequest.Questions, testSection);
                    if (!methodResultCreated.IsOK)
                    {
                        methodResult.AddErrorBadRequest(methodResultCreated.ErrorMessages);
                        return methodResult;
                    }
                }
            }
            var testSectionToDelete = testSections.Where(x => x != null && x.Id != Guid.Empty && !testSectionRequests.Any(y => y.Id.HasValue && y.Id == x.Id)).ToList();
            if (testSectionToDelete.Any())
            {
                DeleteTestSections.AddRange(testSectionToDelete);
            }
            methodResult.Result = test;
            return methodResult;
        }

        private async Task<VoidMethodResult> UpdateQuestions(IList<UpdateQuestionCommandModel>? questionRequests, TestSection testSection)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            if (questionRequests == null || !questionRequests.Any())
            {
                return methodResult;
            }
            var questions = await _testSectionQuestionRepository.Queryable.Where(x => x.TestSectionId == testSection.Id).Select(x => x.Question).ToListAsync();

            foreach (var questionRequest in questionRequests)
            {
                if (questionRequest == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questionRequest));
                    return methodResult;
                }

                Question? question = null;
                if (questionRequest.Id.HasValue)
                {
                    question = questions.FirstOrDefault(x => x.Id == questionRequest.Id);
                    if (question == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question), questionRequest.Id);
                        return methodResult;
                    }

                    _mapper.Map(questionRequest, question);
                }
                else
                {
                    question = _mapper.Map<Question>(questionRequest);
                }
                var method = _questionConverter.HandleQuestion(question);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                if (question.Id == Guid.Empty)
                {
                    testSection.TestSectionQuestions.Add(new TestSectionQuestion
                    {
                        Question = method.Result ?? question,
                    });
                }
            }
            var questionsToDelete = questions.Where(x => x != null && x.Id != Guid.Empty && !questionRequests.Any(y => y.Id.HasValue && y.Id == x.Id)).ToList();
            if (questionsToDelete.Any())
            {
                DeleteQuestions.AddRange(questionsToDelete);
            }

            return methodResult;
        }

        private async Task<VoidMethodResult> UpdateTestAISettings(IList<UpdateTestAISettingCommandModel>? requests, TestSection testSection)
        {
            var methodResult = new VoidMethodResult();
            if (requests == null || !requests.Any())
            {
                return methodResult;
            }
            var existingSettings = await _testAISettingRepository.Queryable
                .Include(x => x.TestAICriteriaSettings)
                .Where(x => x.TestSectionId == testSection.Id)
                .ToListAsync();

            foreach (var request in requests)
            {
                var result = HandleTestAISettingUpdate(request, testSection, existingSettings);
                if (!result.IsOK)
                {
                    methodResult.AddErrorBadRequest(result.ErrorMessages);
                    return methodResult;
                }
            }

            var toDeleteSettings = existingSettings
                .Where(x => x.Id != Guid.Empty && !requests.Any(r => r.Id == x.Id))
                .ToList();

            if (toDeleteSettings.Any())
            {
                DeleteTestAISettings.AddRange(toDeleteSettings);
            }

            return methodResult;
        }

        private VoidMethodResult HandleTestAISettingUpdate(UpdateTestAISettingCommandModel? request, TestSection testSection, List<TestAISetting> existingSettings)
        {
            var methodResult = new VoidMethodResult();
            if (request == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request));
                return methodResult;
            }

            TestAISetting? testAISetting;
            if (request.Id.HasValue)
            {
                testAISetting = existingSettings.FirstOrDefault(x => x.Id == request.Id);
                if (testAISetting == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(TestAISetting), request.Id);
                    return methodResult;
                }
                _mapper.Map(request, testAISetting);
            }
            else
            {
                testAISetting = _mapper.Map<TestAISetting>(request);
            }

            if (!testAISetting.IsValid())
            {
                methodResult.AddErrorBadRequest(testAISetting.ErrorMessages);
                return methodResult;
            }

            foreach (var criteriaRequest in request.TestAICriteriaSettings)
            {
                var result = HandleTestAICriteriaSettingUpdate(criteriaRequest, testAISetting);
                if (!result.IsOK)
                {
                    methodResult.AddErrorBadRequest(result.ErrorMessages);
                    return methodResult;
                }
            }

            if (testAISetting.Id == Guid.Empty)
            {
                testSection.TestAISettings.Add(testAISetting);
            }

            var toDeleteCriteria = testAISetting.TestAICriteriaSettings
                .Where(x => x.Id != Guid.Empty && !request.TestAICriteriaSettings.Any(r => r.Id == x.Id))
                .ToList();

            if (toDeleteCriteria.Any())
            {
                DeleteTestAICriteriaSettings.AddRange(toDeleteCriteria);
            }

            return methodResult;
        }

        private VoidMethodResult HandleTestAICriteriaSettingUpdate(UpdateTestAICriteriaSettingCommandModel? request, TestAISetting setting)
        {
            var methodResult = new VoidMethodResult();
            if (request == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request));
                return methodResult;
            }

            TestAICriteriaSetting? criteria;
            if (request.Id.HasValue)
            {
                criteria = setting.TestAICriteriaSettings.FirstOrDefault(x => x.Id == request.Id);
                if (criteria == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(criteria), request.Id);
                    return methodResult;
                }
                _mapper.Map(request, criteria);
            }
            else
            {
                criteria = _mapper.Map<TestAICriteriaSetting>(request);
            }

            if (!criteria.IsValid())
            {
                methodResult.AddErrorBadRequest(criteria.ErrorMessages);
                return methodResult;
            }

            if (criteria.Id == Guid.Empty)
            {
                setting.TestAICriteriaSettings.Add(criteria);
            }

            return methodResult;
        }

        #endregion Update
    }
}
