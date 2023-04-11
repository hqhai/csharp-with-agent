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
        private readonly IVideoResultRepository _videoResultRepository;

        public CreateAnswerThenUpdateVideoResultHandler(IVideoResultRepository videoResultRepository
            , IVideoRepository videoRepository)
        {
            _videoResultRepository = videoResultRepository;
            _videoRepository = videoRepository;
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
            var answerCount = await _videoResultRepository.Queryable
                                                .Include(x => x.VideoTimeCodeAnswers)
                                                .Where(x => x.Id == videoResult.Id && x.VideoTimeCodeAnswers != null)
                                                .SumAsync(x => x.VideoTimeCodeAnswers.Count);

            var questionCount = await _videoRepository.Queryable
                                                .Include(x => x.VideoTimeCodes.Where(n => !n.IsDeleted))
                                                .ThenInclude(x => x.TimeCodeExercises.Where(n => !n.IsDeleted))
                                                .ThenInclude(x => x.Exercise)
                                                .ThenInclude(x => x!.ExerciseQuestions.Where(n => !n.IsDeleted))
                                                .Where(x => x.Id == videoResult.VideoId)
                                                .SumAsync(x => x.VideoTimeCodes.SelectMany(n => n.TimeCodeExercises)
                                                                                .Select(n => n.Exercise)
                                                                                .SelectMany(n => n!.ExerciseQuestions).Count());

            return answerCount == questionCount;
        }
    }
}
