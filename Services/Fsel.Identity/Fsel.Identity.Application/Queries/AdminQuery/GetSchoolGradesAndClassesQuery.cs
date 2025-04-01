namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetSchoolGradesAndClassesQuery : IRequest<MethodResult<SchoolGradeClassModel>>
    {
    }

    public class GetSchoolGradesAndClassesQueryHandler : IRequestHandler<GetSchoolGradesAndClassesQuery, MethodResult<SchoolGradeClassModel>>
    {
        private readonly AuthContext _authContext;
        private readonly UserManager<User> _userManager;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private const string? Teacher = "Giáo Viên";

        public GetSchoolGradesAndClassesQueryHandler(AuthContext authContext, UserManager<User> userManager, ICompetitionEventsRepository competitionEventsRepository, IStudentRepository studentRepository, IStudentCompetitionEventsRepository studentCompetitionEventsRepository)
        {
            _authContext = authContext;
            _userManager = userManager;
            _competitionEventsRepository = competitionEventsRepository;
            _studentRepository = studentRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
        }

        public async Task<MethodResult<SchoolGradeClassModel>> Handle(GetSchoolGradesAndClassesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SchoolGradeClassModel>();

            var currentUser = await _userManager.Users.Include(p => p.UserSchools).FirstOrDefaultAsync(p => p.Id == _authContext.CurrentUserId, cancellationToken);
            if (currentUser == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(currentUser));
                return methodResult;
            }

            var schoolId = currentUser.UserSchools.FirstOrDefault()?.SchoolId;
            if (!schoolId.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(schoolId));
                return methodResult;
            }

            var competitionEvents = await _competitionEventsRepository.Queryable.ToListAsync(cancellationToken);
            var userCompetitionEvents = competitionEvents.Where(p => p.SchoolIds != null && p.SchoolIds.Contains(schoolId.Value) && (p.Category == EnumCompetitionEventCategory.Student || p.Category == EnumCompetitionEventCategory.Teacher)).ToList();

            if (userCompetitionEvents == null || userCompetitionEvents.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(userCompetitionEvents));
                return methodResult;
            }

            var competitionEventIds = userCompetitionEvents.Select(p => p.Id);

            var query = from s in _studentRepository.Queryable
                        join sce in _studentCompetitionEventsRepository.Queryable on s.Id equals sce.StudentId
                        where s.SchoolId == schoolId.Value && competitionEventIds.Contains(sce.CompetitionEventId)
                        select new
                        {
                            Student = s,
                            Grade = s.SchoolGrade,
                            Class = s.SchoolClass
                        };

            var students = query.ToList();

            var gradeClassList = students
                .Where(s => !string.IsNullOrEmpty(s.Class) && char.IsDigit(s.Class[0]))
                .GroupBy(s => Regex.Match(s.Class, @"^\d+").Value)
                .Select(g => new SchoolGradeClass
                {
                    Grade = g.Key,
                    Classes = g.Select(s => s.Class).Distinct().OrderBy(p => p).ToList()
                })
                .ToList();

            gradeClassList.Add(new SchoolGradeClass()
            {
                Grade = Teacher,
                Classes = new List<string>() { Teacher }
            });

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            methodResult.Result = new SchoolGradeClassModel()
            {
                IsHaveConfigStudent = userCompetitionEvents.Any(p => p.Category == EnumCompetitionEventCategory.Student && p.EventContent != null && p.EventContent.ActionConfigs != null && p.EventContent.ActionConfigs.Any(x => x.Action == EnumSchoolEventRuleAction.ImportStudent && x.EndDate.HasValue && x.EndDate.Value > currentDate)),

                IsHaveConfigTeacher = userCompetitionEvents.Any(p => p.Category == EnumCompetitionEventCategory.Teacher && p.EventContent != null && p.EventContent.ActionConfigs != null && p.EventContent.ActionConfigs.Any(x => x.Action == EnumSchoolEventRuleAction.ImportStudent && x.EndDate.HasValue && x.EndDate.Value > currentDate)),

                GradesAndClasses = gradeClassList.OrderBy(p => p.Grade).ToList()
            };
            return methodResult;
        }
    }
}
