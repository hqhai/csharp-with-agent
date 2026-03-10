// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoResultCmd.V1i2
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.ApplicationServices.CacheServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateVideoResultCommand : IRequest<MethodResult<bool>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class CreateVideoResultCommandHandler : IRequestHandler<CreateVideoResultCommand, MethodResult<bool>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IRequestSafeCachingService _requestSafeCachingService;

        public CreateVideoResultCommandHandler(
            ILessonResultRepository lessonResultRepository,
            ILessonModuleRepository lessonModuleRepository,
            IVideoRepository videoRepository,
            IVideoResultRepository videoResultRepository,
            IRequestSafeCachingService requestSafeCachingService)
        {
            _lessonResultRepository = lessonResultRepository;
            _lessonModuleRepository = lessonModuleRepository;
            _videoRepository = videoRepository;
            _videoResultRepository = videoResultRepository;
            _requestSafeCachingService = requestSafeCachingService;
        }

        public async Task<MethodResult<bool>> Handle(CreateVideoResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var lessonResult = await _lessonResultRepository.ReadQueryable.FirstOrDefaultAsync(x => x.Id == request.LessonResultId, cancellationToken);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonResult));
                return methodResult;
            }

            var lessonModule = await _lessonModuleRepository.ReadQueryable.Where(x => x.LessonId == lessonResult.LessonId)
                                                            .OrderBy(x => x.OpenOrder)
                                                            .FirstOrDefaultAsync(cancellationToken);
            if (lessonModule == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonModule), lessonResult.LessonId);
                return methodResult;
            }
            if (lessonModule.LessonConfigType != EnumLessonConfigType.Video)
            {
                return methodResult;
            }

            var video = await _videoRepository.ReadQueryable.Where(x => x.OriginalId == lessonModule.OriginalId)
                                              .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                                              .FirstOrDefaultAsync(cancellationToken);

            if (video == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(video), lessonModule.OriginalId);
                return methodResult;
            }

            var videoResult = await _videoResultRepository.ReadQueryable.Where(x => x.LessonResultId == lessonResult.Id)
                                                          .Where(x => x.LessonModuleId == lessonModule.Id)
                                                          .FirstOrDefaultAsync(cancellationToken);
            if (videoResult != null)
            {
                return methodResult;
            }

            videoResult = new VideoResult
            {
                LessonModuleId = lessonModule.Id,
                LessonResultId = lessonResult.Id,
                StudentId = lessonResult.StudentId,
                Status = EnumResultStatus.New,
                VideoId = video.Id,
            };

            try
            {
                await _requestSafeCachingService.SafeRequest<VideoResult>(
                    key: $"Add_VideoResult_{videoResult.LessonModuleId}_{videoResult.LessonResultId}_{videoResult.IsDeleted}",
                    safeFunction: async () =>
                    {
                        await _videoResultRepository.BulkMergeAsync(new List<VideoResult> { videoResult }, bulk =>
                        {
                            bulk.ColumnPrimaryKeyExpression = c => new { c.LessonModuleId, c.LessonResultId, c.IsDeleted };
                        });
                        return videoResult;
                    });
            }
            catch { }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
