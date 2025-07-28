using Fsel.Common.ActionResults;
using Fsel.Shared.Enums;
using Fsel.System.Domain.IRepositories;
using Fsel.System.Domain.Models.EntityModels;
using Fsel.System.Domain.Models.QueryModels;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fsel.System.Application.Queries.Reports
{
    public class AggregateDataStudentsInEventQuery : AggregateDataStudentsInEventQueryModel, IRequest<MethodResult<IList<AggregateDataStudentsInEventModel>>>
    {
    }

    public class AggregateDataStudentsInEventQueryHandler : IRequestHandler<AggregateDataStudentsInEventQuery, MethodResult<IList<AggregateDataStudentsInEventModel>>>
    {
        private readonly IQuestBoardStudentRepository _questBoardStudentRepository;
        private readonly IQuestBoardOverallStudentRepository _questBoardOverallStudentRepository;
        private readonly ITokenHistoryRepository _tokenHistoryRepository;
        private readonly IQuestBoardRepository _questBoardRepository;
        private readonly IQuestBoardOverallRepository _questBoardOverallRepository;

        public AggregateDataStudentsInEventQueryHandler(IQuestBoardStudentRepository questBoardStudentRepository, IQuestBoardOverallStudentRepository questBoardOverallStudentRepository, ITokenHistoryRepository tokenHistoryRepository, IQuestBoardRepository questBoardRepository, IQuestBoardOverallRepository questBoardOverallRepository)
        {
            _questBoardStudentRepository = questBoardStudentRepository;
            _questBoardOverallStudentRepository = questBoardOverallStudentRepository;
            _tokenHistoryRepository = tokenHistoryRepository;
            _questBoardRepository = questBoardRepository;
            _questBoardOverallRepository = questBoardOverallRepository;
        }

        public async Task<MethodResult<IList<AggregateDataStudentsInEventModel>>> Handle(AggregateDataStudentsInEventQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<AggregateDataStudentsInEventModel>>();

            var studentIds = request.Students?.Select(p => p.StudentId);
            var userIds = request.Students?.Select(p => p.UserId);
            if (studentIds == null || !studentIds.Any())
            {
                return methodResult;
            }

            var questBoardDict = await _questBoardRepository.Queryable
                 .ToDictionaryAsync(x => x.Id, cancellationToken);

            var questBoardOverallDict = await _questBoardOverallRepository.Queryable
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            var questBoardStudentEntities = await _questBoardStudentRepository.Queryable
                .WhereBulkContains(studentIds, p => p.StudentId)
                .Where(p => p.CreatedDate >= request.StartDate && p.CreatedDate <= request.EndDate)
                .ToListAsync(cancellationToken);

            var questBoardStudents = questBoardStudentEntities
                .Where(p => questBoardDict.TryGetValue(p.QuestBoardId, out var questBoard) &&
                            questBoard.TargetValue <= p.CurrentValue)
                .ToList();

            var questBoardOverallStudentEntities = await _questBoardOverallStudentRepository.Queryable
                .WhereBulkContains(studentIds, p => p.StudentId)
                .Where(p => p.CreatedDate >= request.StartDate && p.CreatedDate <= request.EndDate)
                .ToListAsync(cancellationToken);

            var questBoardOverallStudents = questBoardOverallStudentEntities
                .Where(p => questBoardOverallDict.TryGetValue(p.QuestBoardOverallId, out var questBoardOverall) &&
                            questBoardOverall.TargetValue <= p.CurrentValue)
                .ToList();

            var tokenHistoryEntities = await _tokenHistoryRepository.Queryable.WhereBulkContains(userIds, p => p.UserId).Where(p => p.Type == EnumTokenHistoryType.Recevived && p.CreatedDate >= request.StartDate && p.CreatedDate <= request.EndDate).ToListAsync(cancellationToken);

            var students = new List<AggregateDataStudentsInEventModel>();

            request.Students.ForEach(p =>
            {
                var questBoards = questBoardStudents.Where(x => x.StudentId == p.StudentId).Count();
                var questBoardOveralls = questBoardOverallStudents.Where(x => x.StudentId == p.StudentId).Count();
                var totalToken = tokenHistoryEntities.Where(x => x.UserId == p.UserId).Sum(p => p.VolatileToken);

                students.Add(new AggregateDataStudentsInEventModel()
                {
                    StudentId = p.StudentId,
                    NumberQuestBoard = questBoards + questBoardOveralls,
                    NumberToken = (int)totalToken
                });
            });
            methodResult.Result = students;
            return methodResult;
        }
    }
}
