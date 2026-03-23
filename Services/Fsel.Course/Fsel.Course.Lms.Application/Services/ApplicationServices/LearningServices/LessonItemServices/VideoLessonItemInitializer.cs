// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.LessonItemServices
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.ApplicationServices.CacheServices;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class VideoLessonItemInitializer : ILessonItemInitializer
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IRequestSafeCachingService _requestSafeCachingService;
        private readonly ILogger<VideoLessonItemInitializer> _logger;

        public VideoLessonItemInitializer(
            IVideoRepository videoRepository,
            IVideoResultRepository videoResultRepository,
            IRequestSafeCachingService requestSafeCachingService,
            ILogger<VideoLessonItemInitializer> logger)
        {
            _videoRepository = videoRepository;
            _videoResultRepository = videoResultRepository;
            _requestSafeCachingService = requestSafeCachingService;
            _logger = logger;
        }

        public async Task<VoidMethodResult> InitializeAsync(
            LessonModule lessonModule,
            LessonResult lessonResult,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonModule);
            ArgumentNullException.ThrowIfNull(lessonResult);

            var methodResult = new VoidMethodResult();

            try
            {
                if (lessonModule.LessonConfigType != EnumLessonConfigType.Video)
                {
                    _logger.LogDebug(
                        "Skip Video initialization because LessonConfigType is not Video. LessonModuleId={LessonModuleId}, LessonResultId={LessonResultId}, ActualType={LessonConfigType}",
                        lessonModule.Id,
                        lessonResult.Id,
                        lessonModule.LessonConfigType);

                    return methodResult;
                }

                var videoResult = await _videoResultRepository.Queryable
                    .Where(x => x.LessonResultId == lessonResult.Id)
                    .Where(x => x.LessonModuleId == lessonModule.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (videoResult != null)
                {
                    if (videoResult.Status == EnumResultStatus.Unfinished)
                    {
                        _logger.LogInformation(
                            "Found existing VideoResult in Unfinished status. Resetting to New. VideoResultId={VideoResultId}, LessonResultId={LessonResultId}, LessonModuleId={LessonModuleId}, StudentId={StudentId}",
                            videoResult.Id,
                            lessonResult.Id,
                            lessonModule.Id,
                            lessonResult.StudentId);

                        videoResult.Status = EnumResultStatus.New;
                        videoResult.NewDate = DateTime.UtcNow;

                        await _videoResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        _logger.LogDebug(
                            "VideoResult already exists, no initialization needed. VideoResultId={VideoResultId}, LessonResultId={LessonResultId}, LessonModuleId={LessonModuleId}, Status={Status}",
                            videoResult.Id,
                            lessonResult.Id,
                            lessonModule.Id,
                            videoResult.Status);
                    }

                    return methodResult;
                }

                var video = await _videoRepository.ReadQueryable
                    .Where(x => x.OriginalId == lessonModule.OriginalId)
                    .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                    .FirstOrDefaultAsync(cancellationToken);

                if (video == null)
                {
                    _logger.LogWarning(
                        "Video not found for LessonModule OriginalId. LessonModuleId={LessonModuleId}, LessonModuleOriginalId={LessonModuleOriginalId}, LessonResultId={LessonResultId}, StudentId={StudentId}",
                        lessonModule.Id,
                        lessonModule.OriginalId,
                        lessonResult.Id,
                        lessonResult.StudentId);

                    methodResult.AddErrorBadRequest(
                        nameof(EnumSystemErrorCode.DataNotExist),
                        nameof(video),
                        lessonModule.OriginalId);

                    return methodResult;
                }

                videoResult = new VideoResult
                {
                    LessonModuleId = lessonModule.Id,
                    LessonResultId = lessonResult.Id,
                    StudentId = lessonResult.StudentId,
                    Status = EnumResultStatus.New,
                    NewDate = DateTime.UtcNow,
                    VideoId = video.Id,
                };

                await _requestSafeCachingService.SafeRequest(
                    key: $"Add_VideoResult_{videoResult.LessonModuleId}_{videoResult.LessonResultId}_{videoResult.IsDeleted}",
                    safeFunction: async () =>
                    {
                        await _videoResultRepository.BulkMergeAsync(
                            new List<VideoResult> { videoResult },
                            bulk =>
                            {
                                bulk.ColumnPrimaryKeyExpression = c => new
                                {
                                    c.LessonModuleId,
                                    c.LessonResultId,
                                    c.IsDeleted
                                };
                            });

                        return videoResult;
                    });

                return methodResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while initializing Video lesson item. LessonModuleId={LessonModuleId}, LessonModuleOriginalId={LessonModuleOriginalId}, LessonResultId={LessonResultId}, StudentId={StudentId}",
                    lessonModule.Id,
                    lessonModule.OriginalId,
                    lessonResult.Id,
                    lessonResult.StudentId);

                throw;
            }
        }
    }
}
