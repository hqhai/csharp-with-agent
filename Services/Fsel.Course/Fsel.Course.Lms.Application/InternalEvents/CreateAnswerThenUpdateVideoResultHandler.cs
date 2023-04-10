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
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;

        public CreateAnswerThenUpdateVideoResultHandler(IVideoResultRepository videoResultRepository
            , IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository)
        {
            _videoResultRepository = videoResultRepository;
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
        }

        public async Task Handle(EntityCreatedEvent<VideoTimeCodeAnswer> notification, CancellationToken cancellationToken)
        {
            //ArgumentNullException.ThrowIfNull(notification);

            //var videoTimeCodeAnswer = notification.Data;
            //var videoTimeCodeAnswers = await _videoTimeCodeAnswerRepository.Queryable.Include(x => x.Question).Where(x => x.VideoResultId == videoTimeCodeAnswer.VideoResultId).ToListAsync(cancellationToken: cancellationToken);
            //var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == videoTimeCodeAnswer.VideoResultId, cancellationToken: cancellationToken);
            //if (videoResult != null && videoResult.Status != EnumResultStatus.Done)
            //{
            //    videoResult.CorrectCount = videoTimeCodeAnswers.Sum(x => x.CorrectCount);
            //    videoResult.CorrectTotal = videoTimeCodeAnswers.Sum(x => x.Question!.CorrectTotal);

            //    if (await CheckStatusVideoResult(videoResult.Id))
            //    {
            //        videoResult.Status = EnumResultStatus.Done;
            //    }
            //    _videoResultRepository.Update(videoResult);
            //    await _videoTimeCodeAnswerRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            //}
        }

        public async Task<bool> CheckStatusVideoResult(Guid id)
        {
            var videoResult = await _videoResultRepository.Queryable.Include(x => x.VideoTimeCodeAnswers)
                                                        .Include(x => x.Video)
                                                        .ThenInclude(x => x!.VideoTimeCodes.Where(y => !y.IsDeleted))
                                                        .ThenInclude(x => x.TimeCodeExercises.Where(y => !y.IsDeleted))
                                                        .ThenInclude(x => x.Exercise)
                                                        .ThenInclude(x => x!.ExerciseQuestions.Where(y => !y.IsDeleted))
                                                        .ThenInclude(x => x.Question)
                                                        .FirstOrDefaultAsync(x => x.Id == id);
            var countTotalQuestion = videoResult!.Video!.VideoTimeCodes.SelectMany(x => x.TimeCodeExercises.Where(y => !y.IsDeleted))
                                                                    .Select(x => x.Exercise)
                                                                    .SelectMany(x => x!.ExerciseQuestions.Where(y => !y.IsDeleted))
                                                                    .Select(x => x.Question).Count();

            return countTotalQuestion == videoResult.VideoTimeCodeAnswers.Count;
        }
    }
}
