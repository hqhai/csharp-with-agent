// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.OtherFeatureCmd
{
    using AutoMapper;
    using Core.Base.Interfaces;
    using Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SetTimeModuleCommand : SetTimeModuleModel, IRequest<bool>
    {
    }

    public class SetTimeModuleCommandHandler : IRequestHandler<SetTimeModuleCommand, bool>
    {
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly DateTimeConverter _dateTimeConverter;
        private readonly IRepository<TestSectionResult> _testSectionResultRepository;

        public SetTimeModuleCommandHandler(IVideoTimeCodeResultRepository videoTimeCodeResultRepository, IMapper mapper, DateTimeConverter dateTimeConverter,
            IRepository<TestSectionResult> testSectionResultRepository)
        {
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _dateTimeConverter = dateTimeConverter;
            _testSectionResultRepository = testSectionResultRepository;
        }

        public async Task<bool> Handle(SetTimeModuleCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            switch (request.Type)
            {
                case nameof(Video):
                    await UpdateVideoTimeCodeAsync(request);
                    break;

                case nameof(PlacementTest):
                case nameof(FinalTest):
                case nameof(MockTest):
                case nameof(Test):
                    await UpdateSectionGroupResultAsync(request);
                    break;

                default:
                    break;
            }

            return true;
        }

        private async Task UpdateVideoTimeCodeAsync(SetTimeModuleCommand request)
        {
            var videoTimeCodeResult = await _videoTimeCodeResultRepository.Queryable.Include(x => x.VideoTimeCode).FirstOrDefaultAsync(x => x.Id == request.ObjectId);
            var videoTimeCode = videoTimeCodeResult?.VideoTimeCode;

            if (videoTimeCodeResult == null || videoTimeCode == null)
            {
                return;
            }

            if (videoTimeCodeResult.Status == EnumResultStatus.Done && videoTimeCodeResult.UpdatedDate.HasValue &&
                videoTimeCodeResult.UpdatedDate.Value.AddMinutes(3) < DateTime.UtcNow)
            {
                return;
            }

            if (videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone)
            {
                videoTimeCodeResult.WorkingTime = _dateTimeConverter.SetWorkingTime(videoTimeCodeResult.WorkingTime, request.AccessTime, videoTimeCode.ExecutionTime);
                var workingTime = (DateTime.UtcNow - videoTimeCodeResult.CreatedDate).TotalMilliseconds;
                if (videoTimeCodeResult.WorkingTime > workingTime)
                {
                    videoTimeCodeResult.WorkingTime = workingTime;
                }
            }
            else
            {
                if (request.SubmissionCount == EnumSubmissionCount.FirstSubmit || videoTimeCodeResult.Status == EnumResultStatus.New)
                {
                    videoTimeCodeResult.WorkingTime = _dateTimeConverter.SetWorkingTime(videoTimeCodeResult.WorkingTime, request.AccessTime, videoTimeCode.ExecutionTime);
                    var workingTime = (DateTime.UtcNow - videoTimeCodeResult.CreatedDate).TotalMilliseconds;
                    if (videoTimeCodeResult.WorkingTime > workingTime)
                    {
                        videoTimeCodeResult.WorkingTime = workingTime;
                    }
                }
                else if (request.SubmissionCount == EnumSubmissionCount.SecondSubmit || videoTimeCodeResult.Status == EnumResultStatus.Process)
                {
                    var accessTime = (DateTime.UtcNow - (videoTimeCodeResult.UpdatedDate ?? videoTimeCodeResult.CreatedDate)).TotalMilliseconds;
                    if (request.AccessTime > accessTime)
                    {
                        request.AccessTime = accessTime;
                    }

                    videoTimeCodeResult.RetryWorkingTime = _dateTimeConverter.SetWorkingTime(videoTimeCodeResult.RetryWorkingTime, request.AccessTime, videoTimeCode.ExecutionTime);
                }
            }

            await _videoTimeCodeResultRepository.BulkUpdateList(new List<VideoTimeCodeResult> { videoTimeCodeResult }, bulk =>
            {
                bulk.ColumnInputExpression = entity => new { entity.WorkingTime, entity.RetryWorkingTime, entity.UpdatedDate };
            });
        }

        private async Task UpdateSectionGroupResultAsync(SetTimeModuleCommand request)
        {
            var sectionGroupResult = await _testSectionResultRepository.Queryable.Include(x => x.TestSection).FirstOrDefaultAsync(x => x.Id == request.ObjectId);

            if (sectionGroupResult.Status == EnumResultStatus.Done && sectionGroupResult.UpdatedDate.HasValue &&
                sectionGroupResult.UpdatedDate.Value.AddMinutes(3) < DateTime.UtcNow)
            {
                return;
            }

            sectionGroupResult.WorkingTime =
                _dateTimeConverter.SetWorkingTime(sectionGroupResult.WorkingTime, request.AccessTime, sectionGroupResult.TestSection?.Config?.ExecutionTime ?? 0);
            var workingTime = (DateTime.UtcNow - sectionGroupResult.CreatedDate).TotalMilliseconds;
            if (sectionGroupResult.WorkingTime > workingTime)
            {
                sectionGroupResult.WorkingTime = workingTime;
            }

            await _testSectionResultRepository.BulkUpdateList(new List<TestSectionResult> { sectionGroupResult }, bulk =>
            {
                bulk.ColumnInputExpression = entity => new { entity.WorkingTime, entity.UpdatedDate };
            });
        }
    }
}
