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

        public AggregateDataStudentsInEventQueryHandler(IQuestBoardStudentRepository questBoardStudentRepository, IQuestBoardOverallStudentRepository questBoardOverallStudentRepository, ITokenHistoryRepository tokenHistoryRepository)
        {
            _questBoardStudentRepository = questBoardStudentRepository;
            _questBoardOverallStudentRepository = questBoardOverallStudentRepository;
            _tokenHistoryRepository = tokenHistoryRepository;
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

            var questBoardEntities = await _questBoardStudentRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId).Where(p => p.Status == EnumQuestBoardStudentStatus.Received && p.CreatedDate >= request.StartDate && p.CreatedDate <= request.EndDate).ToListAsync(cancellationToken);

            var questBoardOverallEntities = await _questBoardOverallStudentRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId).Where(p => p.Status == EnumQuestBoardOverallStudentStatus.Received && p.CreatedDate >= request.StartDate && p.CreatedDate <= request.EndDate).ToListAsync(cancellationToken);

            var tokenHistoryEntities = await _tokenHistoryRepository.Queryable.WhereBulkContains(userIds, p => p.UserId).Where(p => p.Type == EnumTokenHistoryType.Recevived && p.CreatedDate >= request.StartDate && p.CreatedDate <= request.EndDate).ToListAsync(cancellationToken);

            var students = new List<AggregateDataStudentsInEventModel>();

            request.Students.ForEach(p =>
            {
                var questBoards = questBoardEntities.Where(x => x.StudentId == p.StudentId).Count();
                var questBoardOveralls = questBoardOverallEntities.Where(x => x.StudentId == p.StudentId).Count();
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
