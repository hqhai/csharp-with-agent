// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.QuestionHelper.TracingQuestionType.Handler
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Common.QuestionHelper.TracingQuestionType.Interface;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models;
    using Microsoft.EntityFrameworkCore;

    public class TracingQuestionTypeVideo : ITracingQuestionType
    {
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly QuestionResultQueueModel _request;
        private readonly IExerciseQuestionRepository _exerciseQuestionRepository;

        public TracingQuestionTypeVideo(IVideoTimeCodeResultRepository videoTimeCodeResultRepository,
                                       QuestionResultQueueModel request,
                                       IExerciseQuestionRepository exerciseQuestionRepository)
        {
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _request = request;
            _exerciseQuestionRepository = exerciseQuestionRepository;
        }

        public async Task<MethodResult<bool>> ExecuteAsync(CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<bool>();

            var videoTimeCodeResult = await _videoTimeCodeResultRepository.Queryable
                                                                          .Include(x => x.VideoTimeCodeAnswers)
                                                                          .FirstOrDefaultAsync(x => x.Id == _request.TResultId, cancellationToken);
            if (videoTimeCodeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCodeResult));
                return methodResult;
            }

            var videoTimeCodeAnswer = videoTimeCodeResult.VideoTimeCodeAnswers.FirstOrDefault(x => x.QuestionId == _request.QuestionId);
            if (videoTimeCodeAnswer != null)
            {
                var dataAnswer = videoTimeCodeAnswer.Answer.Deserialize<TracingAnswer>();
                if (dataAnswer != null)
                {
                    dataAnswer.CountFail = _request.CountFail;
                    dataAnswer.CountStrokes = _request.CountStrokes;
                }

                videoTimeCodeAnswer.Answer = dataAnswer;
            }
            else
            {
                var exerciseQuestion = await _exerciseQuestionRepository.Queryable.FirstOrDefaultAsync(x => x.QuestionId == _request.QuestionId, cancellationToken);
                var dataAnswer = new TracingAnswer
                {
                    IsExact = false,
                    CountFail = _request.CountFail,
                    CountStrokes = _request.CountStrokes
                };

                videoTimeCodeResult.VideoTimeCodeAnswers.Add(new VideoTimeCodeAnswer()
                {
                    QuestionId = _request.QuestionId,
                    ExerciseId = exerciseQuestion?.ExerciseId ?? Guid.Empty,
                    VideoTimeCodeId = videoTimeCodeResult.VideoTimeCodeId,
                    VideoResultId = videoTimeCodeResult.VideoResultId,
                    VideoTimeCodeResultId = _request.TResultId,
                    Answer = dataAnswer,
                    Status = EnumAnswerStatus.Process
                });
            }

            await _videoTimeCodeResultRepository.ExecuteTransactionAsync(async () =>
            {
                _videoTimeCodeResultRepository.Update(videoTimeCodeResult);
                await _videoTimeCodeResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
