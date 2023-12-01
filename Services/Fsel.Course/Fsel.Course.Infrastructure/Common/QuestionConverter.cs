// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.Models.EntityModels;

    public class QuestionConverter
    {
        private readonly IMapper _mapper;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly AnswerTypeConverter _answerTypeConverter;

        public QuestionConverter(IMapper mapper, QuestionTypeConverter questionTypeConverter, AnswerTypeConverter answerTypeConverter)
        {
            _mapper = mapper;
            _questionTypeConverter = questionTypeConverter;
            _answerTypeConverter = answerTypeConverter;
        }

        public QuestionModel GetQuestion(Question question, object? answer = null, EnumResultStatus status = EnumResultStatus.Done)
        {
            ArgumentNullException.ThrowIfNull(question);
            var questionModel = _mapper.Map<QuestionModel>(question);
            questionModel.Config = _questionTypeConverter.QuestionTypeConverterObject(question.Config, question.QuestionType, false, status).Item1;
            questionModel.ResultAnswer = _mapper.Map<AnswerModel>(answer);
            questionModel.SectionId = question.SectionQuestions.Any() ? question.SectionQuestions.Select(x => x.Section?.Id ?? x.SectionPart?.SectionId).FirstOrDefault() : default;
            return questionModel;
        }

        public MethodResult<(Question, object?, int)> HandleQuestionAnswer(Question? question, object? answer, bool isSubmit, object? oldAnswer = default, bool isTryAgain = false, bool isMandatoryAnswer = false)
        {
            var methodResult = new MethodResult<(Question, object?, int)>();
            if (question == null || question.Config == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question));
                return methodResult;
            }
            var (answerConfig, correctCount, isAnswerMissing) = _answerTypeConverter.GetTotalCorrectByAnswerType(answer, oldAnswer, question.Config, question.QuestionType, isTryAgain, isSubmit, isMandatoryAnswer);
            if (answerConfig == null && !string.IsNullOrEmpty(answer?.ToString()))
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(answerConfig), answerConfig);
                return methodResult;
            }
            if (isMandatoryAnswer && isAnswerMissing)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkAnswerErrorCode.QuestionNotCompleted), nameof(question), new object[] { question.Id });
                return methodResult;
            }
            methodResult.Result = (question, answerConfig, correctCount);
            return methodResult;
        }
    }
}
