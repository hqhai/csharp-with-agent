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
    /// Validator for IELTS type
    /// </summary>
    public class IeltsTypeValidator : BaseUpdateExamPracticeValidator
    {
        public override EnumExamPracticeType ExamPracticeType => EnumExamPracticeType.IELTS;

        protected override async Task ValidateTypeSpecificAsync(
            UpdateExamPracticeCommandModel request,
            ExamPractice entity,
            IExamPracticeRepository repo,
            ExamPracticeValidationContext validationContext,
            VoidMethodResult errorResult,
            CancellationToken ct)
        {
            // IELTS specific: currently no additional business rules
            // Province, SchoolGrade not applicable for IELTS
            await Task.CompletedTask;
        }

        protected override bool ValidateTypeSubType(UpdateExamPracticeCommandModel request, out string error)
        {
            // IELTS type: only FullMockTest or SkillMockTest
            var validSubTypes = new[] {
                EnumExamPracticeSubType.FullMockTest,
                EnumExamPracticeSubType.SkillMockTest
            };

            if (!validSubTypes.Contains(request.SubType))
            {
                error = $"{nameof(EnumSystemErrorCode.InValidFormat)}|{nameof(request.Type)}|{request.SubType}";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}