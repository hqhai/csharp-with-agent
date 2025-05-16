// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Common
{
    using System.Linq.Dynamic.Core;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPractices;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPracticeSections;
    using Microsoft.EntityFrameworkCore;

    public class ExamPracticeHelper
    {
        private readonly IMapper _mapper;
        private readonly IExamPracticeSectionRepository _examPracticeSectionRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly QuestionHelper _questionHelper;

        public ExamPracticeHelper(IMapper mapper,
            IExamPracticeSectionRepository examPracticeSectionRepository,
            IQuestionRepository questionRepository,
            QuestionHelper questionHelper)
        {
            _mapper = mapper;
            _examPracticeSectionRepository = examPracticeSectionRepository;
            _questionRepository = questionRepository;
            _questionHelper = questionHelper;
        }

        public async Task<bool> IsChangeValueActive(ExamPractice examPractice, UpdateExamPracticeCommandModel request)
        {
            if (examPractice == null)
            {
                return false;
            }
            if (request == null)
            {
                return false;
            }

            if (examPractice.ExecutionTime != request.ExecutionTime)
            {
                return true;
            }
            if (request.ExamPracticeSections.Any(x => !x.Id.HasValue))
            {
                return true;
            }

            var examPracticeSections = await _examPracticeSectionRepository.Queryable.Where(x => x.ExamPracticeId == examPractice.Id).ToListAsync();
            if (examPracticeSections.Count != request.ExamPracticeSections.Count)
            {
                return true;
            }
            foreach (var item in request.ExamPracticeSections)
            {
                var examPracticeSection = examPracticeSections.FirstOrDefault(x => x.Id == item.Id);
                if (examPracticeSection == null)
                {
                    return true;
                }
                if (item.DisplayOrder != examPracticeSection.DisplayOrder)
                {
                    return true;
                }
                if (item.Type != examPracticeSection.Type)
                {
                    return true;
                }
                if (item.Config?.Instruction != examPracticeSection.Config?.Instruction)
                {
                    return true;
                }
                foreach (var questionRequest in item.Questions)
                {
                    var question = examPracticeSection.Questions.FirstOrDefault(x => x.Id == questionRequest.Id);
                    if (question == null)
                    {
                        return true;
                    }
                    if (question.QuestionType != questionRequest.QuestionType)
                    {
                        return true;
                    }
                    if (question.CorrectTotal != questionRequest.CorrectTotal)
                    {
                        return true;
                    }
                    if (question.Config.Serialize() != questionRequest.Config.Serialize())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsValidate(CreateExamPracticeCommandModel request)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (request.IsDraft)
            {
                return true;
            }

            return true;
        }

        public async Task<bool> IsValidateActiveStatus(ExamPractice examPractice)
        {
            var examPracticeSections = await _examPracticeSectionRepository.Queryable.Where(x => x.ExamPracticeId == examPractice.Id).ToListAsync();
            var currentSections = await GetLeafSectionsWithQuestionsAsync(examPracticeSections);
            return AllSectionsHaveAtLeastOneQuestion(currentSections);
        }

        private async Task<List<ExamPracticeSection>> GetLeafSectionsWithQuestionsAsync(List<ExamPracticeSection> sections)
        {
            var result = new List<ExamPracticeSection>();

            foreach (var section in sections)
            {
                // Tìm các section con
                var subSections = await _examPracticeSectionRepository.Queryable
                    .Where(x => x.ParentExamPracticeSectionId == section.Id)
                    .ToListAsync();

                if (subSections.Any())
                {
                    var childLeafSections = await GetLeafSectionsWithQuestionsAsync(subSections);
                    result.AddRange(childLeafSections);
                }
                else
                {
                    section.Questions = await _questionRepository.Queryable
                        .Where(q => q.ExamPracticeSectionId == section.Id)
                        .ToListAsync();

                    result.Add(section);
                }
            }

            return result;
        }

        public bool AllSectionsHaveAtLeastOneQuestion(IEnumerable<ExamPracticeSection> sections)
        {
            if (sections == null)
            {
                sections = new List<ExamPracticeSection>();
            }
            foreach (var section in sections)
            {
                if (section.Questions == null || !section.Questions.Any())
                {
                    return false;
                }

                foreach (var question in section.Questions)
                {
                    if (QuestionTypeHelper.ValidateQuestionExamPractice(question.Config, question.QuestionType))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool AllSectionsHaveAtLeastOneQuestion(IEnumerable<CreateExamPracticeSectionCommandModel> sections)
        {
            if (sections == null)
            {
                sections = new List<CreateExamPracticeSectionCommandModel>();
            }
            foreach (var section in sections)
            {
                if (!section.Type.HasValue)
                {
                    return false;
                }
                if (section.Config == null || string.IsNullOrEmpty(section.Config.Instruction))
                {
                    return false;
                }
                if (section.Questions == null || !section.Questions.Any())
                {
                    return false;
                }

                foreach (var question in section.Questions)
                {
                    if (QuestionTypeHelper.ValidateQuestionExamPractice(question.Config, question.QuestionType))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool AllSectionsHaveAtLeastOneQuestion(IEnumerable<UpdateExamPracticeSectionCommandModel> sections)
        {
            if (sections == null)
            {
                sections = new List<UpdateExamPracticeSectionCommandModel>();
            }
            foreach (var section in sections)
            {
                if (!section.Type.HasValue)
                {
                    return false;
                }
                if (section.Config == null || string.IsNullOrEmpty(section.Config.Instruction))
                {
                    return false;
                }
                if (section.Questions == null || !section.Questions.Any())
                {
                    return false;
                }

                foreach (var question in section.Questions)
                {
                    if (QuestionTypeHelper.ValidateQuestionExamPractice(question.Config, question.QuestionType))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public IList<UpdateExamPracticeSectionCommandModel> GetLeafSections(IList<UpdateExamPracticeSectionCommandModel> sections)
        {
            var result = new List<UpdateExamPracticeSectionCommandModel>();
            if (sections == null)
            {
                sections = new List<UpdateExamPracticeSectionCommandModel>();
            }
            foreach (var section in sections)
            {
                if (section.ChildrenExamPracticeSections == null || !section.ChildrenExamPracticeSections.Any())
                {
                    result.Add(section);
                }
                else
                {
                    result.AddRange(GetLeafSections(section.ChildrenExamPracticeSections));
                }
            }
            return result;
        }

        public IList<CreateExamPracticeSectionCommandModel> GetLeafSections(IList<CreateExamPracticeSectionCommandModel> sections)
        {
            var result = new List<CreateExamPracticeSectionCommandModel>();
            if (sections == null)
            {
                sections = new List<CreateExamPracticeSectionCommandModel>();
            }
            foreach (var section in sections)
            {
                if (section.ChildrenExamPracticeSections == null || !section.ChildrenExamPracticeSections.Any())
                {
                    result.Add(section);
                }
                else
                {
                    result.AddRange(GetLeafSections(section.ChildrenExamPracticeSections));
                }
            }
            return result;
        }

        public VoidMethodResult MapSectionsRecursively(IList<CreateExamPracticeSectionCommandModel> examPracticeSections, IList<ExamPracticeSection> targetList)
        {
            var voidMethodResult = new VoidMethodResult();
            if (examPracticeSections == null)
            {
                return voidMethodResult;
            }
            if (targetList == null)
            {
                targetList = new List<ExamPracticeSection>();
            }
            foreach (var examPracticeSectionRequest in examPracticeSections)
            {
                var examPracticeSection = _mapper.Map<ExamPracticeSection>(examPracticeSectionRequest);
                if (!examPracticeSection.IsValid())
                {
                    voidMethodResult.AddErrorBadRequest(examPracticeSection.ErrorMessages);
                    return voidMethodResult;
                }
                targetList.Add(examPracticeSection);

                // Nếu có Children, gọi đệ quy
                if (examPracticeSectionRequest.ChildrenExamPracticeSections != null &&
                    examPracticeSectionRequest.ChildrenExamPracticeSections.Any())
                {
                    var childSections = new List<ExamPracticeSection>();
                    var methodChildren = MapSectionsRecursively(examPracticeSectionRequest.ChildrenExamPracticeSections, childSections);
                    if (!methodChildren.IsOK)
                    {
                        voidMethodResult.AddErrorBadRequest(methodChildren.ErrorMessages);
                        return voidMethodResult;
                    }
                    examPracticeSection.ExamPracticeSections = childSections;
                }

                // Nếu không có children nhưng có questions
                if (examPracticeSectionRequest.Questions != null && examPracticeSectionRequest.Questions.Any())
                {
                    var questions = new List<Question>();
                    foreach (var questionRequest in examPracticeSectionRequest.Questions)
                    {
                        var question = _mapper.Map<Question>(questionRequest);
                        if (!question.IsValid())
                        {
                            voidMethodResult.AddErrorBadRequest(question.ErrorMessages);
                            return voidMethodResult;
                        }
                        var method = _questionHelper.HandleQuestion(question, true);
                        if (!method.IsOK)
                        {
                            voidMethodResult.AddErrorBadRequest(method.ErrorMessages);
                            return voidMethodResult;
                        }
                        question = method.Result ?? question;
                        questions.Add(question);
                    }

                    examPracticeSection.Questions = questions;
                }
            }
            return voidMethodResult;
        }

        public async Task<VoidMethodResult> MapSectionsRecursively(ExamPractice examPractice, IList<UpdateExamPracticeSectionCommandModel> examPracticeSections, IList<ExamPracticeSection> targetList)
        {
            var voidMethodResult = new VoidMethodResult();

            if (examPracticeSections == null)
            {
                return voidMethodResult;
            }
            if (targetList == null)
            {
                targetList = new List<ExamPracticeSection>();
            }
            foreach (var examPracticeSectionRequest in examPracticeSections)
            {
                ExamPracticeSection? examPracticeSection;
                if (examPracticeSectionRequest.Id.HasValue)
                {
                    examPracticeSection = await _examPracticeSectionRepository.Queryable
                        .Include(x => x.ExamPracticeSections)
                        .FirstOrDefaultAsync(x => x.Id == examPracticeSectionRequest.Id.Value);
                    if (examPracticeSection == null)
                    {
                        voidMethodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeSectionRequest), examPracticeSectionRequest.Id);
                        return voidMethodResult;
                    }
                    _mapper.Map(examPracticeSectionRequest, examPracticeSection);
                    if (!examPracticeSection.IsValid())
                    {
                        voidMethodResult.AddErrorBadRequest(examPracticeSection.ErrorMessages);
                        return voidMethodResult;
                    }
                }
                else
                {
                    examPracticeSection = _mapper.Map<ExamPracticeSection>(examPracticeSectionRequest);
                    if (!examPracticeSection.IsValid())
                    {
                        voidMethodResult.AddErrorBadRequest(examPracticeSection.ErrorMessages);
                        return voidMethodResult;
                    }
                }

                targetList.Add(examPracticeSection);
                // Đệ quy xử lý children nếu có
                if (examPracticeSectionRequest.ChildrenExamPracticeSections != null && examPracticeSectionRequest.ChildrenExamPracticeSections.Any())
                {
                    var childSections = new List<ExamPracticeSection>();
                    var childResult = await MapSectionsRecursively(examPractice, examPracticeSectionRequest.ChildrenExamPracticeSections, childSections);
                    if (!childResult.IsOK)
                    {
                        voidMethodResult.AddErrorBadRequest(childResult.ErrorMessages);
                        return voidMethodResult;
                    }
                    var requestIds = examPracticeSectionRequest.ChildrenExamPracticeSections
                        .Where(c => c.Id.HasValue)
                        .Select(c => c.Id.GetValueOrDefault())
                        .ToHashSet();

                    var toRemove = examPracticeSection.ExamPracticeSections
                        .Where(c => c.Id != Guid.Empty && !requestIds.Contains(c.Id))
                        .ToList();

                    var questions = await _questionRepository.Queryable.WhereBulkContains(toRemove.Select(x => x.Id), x => x.ExamPracticeSectionId).ToListAsync();
                    if (toRemove.Any())
                    {
                        await _examPracticeSectionRepository.ExecuteTransactionAsync(async () =>
                        {
                            await _examPracticeSectionRepository.DeleteListAsync(toRemove);
                            await _examPracticeSectionRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
                            if (questions.Any())
                            {
                                await _questionRepository.DeleteListAsync(questions);
                                await _questionRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
                            }

                            return voidMethodResult;
                        });
                    }

                    examPracticeSection.ExamPracticeSections = childSections;
                }
                else
                {
                    examPracticeSection.ExamPracticeSections.Clear();
                }
                // Nếu không có children thì cập nhật questions (nếu có)
                if (examPracticeSectionRequest.Questions != null && examPracticeSectionRequest.Questions.Any())
                {
                    var listQuestion = await _questionRepository.Queryable.Where(x => x.ExamPracticeSectionId == examPracticeSectionRequest.Id).ToListAsync();

                    var questions = new List<Question>();
                    foreach (var questionRequest in examPracticeSectionRequest.Questions)
                    {
                        Question? question;
                        if (questionRequest.Id.HasValue)
                        {
                            question = await _questionRepository.GetByIdAsync(questionRequest.Id.Value);
                            if (question == null)
                            {
                                voidMethodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question), questionRequest.Id);
                                return voidMethodResult;
                            }

                            _mapper.Map(questionRequest, question);
                        }
                        else
                        {
                            question = _mapper.Map<Question>(questionRequest);
                        }
                        var method = _questionHelper.HandleQuestion(question, true);
                        if (!method.IsOK)
                        {
                            voidMethodResult.AddErrorBadRequest(method.ErrorMessages);
                            return voidMethodResult;
                        }
                        question = method.Result ?? question;
                        if (!question.IsValid())
                        {
                            voidMethodResult.AddErrorBadRequest(question.ErrorMessages);
                            return voidMethodResult;
                        }
                        questions.Add(question);
                    }
                    var requestQIds = examPracticeSectionRequest.Questions
                                            .Where(q => q.Id.HasValue)
                                            .Select(q => q.Id.GetValueOrDefault())
                                            .ToHashSet();

                    var toRemoveQs = listQuestion
                        .Where(q => q.Id != Guid.Empty && !requestQIds.Contains(q.Id))
                        .ToList();
                    if (toRemoveQs.Any())
                    {
                        await _questionRepository.ExecuteTransactionAsync(async () =>
                        {
                            await _questionRepository.DeleteListAsync(toRemoveQs);
                            await _questionRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
                            return voidMethodResult;
                        });
                    }

                    examPracticeSection.Questions = questions;
                }
                else
                {
                    examPracticeSection.Questions.Clear();
                }
            }

            return voidMethodResult;
        }

        public async Task<VoidMethodResult> DeleteExamPracticeSectionsAsync(UpdateExamPracticeCommandModel request)
        {
            ArgumentNullException.ThrowIfNull(request);
            var voidMethodResult = new VoidMethodResult();
            var examPracticeSectionIds = request.ExamPracticeSections
                               .Where(c => c.Id.HasValue)
                               .Select(c => c.Id.GetValueOrDefault())
                               .ToHashSet();
            var examPracticeSections = await _examPracticeSectionRepository.Queryable
                                                .Where(x => x.ExamPracticeId.HasValue && x.ExamPracticeId == request.Id)
                                                .ToListAsync();

            var toRemoves = examPracticeSections.Where(c => c.Id != Guid.Empty && !examPracticeSectionIds.Contains(c.Id)).ToList();
            var listExamPracticeSections = await GetLeafExamPracticeSectionsAsync(toRemoves);
            listExamPracticeSections.AddRange(toRemoves);
            var questions = await _questionRepository.Queryable.WhereBulkContains(listExamPracticeSections.Select(x => x.Id), x => x.ExamPracticeSectionId).ToListAsync();

            await _examPracticeSectionRepository.ExecuteTransactionAsync(async () =>
            {
                await _examPracticeSectionRepository.DeleteListAsync(listExamPracticeSections);
                await _examPracticeSectionRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

                await _questionRepository.DeleteListAsync(questions);
                await _questionRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

                return voidMethodResult;
            });
            return voidMethodResult;
        }

        private async Task<List<ExamPracticeSection>> GetLeafExamPracticeSectionsAsync(List<ExamPracticeSection> sections)
        {
            // Lấy tất cả các section con của các section đầu vào
            var childSections = await _examPracticeSectionRepository.Queryable
                .Where(x => x.ParentExamPracticeSectionId.HasValue && sections.Select(s => s.Id).Contains(x.ParentExamPracticeSectionId.Value))
                .ToListAsync();

            // Lọc ra những section hiện tại không có con -> nút lá
            var leafSections = sections
                .Where(s => !childSections.Any(cs => cs.ParentExamPracticeSectionId == s.Id))
                .ToList();

            // Nếu không có section con -> kết thúc
            if (!childSections.Any())
            {
                return leafSections;
            }

            var childLeafSections = await GetLeafExamPracticeSectionsAsync(childSections);
            return leafSections.Concat(childLeafSections).ToList();
        }
    }
}
