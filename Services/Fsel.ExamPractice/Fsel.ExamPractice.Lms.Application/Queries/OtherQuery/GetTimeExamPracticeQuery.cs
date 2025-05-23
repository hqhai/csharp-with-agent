// Copyright (c) Atlantic. All rights reserved.

using MediatR;

namespace Fsel.ExamPractice.Lms.Application.Queries.OtherQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Infrastructure.Common;
    using Fsel.ExamPractice.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;
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
            var getTimeModule = new GetTimeModuleModel();
            switch (request.Type)
            {
                case nameof(EnumExamPracticeType.ExamPractice):
                    getTimeModule = await GetExamPracticeResultAsync(request);
                    break;

                case nameof(EnumExamPracticeType.IELTS):
                    getTimeModule = await GetExamPracticeSectionResultAsync(request);
                    break;

                default:
                    break;
            }
            await _getTimeModulePublisher.Publish(getTimeModule, cancellationToken).ConfigureAwait(false);
            return getTimeModule ?? new GetTimeModuleModel();
        }

        private async Task<GetTimeModuleModel?> GetExamPracticeResultAsync(GetTimeExamPracticeQuery request)
        {
            var examPracticeResult = await _examPracticeResultRepository.Queryable.Include(x => x.ExamPractice).FirstOrDefaultAsync(x => x.Id == request.ObjectId);
            if (examPracticeResult == null)
            {
                return default;
            }
            var executionTime = examPracticeResult.ExamPractice?.ExecutionTime ?? default;
            if (examPracticeResult.PracticeMode == EnumPracticeMode.Practice && examPracticeResult.Config != null && examPracticeResult.Config.PracticeTimeLimitOption != EnumPracticeTimeLimitOption.ExamBased)
            {
                executionTime = examPracticeResult.Config?.ExecutionTime ?? default;
            }
            var workingTime = DateTimeHelper.SetWorkingTime(examPracticeResult.WorkingTime, request.AccessTime, executionTime);
            return new GetTimeModuleModel
            {
                WorkingTime = workingTime,
                RemainingTime = executionTime - workingTime > 0 ? executionTime - workingTime : default,
                UserId = examPracticeResult.CreatedUserId
            };
        }

        private async Task<GetTimeModuleModel?> GetExamPracticeSectionResultAsync(GetTimeExamPracticeQuery request)
        {
            var examPracticeSectionResult = await _examPracticeSectionResultRepository.Queryable.Include(x => x.ExamPracticeSection).FirstOrDefaultAsync(x => x.Id == request.ObjectId);
            var examPracticeSection = examPracticeSectionResult?.ExamPracticeSection;
            if (examPracticeSectionResult == null || examPracticeSection == null)
            {
                return default;
            }
            var examPracticeResult = await _examPracticeResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == examPracticeSectionResult.ExamPracticeResultId);
            if (examPracticeResult == null)
            {
                return default;
            }
            var executionTime = examPracticeSection.Config?.ExecutionTime ?? default;
            if (examPracticeResult.PracticeMode == EnumPracticeMode.Practice && examPracticeResult.Config != null && examPracticeResult.Config.PracticeTimeLimitOption != EnumPracticeTimeLimitOption.ExamBased)
            {
                executionTime = examPracticeResult.Config?.ExecutionTime ?? default;
            }

            var workingTime = DateTimeHelper.SetWorkingTime(examPracticeSectionResult.WorkingTime, request.AccessTime, executionTime);
            return new GetTimeModuleModel
            {
                WorkingTime = workingTime,
                RemainingTime = executionTime - workingTime > 0 ? executionTime - workingTime : default,
                UserId = examPracticeSectionResult.CreatedUserId
            };
        }
    }
}
