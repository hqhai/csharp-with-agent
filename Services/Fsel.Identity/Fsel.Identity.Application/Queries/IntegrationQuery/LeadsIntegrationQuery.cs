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
    using Fsel.Identity.Application.Services.SystemService.Model;
    using Fsel.Identity.Application.Services.SystemService.QueryModels;
    using Fsel.Identity.Domain.Entities;
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

        public LeadsIntegrationQueryHandler(IOrderService orderService,
                                            IHumanRepository humanRepository,
                                            ILmsCourseService lmsCourseService,
                                            ISystemService systemService,
                                            IMapper mapper)
        {
            _orderService = orderService;
            _humanRepository = humanRepository;
            _lmsCourseService = lmsCourseService;
            _systemService = systemService;
            _mapper = mapper;
        }
        public async Task<MethodResult<PagingItemsModel<LeadsIntegrationModel>>> Handle(LeadsIntegrationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<LeadsIntegrationModel>>();

            #region Lấy dữ liệu thay đổi trong khoảng thời gian
            List<Guid> distinctUserIds = new List<Guid>();

            if (string.IsNullOrEmpty(request.Email))
            {
                var userInteractedInRange = await UserInteractedInRange(request.StartDate, request.EndDate, cancellationToken);
                if (!userInteractedInRange.IsOK || userInteractedInRange.Result == null)
                {
                    methodResult.AddErrorBadRequest(userInteractedInRange.ErrorMessages);
                    return methodResult;
                }

                distinctUserIds = userInteractedInRange.Result.ToList();
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
            }
            #endregion

            #region Lấy dữ liệu
            // lấy client
            var clientUsers = await _orderService.GetOrderByStatusAsync(new GetOrderByStatusQueryModel { StartDate = request.StartDate, EndDate = request.EndDate, Status = true });
            if (!clientUsers.IsSuccessStatusCode)
            {
                methodResult.AddError(clientUsers.Error);
                return methodResult;
            }

            var clientUserResults = clientUsers.Content?.Result;
            if (clientUserResults == null)
            {
                methodResult.AddError(clientUsers.Error);
                return methodResult;
            }

            // bỏ những lead đã thành client
            var clientUserResultIds = clientUserResults.Select(x => x.UserId).Distinct().ToList();
            distinctUserIds = distinctUserIds.Where(x => !clientUserResultIds.Contains(x)).ToList();

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
                                                     .Where(x => x.UserId.HasValue && paging.Contains(x.UserId.Value))
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
                    SetUserData(leadsIntegration, userCombines, out isCutOff);
                }

                if (ptTestResults != null && ptTestResults.Any())
                {
                    SetPalcementTestData(leadsIntegration, ptTestResults);
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
                    SetSchoolData(leadsIntegration, schools);
                }

                if (featureAccessTimes != null && featureAccessTimes.Any())
                {
                    SetFeatureAccessTimeData(leadsIntegration, featureAccessTimes);
                }

                if (infoCourses != null && infoCourses.Any())
                {
                    SetCourseData(leadsIntegration, infoCourses);
                }

                if (courseSuggests != null && courseSuggests.Any())
                {
                    SetCourseSuggestData(leadsIntegration, courseSuggests);
                }
            };
            #endregion

            leadsIntegrations = leadsIntegrations.OrderByDescending(x => x.Status).ToList();
            methodResult.Result = new PagingItemsModel<LeadsIntegrationModel>(leadsIntegrations, request, distinctUserIds.Count);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<MethodResult<IList<Guid>>> UserInteractedInRange(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            MethodResult<IList<Guid>> methodResult = new MethodResult<IList<Guid>>();

            var courseIntegrationHasTimeQueryModel = new CourseIntegrationQueryModel
            {
                StartDate = startDate,
                EndDate = endDate
            };

            // lấy Pt có thay đổi trong khoảng thời gian
            var ptTestHasTimes = await _lmsCourseService.GetPalcementTestResults(courseIntegrationHasTimeQueryModel);
            if (!ptTestHasTimes.IsSuccessStatusCode)
            {
                methodResult.AddError(ptTestHasTimes.Error);
                return methodResult;
            }
            var ptTestResultHasTimes = ptTestHasTimes.Content?.Result;
            if (ptTestResultHasTimes == null)
            {
                methodResult.AddError(ptTestHasTimes.Error);
                return methodResult;
            }
            var userPtTestHasTimeIds = ptTestResultHasTimes.Select(x => x.UserId).Distinct().ToList();

            // lấy unit lesson có thay đổi trong khoảng thời gian
            var unitHasTimes = await _lmsCourseService.GetUnitResults(courseIntegrationHasTimeQueryModel);
            if (!unitHasTimes.IsSuccessStatusCode)
            {
                methodResult.AddError(unitHasTimes.Error);
                return methodResult;
            }
            var unitResultHasTimes = unitHasTimes.Content?.Result;
            if (unitResultHasTimes == null)
            {
                methodResult.AddError(unitHasTimes.Error);
                return methodResult;
            }
            var userUnitResultHasTimeIds = unitResultHasTimes.Select(x => x.UserId).Distinct().ToList();

            // lấy order có thay đổi trong khoảng thời gian
            var orderHasTimes = await _orderService.GetOrderByStatusAsync(new GetOrderByStatusQueryModel { StartDate = startDate, EndDate = endDate, Status = false });
            if (!orderHasTimes.IsSuccessStatusCode)
            {
                methodResult.AddError(orderHasTimes.Error);
                return methodResult;
            }
            var orderResultHasTimes = orderHasTimes.Content?.Result;
            if (orderResultHasTimes == null)
            {
                methodResult.AddError(orderHasTimes.Error);
                return methodResult;
            }
            var userOrderIds = orderResultHasTimes.Select(x => x.UserId).Distinct().ToList();

            //lấy User đăng ký trong khoảng thời gian
            var users = await _humanRepository.Queryable
                                              .Where(x => x.UpdatedDate == null ? (x.CreatedDate >= startDate && x.CreatedDate <= endDate) : (x.UpdatedDate.Value >= startDate && x.UpdatedDate.Value <= endDate))
                                              .ToListAsync(cancellationToken);
            if (users == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(users));
                return methodResult;
            }
            var userIdentityHasTimeIds = users.Select(x => x.UserId ?? Guid.Empty).ToList();

            // hợp nhất UserId chưa có order
            var userIds = userIdentityHasTimeIds.Concat(userPtTestHasTimeIds).Concat(userUnitResultHasTimeIds).Concat(userOrderIds).ToList();


            methodResult.Result = userIds.Distinct().ToList();
            return methodResult;
        }

        private static void SetUserData(LeadsIntegrationModel leadsIntegration, IList<Human> humans, out bool isCutOff)
        {
            var human = humans.FirstOrDefault(x => x.UserId == leadsIntegration.UserId);
            if (human != null && human.User != null && (human.User.EmailConfirmed || human.User.PhoneNumberConfirmed))
            {
                leadsIntegration.Status = EnumIntegrationStatus.Confirm;
            }

            isCutOff = human?.User?.Status == EnumUserStatus.Disable;
        }

        private static void SetPalcementTestData(LeadsIntegrationModel leadsIntegration, IList<PlacementTestResultModel> placementTestResults)
        {
            var ptTestResult = placementTestResults?.FirstOrDefault(x => x.UserId == leadsIntegration.UserId);
            if (ptTestResult != null)
            {
                leadsIntegration.PTLevel = ptTestResult.Level;
                leadsIntegration.PlacementTestResults = ptTestResult.PlacementTestResults;
                leadsIntegration.Status = ptTestResult.Status == "Done" ? EnumIntegrationStatus.PTDone : EnumIntegrationStatus.PTProgress;
            }
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
                leadsIntegration.ExpireDate = orderTrial.ExpireDate;
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

        private static void SetSchoolData(LeadsIntegrationModel leadsIntegration, IList<SchoolModel> schools)
        {
            var school = schools.FirstOrDefault(x => x.Id == leadsIntegration.SchoolId);
            leadsIntegration.LongPathSchool = school?.LongPath;
            leadsIntegration.LongPathLocation = school?.Location?.LongPath;
        }

        private static void SetFeatureAccessTimeData(LeadsIntegrationModel leadsIntegration, IList<GetFeatureAccessTimeQueryModel> featureAccessTimes)
        {
            var featureAccessTime = featureAccessTimes.FirstOrDefault(x => x.CreatedUserId == leadsIntegration.UserId);
            leadsIntegration.LastDate = featureAccessTime?.LastVisited;
            leadsIntegration.AccessTime = featureAccessTime?.AccessTime;
        }

        private static void SetCourseData(LeadsIntegrationModel leadsIntegration, IList<InfoCourseIntegrationModel> infoCourses)
        {
            var infoCourseUsers = infoCourses.Where(x => x.UserId == leadsIntegration.UserId).Where(x => x.InfoCourseIntegrationDetails != null).SelectMany(x => x.InfoCourseIntegrationDetails!).ToList();
            leadsIntegration.CourseIntegrations = infoCourseUsers;
        }

        private static void SetCourseSuggestData(LeadsIntegrationModel leadsIntegration, IList<CourseSuggestUsersModel> courseSuggestUsers)
        {
            var courseSuggestUser = courseSuggestUsers.FirstOrDefault(x => x.UserId == leadsIntegration.UserId);
            leadsIntegration.CourseSuggests = courseSuggestUser?.CourseSuggestConfigs;
        }
    }
}
