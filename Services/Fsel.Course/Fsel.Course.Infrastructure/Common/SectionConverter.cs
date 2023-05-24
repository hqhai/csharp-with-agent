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

        public SectionGroupModel GetSectionGroupModel(SectionGroup? sectionGroup)
        {
            var sectionGroupModel = _mapper.Map<SectionGroupModel>(sectionGroup);
            sectionGroupModel.Sections = GetSectionModels(sectionGroup.Sections.ToList());

            return sectionGroupModel;
        }

        public IList<SectionModel> GetSectionModels(IList<Section> sections)
        {
            return sections.Select(x => new SectionModel
            {
                Id = x.Id,
                Name = x.Name,
                MediaPost = x.MediaPost,
                TargetWord = x.TargetWord,
                SectionParts = GetSectionPartModels(x.SectionParts.ToList()),
                Questions = GetQuestionModels(x.SectionQuestions.ToList()),
            }).ToList();
        }

        public IList<SectionPartModel> GetSectionPartModels(IList<SectionPart> sectionParts)
        {
            return sectionParts.Select(x => new SectionPartModel
            {
                Id = x.Id,
                PartName = x.PartName,
                SectionId = x.SectionId,
                Questions = GetQuestionModels(x.SectionQuestions.ToList())
            }).ToList();
        }

        public IList<QuestionModel> GetQuestionModels(IList<SectionQuestion> sectionQuestions)
        {
            return sectionQuestions.Select(x => x.Question).Select(x => new QuestionModel
            {
                Id = x!.Id,
                QuestionType = x.QuestionType,
                Explanation = x.Explanation,
                Ungraded = x.Ungraded,
                CorrectTotal = x.CorrectTotal,
                Config = x.Config
            }).ToList();
        }

        public void GetSectionQuestion(MethodResult<PlacementTestModel> methodResult, CreateQuestionCommandModel question, Section? section, SectionPart? sectionPart)
        {
            if (question == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNull), nameof(question));
            }
            else
            {
                Question newQuestion = _mapper.Map<Question>(question);
                var (config, correctTotal) = _questionTypeConverter.QuestionTypeConverterObject(question.Config, newQuestion.QuestionType, isShowCorrectTotal: !question.Ungraded, false);
                if (config == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.ConfigIsInTheWrongFormat), nameof(question.Config), question.Config);
                }
                if (!newQuestion.IsValid())
                {
                    methodResult.AddErrorBadRequest(newQuestion.ErrorMessages);
                }
                newQuestion.CorrectTotal = correctTotal;

                if (section != null)
                {
                    section.SectionQuestions.Add(new SectionQuestion
                    {
                        Question = newQuestion,
                    });
                }
                else if (sectionPart != null)
                {
                    sectionPart.SectionQuestions.Add(new SectionQuestion
                    {
                        Question = newQuestion,
                    });
                }
            }
        }
    }
}
