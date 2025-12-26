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

                case EnumQuestionType.ColorMatchingType:
                    await ColorMatchingQuestionHandler(videoTimeCodeResult, cancellationToken);
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

        private async Task UpsertVideoAnswerAsync<TAnswer>(
          VideoTimeCodeResult result,
          Guid questionId,
          Action<TAnswer> mutate,                 // cập nhật dữ liệu answer
          Func<TAnswer> createFactory,            // khởi tạo answer mặc định
          CancellationToken ct
      ) where TAnswer : class
        {
            var vta = result.VideoTimeCodeAnswers
                            .FirstOrDefault(x => x.QuestionId == questionId);

            TAnswer answer;

            if (vta != null)
            {
                // Deserialize sang kiểu mong muốn, nếu null thì tạo mới
                answer = vta.Answer.Deserialize<TAnswer>() ?? createFactory();
                mutate(answer);
                vta.Answer = answer;
            }
            else
            {
                var exerciseId = await GetExerciseIdAsync(ct);
                answer = createFactory();
                mutate(answer);

                result.VideoTimeCodeAnswers.Add(new VideoTimeCodeAnswer
                {
                    QuestionId = questionId,
                    ExerciseId = exerciseId,
                    VideoTimeCodeId = result.VideoTimeCodeId,
                    VideoResultId = result.VideoResultId,
                    VideoTimeCodeResultId = result.VideoTimeCodeId, // hoặc _request.TResultId nếu đúng
                    Answer = answer,
                    Status = EnumAnswerStatus.Process
                });
            }
        }

        private async Task<Guid> GetExerciseIdAsync(CancellationToken cancellationToken)
        {
            return await _exerciseQuestionRepository.Queryable
                .Where(x => x.QuestionId == _request.QuestionId)
                .Select(x => x.ExerciseId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task TracingQuestionHandler(VideoTimeCodeResult videoTimeCodeResult, CancellationToken cancellationToken)
        {
            var cfg = _request.Config.Deserialize<TracingQuestionConfigModel>();
            if (cfg == null)
            {
                return;
            }
            // Hàm lấy ExerciseId (chỉ gọi khi cần tạo mới answer)

            await UpsertVideoAnswerAsync(
                videoTimeCodeResult,
                _request.QuestionId,
                // mutate: cách cập nhật dữ liệu khi đã có answer
                ans =>
                {
                    ans.CountFail = cfg.CountFail;
                    ans.CountStrokes = cfg.CountStrokes;
                },
                // createFactory: tạo answer mặc định khi chưa có
                () => new TracingAnswer
                {
                    IsExact = false,
                    CountFail = cfg.CountFail,
                    CountStrokes = cfg.CountStrokes
                }, cancellationToken);
        }

        private async Task ColorMatchingQuestionHandler(VideoTimeCodeResult videoTimeCodeResult, CancellationToken cancellationToken)
        {
            var cfg = _request.Config.Deserialize<ColorMatchingTypeAnswer>();
            if (cfg == null)
            {
                return;
            }

            await UpsertVideoAnswerAsync(
                 videoTimeCodeResult,
                _request.QuestionId,
                  // mutate: cách cập nhật khi có dữ liệu
                  ans =>
                  {
                      ans.CountFail = cfg.CountFail;
                      ans.Answers = cfg.Answers;
                      // Có thể gắn thêm các field khác nếu cần
                  },

                  // createFactory: cách khởi tạo mặc định khi chưa có Answer
                  () => new ColorMatchingTypeAnswer
                  {
                      CountFail = cfg.CountFail,
                      Answers = cfg.Answers
                  }, cancellationToken
              );
        }
    }
}
