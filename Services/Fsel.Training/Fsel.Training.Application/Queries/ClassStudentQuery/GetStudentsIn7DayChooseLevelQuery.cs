namespace Fsel.Training.Application.Queries.ClassStudentQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Training.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentsIn7DayChooseLevelQuery : IRequest<MethodResult<IList<StudentsIn7DayChooseLevelModel>>>
    {
    }

    public class GetStudentsIn7DayChooseLevelQueryHandler : IRequestHandler<GetStudentsIn7DayChooseLevelQuery, MethodResult<IList<StudentsIn7DayChooseLevelModel>>>
    {
        private readonly IClassStudentRepository _classStudentRepository;

        public GetStudentsIn7DayChooseLevelQueryHandler(IClassStudentRepository classStudentRepository)
        {
            _classStudentRepository = classStudentRepository;
        }

        public async Task<MethodResult<IList<StudentsIn7DayChooseLevelModel>>> Handle(GetStudentsIn7DayChooseLevelQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<StudentsIn7DayChooseLevelModel>>();

            var currentDate = DateTime.UtcNow.AddDays(-7);

            var classStudents = await _classStudentRepository.Queryable.Where(p => p.CreatedDate.Date >= currentDate.Date).OrderByDescending(p => p.CreatedDate).ToListAsync(cancellationToken);
            classStudents = classStudents.DistinctBy(p => p.StudentId).ToList();
            var students = classStudents.Select(p => new StudentsIn7DayChooseLevelModel()
            {
                UserId = p.CreatedUserId,
                StudentId = p.StudentId,
                CreatedDate = p.CreatedDate
            }).ToList();

            methodResult.Result = students;
            return methodResult;
        }
    }
}
