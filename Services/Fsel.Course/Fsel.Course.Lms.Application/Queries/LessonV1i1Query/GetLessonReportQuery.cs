// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonV1i1Query
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.V1i1;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLessonReportQuery : IRequest<MethodResult<LessonReportModel>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class GetLessonReportQueryHandler : IRequestHandler<GetLessonReportQuery, MethodResult<LessonReportModel>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoRepository _videoRepository;

        public GetLessonReportQueryHandler(ILessonResultRepository lessonResultRepository, IVideoResultRepository videoResultRepository, IVideoRepository videoRepository)
        {
            _lessonResultRepository = lessonResultRepository;
            _videoResultRepository = videoResultRepository;
            _videoRepository = videoRepository;
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
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == request.LessonResultId, cancellationToken);
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
                                                        .ThenInclude(x => x!.VideoTimeCodeAnswers.Where(x => x.VideoResultId == videoResult.Id))
                                                        .FirstOrDefaultAsync(x => x.Id == videoResult.VideoId, cancellationToken);
            if (video == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(video));
                return methodResult;
            }
            methodResult.Result = GetLessonReport(video);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static LessonReportModel GetLessonReport(Video video)
        {
            var lessonReport = new LessonReportModel();
            var questions = video.VideoTimeCodes.SelectMany(x => x.TimeCodeExercises).Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question);
            var answers = questions.SelectMany(x => x.VideoTimeCodeAnswers);
            lessonReport.AnswerTime = video.VideoTimeCodes.SelectMany(x => x.VideoTimeCodeResults).Sum(x => x.WorkingTime + x.RetryWorkingTime);
            lessonReport.Percent = NumberHelper.GetPercent(answers.Sum(x => x.CorrectCount), questions.Sum(x => x!.CorrectTotal));
            lessonReport.NumberOfCorrect = answers.Count(x => x!.IsCorrect == true);
            lessonReport.TotalQuestion = questions.Count();
            return lessonReport;
        }
    }
}
