// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.Models.CommandModels.Questions;
    using Fsel.Course.Domain.Models.EntityModels;

    public class SectionConverter
    {
        private readonly IMapper _mapper;
        private readonly QuestionTypeConverter _questionTypeConverter;

        public SectionConverter(IMapper mapper, QuestionTypeConverter questionTypeConverter)
        {
            _mapper = mapper;
            _questionTypeConverter = questionTypeConverter;
        }

        public SectionGroupModel GetSectionGroupModel(SectionGroup? sectionGroup, bool isDisableAnswers = false)
        {
            var sectionGroupModel = _mapper.Map<SectionGroupModel>(sectionGroup);
            sectionGroupModel.Sections = GetSectionModels(sectionGroup?.Sections.ToList(), isDisableAnswers);

            return sectionGroupModel;
        }

        public IList<SectionModel>? GetSectionModels(IList<Section>? sections, bool isDisableAnswers = false)
        {
            return sections?.Select(x => new SectionModel
            {
                Id = x.Id,
                Name = x.Name,
                MediaPost = x.MediaPost,
                TargetWord = x.TargetWord,
                SectionParts = GetSectionPartModels(x.SectionParts.ToList()),
                Questions = GetQuestionModels(x.SectionQuestions.ToList(), isDisableAnswers),
                SectionTimeCodes = GetSectionTimeCodeModels(x.SectionTimeCodes.ToList())
            }).ToList();
        }

        public IList<SectionPartModel> GetSectionPartModels(IList<SectionPart> sectionParts, bool isDisableAnswers = false)
        {
            return sectionParts.Select(x => new SectionPartModel
            {
                Id = x.Id,
                PartName = x.PartName,
                SectionId = x.SectionId,
                Questions = GetQuestionModels(x.SectionQuestions.ToList(), isDisableAnswers)
            }).ToList();
        }

        public IList<SectionTimeCodeModel> GetSectionTimeCodeModels(IList<SectionTimeCode> sectionParts)
        {
            return sectionParts.Select(x => new SectionTimeCodeModel
            {
                Id = x.Id,
                DisplayTime = x.DisplayTime,
                ExecutionTime = x.ExecutionTime,
                Name = x.Name,
            }).ToList();
        }

        public IList<QuestionModel> GetQuestionModels(IList<SectionQuestion> sectionQuestions, bool isDisableAnswers = false)
        {
            return sectionQuestions.Select(x => x.Question).Select(x => new QuestionModel
            {
                Id = x!.Id,
                QuestionType = x.QuestionType,
                Explanation = x.Explanation,
                Ungraded = x.Ungraded,
                CorrectTotal = x.CorrectTotal,
                Config = _questionTypeConverter.QuestionTypeConverterObject(x.Config, x.QuestionType, isDisableAnswers: isDisableAnswers).Item1
            }).ToList();
        }

        public VoidMethodResult AddQuestionToSession(dynamic section, IList<CreateQuestionCommandModel>? questionModels)
        {
            VoidMethodResult methodResult = new VoidMethodResult();
            var questions = _mapper.Map<IList<Question>>(questionModels);

            if (questions == null || questions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionsNull), nameof(questions));
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
    }
}
