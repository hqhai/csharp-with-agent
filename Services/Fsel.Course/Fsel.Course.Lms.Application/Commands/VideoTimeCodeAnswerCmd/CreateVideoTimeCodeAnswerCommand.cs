// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.VideoTimeCodeAnswers;
    using Fsel.Course.Infrastructure.Common;
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
        private readonly ILessonRepository _lessonRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly AnswerTypeCountConverter _answerTypeCountConverter;
        private readonly AnswerTypeValidatetion _answerTypeValidatetion;

        public CreateVideoTimeCodeAnswerCommandHandler(
            IVideoResultRepository videoResultRepository
            , IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository
            , AuthContext authContext
            , ILessonRepository lessonRepository
            , IQuestionRepository questionRepository
            , AnswerTypeCountConverter answerTypeCountConverter
            , AnswerTypeValidatetion answerTypeValidatetion)
        {
            _videoResultRepository = videoResultRepository;
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _authContext = authContext;
            _lessonRepository = lessonRepository;
            _questionRepository = questionRepository;
            _answerTypeCountConverter = answerTypeCountConverter;
            _answerTypeValidatetion = answerTypeValidatetion;
        }

        public async Task<MethodResult<bool>> Handle(CreateVideoTimeCodeAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            #region Validation

            var lesson = await _lessonRepository.Queryable.Include(x => x.LessonResults)
                                                        .Include(x => x.LessonVideos)
                                                        .ThenInclude(x => x.Video)
                                                        .AsNoTracking()
                                                        .Where(x => x.Id == request.LessonId)
                                                        .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (lesson == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonNotExist));
                return methodResult;
            }
            var videoId = lesson.LessonVideos.Select(x => x.VideoId).FirstOrDefault();
            var lessonResultId = lesson.LessonResults.Select(x => x.Id).FirstOrDefault();

            var videoResult = await _videoResultRepository.Queryable.Include(x => x.VideoTimeCodeAnswers)
                                                    .Where(x => x.VideoId == videoId && x.LessonResultId == lessonResultId && x.StudentId == _authContext.CurrentUserId)
                                                    .AsNoTracking()
                                                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoResultErrorCode.VideoResultNotExist));
                return methodResult;
            }

            var questionIds = request.Answers.Select(x => x.QuestionId).ToList();
            var questions = await _questionRepository.Queryable.Include(x => x.ExerciseQuestions)
                                                             .ThenInclude(x => x.Exercise)
                                                             .ThenInclude(x => x.TimeCodeExercises)
                                                             .ThenInclude(x => x.VideoTimeCode)
                                                             .AsNoTracking()
                                                             .Where(x => questionIds.Contains(x.Id)).ToListAsync(cancellationToken: cancellationToken);
            if (questions == null || questions.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionNotIsExist));
                return methodResult;
            }

            var videotimecodeAnswerCreates = new List<VideoTimeCodeAnswer>();
            var videotimecodeAnswerUpdates = new List<VideoTimeCodeAnswer>();
            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId) ?? new Question();
                var exercise = question.ExerciseQuestions.Select(x => x.Exercise).FirstOrDefault();
                var exerciseId = exercise?.Id;
                var videoTimeCodeId = exercise?.TimeCodeExercises.Select(x => x.VideoTimeCodeId).FirstOrDefault();

                var videotimecodeAnswer = await _videoTimeCodeAnswerRepository.Queryable.Where(x => x.QuestionId == question.Id && x.ExerciseId == exerciseId)
                                                                                        .Where(x => x.VideoTimeCodeId == videoTimeCodeId && x.VideoResultId == videoResult.Id)
                                                                                        .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                #endregion Validation

                if (videotimecodeAnswer == null && item.Answer != null && question.Config != null)
                {
                    var isCheck = _answerTypeValidatetion.TryParseAnswerType(item.Answer, question.QuestionType);
                    if (!isCheck)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumVideoTimeCodeAnswerErrorCode.AnswerIsInTheWrongFormat));
                        return methodResult;
                    }
                    videotimecodeAnswerCreates.Add(new VideoTimeCodeAnswer
                    {
                        Answer = item.Answer,
                        VideoTimeCodeId = videoTimeCodeId ?? Guid.Empty,
                        ExerciseId = exerciseId ?? Guid.Empty,
                        QuestionId = question.Id,
                        VideoResultId = videoResult.Id,
                        CorrectCount = _answerTypeCountConverter.GetTotalCorrectByAsnwerType(item.Answer, question.Config, question.QuestionType)
                    });
                }
                else if (videotimecodeAnswer != null && item.Answer != null && question.Config != null)
                {
                    videotimecodeAnswer.CorrectCount = _answerTypeCountConverter.GetTotalCorrectByAsnwerType(item.Answer, question.Config, question.QuestionType);
                    videotimecodeAnswerUpdates.Add(videotimecodeAnswer);
                }
            }

            await _videoTimeCodeAnswerRepository.ExecuteTransactionAsync(async () =>
            {
                await _videoTimeCodeAnswerRepository.AddList(videotimecodeAnswerCreates);
                _videoTimeCodeAnswerRepository.UpdateList(videotimecodeAnswerUpdates);
                await _videoTimeCodeAnswerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
