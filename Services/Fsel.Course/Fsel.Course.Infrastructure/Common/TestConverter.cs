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

    public class TestConverter
    {
        private readonly ITestAISettingRepository _testAISettingRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ITestSectionRepository _testSectionRepository;
        private readonly ITestSectionQuestionRepository _testSectionQuestionRepository;
        private readonly IMapper _mapper;
        private readonly QuestionConverter _questionConverter;
        private readonly ITestAICriteriaSettingRepository _testAICriteriaSettingRepository;
        private List<Question> DeleteQuestions = new List<Question>();
        private List<TestSection> DeleteTestSections = new List<TestSection>();
        private List<TestAISetting> DeleteTestAISettings = new List<TestAISetting>();
        private List<TestAICriteriaSetting> DeleteTestAICriteriaSettings = new List<TestAICriteriaSetting>();
        private VoidMethodResult VoidMethodResult = new VoidMethodResult();
        private int NumberQuestion = 0;

        public TestConverter(ITestAISettingRepository testAISettingRepository,
            IQuestionRepository questionRepository,
            ITestSectionRepository testSectionRepository,
            ITestSectionQuestionRepository testSectionQuestionRepository,
            IMapper mapper,
            QuestionConverter questionConverter,
            ITestAICriteriaSettingRepository testAICriteriaSettingRepository)
        {
            _testAISettingRepository = testAISettingRepository;
            _questionRepository = questionRepository;
            _testSectionRepository = testSectionRepository;
            _testSectionQuestionRepository = testSectionQuestionRepository;
            _mapper = mapper;
            _questionConverter = questionConverter;
            _testAICriteriaSettingRepository = testAICriteriaSettingRepository;
        }

        #region Validate

        public VoidMethodResult IsValidateQuestion(IEnumerable<UpdateTestSectionCommandModel> testSections)
        {
            var methodResult = new VoidMethodResult();
            if (testSections == null || !testSections.Any())
            {
                return methodResult;
            }

            foreach (var testSection in testSections)
            {
                if (testSection.Questions != null && testSection.Questions.Any())
                {
                    foreach (var questionRequest in testSection.Questions)
                    {
                        var question = _mapper.Map<Question>(questionRequest);
                        if (!question.IsValid())
                        {
                            methodResult.AddErrorBadRequest(question.ErrorMessages);
                        }
                        var method = _questionConverter.HandleQuestion(question);
                        if (!method.IsOK)
                        {
                            methodResult.AddErrorBadRequest(method.ErrorMessages);
                        }
                    }
                }

                if (testSection.Childrens != null && testSection.Childrens.Any())
                {
                    var childResult = IsValidateQuestion(testSection.Childrens);
                    if (!childResult.IsOK)
                    {
                        methodResult.AddErrorBadRequest(childResult.ErrorMessages);
                    }
                }
            }

            return methodResult;
        }

        private void ValidateTestLayout(CreateTestSectionCommandModel testSectionRequest)
        {
            if (testSectionRequest.LayoutType.HasValue)
            {
                var isLayOutBasicError = testSectionRequest.LayoutType == EnumTestLayoutType.Basic && testSectionRequest.TestAISettings.Any();
                var isLayOutError = testSectionRequest.LayoutType != EnumTestLayoutType.Basic && testSectionRequest.Questions.Any();
                if (isLayOutError || isLayOutBasicError)
                {
                    VoidMethodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(testSectionRequest), testSectionRequest.Name);
                }
            }
        }

        private void ValidateTestLayout(UpdateTestSectionCommandModel testSectionRequest)
        {
            if (testSectionRequest.LayoutType.HasValue)
            {
                var isLayOutBasicError = testSectionRequest.LayoutType == EnumTestLayoutType.Basic && testSectionRequest.TestAISettings.Any();
                var isLayOutError = testSectionRequest.LayoutType != EnumTestLayoutType.Basic && testSectionRequest.Questions.Any();
                if (isLayOutError || isLayOutBasicError)
                {
                    VoidMethodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(testSectionRequest), testSectionRequest.Name);
                }
            }
        }

        private void ValidateObjectExistence(TestSection? testSection)
        {
            if (testSection == null)
            {
                VoidMethodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(testSection));
            }
        }

        private void ValidateObjectExistence(TestAISetting? testAISetting)
        {
            if (testAISetting == null)
            {
                VoidMethodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(testAISetting));
            }
        }

        private void ValidateObjectExistence(TestAICriteriaSetting? testAICriteriaSetting)
        {
            if (testAICriteriaSetting == null)
            {
                VoidMethodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(testAICriteriaSetting));
            }
        }

        private void ValidateObjectExistence(Question? question)
        {
            if (question == null)
            {
                VoidMethodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question));
            }
        }

        private void ValidateIsValid(TestAICriteriaSetting testAICriteriaSetting)
        {
            if (!testAICriteriaSetting.IsValid())
            {
                VoidMethodResult.AddErrorBadRequest(testAICriteriaSetting.ErrorMessages);
            }
        }

        private void ValidateIsValid(TestAISetting testAISetting)
        {
            if (!testAISetting.IsValid())
            {
                VoidMethodResult.AddErrorBadRequest(testAISetting.ErrorMessages);
            }
        }

        private void ValidateIsValid(TestSection testSection)
        {
            if (!testSection.IsValid())
            {
                VoidMethodResult.AddErrorBadRequest(testSection.ErrorMessages);
            }
        }

        private void ValidateIsValid(Question question)
        {
            if (!question.IsValid())
            {
                VoidMethodResult.AddErrorBadRequest(question.ErrorMessages);
            }
        }

        #endregion Validate

        #region Insert Test

        public VoidMethodResult InsertSectionRecursive(IList<CreateTestSectionCommandModel>? testSectionRequests, TestSection? testSectionParent = null, Test? test = null)
        {
            VoidMethodResult voidMethodResult = new VoidMethodResult();
            if (testSectionRequests == null || !testSectionRequests.Any())
            {
                return voidMethodResult;
            }

            foreach (var testSectionRequest in testSectionRequests)
            {
                ValidateTestLayout(testSectionRequest);
                var testSection = ResolveTestSectionFromRequest(testSectionRequest, testSectionRequests.IndexOf(testSectionRequest));
                if (testSection != null)
                {
                    AssignTestReferenceIfNeeded(testSection, testSectionParent, test);
                    HandleSectionChildrenAndConfigs(testSectionRequest, testSection);
                }
                else
                {
                    ValidateObjectExistence(testSection);
                }
            }
            if (!VoidMethodResult.IsOK)
            {
                voidMethodResult.AddErrorBadRequest(VoidMethodResult.ErrorMessages);
            }
            return voidMethodResult;
        }

        private TestSection? ResolveTestSectionFromRequest(CreateTestSectionCommandModel? request, int displayOrder)
        {
            var testSection = _mapper.Map<TestSection>(request);
            if (testSection != null)
            {
                testSection.DisplayOrder = displayOrder;
                ValidateIsValid(testSection);
            }
            else
            {
                ValidateObjectExistence(testSection);
            }
            return testSection;
        }

        private void HandleSectionChildrenAndConfigs(CreateTestSectionCommandModel testSectionRequest, TestSection testSection)
        {
            if (testSectionRequest.Childrens.Any())
            {
                InsertSectionRecursive(testSectionRequest.Childrens, testSection);
            }
            if (testSectionRequest.TestAISettings.Any())
            {
                InsertTestAISettings(testSectionRequest.TestAISettings, testSection);
            }
            if (testSectionRequest.Questions.Any())
            {
                InsertQuestions(testSectionRequest.Questions, testSection);
            }
        }

        #region Insert Question

        private void InsertQuestions(IList<CreateQuestionCommandModel>? questionRequests, TestSection testSection)
        {
            if (questionRequests == null || !questionRequests.Any())
            {
                return;
            }
            foreach (var questionRequest in questionRequests)
            {
                HandleSingleQuestionInsert(questionRequest, testSection);
            }
        }

        private void HandleSingleQuestionInsert(CreateQuestionCommandModel? request, TestSection testSection)
        {
            var question = _mapper.Map<Question>(request);
            if (question != null)
            {
                ValidateIsValid(question);
                var convertResult = _questionConverter.HandleQuestion(question);
                if (!convertResult.IsOK)
                {
                    VoidMethodResult.AddErrorBadRequest(convertResult.ErrorMessages);
                }
                if (convertResult.Result != null)
                {
                    var data = Enumerable.Range(NumberQuestion + 1, convertResult.Result.CorrectTotal).ToList();
                    convertResult.Result.SubQuestionIndexs = data;
                    NumberQuestion += convertResult.Result.CorrectTotal;
                }
                testSection.TestSectionQuestions.Add(new TestSectionQuestion
                {
                    Question = convertResult.Result ?? question
                });
            }
            else
            {
                ValidateObjectExistence(question);
            }
        }

        #endregion Insert Question

        #region Insert TestAISetting

        private void InsertTestAISettings(IList<CreateTestAISettingCommandModel>? testAISettingRequests, TestSection testSection)
        {
            if (testAISettingRequests == null || !testAISettingRequests.Any())
            {
                return;
            }
            foreach (var testAISettingRequest in testAISettingRequests)
            {
                var testAISetting = _mapper.Map<TestAISetting>(testAISettingRequest);
                ValidateObjectExistence(testAISetting);
                if (testAISetting != null)
                {
                    ValidateIsValid(testAISetting);
                    InsertTestAISettings(testAISetting);
                    testSection.TestAISettings.Add(testAISetting);
                }
            }
        }

        private void InsertTestAISettings(TestAISetting testAISetting)
        {
            if (!testAISetting.TestAICriteriaSettings.Any())
            {
                return;
            }
            foreach (var testAICriteriaSetting in testAISetting.TestAICriteriaSettings)
            {
                ValidateObjectExistence(testAICriteriaSetting);
                if (testAICriteriaSetting != null)
                {
                    ValidateIsValid(testAICriteriaSetting);
                }
            }
        }

        #endregion Insert TestAISetting

        #endregion Insert Test

        #region Update Test

        #region Add Remove Data

        private void RemoveObsoleteTestSections(IList<TestSection> oldSections, IList<UpdateTestSectionCommandModel> newRequests)
        {
            var toDelete = oldSections
                .Where(x => x != null && x.Id != Guid.Empty && !newRequests.Any(y => y.Id.HasValue && y.Id == x.Id))
                .ToList();
            if (toDelete.Any())
            {
                DeleteTestSections.AddRange(toDelete);
            }
        }

        private void HandleDeletedQuestions(List<Question> existingQuestions, IList<UpdateQuestionCommandModel> requests, TestSection testSection)
        {
            var toDelete = existingQuestions
                .Where(q => q != null && q.Id != Guid.Empty && !requests.Any(r => r.Id.HasValue && r.Id == q.Id))
                .ToList();
            if (toDelete.Any())
            {
                DeleteQuestions.AddRange(toDelete);
            }
        }

        private void RemoveObsoleteAISettings(List<TestAISetting> existingSettings, IList<UpdateTestAISettingCommandModel> requests)
        {
            var toDelete = existingSettings
                .Where(x => x.Id != Guid.Empty && !requests.Any(r => r.Id == x.Id))
                .ToList();

            if (toDelete.Any())
            {
                DeleteTestAISettings.AddRange(toDelete);
            }
        }

        private void RemoveObsoleteAICriteriaSettings(List<TestAICriteriaSetting> existingAICriteriaSettings, IList<UpdateTestAICriteriaSettingCommandModel> requests)
        {
            var toDeleteCriteria = existingAICriteriaSettings.Where(x => x.Id != Guid.Empty && !requests.Any(r => r.Id == x.Id))
                                                                       .ToList();
            if (toDeleteCriteria.Any())
            {
                DeleteTestAICriteriaSettings.AddRange(toDeleteCriteria);
            }
        }

        #endregion Add Remove Data

        #region Delete Update

        private async Task<VoidMethodResult> DeleteQuestionsAsync(IList<Question> questions)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            if (questions.Any())
            {
                await _questionRepository.ExecuteTransactionAsync(async () =>
                {
                    await _questionRepository.DeleteListAsync(questions);
                    await _questionRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
                    return methodResult;
                });
            }
            return methodResult;
        }

        private async Task<VoidMethodResult> DeleteTestSectionsAsync(IList<TestSection> testSections)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            if (testSections.Any())
            {
                await _testSectionRepository.ExecuteTransactionAsync(async () =>
                {
                    await _testSectionRepository.DeleteListAsync(testSections);
                    await _testSectionRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
                    return methodResult;
                });
            }
            return methodResult;
        }

        private async Task<VoidMethodResult> DeleteTestAISettingsAsync(IList<TestAISetting> testAISettings)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            if (testAISettings.Any())
            {
                await _testAISettingRepository.ExecuteTransactionAsync(async () =>
                {
                    await _testAISettingRepository.DeleteListAsync(testAISettings);
                    await _testAISettingRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
                    return methodResult;
                });
            }
            return methodResult;
        }

        private async Task<VoidMethodResult> DeleteTestAICriteriaSettingsAsync(IList<TestAICriteriaSetting> testAICriteriaSettings)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            if (testAICriteriaSettings.Any())
            {
                await _testAICriteriaSettingRepository.ExecuteTransactionAsync(async () =>
                {
                    await _testAICriteriaSettingRepository.DeleteListAsync(testAICriteriaSettings);
                    await _testAICriteriaSettingRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
                    return methodResult;
                });
            }
            return methodResult;
        }

        public async Task<VoidMethodResult> DeleteDataAsync(Test test)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            await DeleteTestAISettingsAsync(DeleteTestAISettings);
            await DeleteQuestionsAsync(DeleteQuestions);
            await DeleteTestAICriteriaSettingsAsync(DeleteTestAICriteriaSettings);
            if (DeleteTestSections.Any())
            {
                var allSectionChilrens = await _testSectionRepository.Queryable.Where(x => x.ParentId.HasValue && x.TestId == test.Id)
                                                                     .WhereBulkContains(DeleteTestSections.Select(x => x.Id), x => x.ParentId).ToListAsync();
                var questions = await _testSectionQuestionRepository.Queryable.WhereBulkContains(allSectionChilrens.Select(x => x.Id), x => x.TestSectionId).Select(x => x.Question).ToListAsync();
                var testAISettings = await _testAISettingRepository.Queryable.Include(x => x.TestAICriteriaSettings).WhereBulkContains(allSectionChilrens.Select(x => x.Id), x => x.TestSectionId).ToListAsync();

                await DeleteQuestionsAsync(questions);
                await DeleteTestAISettingsAsync(testAISettings);

                allSectionChilrens.AddRange(DeleteTestSections);
                await DeleteTestSectionsAsync(allSectionChilrens);
            }
            return methodResult;
        }

        #endregion Delete Update

        #region Get

        private async Task<IList<TestSection>> GetTestSectionAsync(TestSection? testSectionParent = null, Test? test = null)
        {
            if (test != null)
            {
                return await _testSectionRepository.Queryable
                    .Where(x => x.TestId == test.Id && x.ParentId == null)
                    .ToListAsync();
            }

            if (testSectionParent != null)
            {
                return await _testSectionRepository.Queryable
                    .Where(x => x.ParentId == testSectionParent.Id)
                    .ToListAsync();
            }

            return new List<TestSection>();
        }

        private async Task<List<Question>> GetExistingQuestionsAsync(Guid sectionId)
        {
            return await _testSectionQuestionRepository.Queryable
                .Where(x => x.TestSectionId == sectionId)
                .Select(x => x.Question)
                .ToListAsync();
        }

        private async Task<List<TestAISetting>> GetExistingAISettingsAsync(Guid sectionId)
        {
            return await _testAISettingRepository.Queryable
                .Include(x => x.TestAICriteriaSettings)
                .Where(x => x.TestSectionId == sectionId)
                .ToListAsync();
        }

        #endregion Get

        #region Update TestSection

        private static void AssignTestReferenceIfNeeded(TestSection section, TestSection? parent, Test? test)
        {
            if (section.Id != Guid.Empty)
            {
                return;
            }
            if (parent != null)
            {
                section.Test = parent.Test;
                parent.TestSections.Add(section);
            }
            else if (test != null)
            {
                section.Test = test;
                test.TestSections.Add(section);
            }
        }

        private TestSection? ResolveTestSectionFromRequest(UpdateTestSectionCommandModel request, IList<TestSection> existingSections, int displayOrder)
        {
            TestSection? testSection = null;
            if (request.Id.HasValue)
            {
                testSection = existingSections.FirstOrDefault(x => x.Id == request.Id);
                _mapper.Map(request, testSection);
            }
            else
            {
                testSection = _mapper.Map<TestSection>(request);
            }

            if (testSection != null)
            {
                testSection.DisplayOrder = displayOrder;
                ValidateIsValid(testSection);
            }
            else
            {
                ValidateObjectExistence(testSection);
            }
            return testSection;
        }

        private async Task HandleSectionChildrenAndConfigsAsync(UpdateTestSectionCommandModel testSectionRequest, TestSection testSection)
        {
            await UpdateSectionRecursive(testSectionRequest.Childrens, testSection);
            await UpdateTestAISettings(testSectionRequest.TestAISettings, testSection);
            await UpdateQuestions(testSectionRequest.Questions, testSection);
        }

        public async Task<MethodResult<Test>> UpdateSectionRecursive(IList<UpdateTestSectionCommandModel>? testSectionRequests, TestSection? testSectionParent = null, Test? test = null)
        {
            MethodResult<Test> methodResult = new MethodResult<Test>();
            testSectionRequests ??= new List<UpdateTestSectionCommandModel>();
            var testSections = await GetTestSectionAsync(testSectionParent, test);
            foreach (var testSectionRequest in testSectionRequests)
            {
                ValidateTestLayout(testSectionRequest);
                var testSection = ResolveTestSectionFromRequest(testSectionRequest, testSections, testSectionRequests.IndexOf(testSectionRequest));
                if (testSection != null)
                {
                    ValidateIsValid(testSection);
                    AssignTestReferenceIfNeeded(testSection, testSectionParent, test);
                    await HandleSectionChildrenAndConfigsAsync(testSectionRequest, testSection);
                }
            }
            RemoveObsoleteTestSections(testSections, testSectionRequests);
            if (!VoidMethodResult.IsOK)
            {
                methodResult.AddErrorBadRequest(VoidMethodResult.ErrorMessages);
                return methodResult;
            }
            methodResult.Result = test;
            return methodResult;
        }

        #endregion Update TestSection

        #region Update Question

        private void HandleSingleQuestionUpdate(UpdateQuestionCommandModel request, List<Question> existingQuestions, TestSection testSection)
        {
            Question? question = null;
            if (request.Id.HasValue)
            {
                question = existingQuestions.FirstOrDefault(q => q.Id == request.Id);
                _mapper.Map(request, question);
            }
            else
            {
                question = _mapper.Map<Question>(request);
            }
            if (question != null)
            {
                var convertResult = _questionConverter.HandleQuestion(question);
                if (!convertResult.IsOK)
                {
                    VoidMethodResult.AddErrorBadRequest(convertResult.ErrorMessages);
                }
                if (convertResult.Result != null)
                {
                    var data = Enumerable.Range(NumberQuestion + 1, convertResult.Result.CorrectTotal).ToList();
                    convertResult.Result.SubQuestionIndexs = data;
                    NumberQuestion += convertResult.Result.CorrectTotal;
                }
                if (question.Id == Guid.Empty)
                {
                    testSection.TestSectionQuestions.Add(new TestSectionQuestion
                    {
                        Question = convertResult.Result ?? question
                    });
                }
                ValidateIsValid(question);
            }
            else
            {
                ValidateObjectExistence(question);
            }
        }

        private async Task UpdateQuestions(IList<UpdateQuestionCommandModel>? questionRequests, TestSection testSection)
        {
            questionRequests ??= new List<UpdateQuestionCommandModel>();
            var existingQuestions = await GetExistingQuestionsAsync(testSection.Id);
            foreach (var questionRequest in questionRequests)
            {
                HandleSingleQuestionUpdate(questionRequest, existingQuestions, testSection);
            }
            HandleDeletedQuestions(existingQuestions, questionRequests, testSection);
        }

        #endregion Update Question

        #region Update TestAISetting

        private async Task UpdateTestAISettings(IList<UpdateTestAISettingCommandModel>? requests, TestSection testSection)
        {
            requests ??= new List<UpdateTestAISettingCommandModel>();
            var existingSettings = await GetExistingAISettingsAsync(testSection.Id);
            foreach (var request in requests)
            {
                HandleTestAISettingUpdate(request, testSection, existingSettings);
            }
            RemoveObsoleteAISettings(existingSettings, requests);
        }

        private void HandleTestAISettingUpdate(UpdateTestAISettingCommandModel request, TestSection section, List<TestAISetting> existingSettings)
        {
            TestAISetting? testAISetting = null;
            if (request.Id.HasValue)
            {
                testAISetting = existingSettings.FirstOrDefault(x => x.Id == request.Id);
                _mapper.Map(request, testAISetting);
            }
            else
            {
                testAISetting = _mapper.Map<TestAISetting>(request);
                section.TestAISettings.Add(testAISetting);
            }
            if (testAISetting != null)
            {
                ValidateIsValid(testAISetting);
                foreach (var criteriaRequest in request.TestAICriteriaSettings)
                {
                    HandleTestAICriteriaSettingUpdate(criteriaRequest, testAISetting);
                }
                RemoveObsoleteAICriteriaSettings(testAISetting.TestAICriteriaSettings.ToList(), request.TestAICriteriaSettings);
            }
            else
            {
                ValidateObjectExistence(testAISetting);
            }
        }

        private void HandleTestAICriteriaSettingUpdate(UpdateTestAICriteriaSettingCommandModel request, TestAISetting setting)
        {
            TestAICriteriaSetting? criteria;
            if (request.Id.HasValue)
            {
                criteria = setting.TestAICriteriaSettings.FirstOrDefault(x => x.Id == request.Id);
                _mapper.Map(request, criteria);
            }
            else
            {
                criteria = _mapper.Map<TestAICriteriaSetting>(request);
                setting.TestAICriteriaSettings.Add(criteria);
            }
            if (criteria != null)
            {
                ValidateIsValid(criteria);
            }
            else
            {
                ValidateObjectExistence(criteria);
            }
        }

        #endregion Update TestAISetting

        #endregion Update Test
    }
}
