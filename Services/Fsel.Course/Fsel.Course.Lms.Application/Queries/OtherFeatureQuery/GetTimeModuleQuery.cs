// Copyright (c) Atlantic. All rights reserved.

using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using MediatR;

namespace Fsel.Course.Lms.Application.Queries.OtherFeatureQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.EntityFrameworkCore;

    public class GetTimeModuleQuery : SetTimeModuleModel, IRequest<GetTimeModuleModel>
    {
    }

    public class GetTimeModuleQueryHandler : IRequestHandler<GetTimeModuleQuery, GetTimeModuleModel>
    {
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly GetTimeModulePublisher _getTimeModulePublisher;
        private readonly DateTimeConverter _dateTimeConverter;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly ITestSectionResultRepository _testSectionResultRepository;

        public GetTimeModuleQueryHandler(IVideoTimeCodeResultRepository videoTimeCodeResultRepository,
            GetTimeModulePublisher getTimeModulePublisher,
            DateTimeConverter dateTimeConverter,
            ISectionGroupResultRepository sectionGroupResultRepository,
            ITestSectionResultRepository testSectionResultRepository)
        {
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _getTimeModulePublisher = getTimeModulePublisher;
            _dateTimeConverter = dateTimeConverter;
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _testSectionResultRepository = testSectionResultRepository;
        }

        public async Task<GetTimeModuleModel> Handle(GetTimeModuleQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var getTimeModule = new GetTimeModuleModel();
            switch (request.Type)
            {
                case nameof(Video):
                    getTimeModule = await GetVideoTimeCodeAsync(request);
                    break;

                case nameof(PlacementTest):
                case nameof(FinalTest):
                case nameof(MockTest):
                    getTimeModule = await GetSectionGroupResultAsync(request);
                    break;

                case nameof(Test):
                    getTimeModule = await GetSectionResultAsync(request);
                    break;

                default:
                    break;
            }
            await _getTimeModulePublisher.Publish(getTimeModule, cancellationToken).ConfigureAwait(false);
            return getTimeModule ?? new GetTimeModuleModel();
        }

        private async Task<GetTimeModuleModel?> GetVideoTimeCodeAsync(GetTimeModuleQuery request)
        {
            var videoTimeCodeResult = await _videoTimeCodeResultRepository.Queryable.Include(x => x.VideoTimeCode).FirstOrDefaultAsync(x => x.Id == request.ObjectId);
            var videoTimeCode = videoTimeCodeResult?.VideoTimeCode;
            if (videoTimeCodeResult == null || videoTimeCode == null)
            {
                return default;
            }
            (double workingTime, double remainingTime) = (default, default);
            if (videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone)
            {
                workingTime = _dateTimeConverter.SetWorkingTime(videoTimeCodeResult.WorkingTime, request.AccessTime, videoTimeCode.ExecutionTime);
            }
            else
            {
                if (videoTimeCodeResult.Status == EnumResultStatus.New)
                {
                    workingTime = _dateTimeConverter.SetWorkingTime(videoTimeCodeResult.WorkingTime, request.AccessTime, videoTimeCode.ExecutionTime);
                }
                else if (videoTimeCodeResult.Status == EnumResultStatus.Process)
                {
                    workingTime = _dateTimeConverter.SetWorkingTime(videoTimeCodeResult.RetryWorkingTime, request.AccessTime, videoTimeCode.ExecutionTime);
                }
            }
            remainingTime = videoTimeCode.ExecutionTime - workingTime > 0 ? videoTimeCode.ExecutionTime - workingTime : default;
            return new GetTimeModuleModel
            {
                WorkingTime = workingTime,
                RemainingTime = remainingTime,
                UserId = videoTimeCodeResult.CreatedUserId
            };
        }

        private async Task<GetTimeModuleModel?> GetSectionResultAsync(GetTimeModuleQuery request)
        {
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Include(x => x.SectionGroup).FirstOrDefaultAsync(x => x.Id == request.ObjectId);
            var sectionGroup = sectionGroupResult?.SectionGroup;
            if (sectionGroupResult == null || sectionGroup == null)
            {
                return default;
            }
            var workingTime = _dateTimeConverter.SetWorkingTime(sectionGroupResult.WorkingTime, request.AccessTime, sectionGroup.ExecutionTime);
            return new GetTimeModuleModel
            {
                WorkingTime = workingTime,
                RemainingTime = sectionGroup.ExecutionTime - workingTime > 0 ? sectionGroup.ExecutionTime - workingTime : default,
                UserId = sectionGroupResult.CreatedUserId
            };
        }

        private async Task<GetTimeModuleModel?> GetSectionGroupResultAsync(GetTimeModuleQuery request)
        {
            var sectionGroupResult = await _testSectionResultRepository.Queryable.Include(x => x.TestSection).FirstOrDefaultAsync(x => x.Id == request.ObjectId);
            var sectionGroup = sectionGroupResult?.TestSection;
            if (sectionGroupResult == null || sectionGroup == null)
            {
                return default;
            }
            var executionTime = sectionGroup.Config?.ExecutionTime ?? default;
            var workingTime = _dateTimeConverter.SetWorkingTime(sectionGroupResult.WorkingTime, request.AccessTime, executionTime);
            return new GetTimeModuleModel
            {
                WorkingTime = workingTime,
                RemainingTime = executionTime - workingTime > 0 ? executionTime - workingTime : default,
                UserId = sectionGroupResult.CreatedUserId
            };
        }
    }
}
