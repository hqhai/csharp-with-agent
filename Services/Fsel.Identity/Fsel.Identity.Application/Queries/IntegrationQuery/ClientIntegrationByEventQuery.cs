// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.IntegrationQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.LmsCourseService.Model;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.QueryModels;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels.IntegrationModel;
    using Fsel.Shared.Enums;
    using MassTransit.Internals;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ClientIntegrationByEventQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<ClientsIntegrationModel>>>
    {
        public string? EventCode { get; set; }
    }

    public class ClientIntegrationByEventQueryHandler : IRequestHandler<ClientIntegrationByEventQuery, MethodResult<PagingItemsModel<ClientsIntegrationModel>>>
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<User> _userManager;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly ISystemService _systemService;
        private readonly IMapper _mapper;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly BaseIntegrationQuery _baseIntegrationQuery;

        public ClientIntegrationByEventQueryHandler(IOrderService orderService,
                                                    UserManager<User> userManager,
                                                    ILmsCourseService lmsCourseService,
                                                    ISystemService systemService,
                                                    IMapper mapper,
                                                    IUserOtpCodeRepository userOtpCodeRepository,
                                                    IStudentCompetitionEventsRepository studentCompetitionEventsRepository,
                                                    ICompetitionEventsRepository competitionEventsRepository,
                                                    IStudentRepository studentRepository,
                                                    BaseIntegrationQuery baseIntegrationQuery)
        {
            _orderService = orderService;
            _userManager = userManager;
            _lmsCourseService = lmsCourseService;
            _systemService = systemService;
            _mapper = mapper;
            _userOtpCodeRepository = userOtpCodeRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _competitionEventsRepository = competitionEventsRepository;
            _studentRepository = studentRepository;
            _baseIntegrationQuery = baseIntegrationQuery;
        }

        public async Task<MethodResult<PagingItemsModel<ClientsIntegrationModel>>> Handle(ClientIntegrationByEventQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ClientsIntegrationModel>>();

            #region Lấy dữ liệu 
            List<Guid> distinctUserIds = new List<Guid>();

            var competitionEventIds = await _competitionEventsRepository.Queryable
                                                                        .Include(x => x.CompetitionEvents)
                                                                        .Where(x => x.EventCode == request.EventCode)
                                                                        .AsNoTracking()
                                                                        .SelectMany(x => x.CompetitionEvents.Select(x => x.Id))
                                                                        .ToListAsync(cancellationToken);

            var studentCompetitionEvents = _studentCompetitionEventsRepository.Queryable.WhereBulkContains((competitionEventIds ?? new List<Guid>()), x => x.CompetitionEventId);

            distinctUserIds = await (from a in studentCompetitionEvents
                                     join b in _studentRepository.Queryable on a.StudentId equals b.Id
                                     join c in _userManager.Users on b.UserId equals c.Id
                                     select c.Id).Distinct().ToListAsync(cancellationToken);

            // lấy order
            var orders = await _orderService.GetOrderByStatusAsync(new GetOrderByStatusQueryModel { UserIds = distinctUserIds, Status = true });
            if (!orders.IsSuccessStatusCode)
            {
                methodResult.AddError(orders.Error);
                return methodResult;
            }
            var orderResults = orders.Content?.Result;
            if (orderResults == null)
            {
                methodResult.AddError(orders.Error);
                return methodResult;
            }

            var clientUserIds = orderResults.Select(x => x.UserId).Distinct().ToList();
            #endregion

            #region Lấy dữ liệu
            // phân trang
            var paging = clientUserIds.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

            // lấy Pt
            var courseIntegrationQueryModel = new CourseIntegrationQueryModel
            {
                UserIds = paging
            };

            var ptTests = await _lmsCourseService.GetPalcementTestResults(courseIntegrationQueryModel);
            var ptTestResults = ptTests.Content?.Result;

            // lấy info course
            var infoCourseResults = await _lmsCourseService.GetInfoCourseIntegration(courseIntegrationQueryModel);
            var infoCourses = infoCourseResults.Content?.Result;

            // lấy user
            var userCombines = await _userManager.Users
                                                     .Include(x => x.Student)
                                                     .ThenInclude(x => x!.ParentStudents)
                                                     .ThenInclude(x => x.Parent)
                                                     .ThenInclude(x => x!.User)
                                                     .Where(x => paging.Contains(x.Id))
                                                     .ToListAsync(cancellationToken);

            // lấy thông tin trường học
            var schoolIds = userCombines.Where(x => x.Student != null && x.Student.SchoolId.HasValue).Select(x => x.Student?.SchoolId ?? Guid.Empty).ToList();
            var schoolResult = await _systemService.GetSchoolByIds(schoolIds);
            var schools = schoolResult.Content?.Result;

            // lấy lần đăng nhập cuối cùng
            var featureAccessTimeResult = await _systemService.GetFeatureAccessTimeByUserIds(paging);
            var featureAccessTimes = featureAccessTimeResult.Content?.Result;

            // lấy course sugget
            var courseSuggestResults = await _systemService.GetCourseSuggestByUserIds(new GetCourseSuggestByUserIdsQueryModel
            {
                GetCourseSuggestByUserIds = userCombines.Select(x => new GetCourseSuggestByUserIdsDetailModel
                {
                    UserId = x.Id,
                    Age = Shared.Helpers.DateTimeHelper.GetYearOld(x.Birthday),
                    BaseCourseLevel = x.Student?.BaseCourseLevel
                }).ToList()
            });
            var courseSuggests = courseSuggestResults.Content?.Result;

            // lấy OTP
            var userOtps = await _userOtpCodeRepository.Queryable
                                                       .WhereBulkContains(paging, x => x.UserId)
                                                       .GroupBy(x => x.UserId)
                                                       .Select(x => new OtpIntegrationModel
                                                       {
                                                           UserId = x.Key ?? Guid.Empty,
                                                           OTPPhoneNumber = x.OrderByDescending(c => c.CreatedDate).FirstOrDefault(c => c.Type == EnumUserOtpCodeType.SMS) != null ?
                                                                            x.OrderByDescending(c => c.CreatedDate).FirstOrDefault(c => c.Type == EnumUserOtpCodeType.SMS)!.OtpCode : default,
                                                           OTPEmail = x.OrderByDescending(c => c.CreatedDate).FirstOrDefault(c => c.Type == EnumUserOtpCodeType.Email) != null ?
                                                                      x.OrderByDescending(c => c.CreatedDate).FirstOrDefault(c => c.Type == EnumUserOtpCodeType.Email)!.OtpCode : default
                                                       }).ToListAsync(cancellationToken);

            // lấy sự kiện
            var studentIds = userCombines.Where(x => x.Student != null).Select(x => x.Student!.Id).ToList();
            var events = await _studentCompetitionEventsRepository.Queryable
                                                                  .WhereBulkContains(studentIds, x => x.StudentId)
                                                                  .GroupBy(x => x.StudentId)
                                                                  .Select(x => new EventUserIntegrationModel
                                                                  {
                                                                      StudentId = x.Key,
                                                                      EventCode = x.OrderByDescending(c => c.CreatedUserId).FirstOrDefault() != null ? x.OrderByDescending(c => c.CreatedUserId).FirstOrDefault()!.CompetitionEvents!.EventCode : null,
                                                                  }).ToListAsync(cancellationToken);
            #endregion

            #region Trả dữ liệu
            List<ClientsIntegrationModel> clientsIntegrations = new List<ClientsIntegrationModel>();
            clientsIntegrations = _mapper.Map<List<ClientsIntegrationModel>>(userCombines);

            foreach (var clientsIntegration in clientsIntegrations)
            {
                clientsIntegration.Status = EnumIntegrationStatus.Register;
                bool isCutOff = false;

                if (userCombines != null && userCombines.Any())
                {
                    _baseIntegrationQuery.SetUserData(clientsIntegration, userCombines, out isCutOff);
                }

                if (ptTestResults != null && ptTestResults.Any())
                {
                    _baseIntegrationQuery.SetPalcementTestData(clientsIntegration, ptTestResults);
                }

                if (orderResults.Any())
                {
                    SetOrderData(clientsIntegration, orderResults);
                }

                if (isCutOff)
                {
                    clientsIntegration.Status = EnumIntegrationStatus.Cutoff;
                }

                if (schools != null && schools.Any())
                {
                    _baseIntegrationQuery.SetSchoolData(clientsIntegration, schools);
                }

                if (featureAccessTimes != null && featureAccessTimes.Any())
                {
                    _baseIntegrationQuery.SetFeatureAccessTimeData(clientsIntegration, featureAccessTimes);
                }

                if (infoCourses != null && infoCourses.Any())
                {
                    _baseIntegrationQuery.SetCourseData(clientsIntegration, infoCourses);
                }

                if (courseSuggests != null && courseSuggests.Any())
                {
                    _baseIntegrationQuery.SetCourseSuggestData(clientsIntegration, courseSuggests);
                }

                if (userOtps != null && userOtps.Any())
                {
                    _baseIntegrationQuery.SetOtpData(clientsIntegration, userOtps);
                }

                if (events != null && events.Any() && userCombines != null && userCombines.Any())
                {
                    _baseIntegrationQuery.SetEventData(clientsIntegration, userCombines, events);
                }
            };
            #endregion

            int totalItem = clientUserIds.Count;
            methodResult.Result = new PagingItemsModel<ClientsIntegrationModel>(clientsIntegrations, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static void SetOrderData(ClientsIntegrationModel clientsIntegration, IList<OrderSearchModel> orders)
        {
            var orderUsers = orders.Where(x => x.UserId == clientsIntegration.UserId).ToList();
            if (orderUsers == null)
            {
                return;
            }

            clientsIntegration.Status = EnumIntegrationStatus.LearningProgress;

            List<OrderIntegrationModel> orderIntegrations = new List<OrderIntegrationModel>();
            foreach (var order in orderUsers)
            {
                var orderIntegration = new OrderIntegrationModel
                {
                    OrderCode = order.Code,
                    StartDate = order.UpdatedDate ?? order.CreatedDate ?? default,
                    EndDate = order.ExpireDate ?? default,
                    ReferralCode = order.ReferralCode,
                    Program = (order.CourseName.HasValue && (int)order.CourseName <= 5) ? EnumCourseType.Academic.ToString() : EnumCourseType.Ielts.ToString(),
                    CourseLevel = order.CourseName.ToString(),
                    CoursePackage = order.MonthNumber ?? default,
                    PaymentMethod = order.PaymentMethod ?? default,
                    DiscountPrice = order.DiscountPrice,
                    TotalPrice = order.TotalPrice,
                    StatusCourseResult = order.StatusCourseResult,
                    Status = order.Status,
                    RevenueType = order.RevenueType,
                    Address = order.Address,
                    PhoneNumber = order.PhoneNumber,
                    Price = order.Price,
                    Voucher = order.Voucher?.Code,
                    CreatedUserId = order.CreatedUserId,
                    CreatedFullName = order.CreatedFullName,
                    CreatedDate = order.CreatedDate,
                    UpdatedUserId = order.UpdatedUserId,
                    UpdatedFullName = order.UpdatedFullName,
                    UpdatedDate = order.UpdatedDate,
                    IsTrial = order.IsTrial
                };

                orderIntegrations.Add(orderIntegration);
            }

            clientsIntegration.OrderIntegrations = orderIntegrations.OrderByDescending(x => x.CreatedDate).ToList();
        }
    }
}
