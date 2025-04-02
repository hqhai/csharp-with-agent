namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
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
        private readonly IHumanRepository _humanRepository;
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
        private const int NumberOfMinutes = 10080;
        private readonly UserDbContext _userDbContext;

        public SearchStudentsByAdminQueryHandler(UserManager<User> userManager, IHumanRepository humanRepository, IStudentRepository studentRepository, IStudentCompetitionEventsRepository studentCompetitionEventsRepository, ICompetitionEventsRepository competitionEventsRepository, ILmsCourseService lmsCourseService, IOrderService orderService, UserDbContext userDbContext)
        {
            _userManager = userManager;
            _humanRepository = humanRepository;
            _studentRepository = studentRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _competitionEventsRepository = competitionEventsRepository;
            _lmsCourseService = lmsCourseService;
            _orderService = orderService;
            _userDbContext = userDbContext;
        }

        public async Task<MethodResult<PagingItemsModel<StudentSearchAdminModel>>> Handle(SearchStudentsByAdminQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<StudentSearchAdminModel>>();

            var users = from u in _userDbContext.Users.IgnoreQueryFilters()

                        join h in _humanRepository.Queryable.IgnoreQueryFilters()
                        on u.Id equals h.UserId into humanGroup
                        from human in humanGroup.DefaultIfEmpty()

                        join s in _studentRepository.Queryable.IgnoreQueryFilters()
                        on human.Id equals s.HumanId into studentGroup
                        from student in studentGroup.DefaultIfEmpty()

                        select new StudentSearchAdminModel
                        {
                            Id = u.Id,
                            IsDeleted = u.IsDeleted,
                            FullName = u.FullName,
                            UserName = u.UserName,
                            PhoneNumber = u.PhoneNumber,
                            Email = human.Email,
                            Birthday = human.Birthday,
                            StudentCode = human.Code,
                            Gender = human.Gender,
                            SchoolName = student.School,
                            Class = student.SchoolClass,
                            Grade = student.SchoolGrade,
                            ExpiredDate = student.ExpiredDate,
                            CourseLevel = student.CourseLevel,
                            Type = student.CourseLevel.HasValue ? student.CourseLevel.GetEnumCourseType() : null,
                            SchoolId = student.SchoolId,
                            CreatedDate = u.CreatedDate,
                            EmailConfirm = u.EmailConfirmed,
                            StudentId = student.Id,
                            CourseId = student.CourseId,
                            ProvinceId = student.ProvinceId,
                            DistrictId = student.DistrictId
                        };

            users = users.Where(p => p.IsDeleted == request.IsDelete);

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                request.Keyword = request.Keyword.Trim().ToLower(System.Globalization.CultureInfo.CurrentCulture);
                if (request.Keyword.IsValidEmail())
                {
                    users = users.Where(m => m.Email != null && m.Email.Contains(request.Keyword));
                }
                else if (request.Keyword.IsValidPhoneNumber())
                {
                    users = users.Where(m => m.PhoneNumber != null && m.PhoneNumber == request.Keyword);
                }
                else if (Guid.TryParse(request.Keyword, out var guid))
                {
                    users = users.Where(m => m.Id == guid);
                }
                else
                {
                    users = users.Where(m => (m.FullName != null && m.FullName.Contains(request.Keyword)) || (m.UserName != null && m.UserName.Contains(request.Keyword)));
                }
            }

            if (request.SchoolId.HasValue)
            {
                users = users.Where(m => m.SchoolId.HasValue && m.SchoolId == request.SchoolId);
            }
            if (request.Grades != null && request.Grades.Count > 0)
            {
                users = users.WhereBulkContains(request.Grades, x => x.Grade);
            }
            if (request.Classes != null && request.Classes.Count > 0)
            {
                users = users.WhereBulkContains(request.Classes, x => x.Class);
            }

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
                if (user.ExpiredDate.HasValue && user.CourseId.HasValue)
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

                var order = orders?.OrderByDescending(p => p.CreatedDate).FirstOrDefault();

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
                        return;
                    }
                }

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
