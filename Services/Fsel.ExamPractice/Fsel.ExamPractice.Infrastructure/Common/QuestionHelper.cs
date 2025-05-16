// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Common
{
    using Fsel.Common.ActionResults;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.Shared.Enums;

    public class QuestionHelper
    {
        public QuestionHelper()
        {
        }

        public MethodResult<Question> HandleQuestion(Question question, bool isUseTypeExercisePreparation = false, bool isCreated = true)
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
