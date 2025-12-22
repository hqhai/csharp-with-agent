namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.LmsCourseService.Model;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Domain.Models.QueryModels.Students;
    using Fsel.Identity.Infrastructure;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentsByAdminQuery : SearchStudentsQueryModel, IRequest<MethodResult<PagingItemsModel<StudentSearchAdminModel>>>
    {
    }

    public class SearchStudentsByAdminQueryHandler : IRequestHandler<SearchStudentsByAdminQuery, MethodResult<PagingItemsModel<StudentSearchAdminModel>>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly IOrderService _orderService;
        private const string Leads = "Leads";
        private const string Client = "Client";
        private const string Studying = "Đang học";
        private const string AboutToExpire = "Sắp hết hạn";
        private const string Expired = "Hết hạn";
        private const string PackageExpires = "Hết hạn gói phí";
        private const string WaitingForApproval = "Chờ duyệt";
        private const string TryLearning = "Học thử";
        private const string TrialPeriodExpires = "Hết hạn học thử";
        private const string PTDone = "Done";
        private const string PTProcess = "Process";
        private const string CompletePT = "Hoàn thành PT";
        private const string WorkingPT = "Đang làm PT";
        private const string ConfirmOTP = "Confirm OTP";
        private const string NotConfirmOTP = "Chưa confirm OTP";
        private const string CutoffData = "Cutoff dữ liệu";
        private const int NumberOfMinutes = 10080;
        private readonly UserDbContext _userDbContext;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly RoleManager<Role> _roleManager;

        public SearchStudentsByAdminQueryHandler(UserManager<User> userManager, IStudentRepository studentRepository, IStudentCompetitionEventsRepository studentCompetitionEventsRepository, ICompetitionEventsRepository competitionEventsRepository, ILmsCourseService lmsCourseService, IOrderService orderService, UserDbContext userDbContext, IUserRoleRepository userRoleRepository, RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _studentRepository = studentRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _competitionEventsRepository = competitionEventsRepository;
            _lmsCourseService = lmsCourseService;
            _orderService = orderService;
            _userDbContext = userDbContext;
            _userRoleRepository = userRoleRepository;
            _roleManager = roleManager;
        }

        public async Task<MethodResult<PagingItemsModel<StudentSearchAdminModel>>> Handle(SearchStudentsByAdminQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<StudentSearchAdminModel>>();

            if (request.PageSize >= 100)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.PageSize), request.PageSize);
                return methodResult;
            }

            var query = from u in _userDbContext.Users.IgnoreQueryFilters()

                        join ur in _userRoleRepository.GetQuery() on u.Id equals ur.UserId

                        join r in _roleManager.Roles on ur.RoleId equals r.Id

                        join s in _studentRepository.Queryable.IgnoreQueryFilters()
                        on u.Id equals s.UserId into studentGroup
                        from student in studentGroup.DefaultIfEmpty()

                        where r.Name == EnumRole.Student.ToString()

                        select new { User = u, Student = student };

            query = query.Where(p => p.User.IsDeleted == request.IsDelete);
            if (request.StudentId.HasValue)
            {
                query = query.Where(m => m.Student.Id == request.StudentId);
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                request.Keyword = request.Keyword.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture);
                if (request.Keyword.IsValidEmail())
                {
                    query = query.Where(m => m.User.Email != null && m.User.Email == request.Keyword);
                }
                else if (request.Keyword.IsValidPhoneNumber())
                {
                    query = query.Where(m => m.User.PhoneNumber != null && m.User.PhoneNumber == request.Keyword);
                }
                else if (Guid.TryParse(request.Keyword, out var guid))
                {
                    query = query.Where(m => m.User.Id == guid);
                }
                else
                {
                    var queryUserName = query.Where(m => (m.User.UserName != null && m.User.UserName == request.Keyword));
                    var queryFullName = query.Where(m => m.User.FullName != null && EF.Functions.Contains(m.User.FullName, $"\"{request.Keyword}\"") && m.User.FullName.Contains(request.Keyword));
                    query = queryUserName.Union(queryFullName);
                }
            }

            if (request.SchoolId.HasValue)
            {
                query = query.Where(m => m.Student.SchoolId.HasValue && m.Student.SchoolId == request.SchoolId);
            }
            //if (request.Grades != null && request.Grades.Count > 0)
            //{
            //    query = query.WhereBulkContains(request.Grades, x => x.Student.SchoolGrade);
            //}
            //if (request.Classes != null && request.Classes.Count > 0)
            //{
            //    query = query.WhereBulkContains(request.Classes, x => x.Student.SchoolGrade);
            //}

            var users = query.Select(p => new StudentSearchAdminModel
            {
                Id = p.User.Id,
                IsDeleted = p.User.IsDeleted,
                FullName = p.User.FullName,
                UserName = p.User.UserName,
                PhoneNumber = p.User.PhoneNumber,
                Email = p.User.Email,
                CreatedDate = p.User.CreatedDate,
                EmailConfirm = p.User.EmailConfirmed,
                PasswordDefault = p.User.DefaultPassword,
                Birthday = p.User.Birthday,
                StudentCode = p.User.Code,
                Gender = p.User.Gender,
                SchoolName = p.Student.School,
                Class = p.Student.SchoolClass,
                Grade = p.Student.SchoolGrade,
                ExpiredDate = p.Student.ExpiredDate,
                CourseLevel = p.Student.CourseLevel,
                Type = p.Student.CourseLevel.HasValue ? p.Student.CourseLevel.GetEnumCourseType() : null,
                SchoolId = p.Student.SchoolId,
                StudentId = p.Student.Id,
                CourseId = p.Student.CourseId,
                ProvinceId = p.Student.ProvinceId,
                DistrictId = p.Student.DistrictId,
                Status = p.User.Status.ToString(),
            });

            int totalItem = await users.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            var lists = await users
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            if (lists.Count > 0)
            {
                var students = lists.Select(p => new AggregateDataStudentsByAdminQueryModel
                {
                    StudentId = p.StudentId,
                    CourseId = p.CourseId
                }).ToList();

                var userIds = lists.Select(p => p.Id).ToList();

                var aggregateDataStudentsTask = _lmsCourseService.AggregateDataStudents(new AggregateDataStudentsByAdminQueryModels()
                {
                    Students = students
                });

                var orderResultsTask = _orderService.GetOrdersByUserIds(new GetOrdersByUserIdsQueryModel()
                {
                    UserIds = userIds
                });

                await Task.WhenAll(aggregateDataStudentsTask, orderResultsTask);

                var aggregateDataStudents = (await aggregateDataStudentsTask).Content?.Result;
                var orders = (await orderResultsTask).Content?.Result;

                var studentIds = lists.Where(p => p.StudentId.HasValue).Select(p => p.StudentId!.Value).ToList();

                var studentCompetitionEvents = await _studentCompetitionEventsRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId).ToListAsync(cancellationToken);
                var competitionEvents = await _competitionEventsRepository.Queryable.ToListAsync(cancellationToken);

                lists.ForEach(p =>
                {
                    GetObjectStatusForUser(p, aggregateDataStudents, orders, studentCompetitionEvents, competitionEvents);
                });
            }

            methodResult.Result = new PagingItemsModel<StudentSearchAdminModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static void GetObjectStatusForUser(StudentSearchAdminModel user, AggregateDataStudentsByAdminModels? aggregateDataStudents, OrdersByUserIdsModels? ordersByUserIds, IList<StudentCompetitionEvent>? studentCompetitionEvents, IList<CompetitionEvent>? competitionEvents)
        {
            var orders = ordersByUserIds?.Users?.FirstOrDefault(p => p.UserId == user.Id)?.Orders;

            var dataStudent = aggregateDataStudents?.Students?.FirstOrDefault(p => p.StudentId == user.StudentId);
            user.IsLearnStudent = dataStudent?.IsLearnStudent ?? default;
            user.TotalLesson = dataStudent?.TotalLesson == 0 ? null : dataStudent?.TotalLesson;
            user.TotalLessonDone = user.TotalLesson == null ? null : dataStudent?.TotalLessonDone;

            if (user.StudentId.HasValue)
            {
                var studentCompetitionEvent = studentCompetitionEvents?.FirstOrDefault(p => p.StudentId == user.StudentId);
                if (studentCompetitionEvent != null)
                {
                    var competitionEvent = competitionEvents?.FirstOrDefault(p => p.Id == studentCompetitionEvent.CompetitionEventId);
                    user.Event = competitionEvent?.EventCode;
                }
            }

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            if (user.Status == EnumUserStatus.Disable.ToString())
            {
                user.Status = CutoffData;
                return;
            }

            if (orders != null && orders.Any(p => p.Status == EnumOrderStatus.Payment && p.RevenueType == EnumPaymentRevenueType.Revenue) && user.ExpiredDate.HasValue && user.CourseId.HasValue)
            {
                TimeSpan difference = user.ExpiredDate.Value - currentDate;
                int minutesDifference = (int)difference.TotalMinutes;
                if (minutesDifference > 0)
                {
                    user.Object = Client;
                    if (minutesDifference > NumberOfMinutes)
                    {
                        user.Status = Studying;
                        return;
                    }
                    else
                    {
                        user.Status = AboutToExpire;
                        return;
                    }
                }
                else
                {
                    user.Object = Leads;
                    user.Status = PackageExpires;
                    return;
                }
            }
            else
            {
                user.Object = Leads;

                var order = orders?.OrderByDescending(p => p.CreatedDate).FirstOrDefault();

                if (user.ExpiredDate.HasValue && user.CourseId.HasValue && order != null && !order.IsTrial)
                {
                    TimeSpan difference = user.ExpiredDate.Value - currentDate;
                    int minutesDifference = (int)difference.TotalMinutes;
                    if (minutesDifference <= 0)
                    {
                        user.Status = Expired;
                        return;
                    }
                }

                if (orders != null && orders.Any(p => p.Status == EnumOrderStatus.New))
                {
                    user.Status = WaitingForApproval;
                    return;
                }

                if (user.ExpiredDate.HasValue && user.CourseId.HasValue && order != null && !order.IsTrial)
                {
                    TimeSpan difference = user.ExpiredDate.Value - currentDate;
                    int minutesDifference = (int)difference.TotalMinutes;
                    if (minutesDifference > 0)
                    {
                        user.Status = Studying;
                        return;
                    }
                }

                if (order != null && order.IsTrial && user.ExpiredDate > currentDate && user.CourseId.HasValue)
                {
                    user.Status = TryLearning;
                    return;
                }
                else if (order != null && order.IsTrial && user.ExpiredDate < currentDate && user.CourseId.HasValue)
                {
                    user.Status = TrialPeriodExpires;
                    return;
                }

                user.TotalLesson = null;
                user.TotalLessonDone = null;

                if (user.StudentId.HasValue)
                {
                    if (dataStudent != null && dataStudent.PTStatus == PTDone)
                    {
                        user.Status = CompletePT;
                        return;
                    }
                    else if (dataStudent != null && dataStudent.PTStatus == PTProcess)
                    {
                        user.Status = WorkingPT;
                        user.CourseLevel = null;
                        return;
                    }
                }

                user.CourseLevel = null;

                if (user.EmailConfirm.HasValue && user.EmailConfirm.Value)
                {
                    user.Status = ConfirmOTP;
                    return;
                }

                user.Status = NotConfirmOTP;
                return;
            }
        }
    }
}
