namespace Fsel.Training.Application.Queries.ClassStudentQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentIdsIn7DayChooseLevelQuery : IRequest<MethodResult<IList<Guid>>>
    {
    }

    public class GetStudentIdsIn7DayChooseLevelQueryHandler : IRequestHandler<GetStudentIdsIn7DayChooseLevelQuery, MethodResult<IList<Guid>>>
    {
        private readonly IClassStudentRepository _classStudentRepository;

        public GetStudentIdsIn7DayChooseLevelQueryHandler(IClassStudentRepository classStudentRepository)
        {
            _classStudentRepository = classStudentRepository;
        }

        public async Task<MethodResult<IList<Guid>>> Handle(GetStudentIdsIn7DayChooseLevelQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<Guid>>();

            var currentDate = DateTime.UtcNow.AddDays(-7);

            var studentIds = await _classStudentRepository.Queryable.Where(p => p.CreatedDate.Date >= currentDate.Date).Select(p => p.StudentId).Distinct().ToListAsync(cancellationToken);

            methodResult.Result = studentIds;
            return methodResult;
        }
    }
}
