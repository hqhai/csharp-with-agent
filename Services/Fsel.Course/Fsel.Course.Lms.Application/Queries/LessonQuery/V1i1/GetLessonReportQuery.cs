// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery.V1i1
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.V1i1;
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
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;

        public GetLessonReportQueryHandler(ILessonResultRepository lessonResultRepository
            , IVideoResultRepository videoResultRepository
            , IVideoTimeCodeRepository videoTimeCodeRepository
            , IVideoTimeCodeResultRepository videoTimeCodeResultRepository)
        {
            _lessonResultRepository = lessonResultRepository;
            _videoResultRepository = videoResultRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
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

            methodResult.Result = await GetLessonReportAsync(videoResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<double> GetAnswerTimeAsync(VideoResult videoResult)
        {
            var videoTimeLenght = videoResult.Video?.TimeCount;
            var totalTime = await (from baseQ in _videoTimeCodeRepository.Queryable
                                   join vtcr in _videoTimeCodeResultRepository.Queryable on baseQ.Id equals vtcr.VideoTimeCodeId
                                   where vtcr.VideoResultId == videoResult.Id && baseQ.TimeCodeType == EnumTimeCodeType.Standalone
                                   select vtcr).SumAsync(x => x.WorkingTime + x.RetryWorkingTime);
            return totalTime + (videoTimeLenght ?? 0);
        }

        private async Task<VideoResult?> GetVideoResultAsync(GetLessonReportQuery request, CancellationToken cancellationToken)
        {
            var videoResult = await _videoResultRepository.Queryable.Include(x => x.Video).FirstOrDefaultAsync(x => x.LessonResultId == request.LessonResultId, cancellationToken);
            if (videoResult == null)
            {
                return videoResult;
            }
            if (videoResult.Status == EnumResultStatus.Done && !videoResult.IsShowToken && videoResult.TotalToken == 0)
            {
                videoResult.IsShowToken = true;
                await _videoResultRepository.BulkUpdateList(new List<VideoResult> { videoResult }, bulk =>
                {
                    bulk.ColumnInputExpression = c => new { c.IsShowToken };
                });
            }
            return videoResult;
        }

        private async Task<LessonReportModel> GetLessonReportAsync(VideoResult videoResult)
        {
            LessonReportModel lessonReport = new LessonReportModel
            {
                CorrectCount = videoResult.CorrectCount,
                CorrectTotal = videoResult.CorrectTotal,
                HighestStreak = videoResult.HighestStreak,
                TimeCodeHighestStreak = videoResult.TimeCodeHighestStreak,
                StatusVideoResult = videoResult.Status,
                IsShowToken = videoResult.IsShowToken,
                TotalToken = videoResult.TotalToken,
                Percent = videoResult.Percent
            };
            lessonReport.AnswerTime = await GetAnswerTimeAsync(videoResult);
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
