// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.OtherFeatureCmd
{
    using AutoMapper;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class SetTimeModuleCommand : SetTimeModuleModel, IRequest<bool>
    {
    }

    public class SetTimeModuleCommandHandler : IRequestHandler<SetTimeModuleCommand, bool>
    {
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<SetTimeModuleCommand> _logger;
        private readonly DateTimeConverter _dateTimeConverter;
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;

        public SetTimeModuleCommandHandler(IVideoTimeCodeResultRepository videoTimeCodeResultRepository, IMapper mapper, ILogger<SetTimeModuleCommand> logger, DateTimeConverter dateTimeConverter, ISectionGroupResultRepository sectionGroupResultRepository)
        {
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _mapper = mapper;
            _logger = logger;
            _dateTimeConverter = dateTimeConverter;
            _sectionGroupResultRepository = sectionGroupResultRepository;
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
            var requestInfo = new
            {
                VideoTimeCodeResult = _mapper.Map<VideoTimeCodeResultModel>(videoTimeCodeResult),
                VideoTimeCode = _mapper.Map<VideoTimeCodeModel>(videoTimeCode)
            }.Serialize();
            _logger.LogInformation($"Log_SetTimeModuleCommand_Handle_UpdateVideoTimeCodeAsync : {requestInfo}");

            if (videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone)
            {
                videoTimeCodeResult.WorkingTime = _dateTimeConverter.SetWorkingTime(videoTimeCodeResult.WorkingTime, request.AccessTime, videoTimeCode.ExecutionTime);
            }
            else
            {
                if (request.SubmissionCount == EnumSubmissionCount.FirstSubmit || videoTimeCodeResult.Status == EnumResultStatus.New)
                {
                    videoTimeCodeResult.WorkingTime = _dateTimeConverter.SetWorkingTime(videoTimeCodeResult.WorkingTime, request.AccessTime, videoTimeCode.ExecutionTime);
                }
                else if (request.SubmissionCount == EnumSubmissionCount.SecondSubmit || videoTimeCodeResult.Status == EnumResultStatus.Process)
                {
                    videoTimeCodeResult.RetryWorkingTime = _dateTimeConverter.SetWorkingTime(videoTimeCodeResult.RetryWorkingTime, request.AccessTime, videoTimeCode.ExecutionTime);
                }
            }
            var requestInfoUpdate = new
            {
                VideoTimeCodeResult = _mapper.Map<VideoTimeCodeResultModel>(videoTimeCodeResult),
                VideoTimeCode = _mapper.Map<VideoTimeCodeModel>(videoTimeCode)
            }.Serialize();
            _logger.LogInformation($"Log_SetTimeModuleCommand_Handle_UpdateVideoTimeCodeAsync_1 : {requestInfoUpdate}");

            await _videoTimeCodeResultRepository.BulkMergeAsync(new List<VideoTimeCodeResult> { videoTimeCodeResult }, bulk =>
            {
                bulk.ColumnInputExpression = entity => new { entity.WorkingTime, entity.RetryWorkingTime };
            });

            var requestInfoUpdate2 = new
            {
                VideoTimeCodeResult = _mapper.Map<VideoTimeCodeResultModel>(videoTimeCodeResult),
                VideoTimeCode = _mapper.Map<VideoTimeCodeModel>(videoTimeCode)
            }.Serialize();
            _logger.LogInformation($"Log_SetTimeModuleCommand_Handle_UpdateVideoTimeCodeAsync_2 : {requestInfoUpdate2}");
        }

        private async Task UpdateSectionGroupResultAsync(SetTimeModuleCommand request)
        {
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Include(x => x.SectionGroup).FirstOrDefaultAsync(x => x.Id == request.ObjectId);

            var requestInfo = new
            {
                sectionGroupResult = _mapper.Map<SectionGroupResultModel>(sectionGroupResult),
            }.Serialize();
            _logger.LogInformation($"Log_SetTimeModuleCommand_Handle_UpdateSectionGroupResultAsync: {requestInfo}");

            if (sectionGroupResult != null && sectionGroupResult.SectionGroup != null)
            {
                sectionGroupResult.WorkingTime = _dateTimeConverter.SetWorkingTime(sectionGroupResult.WorkingTime, request.AccessTime, sectionGroupResult.SectionGroup.ExecutionTime);

                var requestInfoUpdate = new
                {
                    sectionGroupResult = _mapper.Map<SectionGroupResultModel>(sectionGroupResult),
                }.Serialize();
                _logger.LogInformation($"Log_SetTimeModuleCommand_Handle_UpdateSectionGroupResultAsync_1 : {requestInfoUpdate}");

                await _sectionGroupResultRepository.BulkMergeAsync(new List<SectionGroupResult> { sectionGroupResult }, bulk =>
                {
                    bulk.ColumnInputExpression = entity => new { entity.WorkingTime };
                });

                var requestInfoUpdate2 = new
                {
                    sectionGroupResult = _mapper.Map<SectionGroupResultModel>(sectionGroupResult),
                }.Serialize();
                _logger.LogInformation($"Log_SetTimeModuleCommand_Handle_UpdateSectionGroupResultAsync_2 : {requestInfoUpdate2}");
            }
        }
    }
}
