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
    using Fsel.ExamPractice.Infrastructure.Common;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Data transfer object for validation context
    /// </summary>
    public class ExamPracticeValidationContext
    {
        public string? ProvinceName { get; set; }
        public int? TotalReadingQuestions { get; set; }
        public int? TotalListeningQuestions { get; set; }
    }

    /// <summary>
    /// Base interface for type-specific validators
    /// </summary>
    public interface IUpdateExamPracticeValidator
    {
        EnumExamPracticeType ExamPracticeType { get; }

        Task<VoidMethodResult> ValidateAsync(
            UpdateExamPracticeCommandModel request,
            ExamPractice entity,
            IExamPracticeRepository repo,
            IExamPracticeResultRepository resultRepo,
            ExamPracticeValidationContext validationContext,
            ExamPracticeHelper helper,
            CancellationToken ct);
    }

    /// <summary>
    /// Base class with common validation logic
    /// </summary>
    public abstract class BaseUpdateExamPracticeValidator : IUpdateExamPracticeValidator
    {
        public abstract EnumExamPracticeType ExamPracticeType { get; }

        public virtual async Task<VoidMethodResult> ValidateAsync(
            UpdateExamPracticeCommandModel request,
            ExamPractice entity,
            IExamPracticeRepository repo,
            IExamPracticeResultRepository resultRepo,
            ExamPracticeValidationContext validationContext,
            ExamPracticeHelper helper,
            CancellationToken ct)
        {
            var errorResult = new VoidMethodResult();

            // Common: Entity status check
            if (entity.Status == EnumExamPracticeStatus.Cloned)
            {
                errorResult.AddErrorBadRequest(nameof(Domain.Enums.ErrorCodes.EnumExamPracticeErrorCode.LockedClonedStatus),
                    nameof(entity.Status), entity.Status);
            }

            if (string.IsNullOrWhiteSpace(request.Code))
            {
                errorResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist),
                    nameof(request.Code), request.Code);
            }

            // Common: Code unique
            var existCode = await repo.Queryable.AnyAsync(x => x.Code == request.Code && x.Id != entity.Id, ct);
            if (existCode)
            {
                errorResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist),
                    nameof(request.Code), request.Code);
            }

            // Common: Type/SubType
            if (!ValidateTypeSubType(request, out var typeError))
            {
                errorResult.AddErrorBadRequest(typeError);
            }

            // Type-specific validation (pass resolved context, not service)
            await ValidateTypeSpecificAsync(request, entity, repo, validationContext, errorResult, ct);

            return errorResult;
        }

        protected abstract Task ValidateTypeSpecificAsync(
            UpdateExamPracticeCommandModel request,
            ExamPractice entity,
            IExamPracticeRepository repo,
            ExamPracticeValidationContext validationContext,
            VoidMethodResult errorResult,
            CancellationToken ct);

        protected abstract bool ValidateTypeSubType(UpdateExamPracticeCommandModel request, out string error);
    }
}
