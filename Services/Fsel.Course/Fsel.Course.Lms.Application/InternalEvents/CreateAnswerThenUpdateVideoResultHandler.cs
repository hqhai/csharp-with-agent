// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CreateAnswerThenUpdateVideoResultHandler : INotificationHandler<EntityCreatedEvent<VideoTimeCodeAnswer>>
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly ITimeCodeExerciseRepository _timeCodeExerciseRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IExerciseQuestionRepository _exerciseQuestionRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;

        public CreateAnswerThenUpdateVideoResultHandler(
            IVideoTimeCodeRepository videoTimeCodeRepository,
            ITimeCodeExerciseRepository timeCodeExerciseRepository,
            IExerciseRepository exerciseRepository,
            IExerciseQuestionRepository exerciseQuestionRepository,
            IVideoResultRepository videoResultRepository,
            IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository,
            IVideoRepository videoRepository)
        {
            _videoRepository = videoRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _timeCodeExerciseRepository = timeCodeExerciseRepository;
            _exerciseRepository = exerciseRepository;
            _exerciseQuestionRepository = exerciseQuestionRepository;
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _videoResultRepository = videoResultRepository;
        }

        public async Task Handle(EntityCreatedEvent<VideoTimeCodeAnswer> notification, CancellationToken cancellationToken)
        {
            //ArgumentNullException.ThrowIfNull(notification);

            //var videoTimeCodeAnswer = notification.Data;
            //var videoResult = videoTimeCodeAnswer.VideoResult;
            //var question = videoTimeCodeAnswer.Question;

            //if (videoResult != null)
            //{
            //    videoResult.CorrectCount += videoTimeCodeAnswer.CorrectCount;
            //    videoResult.CorrectTotal += question?.CorrectTotal ?? default;

            //    if (await IsVideoResultDone(videoResult))
            //    {
            //        videoResult.Status = EnumResultStatus.Done;
            //    }

            //    _videoResultRepository.Update(videoResult);
            //    await _videoResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            //}
        }

        public async Task<bool> IsVideoResultDone(VideoResult videoResult)
        {
            var answerQuery = from vtca in _videoTimeCodeAnswerRepository.Queryable
                              where vtca.VideoResultId == videoResult.Id
                              select 1;

            var questionQuery = from v in _videoRepository.Queryable
                                join vt in _videoTimeCodeRepository.Queryable on v.Id equals vt.VideoId
                                join te in _timeCodeExerciseRepository.Queryable on vt.Id equals te.VideoTimeCodeId
                                join e in _exerciseRepository.Queryable on te.ExerciseId equals e.Id
                                join eq in _exerciseQuestionRepository.Queryable on e.Id equals eq.ExerciseId
                                where v.Id == videoResult.VideoId
                                select 1;

            var answerCount = await answerQuery.CountAsync();
            var questionCount = await questionQuery.CountAsync();

            return answerCount == questionCount;
        }
    }
}
