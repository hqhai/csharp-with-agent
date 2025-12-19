// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Queries.OtherQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Infrastructure.Common;
    using Fsel.ExamPractice.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetTimeExamPracticeQuery : SetTimeExamPracticeModel, IRequest<GetTimeModuleModel>
    {
    }

    public class GetTimeExamPracticeQueryHandler : IRequestHandler<GetTimeExamPracticeQuery, GetTimeModuleModel>
    {
        private readonly GetTimeModulePublisher _getTimeModulePublisher;
        private readonly IExamPracticeResultRepository _examPracticeResultRepository;
        private readonly IExamPracticeSectionResultRepository _examPracticeSectionResultRepository;

        public GetTimeExamPracticeQueryHandler(GetTimeModulePublisher getTimeModulePublisher,
            IExamPracticeResultRepository examPracticeResultRepository,
            IExamPracticeSectionResultRepository examPracticeSectionResultRepository)
        {
            _getTimeModulePublisher = getTimeModulePublisher;
            _examPracticeResultRepository = examPracticeResultRepository;
            _examPracticeSectionResultRepository = examPracticeSectionResultRepository;
        }

        public async Task<GetTimeModuleModel> Handle(GetTimeExamPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            // 1) Chọn nguồn thời gian theo Type
            GetTimeModuleModel? model = request.Type switch
            {
                nameof(EnumExamPracticeType.ExamPractice)
                    => await BuildFromExamPracticeResultAsync(request, cancellationToken),

                nameof(EnumExamPracticeType.IELTS) or nameof(EnumExamPracticeType.Vstep)
                    => await BuildFromSectionResultAsync(request, cancellationToken),

                _ => null
            };

            // 2) Không publish null
            model ??= new GetTimeModuleModel();
            await _getTimeModulePublisher.Publish(model, cancellationToken).ConfigureAwait(false);
            return model;
        }

        /// <summary>
        /// Case: tính thời gian theo ExamPracticeResult (dùng ExamPractice.ExecutionTime nếu không phải “Practice Non-ExamBased”)
        /// </summary>
        private async Task<GetTimeModuleModel?> BuildFromExamPracticeResultAsync(GetTimeExamPracticeQuery request, CancellationToken ct)
        {
            var result = await _examPracticeResultRepository.Queryable
                .AsNoTracking()
                .Include(x => x.ExamPractice)
                .FirstOrDefaultAsync(x => x.Id == request.ObjectId, ct);

            if (result == null)
            {
                return null;
            }
            var executionTime = ResolveExecutionTime(
                result,
                fallbackExecutionTime: result.ExamPractice?.ExecutionTime
            );

            var (working, remaining) = ComputeWorkingAndRemaining(
                currentWorkingTime: result.WorkingTime,
                accessTime: request.AccessTime,
                executionTime: executionTime
            );

            return new GetTimeModuleModel
            {
                WorkingTime = working,
                RemainingTime = remaining,
                UserId = result.CreatedUserId
            };
        }

        /// <summary>
        /// Case: tính thời gian theo ExamPracticeSectionResult (dùng Section.Config.ExecutionTime nếu không phải “Practice Non-ExamBased”)
        /// </summary>
        private async Task<GetTimeModuleModel?> BuildFromSectionResultAsync(GetTimeExamPracticeQuery request, CancellationToken ct)
        {
            var examPracticeSectionResult = await _examPracticeSectionResultRepository.Queryable
                .AsNoTracking()
                .Include(x => x.ExamPracticeSection)
                .FirstOrDefaultAsync(x => x.Id == request.ObjectId, ct);

            var section = examPracticeSectionResult?.ExamPracticeSection;
            if (examPracticeSectionResult == null || section == null)
            {
                return null;
            }
            var examPracticeResult = await _examPracticeResultRepository.GetByIdAsync(examPracticeSectionResult.ExamPracticeResultId);
            if (examPracticeResult == null)
            {
                return null;
            }
            var executionTime = ResolveExecutionTime(
                examPracticeResult,
                fallbackExecutionTime: section.Config?.ExecutionTime
            );

            var (working, remaining) = ComputeWorkingAndRemaining(
                currentWorkingTime: examPracticeSectionResult.WorkingTime,
                accessTime: request.AccessTime,
                executionTime: executionTime
            );

            return new GetTimeModuleModel
            {
                WorkingTime = working,
                RemainingTime = remaining,
                UserId = examPracticeSectionResult.CreatedUserId
            };
        }

        #region Time helpers

        /// <summary>
        /// Quy tắc chọn ExecutionTime:
        /// - Nếu Practice mode & TimeLimit != ExamBased => lấy từ result.Config.ExecutionTime
        /// - Ngược lại => lấy fallback (ExamPractice.ExecutionTime hoặc Section.Config.ExecutionTime)
        /// </summary>
        private static double ResolveExecutionTime(ExamPracticeResult examPracticeResult, double? fallbackExecutionTime)
        {
            var isPracticeNonExamBased =
               examPracticeResult.PracticeMode == EnumPracticeMode.Practice &&
               examPracticeResult.Config != null &&
               examPracticeResult.Config.PracticeTimeLimitOption != EnumPracticeTimeLimitOption.ExamBased;

            if (isPracticeNonExamBased)
            {
                return examPracticeResult.Config?.ExecutionTime ?? default;
            }

            return fallbackExecutionTime ?? default;
        }

        /// <summary>
        /// Tính WorkingTime mới (SetWorkingTime) và RemainingTime (không âm).
        /// </summary>
        private static (double working, double remaining) ComputeWorkingAndRemaining(double currentWorkingTime, double accessTime, double executionTime)
        {
            var working = DateTimeHelper.SetWorkingTime(currentWorkingTime, accessTime, executionTime);
            var remaining = Math.Max(0d, executionTime - working);
            return (working, remaining);
        }

        #endregion Time helpers
    }
}
