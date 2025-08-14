// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.QuestionHelper.QuestionTypes.Handler
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Common.QuestionHelper.QuestionTypes.Interface;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Shared.Models.ShareModels.QuestionResultConfigModels;
    using Microsoft.EntityFrameworkCore;

    public class QuestionTypeVideoHandler : IQuestionType
    {
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly QuestionResultQueueModel _request;
        private readonly IExerciseQuestionRepository _exerciseQuestionRepository;

        public QuestionTypeVideoHandler(IVideoTimeCodeResultRepository videoTimeCodeResultRepository,
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

            switch (_request.QuestionType)
            {
                case EnumQuestionType.Tracing:
                    await TracingQuestionHandler(videoTimeCodeResult, cancellationToken);
                    break;
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

        private async Task TracingQuestionHandler(VideoTimeCodeResult videoTimeCodeResult, CancellationToken cancellationToken)
        {
            var configConvert = _request.Config.Deserialize<TracingQuestionConfigModel>();
            if (configConvert == null)
            {
                return;
            }

            var videoTimeCodeAnswer = videoTimeCodeResult.VideoTimeCodeAnswers.FirstOrDefault(x => x.QuestionId == _request.QuestionId);
            if (videoTimeCodeAnswer != null)
            {
                var dataAnswer = videoTimeCodeAnswer.Answer.Deserialize<TracingAnswer>();
                if (dataAnswer != null)
                {
                    dataAnswer.CountFail = configConvert.CountFail;
                    dataAnswer.CountStrokes = configConvert.CountStrokes;
                }

                videoTimeCodeAnswer.Answer = dataAnswer;
            }
            else
            {
                var exerciseQuestion = await _exerciseQuestionRepository.Queryable.FirstOrDefaultAsync(x => x.QuestionId == _request.QuestionId, cancellationToken);
                var dataAnswer = new TracingAnswer
                {
                    IsExact = false,
                    CountFail = configConvert.CountFail,
                    CountStrokes = configConvert.CountStrokes
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
        }
    }
}
