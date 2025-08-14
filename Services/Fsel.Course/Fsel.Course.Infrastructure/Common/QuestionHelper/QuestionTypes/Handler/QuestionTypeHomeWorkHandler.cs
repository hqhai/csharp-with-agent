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
                    await TracingQuestionHandler(homeWorkResult, homeWorkQuestion, cancellationToken);
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

        private async Task TracingQuestionHandler(HomeWorkResult homeWorkResult, HomeWorkQuestion homeWorkQuestion, CancellationToken cancellationToken)
        {
            var configConvert = _request.Config.Deserialize<TracingQuestionConfigModel>();
            if (configConvert == null)
            {
                return;
            }

            var homeWorkAnswer = homeWorkResult.HomeWorkAnswers.FirstOrDefault(x => homeWorkQuestion.QuestionId == _request.QuestionId);
            if (homeWorkAnswer != null)
            {
                var dataAnswer = homeWorkAnswer.Answer.Deserialize<TracingAnswer>();
                if (dataAnswer != null)
                {
                    dataAnswer.CountFail = configConvert.CountFail;
                    dataAnswer.CountStrokes = configConvert.CountStrokes;
                }

                homeWorkAnswer.Answer = dataAnswer;
            }
            else
            {
                var dataAnswer = new TracingAnswer
                {
                    IsExact = false,
                    CountFail = configConvert.CountFail,
                    CountStrokes = configConvert.CountStrokes
                };

                homeWorkResult.HomeWorkAnswers.Add(new HomeWorkAnswer
                {
                    HomeWorkQuestionId = homeWorkQuestion.Id,
                    HomeWorkResultId = _request.TResultId,
                    Answer = dataAnswer,
                    Status = EnumAnswerStatus.Process
                });
            }
        }
    }
}
