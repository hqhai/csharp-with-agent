// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.Questions;
    using Fsel.Course.Domain.Models.CommandModels.Sections;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;

    public class SectionConverter
    {
        private readonly IMapper _mapper;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IQuestionRepository _questionRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ISectionQuestionRepository _sectionQuestionRepository;

        public SectionConverter(IMapper mapper
            , QuestionTypeConverter questionTypeConverter
            , IQuestionRepository questionRepository
            , ISectionGroupRepository sectionGroupRepository
            , ISectionQuestionRepository sectionQuestionRepository)
        {
            _mapper = mapper;
            _questionTypeConverter = questionTypeConverter;
            _questionRepository = questionRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _sectionQuestionRepository = sectionQuestionRepository;
        }

        public SectionGroupModel GetSectionGroupModel(SectionGroup? sectionGroup, bool isDisableAnswers = false)
        {
            var sectionGroupModel = _mapper.Map<SectionGroupModel>(sectionGroup);
            sectionGroupModel.MockTestScores = _mapper.Map<IList<MockTestScoreModel>>(sectionGroup?.MockTestScores);
            sectionGroupModel.Sections = GetSectionModels(sectionGroup?.Sections.ToList(), isDisableAnswers);
            sectionGroupModel.TotalQuestion = GetTotalQuestion(sectionGroup);
            return sectionGroupModel;
        }

        public long GetTotalQuestion(SectionGroup? sectionGroup)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            if (sectionGroup.CourseSkill == EnumCourseSkill.Listening)
            {
                return sectionGroup.Sections.Any(x => x.SectionParts != null && x.SectionParts.Count > 0) ? sectionGroup.Sections.SelectMany(x => x.SectionParts).SelectMany(x => x.SectionQuestions).Select(x => x.Question).Count() : 0;
            }
            else if (sectionGroup.CourseSkill == EnumCourseSkill.Reading)
            {
                return sectionGroup.Sections.Any(x => x.SectionParts != null && x.SectionParts.Count > 0) ? sectionGroup.Sections.SelectMany(x => x.SectionParts).SelectMany(x => x.SectionQuestions).Select(x => x.Question).Count() : 0;
            }
            else if (sectionGroup.CourseSkill == EnumCourseSkill.Vocabulary)
            {
                return sectionGroup.Sections.Any(x => x.SectionParts != null && x.SectionParts.Count > 0) ? sectionGroup.Sections.SelectMany(x => x.SectionParts).SelectMany(x => x.SectionQuestions).Select(x => x.Question).Count() : 0;
            }
            else if (sectionGroup.CourseSkill == EnumCourseSkill.Grammar)
            {
                return sectionGroup.Sections.Any(x => x.SectionParts != null && x.SectionParts.Count > 0) ? sectionGroup.Sections.SelectMany(x => x.SectionParts).SelectMany(x => x.SectionQuestions).Select(x => x.Question).Count() : 0;
            }
            else if (sectionGroup.CourseSkill == EnumCourseSkill.Speaking)

            {
                return sectionGroup.Sections.Any(x => x.SectionTimeCodes != null && x.SectionTimeCodes.Count > 0) ? sectionGroup.Sections.SelectMany(x => x.SectionTimeCodes).Count() : 0;
            }
            else if (sectionGroup.CourseSkill == EnumCourseSkill.Writing)
            {
                return sectionGroup.Sections.Any() ? sectionGroup.Sections.Count : 0;
            }

            return 0;
        }

        public IList<SectionModel>? GetSectionModels(IList<Section>? sections, bool isDisableAnswers = false)
        {
            return sections?.OrderBy(x => x.CreatedDate).Select(x => new SectionModel
            {
                Id = x.Id,
                Name = x.Name,
                MediaPost = x.MediaPost,
                TargetWord = x.TargetWord,
                DisplayOrder = x.DisplayOrder,
                SubFilePath = x.SubFilePath,
                VideoFilePath = x.VideoFilePath,
                SectionParts = GetSectionPartModels(x.SectionParts.ToList(), isDisableAnswers),
                Questions = GetQuestionModels(x.SectionQuestions.ToList(), isDisableAnswers),
                SectionTimeCodes = GetSectionTimeCodeModels(x.SectionTimeCodes.ToList()),
                ExtraPracticeAnswer = x!.ExtraPracticeAnswers.Count > 0 ? _mapper.Map<ExtraPracticeAnswerModel>(x.ExtraPracticeAnswers.FirstOrDefault()) : null,
                MockTestAnswer = x!.MockTestAnswers.Count > 0 ? _mapper.Map<MockTestAnswerModel>(x.MockTestAnswers.FirstOrDefault()) : null
            }).ToList();
        }

        public IList<SectionPartModel> GetSectionPartModels(IList<SectionPart> sectionParts, bool isDisableAnswers = false)
        {
            return sectionParts.OrderBy(x => x.CreatedDate).Select(x => new SectionPartModel
            {
                Id = x.Id,
                PartName = x.PartName,
                SectionId = x.SectionId,
                Questions = GetQuestionModels(x.SectionQuestions.ToList(), isDisableAnswers)
            }).ToList();
        }

        public IList<SectionTimeCodeModel> GetSectionTimeCodeModels(IList<SectionTimeCode> sectionTimeCodes)
        {
            return sectionTimeCodes.OrderBy(x => x.DisplayTime).Select(x => new SectionTimeCodeModel
            {
                Id = x.Id,
                DisplayTime = x.DisplayTime,
                ExecutionTime = x.ExecutionTime,
                Name = x.Name,
                ExtraPracticeAnswer = x!.ExtraPracticeAnswers.Count > 0 ? _mapper.Map<ExtraPracticeAnswerModel>(x.ExtraPracticeAnswers.FirstOrDefault()) : null,
                MockTestAnswer = x!.MockTestAnswers.Count > 0 ? _mapper.Map<MockTestAnswerModel>(x.MockTestAnswers.FirstOrDefault()) : null
            }).ToList();
        }

        public IList<QuestionModel> GetQuestionModels(IList<SectionQuestion> sectionQuestions, bool isDisableAnswers = false)
        {
            return sectionQuestions.OrderBy(x => x.CreatedDate).Select(x => new QuestionModel
            {
                Id = x.Question!.Id,
                QuestionType = x.Question.QuestionType,
                Explanation = x.Question.Explanation,
                Ungraded = x.Question.Ungraded,
                CorrectTotal = x.Question.CorrectTotal,
                Config = _questionTypeConverter.QuestionTypeConverterObject(x.Question.Config, x.Question.QuestionType, isDisableAnswers: isDisableAnswers).Item1,
                ResultAnswer = _mapper.Map<AnswerModel>(x.MockTestAnswers.FirstOrDefault())
            }).ToList();
        }

        public VoidMethodResult AddQuestionToSession(dynamic section, IList<CreateQuestionCommandModel>? questionModels)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            var questions = _mapper.Map<IList<Question>>(questionModels);

            if (questions == null || questions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                return methodResult;
            }
            foreach (var question in questions)
            {
                var (config, correctTotal) = _questionTypeConverter.QuestionTypeConverterObject(question.Config, question.QuestionType, isShowCorrectTotal: !question.Ungraded, false);
                if (config == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.ConfigIsInTheWrongFormat), nameof(question.Config), question.Config);
                }
                if (!question.IsValid())
                {
                    methodResult.AddErrorBadRequest(question.ErrorMessages);
                }
                question.CorrectTotal = correctTotal;

                if (section != null)
                {
                    section.SectionQuestions.Add(new SectionQuestion
                    {
                        Question = question,
                    });
                }
            }

            if (!section.IsValid())
            {
                methodResult.AddErrorBadRequest(section.ErrorMessages);
                return methodResult;
            }

            return methodResult;
        }

        public VoidMethodResult AddSessionToSessionGroup(dynamic sectionGroup, IList<CreateSectionCommandModel>? sectionModels, EnumCourseType? type)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            if (sectionModels == null || sectionModels.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup.Sections));
                return methodResult;
            }
            IList<Section> sections = sectionGroup.Sections;
            foreach (var section in sectionModels)
            {
                if (section == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(section));
                    return methodResult;
                }
                Section newSection = sections.ElementAt(sectionModels.IndexOf(section));
                if (sectionGroup.CourseSkill != EnumCourseSkill.Speaking && sectionGroup.CourseSkill != EnumCourseSkill.Writing)
                {
                    if (section.SectionParts != null && section.Questions != null && section.SectionParts.Count > 0 && section.Questions.Count > 0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSectionErrorCode.OnlyOneOfTwoSectionPartsOrQuestions));
                        return methodResult;
                    }
                    if (type == EnumCourseType.Ielts)
                    {
                        if (section.SectionParts == null || section.SectionParts.Count == 0)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(section.SectionParts));
                            return methodResult;
                        }
                        foreach (var sectionPart in section.SectionParts)
                        {
                            if (sectionPart == null)
                            {
                                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionPart));
                                return methodResult;
                            }
                            else
                            {
                                SectionPart newSectionPart = newSection.SectionParts.ElementAt(section.SectionParts.IndexOf(sectionPart));

                                var method = AddQuestionToSession(newSectionPart, sectionPart.Questions);
                                if (!method.IsOK)
                                {
                                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                                }
                            }
                        }
                        var correctCount = newSection.SectionParts.SelectMany(x => x.SectionQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal);
                        if (!SectionValidation.IsCheckSection(sectionGroup.CourseSkill, section.DisplayOrder, correctCount))
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.MustCorrectScore), nameof(section.DisplayOrder), section.DisplayOrder);
                            return methodResult;
                        }
                    }
                    else
                    {
                        var method = AddQuestionToSession(newSection, section.Questions);
                        if (!method.IsOK)
                        {
                            methodResult.AddErrorBadRequest(method.ErrorMessages);
                            return methodResult;
                        }
                    }
                }
                else if (sectionGroup.CourseSkill == EnumCourseSkill.Speaking)
                {
                    if (section.SectionTimeCodes == null || section.SectionTimeCodes.Count == 0)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionGroup.CourseSkill));
                        return methodResult;
                    }
                    foreach (var sectionTimeCode in section.SectionTimeCodes)
                    {
                        if (sectionTimeCode == null)
                        {
                            methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(sectionTimeCode));
                            return methodResult;
                        }
                        else
                        {
                            SectionTimeCode newSectionTimeCode = newSection.SectionTimeCodes.ElementAt(section.SectionTimeCodes.IndexOf(sectionTimeCode));
                        }
                    }
                }

                if (!newSection.IsValid())
                {
                    methodResult.AddErrorBadRequest(newSection.ErrorMessages);
                    return methodResult;
                }
            }

            return methodResult;
        }

        public async Task<bool> DeleteSectionGroup(IList<SectionGroup> sectionGroups, IList<SectionQuestion> sectionQuestions, IList<Question> questions)
        {
            sectionGroups.ForEach(async x => await _sectionGroupRepository.DeleteAsync(x));
            await _sectionGroupRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

            sectionQuestions.ForEach(async x => await _sectionQuestionRepository.DeleteAsync(x));
            await _sectionQuestionRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

            questions.ForEach(async x => await _questionRepository.DeleteAsync(x));
            await _questionRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

            return true;
        }
    }
}
