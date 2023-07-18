// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class UpdatedInvoiceInputThenUpdateExtraPracticeResultHandler :
            INotificationHandler<EntityChangedEvent<ExtraPracticeExerciseResult>>
    {
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;
        private readonly IExtraPracticeExerciseResultRepository _extraPracticeExerciseResultRepository;
        private readonly IExtraPracticeRepository _extraPracticeRepository;

        public UpdatedInvoiceInputThenUpdateExtraPracticeResultHandler(
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
            var extraPracticeResultId = notification.Data.ExtraPracticeResultId;
            var extraPracticeResult = await _extraPracticeResultRepository.GetByIdAsync(extraPracticeResultId);

            var extraPracticeExerciseResults = await _extraPracticeExerciseResultRepository.Queryable.Where(x => x.ExtraPracticeResultId == extraPracticeResultId).ToListAsync(cancellationToken);

            if (extraPracticeResult != null)
            {
                var extraPractice = await _extraPracticeRepository.GetByIdAsync(extraPracticeResult.Id);
                if (extraPractice != null && extraPractice.Type == EnumExtraPracticeType.Book)
                {
                    extraPractice = await _extraPracticeRepository.Queryable
                                                     .Include(x => x.ExtraPracticeChapters.Where(y => !y.IsDeleted))
                                                         .ThenInclude(x => x.ExtraPracticeExercises.Where(y => !y.IsDeleted))
                                                         .ThenInclude(x => x.Exercise)
                                                     .Include(x => x.ExtraPracticeChapters.Where(y => !y.IsDeleted))
                                                         .ThenInclude(x => x.ExtraPracticeExercises.Where(y => !y.IsDeleted))
                                                         .ThenInclude(x => x.ExtraPracticeExerciseResults.Where(y => y.Status == EnumResultStatus.Done && !y.IsDeleted && y.StudentId == extraPracticeResult.StudentId))
                                                     .FirstOrDefaultAsync(x => x.Id == extraPracticeResult.ExtraPracticeId, cancellationToken);
                    var exerciseCount = extraPractice!.ExtraPracticeChapters.SelectMany(x => x.ExtraPracticeExercises).Select(x => x.Exercise).Count();
                    var extraPracticeExerciseResultCount = extraPractice.ExtraPracticeChapters.SelectMany(x => x.ExtraPracticeExercises).Select(x => x.ExtraPracticeExerciseResults).Count();
                    extraPracticeResult.CorrectCount = extraPracticeExerciseResults.Sum(x => x.CorrectCount);
                    extraPracticeResult.CorrectTotal = extraPracticeExerciseResults.Sum(x => x.CorrectTotal);

                    if (exerciseCount == extraPracticeExerciseResultCount)
                    {
                        extraPracticeResult.Status = EnumResultStatus.Done;
                        extraPracticeResult.Percent = 100;
                    }

                    _extraPracticeResultRepository.Update(extraPracticeResult);
                    await _extraPracticeResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                else if (extraPractice != null && extraPractice.Type == EnumExtraPracticeType.VideoEmbed)
                {
                    extraPractice = await _extraPracticeRepository.Queryable.Include(x => x.ExtraPracticeExercises.Where(y => !y.IsDeleted))
                                                            .ThenInclude(x => x.Exercise)
                                                         .Include(x => x.ExtraPracticeExercises.Where(y => !y.IsDeleted))
                                                            .ThenInclude(x => x.ExtraPracticeExerciseResults.Where(y => y.Status == EnumResultStatus.Done && !y.IsDeleted && y.StudentId == extraPracticeResult.StudentId))
                                                         .FirstOrDefaultAsync(x => x.Id == extraPracticeResult.ExtraPracticeId, cancellationToken);
                    var exerciseCount = extraPractice!.ExtraPracticeExercises.Select(x => x.Exercise).Count();
                    var extraPracticeExerciseResultCount = extraPractice.ExtraPracticeExercises.Select(x => x.ExtraPracticeExerciseResults).Count();
                    extraPracticeResult.CorrectCount = extraPracticeExerciseResults.Sum(x => x.CorrectCount);
                    extraPracticeResult.CorrectTotal = extraPracticeExerciseResults.Sum(x => x.CorrectTotal);

                    if (exerciseCount == extraPracticeExerciseResultCount)
                    {
                        extraPracticeResult.Status = EnumResultStatus.Done;
                        extraPracticeResult.Percent = extraPracticeResult.CorrectTotal > 0 ? (double)extraPracticeResult.CorrectCount / extraPracticeResult.CorrectTotal : 0;
                    }

                    _extraPracticeResultRepository.Update(extraPracticeResult);
                    await _extraPracticeResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
