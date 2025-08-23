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

        public async Task HandlerChildents(IList<UpdateExamPracticeSectionCommandModel> newExamPracticeSections, ICollection<ExamPracticeSection> examPracticeSectionBelongParents, Guid? examPracticeId, Guid? examPracticeSectionParentId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(newExamPracticeSections);
            ArgumentNullException.ThrowIfNull(examPracticeSectionBelongParents);

            var oldExamPracticeSections = await GetExamPracticeSectionSectionAsync(examPracticeId, examPracticeSectionParentId, cancellationToken);
            if (oldExamPracticeSections != null && oldExamPracticeSections.Any())
            {
                // xoá nhưng đối tượng không được update
                var removedPracticeSections = oldExamPracticeSections.Where(x => x.Id != Guid.Empty).ExceptBy(newExamPracticeSections.Select(x => x.Id), u => u.Id).ToList();
                _examPracticeSections.AddRange(removedPracticeSections);
            }

            foreach (var newExamPracticeSection in newExamPracticeSections)
            {
                if (!newExamPracticeSection.Id.HasValue)
                {
                    examPracticeSectionBelongParents.Add(_mapper.Map<ExamPracticeSection>(newExamPracticeSection));
                }
                else
                {
                    var oldExamPracticeSection = oldExamPracticeSections?.FirstOrDefault(x => x.Id == newExamPracticeSection.Id);
                    _mapper.Map(newExamPracticeSection, oldExamPracticeSection);
                    if (oldExamPracticeSection == null)
                    {
                        continue;
                    }

                    QuestionHandler(oldExamPracticeSection.Questions, newExamPracticeSection.Questions);
                    ExamPracticeAISettingHandler(oldExamPracticeSection.ExamPracticeAISettings, newExamPracticeSection.ExamPracticeAISettings);

                    if (newExamPracticeSection.ChildrenExamPracticeSections.Any())
                    {
                        await HandlerChildents(newExamPracticeSection.ChildrenExamPracticeSections, oldExamPracticeSection.ExamPracticeSections, null, oldExamPracticeSection.Id, cancellationToken);
                    }
                }
            }
        }

        private void QuestionHandler(ICollection<Question> oldQuestions, IList<UpdateQuestionCommandModel> newQuestions)
        {
            foreach (var newQuestion in newQuestions)
            {
                if (!newQuestion.Id.HasValue)
                {
                    oldQuestions.Add(_mapper.Map<Question>(newQuestion));
                }
                else
                {
                    var oldQuestion = oldQuestions.FirstOrDefault(x => x.Id == newQuestion.Id);
                    _mapper.Map(newQuestion, oldQuestion);
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
                if (!newExamPracticeAISetting.Id.HasValue)
                {
                    oldExamPracticeAISettings.Add(_mapper.Map<ExamPracticeAISetting>(newExamPracticeAISetting));
                }
                else
                {
                    var oldExamPracticeAISetting = oldExamPracticeAISettings.FirstOrDefault(x => x.Id == newExamPracticeAISetting.Id);
                    if (oldExamPracticeAISetting == null)
                    {
                        continue;
                    }

                    _mapper.Map(newExamPracticeAISetting, oldExamPracticeAISetting);
                    if (newExamPracticeAISetting.ExamPracticeAICriteriaSettings != null && newExamPracticeAISetting.ExamPracticeAICriteriaSettings.Any())
                    {
                        ExamPracticeAICriteriaSettingHandler(oldExamPracticeAISetting.ExamPracticeAICriteriaSettings, newExamPracticeAISetting.ExamPracticeAICriteriaSettings);
                    }
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
                if (!newExamPracticeAICriteriaSetting.Id.HasValue)
                {
                    oldExamPracticeAICriteriaSettings.Add(_mapper.Map<ExamPracticeAICriteriaSetting>(newExamPracticeAICriteriaSetting));
                }
                else
                {
                    var oldExamPracticeAICriteriaSetting = oldExamPracticeAICriteriaSettings.FirstOrDefault(x => x.Id == newExamPracticeAICriteriaSetting.Id);
                    _mapper.Map(newExamPracticeAICriteriaSetting, oldExamPracticeAICriteriaSetting);
                }
            }

            // xoá nhưng đối tượng không được update
            var removedExamPracticeAICriteriaSettings = oldExamPracticeAICriteriaSettings.Where(x => x.Id != Guid.Empty).ExceptBy(newExamPracticeAICriteriaSettings.Select(x => x.Id), u => u.Id).ToList();
            _examPracticeAICriteriaSettings.AddRange(removedExamPracticeAICriteriaSettings ?? new List<ExamPracticeAICriteriaSetting>());
        }

        private async Task<IList<ExamPracticeSection>> GetExamPracticeSectionSectionAsync(Guid? examPracticeId, Guid? examPracticeSectionParentId, CancellationToken cancellationToken)
        {
            if (examPracticeId.HasValue)
            {
                return await _examPracticeSectionRepository.Queryable
                                                           .Include(x => x.Questions)
                                                           .Include(x => x.ExamPracticeAISettings)
                                                           .ThenInclude(x => x.ExamPracticeAICriteriaSettings)
                                                           .Where(x => x.ExamPracticeId == examPracticeId && !x.ParentExamPracticeSectionId.HasValue)
                                                           .ToListAsync(cancellationToken);
            }

            if (examPracticeSectionParentId.HasValue)
            {
                return await _examPracticeSectionRepository.Queryable
                                                           .Include(x => x.Questions)
                                                           .Include(x => x.ExamPracticeAISettings)
                                                           .ThenInclude(x => x.ExamPracticeAICriteriaSettings)
                                                           .Where(x => x.ParentExamPracticeSectionId == examPracticeSectionParentId)
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
