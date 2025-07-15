// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Common
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums.ErrorCodes;
    using Fsel.Shared.Enums;

    public static class QuestionHelper
    {
        public static MethodResult<(Question, object?, short, bool)> HandleQuestionAnswer(Question? question, object? answer, bool isSubmit, object? oldAnswer = default, bool isTryAgain = false, bool isMandatoryAnswer = false)
        {
            var methodResult = new MethodResult<(Question, object?, short, bool)>();
            if (question == null || question.Config == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question));
                return methodResult;
            }
            var methodValidate = AnswerTypeHelper.ValidateAnswerLength(answer, question);
            if (!methodValidate.IsOK)
            {
                methodResult.AddErrorBadRequest(methodValidate.ErrorMessages);
                return methodResult;
            }

            var (answerConfig, correctCount, isAnswerMissing, isAnswered) = AnswerTypeHelper.GetTotalCorrectByAnswerType(answer, oldAnswer, question, isTryAgain, isSubmit, isMandatoryAnswer);
            if (answerConfig == null && !string.IsNullOrEmpty(answer?.ToString()))
            {
                methodResult.AddErrorBadRequest(nameof(EnumAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(answerConfig), answerConfig);
                return methodResult;
            }
            if (isMandatoryAnswer && isAnswerMissing)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNotCompleted), nameof(question), new object[] { question.Id, answer ?? string.Empty });
                return methodResult;
            }
            methodResult.Result = (question, answerConfig, correctCount, isAnswered);
            return methodResult;
        }

        public static MethodResult<(Question, object?, short, bool)> HandleAnswerTest(Question? question, object? answer, bool isSubmit, bool isMandatoryAnswer = false)
        {
            return HandleQuestionAnswer(question, answer, isSubmit, default, false, isMandatoryAnswer);
        }

        public static MethodResult<Question> HandleQuestion(Question question, bool isUseTypeExercisePreparation = false, bool isCreated = true)
        {
            ArgumentNullException.ThrowIfNull(question);
            var methodResult = new MethodResult<Question>();
            var isShowCorrectTotal = (isUseTypeExercisePreparation || question.QuestionType != EnumQuestionType.ExercisePreparation);
            (question.Config, question.CorrectTotal) = QuestionTypeHelper.QuestionTypeConverterObject(question.Config, question.QuestionType, isShowCorrectTotal, false, isCreated);
            if (question.Config == null)
            {
                methodResult.AddErrorBadRequest(nameof(Domain.Enums.ErrorCodes.EnumQuestionErrorCode.ConfigIsInTheWrongFormat), new Error[]{
                    new Error
                    {
                        FieldName = nameof(question.Config)
                    },
                    new Error
                    {
                        FieldName = nameof(question.QuestionType),
                        ErrorValues = new List<object>{ question.QuestionType }
                    }
                });
                return methodResult;
            }
            var isError = QuestionTypeHelper.ValidateQuestion(question.Config, question.QuestionType);
            if (isError)
            {
                methodResult.AddErrorBadRequest(nameof(Domain.Enums.ErrorCodes.EnumQuestionErrorCode.ConfigIsInTheWrongFormat), new Error[]{
                    new Error
                    {
                        FieldName = nameof(question.Config),
                        ErrorValues = new List<object>{ question.Config}
                    },
                    new Error
                    {
                        FieldName = nameof(question.QuestionType),
                        ErrorValues = new List<object>{ question.QuestionType }
                    }
                });
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
