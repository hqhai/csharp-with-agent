// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Repositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class UpdatedExtraPracticeExerciseResultThenUpdateExtraPracticeResultHandler :
            INotificationHandler<EntityChangedEvent<ExtraPracticeExerciseResult>>
    {
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;
        private readonly IExtraPracticeExerciseRepository _extraPracticeExerciseRepository;
        private readonly IExtraPracticeAnswerRepository _extraPracticeAnswerRepository;
        private readonly IExtraPracticeChapterRepository _extraPracticeChapterRepository;
        private readonly IExtraPracticeExerciseResultRepository _extraPracticeExerciseResultRepository;
        private readonly IExtraPracticeRepository _extraPracticeRepository;

        public UpdatedExtraPracticeExerciseResultThenUpdateExtraPracticeResultHandler(
            IExtraPracticeResultRepository extraPracticeResultRepository,
            IExtraPracticeExerciseRepository extraPracticeExerciseRepository,
            IExtraPracticeAnswerRepository extraPracticeAnswerRepository,
            IExtraPracticeChapterRepository extraPracticeChapterRepository,
            IExtraPracticeExerciseResultRepository extraPracticeExerciseResultRepository,
            IExtraPracticeRepository extraPracticeRepository)
        {
            _extraPracticeResultRepository = extraPracticeResultRepository;
            _extraPracticeExerciseRepository = extraPracticeExerciseRepository;
            _extraPracticeAnswerRepository = extraPracticeAnswerRepository;
            _extraPracticeChapterRepository = extraPracticeChapterRepository;
            _extraPracticeExerciseResultRepository = extraPracticeExerciseResultRepository;
            _extraPracticeRepository = extraPracticeRepository;
        }

        public async Task Handle(EntityChangedEvent<ExtraPracticeExerciseResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var extraPraticeExerciseResult = notification.Data;
            if (extraPraticeExerciseResult.Status == EnumResultStatus.Done)
            {
                var extraPracticeResult = await _extraPracticeResultRepository.GetByIdAsync(extraPraticeExerciseResult.ExtraPracticeResultId);
                var extraPracticeExerciseResults = await _extraPracticeExerciseResultRepository.Queryable
                                .Where(x => x.ExtraPracticeResultId == extraPraticeExerciseResult.ExtraPracticeResultId && x.StudentId == extraPraticeExerciseResult.StudentId).ToListAsync(cancellationToken);
                var extraPractice = await _extraPracticeRepository.GetByIdAsync(extraPracticeResult?.ExtraPracticeId ?? default);

                if (extraPracticeResult != null && extraPractice != null)
                {
                    if (extraPractice.Type == EnumExtraPracticeType.Book)
                    {
                        extraPractice = await _extraPracticeRepository.Queryable
                                                         .Include(x => x.ExtraPracticeChapters.Where(y => !y.IsDeleted))
                                                             .ThenInclude(x => x.ExtraPracticeExercises.Where(y => !y.IsDeleted))
                                                             .ThenInclude(x => x.Exercise)
                                                         .Include(x => x.ExtraPracticeChapters.Where(y => !y.IsDeleted))
                                                             .ThenInclude(x => x.ExtraPracticeExercises.Where(y => !y.IsDeleted))
                                                             .ThenInclude(x => x.ExtraPracticeExerciseResults)
                                                         .FirstOrDefaultAsync(x => x.Id == extraPracticeResult.ExtraPracticeId, cancellationToken);
                        if (extraPractice != null)
                        {
                            var extraPracticeChapters = extraPractice.ExtraPracticeChapters;
                            var extraPracticeExercises = extraPracticeChapters.SelectMany(x => x.ExtraPracticeExercises).ToList();
                            var extraPracticeExercise = extraPracticeExercises.FirstOrDefault(x => x.Id == extraPraticeExerciseResult.ExtraPracticeExerciseId);

                            var exerciseCount = extraPracticeExercises.Select(x => x.Exercise).Count();
                            var resultCount = extraPracticeExercises.SelectMany(x => x.ExtraPracticeExerciseResults)
                                                                                  .Where(y => y.StudentId == extraPracticeResult.StudentId && y.Status == EnumResultStatus.Done)
                                                                                  .Count();

                            if (extraPracticeExercise != null)
                            {
                                var extraPracticeChapter = extraPracticeChapters.FirstOrDefault(x => x.Id == extraPracticeExercise.ExtraPracticeChapterId);
                                var exerciseChapterCount = extraPracticeChapter?.ExtraPracticeExercises.Count;
                                var resultChapterCount = extraPracticeChapter?.ExtraPracticeExercises.SelectMany(x => x.ExtraPracticeExerciseResults)
                                                                                  .Where(y => y.StudentId == extraPracticeResult.StudentId && y.Status == EnumResultStatus.Done)
                                                                                  .Count();
                                if (exerciseCount == resultCount)
                                {
                                    await UpdateExtraPracticeResultTypeBook(extraPracticeResult, cancellationToken);
                                }
                                else if (resultChapterCount == exerciseChapterCount && extraPracticeChapter != null)
                                {
                                    extraPracticeChapter = extraPracticeChapters.OrderBy(x => x.CreatedDate).FirstOrDefault(x => x.Id != extraPracticeExercise.ExtraPracticeChapterId && x.PageNumber > extraPracticeChapter.PageNumber);
                                    var extraPracticeExerciseResult = extraPracticeChapter?.ExtraPracticeExercises
                                                                            .SelectMany(x => x.ExtraPracticeExerciseResults)
                                                                            .OrderBy(x => x.CreatedDate)
                                                                            .FirstOrDefault(x => x.ExecuteCount == 0 && x.StudentId == extraPraticeExerciseResult.StudentId);

                                    if (extraPracticeExerciseResult != null)
                                    {
                                        extraPracticeExerciseResult.Status = EnumResultStatus.New;
                                        await _extraPracticeExerciseResultRepository.BulkUpdateList(new List<ExtraPracticeExerciseResult> { extraPracticeExerciseResult }, bulk =>
                                        {
                                            bulk.IgnoreOnUpdateExpression = c => new { c.ExtraPracticeResultId, c.StudentId, c.ExtraPracticeExerciseId };
                                        });
                                    }
                                    await UpdateExtraPracticeExerciseResultTypeBook(extraPracticeResult, cancellationToken);
                                }
                                else
                                {
                                    var extraPracticeExerciseResult = extraPracticeChapter?.ExtraPracticeExercises
                                                                            .SelectMany(x => x.ExtraPracticeExerciseResults)
                                                                            .OrderBy(x => x.CreatedDate)
                                                                            .FirstOrDefault(x => x.ExecuteCount == 0 && x.StudentId == extraPraticeExerciseResult.StudentId);

                                    if (extraPracticeExerciseResult != null)
                                    {
                                        extraPracticeExerciseResult.Status = EnumResultStatus.New;
                                        await _extraPracticeExerciseResultRepository.BulkUpdateList(new List<ExtraPracticeExerciseResult> { extraPracticeExerciseResult }, bulk =>
                                        {
                                            bulk.IgnoreOnUpdateExpression = c => new { c.ExtraPracticeResultId, c.StudentId, c.ExtraPracticeExerciseId };
                                        });
                                    }
                                }
                            }
                        }
                    }
                    else if (extraPractice.Type == EnumExtraPracticeType.VideoEmbed)
                    {
                        extraPractice = await _extraPracticeRepository.Queryable.Include(x => x.ExtraPracticeExercises.Where(y => !y.IsDeleted))
                                                                .ThenInclude(x => x.Exercise)
                                                             .Include(x => x.ExtraPracticeExercises.Where(y => !y.IsDeleted))
                                                                .ThenInclude(x => x.ExtraPracticeExerciseResults)
                                                             .FirstOrDefaultAsync(x => x.Id == extraPracticeResult.ExtraPracticeId, cancellationToken);
                        if (extraPractice != null)
                        {
                            var extraPracticeExercises = extraPractice.ExtraPracticeExercises;
                            var exerciseCount = extraPracticeExercises.Select(x => x.Exercise).Count();
                            var resultCount = extraPracticeExercises.SelectMany(x => x.ExtraPracticeExerciseResults)
                                                                                  .Where(y => y.StudentId == extraPracticeResult.StudentId && y.Status == EnumResultStatus.Done)
                                                                                  .Count();

                            if (exerciseCount == resultCount)
                            {
                                await UpdateExtraPracticeResult(extraPracticeResult, cancellationToken);
                            }
                            else
                            {
                                var extraPracticeExerciseResult = extraPractice.ExtraPracticeExercises.SelectMany(x => x.ExtraPracticeExerciseResults)
                                                                        .OrderBy(x => x.CreatedDate)
                                                                        .FirstOrDefault(x => x.Status == EnumResultStatus.Unfinished);
                                if (extraPracticeExerciseResult != null)
                                {
                                    extraPracticeExerciseResult.Status = EnumResultStatus.New;
                                    await _extraPracticeExerciseResultRepository.BulkUpdateList(new List<ExtraPracticeExerciseResult> { extraPracticeExerciseResult }, bulk =>
                                    {
                                        bulk.IgnoreOnUpdateExpression = c => new { c.ExtraPracticeResultId, c.StudentId, c.ExtraPracticeExerciseId };
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task UpdateExtraPracticeResult(ExtraPracticeResult? extraPracticeResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(extraPracticeResult);
            var correctCounts = from baseQ in _extraPracticeRepository.Queryable
                                join ee in _extraPracticeExerciseRepository.Queryable on baseQ.Id equals ee.ExtraPracticeId
                                join eer in _extraPracticeExerciseResultRepository.Queryable on ee.Id equals eer.ExtraPracticeExerciseId
                                join ea in _extraPracticeAnswerRepository.Queryable on eer.Id equals ea.ExtraPracticeExerciseResultId
                                where baseQ.Id == extraPracticeResult.ExtraPracticeId
                                select new
                                {
                                    CorrectCount = ea.CorrectCount
                                };
            extraPracticeResult.CorrectCount = correctCounts.Sum(x => x.CorrectCount);
            extraPracticeResult.Status = EnumResultStatus.Done;
            await _extraPracticeResultRepository.BulkUpdateList(new List<ExtraPracticeResult> { extraPracticeResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.ExtraPracticeId, c.StudentId };
            });
        }

        private async Task UpdateExtraPracticeResultTypeBook(ExtraPracticeResult? extraPracticeResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(extraPracticeResult);
            var correctCounts = from baseQ in _extraPracticeRepository.Queryable
                                join ec in _extraPracticeChapterRepository.Queryable on baseQ.Id equals ec.ExtraPracticeId
                                join ee in _extraPracticeExerciseRepository.Queryable on ec.Id equals ee.ExtraPracticeChapterId
                                join eer in _extraPracticeExerciseResultRepository.Queryable on ee.Id equals eer.ExtraPracticeExerciseId
                                join ea in _extraPracticeAnswerRepository.Queryable on eer.Id equals ea.ExtraPracticeExerciseResultId
                                where baseQ.Id == extraPracticeResult.ExtraPracticeId
                                select new
                                {
                                    CorrectCount = ea.CorrectCount
                                };

            extraPracticeResult.CorrectCount = correctCounts.Sum(x => x.CorrectCount);
            extraPracticeResult.Status = EnumResultStatus.Done;
            await _extraPracticeResultRepository.BulkUpdateList(new List<ExtraPracticeResult> { extraPracticeResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.ExtraPracticeId, c.StudentId };
            });
        }

        private async Task UpdateExtraPracticeExerciseResultTypeBook(ExtraPracticeResult? extraPracticeResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(extraPracticeResult);
            var correctCounts = from baseQ in _extraPracticeRepository.Queryable
                                join ec in _extraPracticeChapterRepository.Queryable on baseQ.Id equals ec.ExtraPracticeId
                                join ee in _extraPracticeExerciseRepository.Queryable on ec.Id equals ee.ExtraPracticeChapterId
                                join eer in _extraPracticeExerciseResultRepository.Queryable on ee.Id equals eer.ExtraPracticeExerciseId
                                join ea in _extraPracticeAnswerRepository.Queryable on eer.Id equals ea.ExtraPracticeExerciseResultId
                                where baseQ.Id == extraPracticeResult.ExtraPracticeId
                                select new
                                {
                                    CorrectCount = ea.CorrectCount
                                };

            extraPracticeResult.CorrectCount = correctCounts.Sum(x => x.CorrectCount);
            extraPracticeResult.Status = EnumResultStatus.Process;
            await _extraPracticeResultRepository.BulkUpdateList(new List<ExtraPracticeResult> { extraPracticeResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.ExtraPracticeId, c.StudentId };
            });
        }
    }
}
