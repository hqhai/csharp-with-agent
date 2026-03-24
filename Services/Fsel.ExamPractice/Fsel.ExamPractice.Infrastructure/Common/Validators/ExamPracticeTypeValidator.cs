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
    /// Validator for ExamPractice type (Đề thi Quốc gia)
    /// </summary>
    public class ExamPracticeTypeValidator : BaseUpdateExamPracticeValidator
    {
        public override EnumExamPracticeType ExamPracticeType => EnumExamPracticeType.ExamPractice;

        protected override async Task ValidateTypeSpecificAsync(
            UpdateExamPracticeCommandModel request,
            ExamPractice entity,
            IExamPracticeRepository repo,
            ExamPracticeValidationContext validationContext,
            VoidMethodResult errorResult,
            CancellationToken ct)
        {
            // Rule: dates - start < end if both exist
            if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate >= request.EndDate)
            {
                errorResult.AddErrorBadRequest(
                    nameof(EnumSystemErrorCode.InValidFormat),
                    nameof(request.StartDate),
                    nameof(request.EndDate));
            }

            // Rule: Province only valid with Practice/HighschoolEntrance
            if (request.ProvinceId.HasValue &&
                request.SubType is not (EnumExamPracticeSubType.Practice or EnumExamPracticeSubType.HighschoolEntrance))
            {
                errorResult.AddErrorBadRequest(
                    nameof(EnumSystemErrorCode.InValidFormat),
                    nameof(request.ProvinceId),
                    request.SubType);
            }

            // Rule: SchoolGrade must be empty if not Practice
            if (request.SubType != EnumExamPracticeSubType.Practice && !string.IsNullOrEmpty(request.SchoolGrade))
            {
                errorResult.AddErrorBadRequest(
                    nameof(EnumSystemErrorCode.InValidFormat),
                    nameof(request.SchoolGrade),
                    request.SubType);
            }

            // Rule: Active status requires start/end dates
            if (entity.Status == EnumExamPracticeStatus.Active)
            {
                if (!request.StartDate.HasValue)
                {
                    errorResult.AddErrorBadRequest(
                        nameof(EnumSystemErrorCode.DataNotExist),
                        nameof(request.StartDate),
                        request.StartDate);
                }
                if (!request.EndDate.HasValue)
                {
                    errorResult.AddErrorBadRequest(
                        nameof(EnumSystemErrorCode.DataNotExist),
                        nameof(request.EndDate),
                        request.EndDate);
                }
                // Active + Practice/HighschoolEntrance requires ProvinceId
                if (request.SubType is EnumExamPracticeSubType.Practice or EnumExamPracticeSubType.HighschoolEntrance)
                {
                    if (!request.ProvinceId.HasValue)
                    {
                        errorResult.AddErrorBadRequest(
                            nameof(EnumSystemErrorCode.InValidFormat),
                            nameof(request.ProvinceId),
                            request.SubType);
                    }
                }
                // Active + Practice requires SchoolGrade
                if (request.SubType == EnumExamPracticeSubType.Practice && string.IsNullOrEmpty(request.SchoolGrade))
                {
                    errorResult.AddErrorBadRequest(
                        nameof(EnumSystemErrorCode.InValidFormat),
                        nameof(request.SchoolGrade),
                        request.SubType);
                }
            }

            // Province name đã được resolve ở handler - pass vào entity
            if (request.ProvinceId.HasValue && !string.IsNullOrEmpty(validationContext.ProvinceName))
            {
                entity.Province = validationContext.ProvinceName;
            }

            await Task.CompletedTask;
        }

        protected override bool ValidateTypeSubType(UpdateExamPracticeCommandModel request, out string error)
        {
            // ExamPractice type: only Practice, UniversityEntrance, HighschoolEntrance
            var validSubTypes = new[] {
                EnumExamPracticeSubType.Practice,
                EnumExamPracticeSubType.UniversityEntrance,
                EnumExamPracticeSubType.HighschoolEntrance
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