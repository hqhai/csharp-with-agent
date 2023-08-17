// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class UpdatedExtraPracticeExerciseResultThenUpdateExtraPracticeResultHandler :
            INotificationHandler<EntityChangedEvent<ExtraPracticeExerciseResult>>
    {
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;
        private readonly IExtraPracticeExerciseResultRepository _extraPracticeExerciseResultRepository;
        private readonly IExtraPracticeRepository _extraPracticeRepository;

        public UpdatedExtraPracticeExerciseResultThenUpdateExtraPracticeResultHandler(
            IExtraPracticeResultRepository extraPracticeResultRepository,
            IExtraPracticeExerciseResultRepository extraPracticeExerciseResultRepository,
            IExtraPracticeRepository extraPracticeRepository)
        {
            _extraPracticeResultRepository = extraPracticeResultRepository;
            _extraPracticeExerciseResultRepository = extraPracticeExerciseResultRepository;
            _extraPracticeRepository = extraPracticeRepository;
        }

        public async Task Handle(EntityChangedEvent<ExtraPracticeExerciseResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            if (notification.Data.Status == EnumResultStatus.Done)
            {
                var extraPracticeResult = await _extraPracticeResultRepository.GetByIdAsync(notification.Data.ExtraPracticeResultId);
                var extraPracticeExerciseResults = await _extraPracticeExerciseResultRepository.Queryable
                                .Where(x => x.ExtraPracticeResultId == notification.Data.ExtraPracticeResultId && x.StudentId == notification.Data.StudentId).ToListAsync(cancellationToken);
                if (extraPracticeResult != null)
                {
                    var extraPractice = await _extraPracticeRepository.GetByIdAsync(extraPracticeResult.ExtraPracticeId);
                    if (extraPractice != null && extraPractice.Type == EnumExtraPracticeType.Book)
                    {
                        extraPractice = await _extraPracticeRepository.Queryable
                                                         .Include(x => x.ExtraPracticeChapters.Where(y => !y.IsDeleted))
                                                             .ThenInclude(x => x.ExtraPracticeExercises.Where(y => !y.IsDeleted))
                                                             .ThenInclude(x => x.Exercise)
                                                         .Include(x => x.ExtraPracticeChapters.Where(y => !y.IsDeleted))
                                                             .ThenInclude(x => x.ExtraPracticeExercises.Where(y => !y.IsDeleted))
                                                             .ThenInclude(x => x.ExtraPracticeExerciseResults)
                                                         .FirstOrDefaultAsync(x => x.Id == extraPracticeResult.ExtraPracticeId, cancellationToken);

                        var exerciseCount = extraPractice!.ExtraPracticeChapters.SelectMany(x => x.ExtraPracticeExercises).Select(x => x.Exercise).Count();
                        var resultCount = extraPractice.ExtraPracticeChapters.SelectMany(x => x.ExtraPracticeExercises).SelectMany(x => x.ExtraPracticeExerciseResults)
                                                                              .Where(y => y.StudentId == extraPracticeResult.StudentId && y.Status == EnumResultStatus.Done)
                                                                              .Count();
                        if (exerciseCount == resultCount)
                        {
                            await UpdateExtraPracticeResultTypeBook(extraPracticeResult, extraPracticeExerciseResults, cancellationToken);
                        }
                        else
                        {
                            var extraPracticeExerciseResult = extraPractice!.ExtraPracticeChapters.SelectMany(x => x.ExtraPracticeExercises)
                                                                    .SelectMany(x => x.ExtraPracticeExerciseResults)
                                                                    .OrderBy(x => x.CreatedDate)
                                                                    .FirstOrDefault(x => x.Status == EnumResultStatus.Unfinished);

                            if (extraPracticeExerciseResult != null)
                            {
                                extraPracticeExerciseResult.Status = EnumResultStatus.New;
                                _extraPracticeExerciseResultRepository.Update(extraPracticeExerciseResult);
                                await _extraPracticeExerciseResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                            }
                        }
                    }
                    else if (extraPractice != null && extraPractice.Type == EnumExtraPracticeType.VideoEmbed)
                    {
                        extraPractice = await _extraPracticeRepository.Queryable.Include(x => x.ExtraPracticeExercises.Where(y => !y.IsDeleted))
                                                                .ThenInclude(x => x.Exercise)
                                                             .Include(x => x.ExtraPracticeExercises.Where(y => !y.IsDeleted))
                                                                .ThenInclude(x => x.ExtraPracticeExerciseResults)
                                                             .FirstOrDefaultAsync(x => x.Id == extraPracticeResult.ExtraPracticeId, cancellationToken);

                        var exerciseCount = extraPractice!.ExtraPracticeExercises.Select(x => x.Exercise).Count();
                        var resultCount = extraPractice.ExtraPracticeExercises.SelectMany(x => x.ExtraPracticeExerciseResults)
                                                                              .Where(y => y.StudentId == extraPracticeResult.StudentId && y.Status == EnumResultStatus.Done)
                                                                              .Count();
                        if (exerciseCount == resultCount)
                        {
                            await UpdateExtraPracticeResult(extraPracticeResult, extraPracticeExerciseResults, cancellationToken);
                        }
                        else
                        {
                            var extraPracticeExerciseResult = extraPractice.ExtraPracticeExercises.SelectMany(x => x.ExtraPracticeExerciseResults)
                                                                    .OrderBy(x => x.CreatedDate)
                                                                    .FirstOrDefault(x => x.Status == EnumResultStatus.Unfinished);
                            if (extraPracticeExerciseResult != null)
                            {
                                extraPracticeExerciseResult.Status = EnumResultStatus.New;
                                _extraPracticeExerciseResultRepository.Update(extraPracticeExerciseResult);
                                await _extraPracticeExerciseResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
        }

        public async Task UpdateExtraPracticeResult(ExtraPracticeResult? extraPracticeResult, IList<ExtraPracticeExerciseResult>? extraPracticeExerciseResults, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(extraPracticeResult);
            ArgumentNullException.ThrowIfNull(extraPracticeExerciseResults);

            extraPracticeResult.CorrectCount += extraPracticeExerciseResults.Sum(x => x.CorrectCount);
            extraPracticeResult.Status = EnumResultStatus.Done;
            extraPracticeResult.Percent = extraPracticeResult.CorrectTotal > 0 ? (double)extraPracticeResult.CorrectCount / extraPracticeResult.CorrectTotal : 0;

            _extraPracticeResultRepository.Update(extraPracticeResult);
            await _extraPracticeResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateExtraPracticeResultTypeBook(ExtraPracticeResult? extraPracticeResult, IList<ExtraPracticeExerciseResult>? extraPracticeExerciseResults, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(extraPracticeResult);
            ArgumentNullException.ThrowIfNull(extraPracticeExerciseResults);

            extraPracticeResult.CorrectCount += extraPracticeExerciseResults.Sum(x => x.CorrectCount);
            extraPracticeResult.Status = EnumResultStatus.Done;
            extraPracticeResult.Percent = extraPracticeResult.CorrectTotal > 0 ? (double)extraPracticeResult.CorrectCount / extraPracticeResult.CorrectTotal : 0;

            _extraPracticeResultRepository.Update(extraPracticeResult);
            await _extraPracticeResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
