namespace Fsel.Identity.Application.Queries.EventQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentIdsInEventByStudentIdsQuery : IRequest<MethodResult<List<Guid>?>>
    {
        public IList<Guid>? StudentIds { get; set; }
    }

    public class GetStudentIdsInEventByStudentIdsQueryHandler : IRequestHandler<GetStudentIdsInEventByStudentIdsQuery, MethodResult<List<Guid>?>>
    {
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;

        public GetStudentIdsInEventByStudentIdsQueryHandler(IStudentCompetitionEventsRepository studentCompetitionEventsRepository, ICompetitionEventsRepository competitionEventsRepository)
        {
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _competitionEventsRepository = competitionEventsRepository;
        }

        public async Task<MethodResult<List<Guid>?>> Handle(GetStudentIdsInEventByStudentIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<List<Guid>?>();

            if (request.StudentIds == null || !request.StudentIds.Any())
            {
                methodResult.Result = null;
                return methodResult;
            }

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var students = await (from sce in _studentCompetitionEventsRepository.Queryable.WhereBulkContains(request.StudentIds, p => p.StudentId)
                                  join ce in _competitionEventsRepository.Queryable on sce.CompetitionEventId equals ce.Id
                                  select new
                                  {
                                      StudentCompetitionEvent = sce,
                                      CompetitionEvent = ce
                                  }).ToListAsync(cancellationToken);

            students = students.Where(p => p.CompetitionEvent.EventContent != null && p.CompetitionEvent.EventContent.EndDate.HasValue && p.CompetitionEvent.EventContent.EndDate > currentDate).ToList();

            methodResult.Result = students.Select(p => p.StudentCompetitionEvent.StudentId).ToList();
            return methodResult;
        }
    }
}
