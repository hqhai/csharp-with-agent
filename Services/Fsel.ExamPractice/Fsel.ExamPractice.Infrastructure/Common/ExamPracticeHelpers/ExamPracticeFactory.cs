// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Common.ExamPracticeHelpers
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using Fsel.Common.Enums;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPracticeAISettings;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPractices;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPracticeSections;
    using Fsel.ExamPractice.Domain.Models.CommandModels.Questions;

    public class ExamPracticeFactory
    {
        private readonly UpdateExamPracticeCommandModel _createRequest;
        private readonly IMapper _mapper;
        private int _countQuestion;

        protected ExamPracticeFactory(UpdateExamPracticeCommandModel createRequest, IMapper mapper)
        {
            _createRequest = createRequest;
            _mapper = mapper;
        }

        public ExamPractice Build(int version = 0, Guid? originalId = null)
        {
            var examPractice = _mapper.Map<ExamPractice>(_createRequest);
            examPractice.VersionStatus = EnumVersionStatus.LastVersion;
            examPractice.Version = version;
            examPractice.OriginalId = originalId.HasValue ? originalId.Value : examPractice.Id;
            examPractice.Status = EnumExamPracticeStatus.Inactive;
            examPractice.ExamPracticeSections = ExamPracticeSectionClassification(_createRequest.ExamPracticeSections).ToList();
            return examPractice;
        }

        private IEnumerable<ExamPracticeSection> ExamPracticeSectionClassification(IList<UpdateExamPracticeSectionCommandModel>? examPracticeSectionRequests)
        {
            if (examPracticeSectionRequests != null && examPracticeSectionRequests.Any())
            {
                for (var i = 0; i < examPracticeSectionRequests.Count; i++)
                {
                    var numberQuestion = _countQuestion;
                    var examPracticeSectionRequest = examPracticeSectionRequests[i];
                    var examPracticeSection = _mapper.Map<ExamPracticeSection>(examPracticeSectionRequest);

                    examPracticeSection.DisplayOrder = i + 1;
                    examPracticeSection.ExamPracticeSections = ExamPracticeSectionClassification(examPracticeSectionRequest.ChildrenExamPracticeSections).ToList();
                    examPracticeSection.ExamPracticeAISettings = ExamPracticeAISettingClassification(examPracticeSectionRequest.ExamPracticeAISettings).ToList();
                    examPracticeSection.Questions = ExamPracticeQuestionClassification(examPracticeSectionRequest.Questions).ToList();

                    if (examPracticeSection.Questions != null && examPracticeSection.Questions.Any())
                    {
                        var data = Enumerable.Range(numberQuestion++, _countQuestion).ToList();
                        examPracticeSection.SubQuestionIndexs = data;
                    }

                    yield return examPracticeSection;
                }
            }
        }

        private IEnumerable<Question> ExamPracticeQuestionClassification(IList<UpdateQuestionCommandModel>? questionRequests)
        {
            if (questionRequests != null && questionRequests.Any())
            {
                for (var i = 0; i < questionRequests.Count; i++)
                {
                    var questionRequest = questionRequests[i];
                    var question = _mapper.Map<Question>(questionRequest);
                    question = QuestionHelper.HandleQuestion(question).Result;
                    if (question != null)
                    {
                        var numberQuestion = _countQuestion;
                        _countQuestion += question.CorrectTotal;
                        var data = Enumerable.Range(numberQuestion++, _countQuestion).ToList();
                        question.SubQuestionIndexs = data;
                    }

                    yield return question ?? new Question();
                }
            }
        }

        private IEnumerable<ExamPracticeAISetting> ExamPracticeAISettingClassification(IList<ExamPracticeAISettingCommandModel>? examPracticeAISettingRequests)
        {
            if (examPracticeAISettingRequests != null && examPracticeAISettingRequests.Any())
            {
                for (var i = 0; i < examPracticeAISettingRequests.Count; i++)
                {
                    var examPracticeAISettingRequest = examPracticeAISettingRequests[i];
                    var examPracticeAISetting = _mapper.Map<ExamPracticeAISetting>(examPracticeAISettingRequest);
                    examPracticeAISetting.ExamPracticeAICriteriaSettings = ExamPracticeAICriteriaSettingClassification(examPracticeAISettingRequest.ExamPracticeAICriteriaSettings).ToList();
                    yield return examPracticeAISetting;
                }
            }
        }

        private IEnumerable<ExamPracticeAICriteriaSetting> ExamPracticeAICriteriaSettingClassification(IList<ExamPracticeAICriteriaSettingCommandModel>? criteriaSettingRequests)
        {
            if (criteriaSettingRequests != null && criteriaSettingRequests.Any())
            {
                foreach (var criteriaRequest in criteriaSettingRequests)
                {
                    var criteria = _mapper.Map<ExamPracticeAICriteriaSetting>(criteriaRequest);
                    yield return criteria;
                }
            }
        }

        public static ExamPracticeFactory Create(UpdateExamPracticeCommandModel createRequest, IMapper mapper)
        {
            return new ExamPracticeFactory(createRequest, mapper);
        }
    }
}
