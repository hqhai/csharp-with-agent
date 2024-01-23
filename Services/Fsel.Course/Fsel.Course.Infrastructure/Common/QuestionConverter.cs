// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Shared.Enums;

    public class QuestionConverter
    {
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly QuestionTypeConverter _questionTypeConverter;

        public QuestionConverter(AnswerTypeConverter answerTypeConverter, QuestionTypeConverter questionTypeConverter)
        {
            _answerTypeConverter = answerTypeConverter;
            _questionTypeConverter = questionTypeConverter;
        }

        public MethodResult<(Question, object?, int, bool)> HandleQuestionAnswer(Question? question, object? answer, bool isSubmit, object? oldAnswer = default, bool isTryAgain = false, bool isMandatoryAnswer = false)
        {
            var methodResult = new MethodResult<(Question, object?, int, bool)>();
            if (question == null || question.Config == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question));
                return methodResult;
            }
            var (answerConfig, correctCount, isAnswerMissing, isAnswered) = _answerTypeConverter.GetTotalCorrectByAnswerType(answer, oldAnswer, question, isTryAgain, isSubmit, isMandatoryAnswer);
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
            methodResult.Result = (question, answerConfig, correctCount, isAnswered);
            return methodResult;
        }

        public MethodResult<Question> HandleQuestion(Question? question, bool isUseTypeExercisePreparation = false)
        {
            ArgumentNullException.ThrowIfNull(question);
            var methodResult = new MethodResult<Question>();
            var isShowCorrectTotal = (isUseTypeExercisePreparation || question.QuestionType != EnumQuestionType.ExercisePreparation) && !question.Ungraded;
            (question.Config, question.CorrectTotal) = _questionTypeConverter.QuestionTypeConverterObject(question!.Config, question.QuestionType, isShowCorrectTotal);
            if (question.Config == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.ConfigIsInTheWrongFormat), nameof(question.Config), question.Config);
                return methodResult;
            }
            if (!question.IsValid())
            {
                methodResult.AddErrorBadRequest(question.ErrorMessages);
                return methodResult;
            }
            methodResult.Result = question;
            return methodResult;
        }
    }
}
