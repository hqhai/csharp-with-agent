// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.TestHelper
{
    using System;
    using System.Collections.Generic;
    using AutoMapper;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Models.CommandModels.Questions;
    using Fsel.Course.Domain.Models.CommandModels.TestAiSettings;
    using Fsel.Course.Domain.Models.CommandModels.Tests;
    using Fsel.Course.Domain.Models.CommandModels.TestSections;
    using Fsel.Shared.Helpers;

    public class TestFactory
    {
        private readonly UpdateTestCommandModel _createRequest;
        private readonly IMapper _mapper;
        private readonly QuestionConverter _questionConverter;

        protected TestFactory(UpdateTestCommandModel createRequest, IMapper mapper, QuestionConverter questionConverter)
        {
            _createRequest = createRequest;
            _mapper = mapper;
            _questionConverter = questionConverter;
        }

        public Test Build(Guid? originalId = null, bool isUsingByClient = false)
        {
            var test = _mapper.Map<Test>(_createRequest);
            test.Id = Guid.Empty;
            test.OriginalId = originalId.HasValue ? originalId.Value : Guid.NewGuid();
            test.VersionStatus = Fsel.Common.Enums.EnumVersionStatus.LastVersion;
            test.TestSections = TestSectionClassification(_createRequest.TestSections, test, new ConfigCounter() { IsUsingByClient = isUsingByClient }).ToList();
            return test;
        }

        private IEnumerable<TestSection> TestSectionClassification(IList<UpdateTestSectionCommandModel>? testSectionRequests, Test test, ConfigCounter configCounter)
        {
            if (testSectionRequests != null && testSectionRequests.Any())
            {
                for (var i = 0; i < testSectionRequests.Count; i++)
                {
                    var numberQuestion = configCounter.CountQuestion;
                    var testSectionRequest = testSectionRequests[i];
                    var testSection = _mapper.Map<TestSection>(testSectionRequest);
                    if (configCounter.IsUsingByClient)
                    {
                        testSection.Id = Guid.Empty;
                    }

                    testSection.DisplayOrder = i + 1;
                    testSection.Test = test;
                    testSection.TestSections = TestSectionClassification(testSectionRequest.Childrens, test, configCounter).ToList();
                    testSection.TestAISettings = TestAISettingClassification(testSectionRequest.TestAISettings, configCounter).ToList();
                    testSection.TestSectionQuestions = TestSectionQuestionClassification(testSectionRequest.Questions, configCounter).ToList();
                    if (testSectionRequest.Questions.Any())
                    {
                        var data = Enumerable.Range(numberQuestion + 1, configCounter.CountQuestion).ToList();
                        testSection.SubQuestionIndexs = data;
                    }
                    yield return testSection;
                }
            }
        }

        private IEnumerable<TestSectionQuestion> TestSectionQuestionClassification(IList<UpdateQuestionCommandModel>? questionRequests, ConfigCounter configCounter)
        {
            if (questionRequests != null && questionRequests.Any())
            {
                for (var i = 0; i < questionRequests.Count; i++)
                {
                    var questionRequest = questionRequests[i];
                    var question = _mapper.Map<Question>(questionRequest);
                    if (configCounter.IsUsingByClient)
                    {
                        question.Id = Guid.Empty;
                    }
                    question = _questionConverter.HandleQuestion(question).Result;
                    if (question != null)
                    {
                        var data = Enumerable.Range(configCounter.CountQuestion + 1, question.CorrectTotal + configCounter.CountQuestion).ToList();
                        question.SubQuestionIndexs = data;
                        configCounter.CountQuestion += question.CorrectTotal;
                    }
                    yield return new TestSectionQuestion
                    {
                        Question = question ?? new Question(),
                    };
                }
            }
        }

        private IEnumerable<TestAISetting> TestAISettingClassification(IList<UpdateTestAISettingCommandModel>? testAISettingRequests, ConfigCounter configCounter)
        {
            if (testAISettingRequests != null && testAISettingRequests.Any())
            {
                for (var i = 0; i < testAISettingRequests.Count; i++)
                {
                    var testAISettingRequest = testAISettingRequests[i];
                    var testAISetting = _mapper.Map<TestAISetting>(testAISettingRequest);
                    if (configCounter.IsUsingByClient)
                    {
                        testAISetting.Id = Guid.Empty;
                    }
                    testAISetting.TestAICriteriaSettings = TestAICriteriaSettingClassification(testAISettingRequest.TestAICriteriaSettings, configCounter).ToList();
                    yield return testAISetting;
                }
            }
        }

        private IEnumerable<TestAICriteriaSetting> TestAICriteriaSettingClassification(IList<UpdateTestAICriteriaSettingCommandModel>? criteriaSettingRequests, ConfigCounter configCounter)
        {
            if (criteriaSettingRequests != null && criteriaSettingRequests.Any())
            {
                foreach (var criteriaRequest in criteriaSettingRequests)
                {
                    var criteria = _mapper.Map<TestAICriteriaSetting>(criteriaRequest);
                    if (configCounter.IsUsingByClient)
                    {
                        criteria.Id = Guid.Empty;
                    }
                    yield return criteria;
                }
            }
        }

        public static TestFactory Create(UpdateTestCommandModel createRequest, IMapper mapper, QuestionConverter questionConverter)
        {
            return new TestFactory(createRequest, mapper, questionConverter);
        }

        public class ConfigCounter
        {
            public int CountQuestion { get; set; }
            public bool IsUsingByClient { get; set; }
        }
    }
}
