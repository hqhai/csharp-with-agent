// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.VideoTimeCodeAnswers;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateVideoTimeCodeAnswerCommand : CreateVideoTimeCodeAnswerCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateVideoTimeCodeAnswerCommandHandler : IRequestHandler<CreateVideoTimeCodeAnswerCommand, MethodResult<bool>>
    {
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly AnswerTypeCountConverter _answerTypeCountConverter;
        private readonly AnswerTypeValidatetion _answerTypeValidatetion;
        private readonly ILessonResultRepository _lessonResultRepository;

        public CreateVideoTimeCodeAnswerCommandHandler(
            IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository
            , IQuestionRepository questionRepository
            , AnswerTypeCountConverter answerTypeCountConverter
            , AnswerTypeValidatetion answerTypeValidatetion
            , ILessonResultRepository lessonResultRepository)
        {
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _questionRepository = questionRepository;
            _answerTypeCountConverter = answerTypeCountConverter;
            _answerTypeValidatetion = answerTypeValidatetion;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateVideoTimeCodeAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            if (request.Answers == null || request.Answers.Any(x => x.Answer == null))
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoTimeCodeAnswerErrorCode.AnswersNotEmpty), nameof(request.Answers), request.Answers);
                return methodResult;
            }
            var lessonResult = _lessonResultRepository.Queryable.Include(x => x.VideoResult).Where(x => x.Id == request.LessonResultId).FirstOrDefault();
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonNotExist));
                return methodResult;
            }
            var videoResult = lessonResult.VideoResult;
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoResultErrorCode.VideoResultNotExist));
                return methodResult;
            }

            var questionIds = request.Answers.Select(x => x.QuestionId).ToList();
            var questions = await _questionRepository.GetIncludeTimeCodeByIdAsync(questionIds);
            if (questions == null || questions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNotIsExist));
                return methodResult;
            }

            var videoTimeCodeAnswerCreates = new List<VideoTimeCodeAnswer>();
            var videoTimeCodeAnswerUpdates = new List<VideoTimeCodeAnswer>();
            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                if (question == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNotIsExist));
                    return methodResult;
                }
                else if (question.Config == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionConfigNotIsExist));
                    return methodResult;
                }

                var exercise = question.ExerciseQuestions.Select(x => x.Exercise).FirstOrDefault();
                var exerciseId = exercise?.Id;
                var videoTimeCodeId = exercise?.TimeCodeExercises.Select(x => x.VideoTimeCodeId).FirstOrDefault();
                var videoTimeCodeAnswer = await _videoTimeCodeAnswerRepository.GetWhereByIdAsync(videoResult.Id, question.Id, exerciseId, videoTimeCodeId);

                if (!_answerTypeValidatetion.TryParseAnswerType(item.Answer, question.QuestionType))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumVideoTimeCodeAnswerErrorCode.AnswerIsInTheWrongFormat));
                    return methodResult;
                }

                var answer = item.Answer;
                var correctCount = _answerTypeCountConverter.GetTotalCorrectByAsnwerType(ref answer, question.Config, question.QuestionType);
                if (question.Ungraded)
                {
                    correctCount = 0;
                }
                if (videoTimeCodeAnswer == null)
                {
                    videoTimeCodeAnswerCreates.Add(new VideoTimeCodeAnswer
                    {
                        Answer = answer,
                        VideoTimeCodeId = videoTimeCodeId ?? Guid.Empty,
                        ExerciseId = exerciseId ?? Guid.Empty,
                        QuestionId = question.Id,
                        VideoResultId = videoResult.Id,
                        CorrectCount = correctCount
                    });
                }
                else
                {
                    videoTimeCodeAnswer.Answer = answer;
                    videoTimeCodeAnswerUpdates.Add(videoTimeCodeAnswer);
                }
            }

            #endregion Validation

            await _videoTimeCodeAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                await _videoTimeCodeAnswerRepository.AddList(videoTimeCodeAnswerCreates);
                _videoTimeCodeAnswerRepository.UpdateList(videoTimeCodeAnswerUpdates);
                await _videoTimeCodeAnswerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
