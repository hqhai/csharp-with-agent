// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.HomeWorkCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.HomeWorkAnswers;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateHomeWorkAnswerCommand : CreateHomeWorkAnswerCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateHomeWorkAnswerCommandHandler : IRequestHandler<CreateHomeWorkAnswerCommand, MethodResult<bool>>
    {
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IHomeWorkQuestionRepository _homeWorkQuestionRepository;
        private readonly IHomeWorkAnswerRepository _homeWorkAnswerRepository;
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly IQuestionRepository _questionRepository;

        public CreateHomeWorkAnswerCommandHandler(IHomeWorkResultRepository homeWorkResultRepository,
            IHomeWorkQuestionRepository homeWorkQuestionRepository,
            IHomeWorkAnswerRepository homeWorkAnswerRepository,
            AnswerTypeConverter answerTypeConverter,
            IQuestionRepository questionRepository
            )
        {
            _homeWorkResultRepository = homeWorkResultRepository;
            _homeWorkQuestionRepository = homeWorkQuestionRepository;
            _homeWorkAnswerRepository = homeWorkAnswerRepository;
            _answerTypeConverter = answerTypeConverter;
            _questionRepository = questionRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateHomeWorkAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Answers);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            if (request.Answers.All(x => x.Answer == null))
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkAnswerErrorCode.AnswerNotEmpty), nameof(request.Answers), request.Answers);
                return methodResult;
            }

            var homeWorkResult = await _homeWorkResultRepository.GetByIdAsync(request.HomeWorkResultId);
            if (homeWorkResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkResultErrorCode.HomeWorkResultIdNotExist));
                return methodResult;
            }

            var homeWorkAnswers = new List<HomeWorkAnswer>();

            foreach (var item in request.Answers)
            {
                var question = await _questionRepository.GetByIdAsync(item.QuestionId);
                if (question == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionIdNotExist), nameof(item.QuestionId), item.QuestionId);
                    return methodResult;
                }
                else if (question.Config == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionConfigNull), nameof(question), question);
                    return methodResult;
                }

                var homeWorkQuestion = await _homeWorkQuestionRepository.Queryable.Where(x => x.HomeWorkId == homeWorkResult.HomeWorkId && x.QuestionId == item.QuestionId)
                                                                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);
                if (homeWorkQuestion == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumHomeWorkQuestionErrorCode.HomeWorkQuestionNotExist), nameof(homeWorkQuestion), homeWorkResult.HomeWorkId, item.QuestionId);
                    return methodResult;
                }

                var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(item.Answer, question.Config, question.QuestionType);
                if (answerConfig == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumHomeWorkAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(answerConfig), answerConfig);
                    return methodResult;
                }

                homeWorkAnswers.Add(new HomeWorkAnswer
                {
                    Answer = answerConfig,
                    CorrectCount = correctCount,
                    HomeWorkQuestionId = homeWorkQuestion.Id,
                    HomeWorkResultId = homeWorkResult.Id
                });
            }

            #endregion Validation

            await _homeWorkAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                await _homeWorkAnswerRepository.AddList(homeWorkAnswers);
                await _homeWorkAnswerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
