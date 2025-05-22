using System.Collections.Concurrent;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Identity.Application.Services.LmsCourseService;
using Fsel.Identity.Application.Services.LmsCourseService.Model;
using Fsel.Identity.Application.Services.LmsCourseService.QueryModels;
using Fsel.Identity.Application.Services.SystemService;
using Fsel.Identity.Application.Services.SystemService.Model;
using Fsel.Identity.Application.Services.SystemService.QueryModels;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.EntityModels;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.StudentCmd.StudentEventCmd
{
    public class AggregateDataStudentsInEventCommand : IRequest<MethodResult<bool>>
    {
    }

    public class AggregateDataStudentsInEventCommandHandler : IRequestHandler<AggregateDataStudentsInEventCommand, MethodResult<bool>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly UserManager<User> _userManager;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly ISystemService _systemService;
        private readonly IStudentEventLearningRecordRepository _studentEventLearningRecordRepository;
        private readonly IStudentDailyStreakRepository _studentDailyStreakRepository;

        public AggregateDataStudentsInEventCommandHandler(ICompetitionEventsRepository competitionEventsRepository, IStudentCompetitionEventsRepository studentCompetitionEventsRepository, IStudentRepository studentRepository, UserManager<User> userManager, ILmsCourseService lmsCourseService, ISystemService systemService, IStudentEventLearningRecordRepository studentEventLearningRecordRepository, IStudentDailyStreakRepository studentDailyStreakRepository)
        {
            _competitionEventsRepository = competitionEventsRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _studentRepository = studentRepository;
            _userManager = userManager;
            _lmsCourseService = lmsCourseService;
            _systemService = systemService;
            _studentEventLearningRecordRepository = studentEventLearningRecordRepository;
            _studentDailyStreakRepository = studentDailyStreakRepository;
        }

        public async Task<MethodResult<bool>> Handle(AggregateDataStudentsInEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date.AddDays(-1);

            var competitionEvents = await _competitionEventsRepository.Queryable.ToListAsync(cancellationToken);

            competitionEvents = competitionEvents.Where(p => p.EventContent != null && p.EventContent.EndDate.HasValue && p.EventContent.EndDate.Value.Date == currentDate.Date).ToList();

            if (competitionEvents == null || !competitionEvents.Any())
            {
                return methodResult;
            }

            var competitionEvent = competitionEvents.First();
            var startDate = competitionEvent.EventContent?.StartDate;
            var endDate = competitionEvent.EventContent?.EndDate;

            if (!startDate.HasValue || !endDate.HasValue)
            {
                return methodResult;
            }

            var competitionEventIds = competitionEvents.Select(p => p.Id).ToList();

            var studentIds = await _studentCompetitionEventsRepository.Queryable.WhereBulkContains(competitionEventIds, p => p.CompetitionEventId).Select(p => p.StudentId).ToListAsync(cancellationToken);

            var students = await (from s in _studentRepository.Queryable.WhereBulkContains(studentIds, p => p.Id)
                                  join u in _userManager.Users on s.UserId equals u.Id
                                  join sce in _studentCompetitionEventsRepository.Queryable on s.Id equals sce.StudentId
                                  select new StudentCompetitionEventModel
                                  {
                                      StudentId = s.Id,
                                      UserId = u.Id,
                                      CompetitionEventId = sce.CompetitionEventId
                                  }).ToListAsync(cancellationToken);

            students = students.DistinctBy(p => p.StudentId).ToList();

            int chunkSize = 10000;
            var chunks = ChunkList(students, chunkSize);

            var dataLearns = new List<AggregateDataLearnStudentsInEventModel>();
            var dataOthers = new List<AggregateDataOtherStudentsInEventModel>();

            foreach (var chunk in chunks)
            {
                var dataLearnResults = await _lmsCourseService.AggregateDataStudentsInEvent(new AggregateDataLearnStudentsInEventQueryModel()
                {
                    StartDate = startDate.Value,
                    EndDate = endDate.Value,
                    StudentIds = chunk.Select(p => p.StudentId).ToList()
                });

                if (!dataLearnResults.IsSuccessStatusCode)
                {
                    methodResult.AddError(dataLearnResults.Error);
                    return methodResult;
                }

                var learnResults = dataLearnResults.Content?.Result;

                if (learnResults != null)
                {
                    dataLearns.AddRange(learnResults);
                }

                var dataOtherResults = await _systemService.AggregateDataStudentsInEvent(new AggregateDataOtherStudentsInEventQueryModel()
                {
                    StartDate = startDate.Value,
                    EndDate = endDate.Value,
                    Students = chunk.Select(p => new AggregateDataOtherStudentInEventQueryModel()
                    {
                        StudentId = p.StudentId,
                        UserId = p.UserId
                    }).ToList()
                });

                if (!dataOtherResults.IsSuccessStatusCode)
                {
                    methodResult.AddError(dataOtherResults.Error);
                    return methodResult;
                }

                var otherResults = dataOtherResults.Content?.Result;

                if (otherResults != null)
                {
                    dataOthers.AddRange(otherResults);
                }
            }

            var studentDailyStreaks = await _studentDailyStreakRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId).Where(p => p.DailyDate.Date >= startDate.Value.Date && p.DailyDate.Date <= endDate.Value.Date).ToListAsync(cancellationToken);

            var studentEventLearningRecords = new ConcurrentBag<StudentEventLearningRecord>();

            Parallel.ForEach(students, p =>
            {
                var dataLearn = dataLearns.FirstOrDefault(x => x.StudentId == p.StudentId);
                var dataOther = dataOthers.FirstOrDefault(x => x.StudentId == p.StudentId);
                var totalDailyStreak = studentDailyStreaks.Count(x => x.StudentId == p.StudentId);

                studentEventLearningRecords.Add(new StudentEventLearningRecord()
                {
                    StudentId = p.StudentId,
                    CompetitionEventId = p.CompetitionEventId,
                    TotalLessons = dataLearn?.TotalLesson ?? 0,
                    TotalLearningDays = totalDailyStreak,
                    TotalVocabulary = dataLearn?.TotalVocabulary ?? 0,
                    TotalReading = dataLearn?.TotalReading ?? 0,
                    TotalListening = dataLearn?.TotalListening ?? 0,
                    TotalGrammar = dataLearn?.TotalGrammar ?? 0,
                    TotalSpeaking = dataLearn?.TotalSpeaking ?? 0,
                    TotalWriting = dataLearn?.TotalWriting ?? 0,
                    TotalQuestBoards = dataOther?.NumberQuestBoard ?? 0,
                    TotalTokens = dataOther?.NumberToken ?? 0,
                });
            });

            await _studentEventLearningRecordRepository.ExecuteTransactionAsync(async () =>
            {
                await _studentEventLearningRecordRepository.BulkMergeAsync(studentEventLearningRecords);
                await _studentCompetitionEventsRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            return methodResult;
        }

        private static List<List<StudentCompetitionEventModel>> ChunkList(List<StudentCompetitionEventModel> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(g => g.Select(x => x.Value).ToList())
                .ToList();
        }
    }
}
