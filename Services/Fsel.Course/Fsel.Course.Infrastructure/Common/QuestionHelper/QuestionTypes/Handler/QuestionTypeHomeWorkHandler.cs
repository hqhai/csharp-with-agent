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

    public class QuestionTypeHomeWorkHandler : IQuestionType
    {
        private readonly QuestionResultQueueModel _request;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IHomeWorkQuestionRepository _homeWorkQuestionRepository;

        public QuestionTypeHomeWorkHandler(QuestionResultQueueModel request,
                                          IHomeWorkResultRepository homeWorkResultRepository,
                                          IHomeWorkQuestionRepository homeWorkQuestionRepository)
        {
            _request = request;
            _homeWorkResultRepository = homeWorkResultRepository;
            _homeWorkQuestionRepository = homeWorkQuestionRepository;
        }

        public async Task<MethodResult<bool>> ExecuteAsync(CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<bool>();

            var homeWorkResult = await _homeWorkResultRepository.Queryable
                                                                .Include(x => x.HomeWorkAnswers)
                                                                .FirstOrDefaultAsync(x => x.Id == _request.TResultId, cancellationToken);
            if (homeWorkResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkResult));
                return methodResult;
            }

            var homeWorkQuestion = _homeWorkQuestionRepository.Queryable.FirstOrDefault(x => x.QuestionId == _request.QuestionId);
            if (homeWorkQuestion == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkQuestion));
                return methodResult;
            }

            switch (_request.QuestionType)
            {
                case EnumQuestionType.Tracing:
                    TracingQuestionHandler(homeWorkResult, homeWorkQuestion);
                    break;

                case EnumQuestionType.ColorMatchingType:
                    ColorMatchingQuestionHandler(homeWorkResult, homeWorkQuestion);
                    break;
            }

            await _homeWorkResultRepository.ExecuteTransactionAsync(async () =>
            {
                _homeWorkResultRepository.Update(homeWorkResult);
                await _homeWorkResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }

        // Upsert 1 HomeWorkAnswer với kiểu dữ liệu Answer là TAnswer
        private static void UpsertAnswer<TAnswer>(
            HomeWorkResult result,
            HomeWorkQuestion homeWorkQuestion,
            Action<TAnswer> mutate,         // cách cập nhật giá trị cho answer
            Func<TAnswer> createFactory     // cách khởi tạo answer nếu chưa có
        ) where TAnswer : class
        {
            // Tìm HomeWorkAnswer cho câu hỏi hiện tại
            var hwAnswer = result.HomeWorkAnswers
                .FirstOrDefault(x => x.HomeWorkQuestionId == homeWorkQuestion.Id);

            // Lấy hoặc tạo dữ liệu answer kiểu TAnswer
            TAnswer ans;
            if (hwAnswer != null)
            {
                // hwAnswer.Answer có thể là object/json => dùng Deserialize<TAnswer>()
                ans = hwAnswer.Answer.Deserialize<TAnswer>() ?? createFactory();
            }
            else
            {
                ans = createFactory();
            }

            // Cho caller mutate nó (gán Count, flags,...)
            mutate(ans);

            // Ghi ngược lại vào result
            if (hwAnswer == null)
            {
                result.HomeWorkAnswers.Add(new HomeWorkAnswer
                {
                    HomeWorkQuestionId = homeWorkQuestion.Id,
                    HomeWorkResultId = result.Id,
                    Answer = ans,
                    IsFirstSubmit = true,
                    Status = EnumAnswerStatus.Process
                });
            }
            else
            {
                hwAnswer.Answer = ans;
            }
        }

        private void TracingQuestionHandler(HomeWorkResult homeWorkResult, HomeWorkQuestion homeWorkQuestion)
        {
            var cfg = _request.Config.Deserialize<TracingQuestionConfigModel>();
            if (cfg == null)
            {
                return;
            }

            UpsertAnswer(
                homeWorkResult,
                homeWorkQuestion,
                // mutate: cách cập nhật khi có dữ liệu
                ans =>
                {
                    ans.CountFail = cfg.CountFail;
                    ans.CountStrokes = cfg.CountStrokes;
                    // Có thể gắn thêm các field khác nếu cần
                },
                // createFactory: cách khởi tạo mặc định khi chưa có Answer
                () => new TracingAnswer
                {
                    IsExact = false,
                    CountFail = cfg.CountFail,
                    CountStrokes = cfg.CountStrokes
                }
            );
        }

        private void ColorMatchingQuestionHandler(HomeWorkResult homeWorkResult, HomeWorkQuestion homeWorkQuestion)
        {
            var cfg = _request.Config.Deserialize<ColorMatchingTypeAnswer>();
            if (cfg == null)
            {
                return;
            }
            UpsertAnswer(
                homeWorkResult,
                homeWorkQuestion,
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
                }
            );
        }
    }
}
