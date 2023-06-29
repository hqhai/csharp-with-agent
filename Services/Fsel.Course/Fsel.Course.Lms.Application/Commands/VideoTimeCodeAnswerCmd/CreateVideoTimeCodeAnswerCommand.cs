// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.VideoTimeCodeAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateVideoTimeCodeAnswerCommand : CreateVideoTimeCodeAnswerCommandModel, IRequest<MethodResult<IList<QuestionModel>>>
    {
    }

    public class CreateVideoTimeCodeAnswerCommandHandler : IRequestHandler<CreateVideoTimeCodeAnswerCommand, MethodResult<IList<QuestionModel>>>
    {
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IMapper _mapper;
        private readonly IQuestionRepository _questionRepository;
        private readonly AnswerTypeConverter _answerTypeConverter;

        public CreateVideoTimeCodeAnswerCommandHandler(
             IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository
            , ILessonResultRepository lessonResultRepository
            , IMapper mapper
            , IQuestionRepository questionRepository
            , AnswerTypeConverter answerTypeConverter)
        {
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _lessonResultRepository = lessonResultRepository;
            _mapper = mapper;
            _questionRepository = questionRepository;
            _answerTypeConverter = answerTypeConverter;
        }

        public async Task<MethodResult<IList<QuestionModel>>> Handle(CreateVideoTimeCodeAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<QuestionModel>> methodResult = new MethodResult<IList<QuestionModel>>();

            #region Validation

            if (request.Answers == null || request.Answers.Any(x => x.Answer == null) || request.Answers.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoTimeCodeAnswerErrorCode.AnswersNull), nameof(request.Answers), request.Answers);
                return methodResult;
            }

            var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.VideoResult).FirstOrDefaultAsync(x => x.Id == request.LessonResultId, cancellationToken);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonResultErrorCode.LessonResultNotExist), nameof(request.LessonResultId), request.LessonResultId);
                return methodResult;
            }
            var lessonResultId = lessonResult.Id;
            var videoResult = lessonResult.VideoResult;

            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoResultErrorCode.VideoResultNull), nameof(videoResult));
                return methodResult;
            }

            var questionIds = request.Answers.Select(x => x.QuestionId).ToList();
            var questions = await _questionRepository.GetIncludeTimeCodeByIdAsync(questionIds);
            if (questions == null || questions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionsNotExist), nameof(questionIds), questionIds);
                return methodResult;
            }

            var videoTimeCodeAnswers = new List<VideoTimeCodeAnswer>();
            var questionModels = new List<QuestionModel>();
            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                if (question == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNull), nameof(item.QuestionId), item.QuestionId);
                    return methodResult;
                }
                else if (question.Config == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionConfigNull), nameof(question), question);
                    return methodResult;
                }

                var exercise = question.ExerciseQuestions.Select(x => x.Exercise).FirstOrDefault();
                var exerciseId = exercise?.Id;
                var videoTimeCodeId = exercise?.TimeCodeExercises.Select(x => x.VideoTimeCodeId).FirstOrDefault();
                var answer = await _videoTimeCodeAnswerRepository.GetAsync(videoResult.Id, question.Id, exerciseId, videoTimeCodeId);

                if (answer == null)
                {
                    var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(item.Answer, question.Config, question.QuestionType);
                    if (answerConfig == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumVideoTimeCodeAnswerErrorCode.AnswerIsInTheWrongFormat), nameof(item.Answer), item.Answer);
                        return methodResult;
                    }

                    answer = new VideoTimeCodeAnswer
                    {
                        Answer = answerConfig,
                        VideoTimeCodeId = videoTimeCodeId ?? Guid.Empty,
                        ExerciseId = exerciseId ?? Guid.Empty,
                        QuestionId = question.Id,
                        VideoResultId = videoResult.Id,
                        CorrectCount = question.Ungraded ? default : correctCount
                    };
                    videoTimeCodeAnswers.Add(answer);
                }
                var questionModel = _mapper.Map<QuestionModel>(question);
                questionModel.ResultAnswer = _mapper.Map<VideoTimeCodeAnswerModel>(answer);
                questionModels.Add(questionModel);
            }

            #endregion Validation

            await _videoTimeCodeAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                if (videoTimeCodeAnswers.Count > 0)
                {
                    await _videoTimeCodeAnswerRepository.AddList(videoTimeCodeAnswers);
                    await _videoTimeCodeAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    methodResult.StatusCode = StatusCodes.Status201Created;
                }
                else
                {
                    methodResult.StatusCode = StatusCodes.Status200OK;
                }
                methodResult.Result = questionModels;
                return methodResult;
            });

            return methodResult;
        }
    }
}
