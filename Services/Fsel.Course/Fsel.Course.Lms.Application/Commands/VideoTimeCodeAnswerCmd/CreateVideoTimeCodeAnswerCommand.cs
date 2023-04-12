// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.VideoTimeCodeAnswers;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateVideoTimeCodeAnswerCommand : CreateVideoTimeCodeAnswerCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateVideoTimeCodeAnswerCommandHandler : IRequestHandler<CreateVideoTimeCodeAnswerCommand, MethodResult<bool>>
    {
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly AuthContext _authContext;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IUserService _userService;
        private readonly AnswerTypeConverter _answerTypeConverter;

        public CreateVideoTimeCodeAnswerCommandHandler(
            IVideoResultRepository videoResultRepository
            , IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository
            , AuthContext authContext
            , ILessonResultRepository lessonResultRepository
            , IQuestionRepository questionRepository
            , IUserService userService
            , AnswerTypeConverter answerTypeConverter)
        {
            _videoResultRepository = videoResultRepository;
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _authContext = authContext;
            _lessonResultRepository = lessonResultRepository;
            _questionRepository = questionRepository;
            _answerTypeConverter = answerTypeConverter;
            _userService = userService;
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

            var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.VideoResult).FirstOrDefaultAsync(x => x.Id == request.LessonResultId, cancellationToken);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonNotExist));
                return methodResult;
            }
            var lessonResultId = lessonResult.Id;
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
                var videoTimeCodeAnswer = await _videoTimeCodeAnswerRepository.GetAsync(videoResult.Id, question.Id, exerciseId, videoTimeCodeId);

                var (answerConfig, correctCount) = _answerTypeConverter.GetTotalCorrectByAsnwerType(item.Answer, question.Config, question.QuestionType);
                if (answerConfig == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumVideoTimeCodeAnswerErrorCode.AnswerIsInTheWrongFormat));
                    return methodResult;
                }

                if (videoTimeCodeAnswer == null)
                {
                    videoTimeCodeAnswerCreates.Add(new VideoTimeCodeAnswer
                    {
                        Answer = answerConfig,
                        VideoTimeCodeId = videoTimeCodeId ?? Guid.Empty,
                        ExerciseId = exerciseId ?? Guid.Empty,
                        QuestionId = question.Id,
                        VideoResultId = videoResult.Id,
                        CorrectCount = question.Ungraded ? default : correctCount
                    });
                }
                else
                {
                    videoTimeCodeAnswer.Answer = answerConfig;
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
