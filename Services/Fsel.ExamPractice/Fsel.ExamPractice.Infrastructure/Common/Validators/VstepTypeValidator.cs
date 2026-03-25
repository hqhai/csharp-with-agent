// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Common.Validators
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPractices;

    /// <summary>
    /// Validator for Vstep type (Bộ đề Vstep)
    /// </summary>
    public class VstepTypeValidator : BaseUpdateExamPracticeValidator
    {
        private const int TotalReadingQuestionVstep = 40;
        private const int TotalListeningQuestionVstep = 35;

        public override EnumExamPracticeType ExamPracticeType => EnumExamPracticeType.Vstep;

        protected override async Task ValidateTypeSpecificAsync(
            UpdateExamPracticeCommandModel request,
            ExamPractice entity,
            IExamPracticeRepository repo,
            ExamPracticeValidationContext validationContext,
            VoidMethodResult errorResult,
            CancellationToken ct)
        {
            // Vstep specific: validate total questions from context (resolved by handler)
            if (validationContext.TotalReadingQuestions.HasValue &&
                validationContext.TotalReadingQuestions < TotalReadingQuestionVstep)
            {
                errorResult.AddErrorBadRequest(
                    nameof(Domain.Enums.ErrorCodes.EnumExamPracticeErrorCode.InvalidReadingQuestionCount),
                    nameof(validationContext.TotalReadingQuestions));
            }

            if (validationContext.TotalListeningQuestions.HasValue &&
                validationContext.TotalListeningQuestions < TotalListeningQuestionVstep)
            {
                errorResult.AddErrorBadRequest(
                    nameof(Domain.Enums.ErrorCodes.EnumExamPracticeErrorCode.InvalidListenningQuestionCount),
                    nameof(validationContext.TotalListeningQuestions));
            }

            await Task.CompletedTask;
        }

        protected override bool ValidateTypeSubType(UpdateExamPracticeCommandModel request, out string error)
        {
            // Vstep type: currently uses ExamPractice subtypes or custom
            // This depends on your business logic - adjust as needed
            error = string.Empty;
            return true;
        }
    }
}