// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Common.ExamPracticeHelpers
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPracticeAISettings;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPracticeSections;
    using Fsel.ExamPractice.Domain.Models.CommandModels.Questions;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;

    public class ExamPracticeConverter
    {
        private readonly IExamPracticeSectionRepository _examPracticeSectionRepository;
        private readonly IExamPracticeAISettingRepository _examPracticeAISetting;
        private readonly IQuestionRepository _questionRepository;
        private readonly IMapper _mapper;
        private readonly IExamPracticeAICriteriaSettingRepository _examPracticeAICriteriaSettingRepository;
        private List<Question> _questions = new List<Question>();
        private List<ExamPracticeAISetting> _examPracticeAISettings = new List<ExamPracticeAISetting>();
        private List<ExamPracticeSection> _examPracticeSections = new List<ExamPracticeSection>();
        private List<ExamPracticeAICriteriaSetting> _examPracticeAICriteriaSettings = new List<ExamPracticeAICriteriaSetting>();

        public ExamPracticeConverter(IExamPracticeSectionRepository examPracticeSectionRepository,
                                     IExamPracticeAISettingRepository examPracticeAISetting,
                                     IQuestionRepository questionRepository,
                                     IMapper mapper,
                                     IExamPracticeAICriteriaSettingRepository examPracticeAICriteriaSettingRepository)
        {
            _examPracticeSectionRepository = examPracticeSectionRepository;
            _examPracticeAISetting = examPracticeAISetting;
            _questionRepository = questionRepository;
            _mapper = mapper;
            _examPracticeAICriteriaSettingRepository = examPracticeAICriteriaSettingRepository;
        }

        public async Task HandlerExamPracticeSections(IList<UpdateExamPracticeSectionCommandModel> newExamPracticeSections, ICollection<ExamPracticeSection> examPracticeSectionBelongParents, Guid? examPracticeId, Guid? examPracticeSectionParentId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(newExamPracticeSections);
            ArgumentNullException.ThrowIfNull(examPracticeSectionBelongParents);

            var oldExamPracticeSections = await GetExamPracticeSectionSectionAsync(examPracticeId, examPracticeSectionParentId, cancellationToken);

            // xoá nhưng đối tượng không được update
            var removedPracticeSections = oldExamPracticeSections.Where(x => x.Id != Guid.Empty).ExceptBy(newExamPracticeSections.Select(x => x.Id), u => u.Id).ToList();
            _examPracticeSections.AddRange(removedPracticeSections);

            foreach (var newExamPracticeSection in newExamPracticeSections)
            {
                var countQuestion = 0;

                ExamPracticeSection? examPracticeSection = null;
                if (!newExamPracticeSection.Id.HasValue)
                {
                    examPracticeSection = _mapper.Map<ExamPracticeSection>(newExamPracticeSection);
                    examPracticeSectionBelongParents.Add(examPracticeSection);
                }
                else
                {
                    examPracticeSection = oldExamPracticeSections.FirstOrDefault(x => x.Id == newExamPracticeSection.Id);
                    _mapper.Map(newExamPracticeSection, examPracticeSection);
                }

                if (examPracticeSection != null)
                {
                    if (examPracticeSection.Config != null && examPracticeSection.CourseSkill.HasValue)
                    {
                        var config = examPracticeSection.Config;
                        var executionTime = examPracticeSection.CourseSkill.Value.GetTimeSkill(examPracticeSection.Config.AudioPath);
                        config.ExecutionTime = executionTime;
                        examPracticeSection.Config = config;
                    }
                    examPracticeSection.ExamPracticeId = examPracticeId;
                    examPracticeSection.DisplayOrder = newExamPracticeSections.IndexOf(newExamPracticeSection) + 1;
                    if (newExamPracticeSection.ChildrenExamPracticeSections.Any())
                    {
                        await HandlerChildents(newExamPracticeSection.ChildrenExamPracticeSections, examPracticeSection.ExamPracticeSections, countQuestion, examPracticeSection, cancellationToken);
                    }
                }
            }
        }

        public async Task HandlerChildents(IList<UpdateExamPracticeSectionCommandModel> newExamPracticeSections, ICollection<ExamPracticeSection> examPracticeSectionBelongParents, int countQuestion, ExamPracticeSection? examPracticeSection, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(newExamPracticeSections);
            ArgumentNullException.ThrowIfNull(examPracticeSectionBelongParents);

            var oldExamPracticeSections = await GetExamPracticeSectionSectionAsync(examPracticeSection?.ExamPracticeId, examPracticeSection?.Id, cancellationToken);

            // xoá nhưng đối tượng không được update
            var removedPracticeSections = oldExamPracticeSections.Where(x => x.Id != Guid.Empty).ExceptBy(newExamPracticeSections.Select(x => x.Id), u => u.Id).ToList();
            _examPracticeSections.AddRange(removedPracticeSections);

            foreach (var newExamPracticeSection in newExamPracticeSections)
            {
                ExamPracticeSection? examPracticeSectionChildent = null;
                if (!newExamPracticeSection.Id.HasValue)
                {
                    examPracticeSectionChildent = _mapper.Map<ExamPracticeSection>(newExamPracticeSection);
                    examPracticeSectionBelongParents.Add(examPracticeSectionChildent);
                }
                else
                {
                    examPracticeSectionChildent = oldExamPracticeSections.FirstOrDefault(x => x.Id == newExamPracticeSection.Id);
                    _mapper.Map(newExamPracticeSection, examPracticeSectionChildent);
                }

                if (examPracticeSectionChildent != null)
                {
                    examPracticeSectionChildent.ExamPracticeId = examPracticeSection?.ExamPracticeId;
                    examPracticeSectionChildent.DisplayOrder = newExamPracticeSections.IndexOf(newExamPracticeSection) + 1;
                    QuestionHandler(examPracticeSectionChildent.Questions, newExamPracticeSection.Questions, countQuestion);
                    countQuestion += newExamPracticeSection.Questions.Count;
                    ExamPracticeAISettingHandler(examPracticeSectionChildent.ExamPracticeAISettings, newExamPracticeSection.ExamPracticeAISettings);
                    if (newExamPracticeSection.ChildrenExamPracticeSections.Any())
                    {
                        await HandlerChildents(newExamPracticeSection.ChildrenExamPracticeSections, examPracticeSectionChildent.ExamPracticeSections, countQuestion, examPracticeSectionChildent, cancellationToken);
                    }
                }
            }
        }

        private void QuestionHandler(ICollection<Question> oldQuestions, IList<UpdateQuestionCommandModel> newQuestions, int countQuestion)
        {
            foreach (var newQuestion in newQuestions)
            {
                Question? question = null;

                if (!newQuestion.Id.HasValue)
                {
                    question = _mapper.Map<Question>(newQuestion);
                    oldQuestions.Add(question);
                }
                else
                {
                    question = oldQuestions.FirstOrDefault(x => x.Id == newQuestion.Id);
                    _mapper.Map(newQuestion, question);
                }

                if (question != null)
                {
                    question.DisplayOrder = newQuestions.IndexOf(newQuestion) + countQuestion;
                    question = QuestionHelper.HandleQuestion(question).Result;
                }
            }

            // xoá nhưng đối tượng không được update
            var removedQuestions = oldQuestions.Where(x => x.Id != Guid.Empty).ExceptBy(newQuestions.Select(x => x.Id), u => u.Id).ToList();
            _questions.AddRange(removedQuestions ?? new List<Question>());
        }

        private void ExamPracticeAISettingHandler(ICollection<ExamPracticeAISetting> oldExamPracticeAISettings, IList<ExamPracticeAISettingCommandModel> newExamPracticeAISettings)
        {
            foreach (var newExamPracticeAISetting in newExamPracticeAISettings)
            {
                ExamPracticeAISetting? examPracticeAISetting = null;

                if (!newExamPracticeAISetting.Id.HasValue)
                {
                    examPracticeAISetting = _mapper.Map<ExamPracticeAISetting>(newExamPracticeAISetting);
                    oldExamPracticeAISettings.Add(examPracticeAISetting);
                }
                else
                {
                    examPracticeAISetting = oldExamPracticeAISettings.FirstOrDefault(x => x.Id == newExamPracticeAISetting.Id);
                    _mapper.Map(newExamPracticeAISetting, examPracticeAISetting);
                }

                if (newExamPracticeAISetting.ExamPracticeAICriteriaSettings != null && newExamPracticeAISetting.ExamPracticeAICriteriaSettings.Any() && examPracticeAISetting != null)
                {
                    ExamPracticeAICriteriaSettingHandler(examPracticeAISetting.ExamPracticeAICriteriaSettings, newExamPracticeAISetting.ExamPracticeAICriteriaSettings);
                }
            }

            // xoá nhưng đối tượng không được update
            var removedExamPracticeAISettings = oldExamPracticeAISettings.Where(x => x.Id != Guid.Empty).ExceptBy(newExamPracticeAISettings.Select(x => x.Id), u => u.Id).ToList();
            _examPracticeAISettings.AddRange(removedExamPracticeAISettings ?? new List<ExamPracticeAISetting>());
        }

        private void ExamPracticeAICriteriaSettingHandler(ICollection<ExamPracticeAICriteriaSetting> oldExamPracticeAICriteriaSettings, IList<ExamPracticeAICriteriaSettingCommandModel> newExamPracticeAICriteriaSettings)
        {
            foreach (var newExamPracticeAICriteriaSetting in newExamPracticeAICriteriaSettings)
            {
                ExamPracticeAICriteriaSetting? examPracticeAICriteriaSetting = null;

                if (!newExamPracticeAICriteriaSetting.Id.HasValue)
                {
                    examPracticeAICriteriaSetting = _mapper.Map<ExamPracticeAICriteriaSetting>(newExamPracticeAICriteriaSetting);
                    oldExamPracticeAICriteriaSettings.Add(examPracticeAICriteriaSetting);
                }
                else
                {
                    examPracticeAICriteriaSetting = oldExamPracticeAICriteriaSettings.FirstOrDefault(x => x.Id == newExamPracticeAICriteriaSetting.Id);
                    _mapper.Map(newExamPracticeAICriteriaSetting, examPracticeAICriteriaSetting);
                }
            }

            // xoá nhưng đối tượng không được update
            var removedExamPracticeAICriteriaSettings = oldExamPracticeAICriteriaSettings.Where(x => x.Id != Guid.Empty).ExceptBy(newExamPracticeAICriteriaSettings.Select(x => x.Id), u => u.Id).ToList();
            _examPracticeAICriteriaSettings.AddRange(removedExamPracticeAICriteriaSettings ?? new List<ExamPracticeAICriteriaSetting>());
        }

        private async Task<IList<ExamPracticeSection>> GetExamPracticeSectionSectionAsync(Guid? examPracticeId, Guid? examPracticeSectionParentId, CancellationToken cancellationToken)
        {
            if (examPracticeSectionParentId.HasValue)
            {
                return await _examPracticeSectionRepository.Queryable
                                                           .Include(x => x.Questions)
                                                           .Include(x => x.ExamPracticeAISettings)
                                                           .ThenInclude(x => x.ExamPracticeAICriteriaSettings)
                                                           .Where(x => x.ParentExamPracticeSectionId == examPracticeSectionParentId)
                                                           .ToListAsync(cancellationToken);
            }

            if (examPracticeId.HasValue)
            {
                return await _examPracticeSectionRepository.Queryable
                                                           .Where(x => x.ExamPracticeId == examPracticeId && !x.ParentExamPracticeSectionId.HasValue)
                                                           .ToListAsync(cancellationToken);
            }

            return new List<ExamPracticeSection>();
        }

        public async Task DeleteObjectInstance()
        {
            await _questionRepository.DeleteListAsync(_questions);
            await _examPracticeAISetting.DeleteListAsync(_examPracticeAISettings);
            await _examPracticeSectionRepository.DeleteListAsync(_examPracticeSections);
            await _examPracticeAICriteriaSettingRepository.DeleteListAsync(_examPracticeAICriteriaSettings);
        }
    }
}
