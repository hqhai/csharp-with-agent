// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery.V1i1
{
    using System;
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.V1i1;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using static Shared.Helpers.MediaHelper;

    public class GetLessonReportQuery : IRequest<MethodResult<LessonReportModel>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class GetLessonReportQueryHandler : IRequestHandler<GetLessonReportQuery, MethodResult<LessonReportModel>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IMediator _mediator;

        public GetLessonReportQueryHandler(ILessonResultRepository lessonResultRepository
            , IVideoResultRepository videoResultRepository
            , IVideoRepository videoRepository
            , IMediator mediator)
        {
            _lessonResultRepository = lessonResultRepository;
            _videoResultRepository = videoResultRepository;
            _videoRepository = videoRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<LessonReportModel>> Handle(GetLessonReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<LessonReportModel>();
            var lessonResult = await _lessonResultRepository.GetByIdAsync(request.LessonResultId);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonResult));
                return methodResult;
            }
            var videoResult = await GetVideoResultAsync(request, cancellationToken);
            if (videoResult == null)
            {
                return methodResult;
            }
            var video = await _videoRepository.Queryable.Include(x => x.VideoTimeCodes)
                                                       .ThenInclude(x => x.VideoTimeCodeResults.Where(x => x.VideoResultId == videoResult.Id))
                                                       .Include(y => y.VideoTimeCodes)
                                                       .ThenInclude(x => x.TimeCodeExercises)
                                                       .ThenInclude(x => x.Exercise)
                                                       .ThenInclude(x => x!.ExerciseQuestions)
                                                       .ThenInclude(x => x.Question)
                                                       .Where(x => x.Id == videoResult.VideoId)
                                                       .AsNoTracking()
                                                       .FirstOrDefaultAsync(cancellationToken);
            if (video == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(video));
                return methodResult;
            }
            methodResult.Result = GetLessonReport(video, videoResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<VideoResult?> GetVideoResultAsync(GetLessonReportQuery request, CancellationToken cancellationToken)
        {
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == request.LessonResultId, cancellationToken);
            if (videoResult == null)
            {
                return videoResult;
            }
            if (videoResult.Status == EnumResultStatus.Done && !videoResult.IsShowToken && videoResult.TotalToken == 0)
            {
                videoResult.IsShowToken = true;
                await _videoResultRepository.BulkUpdateList(new List<VideoResult> { videoResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.LessonResultId, c.StudentId, c.VideoId };
                });
            }
            return videoResult;
        }

        private static LessonReportModel GetLessonReport(Video video, VideoResult videoResult)
        {
            LessonReportModel lessonReport = new LessonReportModel();
            var videoTimeCodes = video.VideoTimeCodes.Where(x => x.TimeCodeType == EnumTimeCodeType.Standalone);
            var questions = videoTimeCodes.SelectMany(x => x.TimeCodeExercises).Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question);
            var videoTimeLenght = GetMediaDurationAsync(video.VideoFilePath);
            lessonReport.AnswerTime = videoTimeCodes.SelectMany(x => x.VideoTimeCodeResults).Sum(x => x.WorkingTime + x.RetryWorkingTime) + (videoTimeLenght ?? 0);
            lessonReport.CorrectCount = videoTimeCodes.SelectMany(x => x.VideoTimeCodeResults).Sum(x => x.CorrectCount);
            lessonReport.CorrectTotal = questions.Where(x => !x!.Ungraded).Sum(x => x!.CorrectTotal);
            lessonReport.Percent = NumberHelper.GetPercent(lessonReport.CorrectCount, lessonReport.CorrectTotal);
            lessonReport.HighestStreak = videoResult.HighestStreak;
            lessonReport.TimeCodeHighestStreak = videoResult.TimeCodeHighestStreak;
            lessonReport.StatusVideoResult = videoResult.Status;
            lessonReport.IsShowToken = videoResult.IsShowToken;
            lessonReport.TotalToken = videoResult.TotalToken;
            lessonReport.Badge = GetBadgeName(lessonReport.Percent);
            lessonReport.BadgeDescription = lessonReport.Badge.GetDescription();
            return lessonReport;
        }

        private static EnumBadge GetBadgeName(double accuracyRate)
        {
            if (accuracyRate >= 90 && accuracyRate <= 100)
            {
                return EnumBadge.S;
            }
            else if (accuracyRate >= 70 && accuracyRate < 90)
            {
                return EnumBadge.A;
            }
            else if (accuracyRate >= 50 && accuracyRate < 70)
            {
                return EnumBadge.B;
            }
            else if (accuracyRate >= 30 && accuracyRate < 50)
            {
                return EnumBadge.C;
            }
            else if (accuracyRate >= 0 && accuracyRate < 30)
            {
                return EnumBadge.D;
            }
            else
            {
                return default;
            }
        }
    }
}
