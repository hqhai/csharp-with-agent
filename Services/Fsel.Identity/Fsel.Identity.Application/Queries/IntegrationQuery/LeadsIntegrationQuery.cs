// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.IntegrationQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.LmsCourseService.Model;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SystemService.QueryModels;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels.IntegrationModel;
    using Fsel.Identity.Domain.Models.QueryModels.Integration;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class LeadsIntegrationQuery : IntegrationQueryModel, IRequest<MethodResult<PagingItemsModel<LeadsIntegrationModel>>>
    {
    }

    public class LeadsIntegrationQueryHandler : IRequestHandler<LeadsIntegrationQuery, MethodResult<PagingItemsModel<LeadsIntegrationModel>>>
    {
        private readonly IOrderService _orderService;
        private readonly IHumanRepository _humanRepository;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly ISystemService _systemService;
        private readonly IMapper _mapper;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly BaseIntegrationQuery _baseIntegrationQuery;

        public LeadsIntegrationQueryHandler(IOrderService orderService,
                                            IHumanRepository humanRepository,
                                            ILmsCourseService lmsCourseService,
                                            ISystemService systemService,
                                            IMapper mapper,
                                            IUserOtpCodeRepository userOtpCodeRepository,
                                            IStudentCompetitionEventsRepository studentCompetitionEventsRepository,
                                            BaseIntegrationQuery baseIntegrationQuery)
        {
            _orderService = orderService;
            _humanRepository = humanRepository;
            _lmsCourseService = lmsCourseService;
            _systemService = systemService;
            _mapper = mapper;
            _userOtpCodeRepository = userOtpCodeRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _baseIntegrationQuery = baseIntegrationQuery;
        }
        public async Task<MethodResult<PagingItemsModel<LeadsIntegrationModel>>> Handle(LeadsIntegrationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<LeadsIntegrationModel>>();

            #region Lấy dữ liệu thay đổi trong khoảng thời gian
            List<Guid> distinctUserIds = new List<Guid>();
            List<OrderSearchModel> orderClients = new List<OrderSearchModel>();

            if (string.IsNullOrEmpty(request.Email))
            {
                var userInteractedInRange = await _baseIntegrationQuery.UserInteractedInRange(request.StartDate, request.EndDate, false, cancellationToken);
                if (!userInteractedInRange.IsOK || userInteractedInRange.Result == null)
                {
                    methodResult.AddErrorBadRequest(userInteractedInRange.ErrorMessages);
                    return methodResult;
                }

                distinctUserIds = userInteractedInRange.Result.ToList();

                // lấy client
                var clientUsers = await _orderService.GetOrderByStatusAsync(new GetOrderByStatusQueryModel { StartDate = request.StartDate, EndDate = request.EndDate, Status = true });
                if (!clientUsers.IsSuccessStatusCode)
                {
                    methodResult.AddError(clientUsers.Error);
                    return methodResult;
                }

                orderClients = clientUsers.Content?.Result?.ToList() ?? new List<OrderSearchModel>();
            }
            else
            {
                var user = await _humanRepository.Queryable.FirstOrDefaultAsync(x => x.Email == request.Email.Trim(), cancellationToken);
                if (user == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), $"{request.Email}");
                    return methodResult;
                }

                distinctUserIds.Add(user.UserId!.Value);

                // lấy client
                var clientUsers = await _orderService.GetOrderByStatusAsync(new GetOrderByStatusQueryModel { UserIds = distinctUserIds, Status = true });
                if (!clientUsers.IsSuccessStatusCode)
                {
                    methodResult.AddError(clientUsers.Error);
                    return methodResult;
                }

                orderClients = clientUsers.Content?.Result?.ToList() ?? new List<OrderSearchModel>();
            }
            #endregion

            #region Lấy dữ liệu
            // bỏ những lead đã thành client
            if (orderClients.Any())
            {
                var clientUserResultIds = orderClients.Select(x => x.UserId).Distinct().ToList();
                distinctUserIds = distinctUserIds.Where(x => !clientUserResultIds.Contains(x)).ToList();
            }

            // phân trang
            var paging = distinctUserIds.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

            // lấy order
            var orders = await _orderService.GetOrderByStatusAsync(new GetOrderByStatusQueryModel { UserIds = paging, Status = false });
            var orderResults = orders.Content?.Result;

            var courseIntegrationQueryModel = new CourseIntegrationQueryModel
            {
                UserIds = paging
            };

            // lấy Pt
            var ptTests = await _lmsCourseService.GetPalcementTestResults(courseIntegrationQueryModel);
            var ptTestResults = ptTests.Content?.Result;

            // lấy info course
            var infoCourseResults = await _lmsCourseService.GetInfoCourseIntegration(courseIntegrationQueryModel);
            var infoCourses = infoCourseResults.Content?.Result;

            // lấy all user từ list hợp nhất
            var userCombines = await _humanRepository.Queryable
                                                     .Include(x => x.User)
                                                     .Include(x => x.Student)
                                                     .ThenInclude(x => x!.ParentStudents)
                                                     .ThenInclude(x => x.Parent)
                                                     .ThenInclude(x => x!.Human)
                                                     .WhereBulkContains(paging, x => x.UserId)
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
                    UserId = x.UserId ?? Guid.Empty,
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
                                                                            x.OrderByDescending(c => c.CreatedDate).FirstOrDefault(c => c.Type == EnumUserOtpCodeType.SMS)!.OTPCode : default,
                                                           OTPEmail = x.OrderByDescending(c => c.CreatedDate).FirstOrDefault(c => c.Type == EnumUserOtpCodeType.Email) != null ?
                                                                      x.OrderByDescending(c => c.CreatedDate).FirstOrDefault(c => c.Type == EnumUserOtpCodeType.Email)!.OTPCode : default
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

            #region SetData
            List<LeadsIntegrationModel> leadsIntegrations = new List<LeadsIntegrationModel>();
            leadsIntegrations = _mapper.Map(userCombines, leadsIntegrations);

            foreach (var leadsIntegration in leadsIntegrations)
            {
                leadsIntegration.Status = EnumIntegrationStatus.Register;
                bool isCutOff = false;

                if (userCombines != null && userCombines.Any())
                {
                    _baseIntegrationQuery.SetUserData(leadsIntegration, userCombines, out isCutOff);
                }

                if (ptTestResults != null && ptTestResults.Any())
                {
                    _baseIntegrationQuery.SetPalcementTestData(leadsIntegration, ptTestResults);
                }

                if (orderResults != null && orderResults.Any())
                {
                    SetOrderData(leadsIntegration, orderResults);
                }

                if (isCutOff)
                {
                    leadsIntegration.Status = EnumIntegrationStatus.Cutoff;
                }

                if (schools != null && schools.Any())
                {
                    _baseIntegrationQuery.SetSchoolData(leadsIntegration, schools);
                }

                if (featureAccessTimes != null && featureAccessTimes.Any())
                {
                    _baseIntegrationQuery.SetFeatureAccessTimeData(leadsIntegration, featureAccessTimes);
                }

                if (infoCourses != null && infoCourses.Any())
                {
                    _baseIntegrationQuery.SetCourseData(leadsIntegration, infoCourses);
                }

                if (courseSuggests != null && courseSuggests.Any())
                {
                    _baseIntegrationQuery.SetCourseSuggestData(leadsIntegration, courseSuggests);
                }

                if (userOtps != null && userOtps.Any())
                {
                    _baseIntegrationQuery.SetOtpData(leadsIntegration, userOtps);
                }

                if (events != null && events.Any() && userCombines != null && userCombines.Any())
                {
                    _baseIntegrationQuery.SetEventData(leadsIntegration, userCombines, events);
                }
            };
            #endregion

            leadsIntegrations = leadsIntegrations.OrderByDescending(x => x.Status).ToList();
            methodResult.Result = new PagingItemsModel<LeadsIntegrationModel>(leadsIntegrations, request, distinctUserIds.Count);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static void SetOrderData(LeadsIntegrationModel leadsIntegration, IList<OrderSearchModel> orders)
        {
            var orderUsers = orders.Where(x => x.UserId == leadsIntegration.UserId).ToList();
            if (orderUsers == null)
            {
                return;
            }

            var orderTrial = orderUsers.FirstOrDefault(x => x.IsTrial);
            if (orderTrial != null)
            {
                leadsIntegration.Status = (orderTrial.ExpireDate.HasValue && orderTrial.ExpireDate < DateTime.UtcNow) ? EnumIntegrationStatus.TrialExpired : EnumIntegrationStatus.Trial;
                leadsIntegration.StartTrial = orderTrial.CreatedDate;
            }

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

            leadsIntegration.OrderIntegrations = orderIntegrations.OrderByDescending(x => x.CreatedDate).ToList();

            var checkOrderExpire = orderUsers.Where(x => x.RevenueType == EnumPaymentRevenueType.Revenue).FirstOrDefault();
            if (checkOrderExpire != null && checkOrderExpire.ExpireDate < DateTime.UtcNow)
            {
                leadsIntegration.Status = EnumIntegrationStatus.PaidExpired;
            }

            var checkNotRevenue = orderUsers.Where(x => !x.IsTrial && x.RevenueType == EnumPaymentRevenueType.NotRevenue).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
            if (checkNotRevenue != null && checkNotRevenue.ExpireDate < DateTime.UtcNow)
            {
                leadsIntegration.Status = EnumIntegrationStatus.Expired;
            }
        }
    }
}
