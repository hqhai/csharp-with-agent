// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.OtherFeatureCmd
{
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Common;
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
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;

        public SetTimeModuleCommandHandler(IVideoTimeCodeResultRepository videoTimeCodeResultRepository, DateTimeConverter dateTimeConverter, ISectionGroupResultRepository sectionGroupResultRepository)
        {
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
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
            if (videoTimeCode.TimeCodeType != EnumTimeCodeType.Standalone)
            {
                videoTimeCodeResult.WorkingTime = _dateTimeConverter.SetWorkingTime(videoTimeCodeResult.WorkingTime, request.AccessTime, videoTimeCode.ExecutionTime);
            }
            else
            {
                if (videoTimeCodeResult.Status == EnumResultStatus.New)
                {
                    videoTimeCodeResult.WorkingTime = _dateTimeConverter.SetWorkingTime(videoTimeCodeResult.WorkingTime, request.AccessTime, videoTimeCode.ExecutionTime);
                }
                else if (videoTimeCodeResult.Status == EnumResultStatus.Process)
                {
                    videoTimeCodeResult.RetryWorkingTime = _dateTimeConverter.SetWorkingTime(videoTimeCodeResult.RetryWorkingTime, request.AccessTime, videoTimeCode.ExecutionTime);
                }
            }

            _videoTimeCodeResultRepository.Update(videoTimeCodeResult);
            await _videoTimeCodeResultRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task UpdateSectionGroupResultAsync(SetTimeModuleCommand request)
        {
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Include(x => x.SectionGroup).FirstOrDefaultAsync(x => x.Id == request.ObjectId);
            if (sectionGroupResult != null && sectionGroupResult.SectionGroup != null)
            {
                sectionGroupResult.WorkingTime = _dateTimeConverter.SetWorkingTime(sectionGroupResult.WorkingTime, request.AccessTime, sectionGroupResult.SectionGroup.ExecutionTime);
                _sectionGroupResultRepository.Update(sectionGroupResult);
                await _sectionGroupResultRepository.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
