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

        /// <summary>
        /// Xử lý yêu cầu lấy danh sách lớp và khối của trường học
        /// </summary>
        /// <param name="request">Yêu cầu từ người dùng</param>
        /// <param name="cancellationToken">Token hủy thao tác</param>
        /// <returns>Kết quả chứa thông tin lớp và khối</returns>
        public async Task<MethodResult<SchoolGradeClassModel>> Handle(GetSchoolGradesAndClassesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<SchoolGradeClassModel>();

            // Lấy thông tin người dùng hiện tại
            var currentUser = await GetCurrentUserAsync(cancellationToken).ConfigureAwait(false);
            if (currentUser == null)
            {
                return BuildErrorResult(methodResult, EnumSystemErrorCode.DataNotExist, "Không tìm thấy thông tin người dùng hiện tại");
            }

            // Lấy thông tin trường học
            var schoolId = GetSchoolId(currentUser);
            if (!schoolId.HasValue)
            {
                return BuildErrorResult(methodResult, EnumSystemErrorCode.DataNotExist, "Không tìm thấy thông tin trường học");
            }

            // Lấy các sự kiện thi đấu
            var userCompetitionEvents = await GetUserCompetitionEventsAsync(schoolId.Value, cancellationToken).ConfigureAwait(false);
            if (userCompetitionEvents == null || userCompetitionEvents.Count == 0)
            {
                return BuildErrorResult(methodResult, EnumSystemErrorCode.DataNotExist, "Không tìm thấy sự kiện thi đấu");
            }

            // Lấy danh sách học sinh và lớp học
            var gradeClassList = await GetGradeClassListAsync(schoolId.Value, userCompetitionEvents.Select(p => p.Id), cancellationToken).ConfigureAwait(false);

            // Tính toán và trả về kết quả
            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
            methodResult.Result = BuildResultModel(currentDate, userCompetitionEvents, gradeClassList);

            return methodResult;
        }

        /// <summary>
        /// Lấy thông tin người dùng hiện tại
        /// </summary>
        /// <param name="cancellationToken">Token hủy thao tác</param>
        /// <returns>Thông tin người dùng</returns>
        private async Task<User> GetCurrentUserAsync(CancellationToken cancellationToken)
        {
            return await _userManager.Users
                .Include(p => p.UserSchools)
                .FirstOrDefaultAsync(p => p.Id == _authContext.CurrentUserId, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Lấy ID trường học từ thông tin người dùng
        /// </summary>
        /// <param name="currentUser">Thông tin người dùng</param>
        /// <returns>ID trường học</returns>
        private static Guid? GetSchoolId(User currentUser)
        {
            return currentUser.UserSchools.FirstOrDefault()?.SchoolId;
        }

        /// <summary>
        /// Tạo kết quả lỗi
        /// </summary>
        /// <param name="methodResult">Đối tượng kết quả</param>
        /// <param name="errorCode">Mã lỗi</param>
        /// <param name="errorMessage">Thông báo lỗi</param>
        /// <returns>Kết quả lỗi</returns>
        private static MethodResult<SchoolGradeClassModel> BuildErrorResult(MethodResult<SchoolGradeClassModel> methodResult, EnumSystemErrorCode errorCode, string errorMessage)
        {
            methodResult.AddErrorBadRequest(nameof(errorCode), errorMessage);
            return methodResult;
        }

        /// <summary>
        /// Lấy danh sách sự kiện thi đấu
        /// </summary>
        /// <param name="schoolId">ID trường học</param>
        /// <param name="cancellationToken">Token hủy thao tác</param>
        /// <returns>Danh sách sự kiện thi đấu</returns>
        private async Task<List<CompetitionEvent>> GetUserCompetitionEventsAsync(Guid schoolId, CancellationToken cancellationToken)
        {
            var competitionEvents = await _competitionEventsRepository.Queryable
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return competitionEvents
                .Where(p => p.SchoolIds != null &&
                      p.SchoolIds.Contains(schoolId) &&
                      (p.Category == EnumCompetitionEventCategory.Student ||
                       p.Category == EnumCompetitionEventCategory.Teacher))
                .ToList();
        }

        /// <summary>
        /// Lấy danh sách lớp và khối
        /// </summary>
        /// <param name="schoolId">ID trường học</param>
        /// <param name="competitionEventIds">Danh sách ID sự kiện thi đấu</param>
        /// <param name="cancellationToken">Token hủy thao tác</param>
        /// <returns>Danh sách lớp và khối</returns>
        private async Task<List<SchoolGradeClass>> GetGradeClassListAsync(Guid schoolId, IEnumerable<Guid> competitionEventIds, CancellationToken cancellationToken)
        {
            var query = from s in _studentRepository.Queryable
                        join sce in _studentCompetitionEventsRepository.Queryable on s.Id equals sce.StudentId
                        where s.SchoolId == schoolId && competitionEventIds.Contains(sce.CompetitionEventId)
                        select new
                        {
                            Student = s,
                            Grade = s.SchoolGrade,
                            Class = s.SchoolClass
                        };

            var students = await query
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

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

            return gradeClassList;
        }

        /// <summary>
        /// Tính toán thời gian hiện tại trong ngày (số giờ từ 00:00)
        /// </summary>
        /// <param name="dateTime">Thời gian hiện tại</param>
        /// <returns>Số giờ từ 00:00 (có thể là số thập phân)</returns>
        private static double CalculateTimeOfDay(DateTime dateTime)
        {
            // Tính toán giờ + phần thập phân của giờ (phút/60 + giây/3600)
            return dateTime.Hour + (dateTime.Minute / 60.0) + (dateTime.Second / 3600.0);
        }

        /// <summary>
        /// Tạo mô hình kết quả
        /// </summary>
        /// <param name="currentDate">Thời gian hiện tại</param>
        /// <param name="userCompetitionEvents">Danh sách sự kiện thi đấu</param>
        /// <param name="gradeClassList">Danh sách lớp và khối</param>
        /// <returns>Mô hình kết quả</returns>
        private static SchoolGradeClassModel BuildResultModel(
            DateTime currentDate,
            List<CompetitionEvent> userCompetitionEvents,
            List<SchoolGradeClass> gradeClassList)
        {
            var currentTimeOfDay = CalculateTimeOfDay(currentDate);

            return new SchoolGradeClassModel()
            {
                IsHaveConfigStudent = HasValidStudentConfig(userCompetitionEvents, currentDate),
                IsHaveConfigTeacher = HasValidTeacherConfig(userCompetitionEvents, currentDate),
                GradesAndClasses = gradeClassList.OrderBy(p => p.Grade).ToList(),
                IsExportAllowed = IsExportTimeAllowed(userCompetitionEvents, currentTimeOfDay)
            };
        }

        /// <summary>
        /// Kiểm tra xem có cấu hình học sinh hợp lệ hay không
        /// </summary>
        /// <param name="userCompetitionEvents">Danh sách sự kiện thi đấu</param>
        /// <param name="currentDate">Thời gian hiện tại</param>
        /// <returns>Có cấu hình học sinh hợp lệ hay không</returns>
        private static bool HasValidStudentConfig(List<CompetitionEvent> userCompetitionEvents, DateTime currentDate)
        {
            return userCompetitionEvents.Any(p =>
                p.Category == EnumCompetitionEventCategory.Student &&
                p.EventContent != null &&
                p.EventContent.ActionConfigs != null &&
                p.EventContent.ActionConfigs.Any(x =>
                    x.Action == EnumSchoolEventRuleAction.ImportStudent &&
                    x.EndDate.HasValue &&
                    x.EndDate.Value > currentDate));
        }

        /// <summary>
        /// Kiểm tra xem có cấu hình giáo viên hợp lệ hay không
        /// </summary>
        /// <param name="userCompetitionEvents">Danh sách sự kiện thi đấu</param>
        /// <param name="currentDate">Thời gian hiện tại</param>
        /// <returns>Có cấu hình giáo viên hợp lệ hay không</returns>
        private static bool HasValidTeacherConfig(List<CompetitionEvent> userCompetitionEvents, DateTime currentDate)
        {
            return userCompetitionEvents.Any(p =>
                p.Category == EnumCompetitionEventCategory.Teacher &&
                p.EventContent != null &&
                p.EventContent.ActionConfigs != null &&
                p.EventContent.ActionConfigs.Any(x =>
                    x.Action == EnumSchoolEventRuleAction.ImportStudent &&
                    x.EndDate.HasValue &&
                    x.EndDate.Value > currentDate));
        }

        /// <summary>
        /// Kiểm tra xem có cho phép xuất dữ liệu không dựa trên thời gian
        /// </summary>
        /// <param name="userCompetitionEvents">Danh sách sự kiện thi đấu</param>
        /// <param name="currentTimeOfDay">Thời gian hiện tại trong ngày (số giờ)</param>
        /// <returns>Có cho phép xuất dữ liệu hay không</returns>
        private static bool IsExportTimeAllowed(List<CompetitionEvent> userCompetitionEvents, double currentTimeOfDay)
        {

            return userCompetitionEvents.Any(p =>
                p.EventContent != null &&
                p.EventContent.ActionConfigs != null &&
                p.EventContent.ActionConfigs.Any(x =>
                    x.StartTime.HasValue &&
                    x.EndTime.HasValue &&
                    x.StartTime.Value <= currentTimeOfDay &&
                    x.EndTime.Value >= currentTimeOfDay));
        }
    }
}
