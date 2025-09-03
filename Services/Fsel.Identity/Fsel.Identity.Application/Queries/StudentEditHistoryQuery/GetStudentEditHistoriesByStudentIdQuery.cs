namespace Fsel.Identity.Application.Queries.StudentEditHistoryQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentEditHistoriesByStudentIdQuery : IRequest<MethodResult<IList<StudentEditHistoryModel>>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetStudentEditHistoriesByStudentIdQueryHandler : IRequestHandler<GetStudentEditHistoriesByStudentIdQuery, MethodResult<IList<StudentEditHistoryModel>>>
    {
        private readonly IStudentEditHistoryRepository _studentEditHistoryRepository;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IStudentRepository _studentRepository;

        public GetStudentEditHistoriesByStudentIdQueryHandler(IStudentEditHistoryRepository studentEditHistoryRepository, UserManager<User> userManager, IMapper mapper, IStudentRepository studentRepository)
        {
            _studentEditHistoryRepository = studentEditHistoryRepository;
            _userManager = userManager;
            _mapper = mapper;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<IList<StudentEditHistoryModel>>> Handle(GetStudentEditHistoriesByStudentIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<StudentEditHistoryModel>>();

            var query = await (from seh in _studentEditHistoryRepository.Queryable
                               join u in _userManager.Users on seh.CreatedUserId equals u.Id into userGroup
                               from u in userGroup.DefaultIfEmpty()
                               where seh.StudentId == request.StudentId
                               select new
                               {
                                   StudentEditHistory = seh,
                                   User = u
                               }).ToListAsync(cancellationToken);

            var result = query.Select(p => new StudentEditHistoryModel()
            {
                Id = p.StudentEditHistory.Id,
                CreatedUserId = p.StudentEditHistory.CreatedUserId,
                CreatedFullName = p.User?.FullName,
                Type = p.StudentEditHistory.Type,
                Description = p.StudentEditHistory.Description,
                CreatedDate = p.StudentEditHistory.CreatedDate.AddHours(7),
                StudentId = p.StudentEditHistory.StudentId,
                EditDetail = _mapper.Map<StudentEditHistoryDetailModel>(p.StudentEditHistory.EditDetail)
            }).ToList();

            var studentIds = result.Select(p => p.StudentId).Distinct().ToList();

            var students = await (from s in _studentRepository.Queryable.WhereBulkContains(studentIds, p => p.Id).IgnoreQueryFilters()
                                  join u in _userManager.Users.IgnoreQueryFilters() on s.UserId equals u.Id
                                  select new
                                  {
                                      Student = s,
                                      User = u
                                  }).ToListAsync(cancellationToken);

            result.ForEach(p =>
            {
                if (p.CreatedUserId == default)
                {
                    var student = students.First();
                    p.CreatedUserId = student.User.Id;
                    p.CreatedFullName = student.User.FullName;
                }
            });

            methodResult.Result = result.OrderByDescending(p => p.CreatedDate).ToList();

            return methodResult;
        }
    }
}
