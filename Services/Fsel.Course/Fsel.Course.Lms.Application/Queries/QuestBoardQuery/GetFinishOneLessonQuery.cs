// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.QuestBoardQuery
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetFinishOneLessonQuery : IRequest<MethodResult<QuestBoardCategoryModel>>
    {
        public Guid StudentId { get; set; }
        public EnumRepeatType? RepeatType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class GetFinishOneLessonQueryHandler : IRequestHandler<GetFinishOneLessonQuery, MethodResult<QuestBoardCategoryModel>>
    {
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly ITimeCodeExerciseRepository _timeCodeExerciseRepository;
        private readonly IExerciseQuestionRepository _exerciseQuestionRepository;
        private readonly IExerciseRepository _exerciseRepository;

        public GetFinishOneLessonQueryHandler(IVideoResultRepository videoResultRepository
            , IVideoTimeCodeRepository videoTimeCodeRepository
            , IVideoRepository videoRepository
            , IQuestionRepository questionRepository
            , IVideoTimeCodeResultRepository videoTimeCodeResultRepository
            , IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository
            , ITimeCodeExerciseRepository timeCodeExerciseRepository
            , IExerciseQuestionRepository exerciseQuestionRepository
            , IExerciseRepository exerciseRepository)
        {
            _videoResultRepository = videoResultRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _videoRepository = videoRepository;
            _questionRepository = questionRepository;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _timeCodeExerciseRepository = timeCodeExerciseRepository;
            _exerciseQuestionRepository = exerciseQuestionRepository;
            _exerciseRepository = exerciseRepository;
        }

        public async Task<MethodResult<QuestBoardCategoryModel>> Handle(GetFinishOneLessonQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<QuestBoardCategoryModel> methodResult = new MethodResult<QuestBoardCategoryModel>();
            QuestBoardCategoryModel questBoardCategoryModel = new QuestBoardCategoryModel();
            var videoResults = await _videoResultRepository.Queryable.Where(x => x.StudentId == request.StudentId).OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken);
            if (videoResults == null || videoResults.Count == 0)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = null;
                return methodResult;
            }
            DateTime currentDate = DateTime.UtcNow;
            VideoResult? videoResult = default;
            if (!request.EndDate.HasValue)
            {
                videoResult = videoResults.FirstOrDefault(x => !x.UpdatedDate.HasValue || x.UpdatedDate.Value > request.StartDate);
            }
            else if (request.RepeatType == EnumRepeatType.Daily)
            {
                DateTime startOfDay = currentDate.Date.AddHours(8);
                DateTime endOfDay = currentDate.Date.AddDays(1);
                videoResult = videoResults.FirstOrDefault(x => !x.UpdatedDate.HasValue || (x.UpdatedDate.Value > startOfDay && x.UpdatedDate.Value < endOfDay) || (x.UpdatedDate.Value < endOfDay));
            }
            else if (request.RepeatType == EnumRepeatType.Weekly)
            {
                DateTime startOfWeek = currentDate.AddDays(-(int)currentDate.DayOfWeek);
                DateTime endOfWeek = startOfWeek.AddDays(6);
                videoResult = GetVideoResult(startOfWeek, endOfWeek, request, videoResults);
            }
            else if (request.RepeatType == EnumRepeatType.Month)
            {
                DateTime startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
                DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                videoResult = GetVideoResult(startOfMonth, endOfMonth, request, videoResults);
            }

            if (videoResult == null)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            questBoardCategoryModel.Percent = await GetDoubleAsync(videoResult, cancellationToken);
            questBoardCategoryModel.ObjectId = videoResult.LessonResultId;
            methodResult.Result = questBoardCategoryModel;
            return methodResult;
        }

        private static VideoResult? GetVideoResult(DateTime startDate, DateTime endDate, GetFinishOneLessonQuery request, IList<VideoResult> videoResults)
        {
            ArgumentNullException.ThrowIfNull(videoResults);
            ArgumentNullException.ThrowIfNull(request);
            VideoResult? videoResult = default;
            if (endDate > request.EndDate && startDate < request.StartDate)
            {
                videoResult = videoResults.FirstOrDefault(x =>
                               (!x.UpdatedDate.HasValue ||
                               (x.UpdatedDate.Value >= request.StartDate && x.UpdatedDate.Value <= request.EndDate)));
            }
            else if (endDate > request.EndDate)
            {
                videoResult = videoResults.FirstOrDefault(x =>
                               (!x.UpdatedDate.HasValue ||
                               (x.UpdatedDate.Value >= startDate && x.UpdatedDate.Value <= request.EndDate)));
            }
            else if (startDate < request.StartDate)
            {
                videoResult = videoResults.FirstOrDefault(x =>
                               (!x.UpdatedDate.HasValue ||
                               (x.UpdatedDate.Value >= request.StartDate && x.UpdatedDate.Value <= endDate)));
            }
            else
            {
                videoResult = videoResults.FirstOrDefault(x =>
                              (!x.UpdatedDate.HasValue ||
                              (x.UpdatedDate.Value >= startDate && x.UpdatedDate.Value <= endDate)));
            }

            return videoResult;
        }

        private async Task<double> GetDoubleAsync(VideoResult videoResult, CancellationToken cancellationToken)
        {
            var answerQuery = from baseQ in _videoResultRepository.Queryable
                              join vtcr in _videoTimeCodeResultRepository.Queryable.Where(x => x.CreatedDate >= videoResult.CreatedDate && x.VideoResultId == videoResult.Id)
                                                                                   .Where(x => !(videoResult.Status == EnumResultStatus.Done) || x.UpdatedDate <= videoResult.UpdatedDate) on baseQ.Id equals vtcr.VideoResultId
                              join vtca in _videoTimeCodeAnswerRepository.Queryable.Where(x => x.CreatedDate >= videoResult.CreatedDate && x.VideoResultId == videoResult.Id) on vtcr.Id equals vtca.VideoTimeCodeResultId
                              join e in _exerciseRepository.Queryable on vtca.ExerciseId equals e.Id
                              join te in _timeCodeExerciseRepository.Queryable on e.Id equals te.ExerciseId
                              join vt in _videoTimeCodeRepository.Queryable on te.VideoTimeCodeId equals vt.Id
                              where baseQ.Id == videoResult.Id && vt.TimeCodeType == EnumTimeCodeType.Standalone
                              group new { vt, vtca } by vt.TimeCodeType into g
                              select new
                              {
                                  Type = g.Key,
                                  CorrectCount = g.Select(x => x.vtca).Count()
                              };

            var questionQuery = from baseQ in _videoResultRepository.Queryable
                                join v in _videoRepository.Queryable on baseQ.VideoId equals v.Id
                                join vt in _videoTimeCodeRepository.Queryable on v.Id equals vt.VideoId
                                join te in _timeCodeExerciseRepository.Queryable on vt.Id equals te.VideoTimeCodeId
                                join e in _exerciseRepository.Queryable on te.ExerciseId equals e.Id
                                join eq in _exerciseQuestionRepository.Queryable on e.Id equals eq.ExerciseId
                                join q in _questionRepository.Queryable on eq.QuestionId equals q.Id
                                where baseQ.Id == videoResult.Id && q.QuestionType != EnumQuestionType.ExercisePreparation && vt.TimeCodeType == EnumTimeCodeType.Standalone
                                group new { vt, q } by vt.TimeCodeType into g
                                select new
                                {
                                    Type = g.Key,
                                    TotalCount = g.Select(x => x.q).Count()
                                };
            var answers = await answerQuery.ToListAsync(cancellationToken);
            var questions = await questionQuery.ToListAsync(cancellationToken);
            return questions?.Sum(x => x.TotalCount) > 0 ? ((double)(answers?.Sum(x => x.CorrectCount) ?? default) / questions?.Sum(x => x.TotalCount) ?? default) * 100 : default;
        }
    }
}
