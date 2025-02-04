// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.IntegrationQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.LmsCourseService.Model;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels.IntegrationModel;
    using Fsel.Identity.Domain.Models.QueryModels.Integration;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ClientIntegrationQuery : IntegrationQueryModel, IRequest<MethodResult<PagingItemsModel<ClientsIntegrationModel>>>
    {
    }

    public class ClientIntegrationQueryHandler : IRequestHandler<ClientIntegrationQuery, MethodResult<PagingItemsModel<ClientsIntegrationModel>>>
    {
        private readonly IOrderService _orderService;
        private readonly IHumanRepository _humanRepository;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly ISystemService _systemService;
        private readonly IMapper _mapper;

        public ClientIntegrationQueryHandler(IOrderService orderService,
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
        public async Task<MethodResult<PagingItemsModel<ClientsIntegrationModel>>> Handle(ClientIntegrationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ClientsIntegrationModel>>();

            List<Guid> distinctUserIds = new List<Guid>();

            if (string.IsNullOrEmpty(request.Email))
            {
                var courseIntegrationHasTimeQueryModel = new CourseIntegrationQueryModel
                {
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
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

                // lấy course có thay đổi trong khoảng thời gian
                var courseHasTimes = await _lmsCourseService.GetUnitResults(courseIntegrationHasTimeQueryModel);
                if (!courseHasTimes.IsSuccessStatusCode)
                {
                    methodResult.AddError(courseHasTimes.Error);
                    return methodResult;
                }
                var courseHasTimeResults = courseHasTimes.Content?.Result;
                if (courseHasTimeResults == null)
                {
                    methodResult.AddError(courseHasTimes.Error);
                    return methodResult;
                }
                var courseHasTimeIds = courseHasTimeResults.Select(x => x.UserId).Distinct().ToList();

                // lấy order có thay đổi trong khoảng thời gian
                var orderHasTimes = await _orderService.GetOrderByStatusAsync(new GetOrderByStatusQueryModel { StartDate = request.StartDate, EndDate = request.EndDate, Status = true });
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
                                                  .Where(x => x.UpdatedDate == null ? (x.CreatedDate >= request.StartDate && x.CreatedDate <= request.EndDate) : (x.UpdatedDate.Value >= request.StartDate && x.UpdatedDate.Value <= request.EndDate))
                                                  .ToListAsync(cancellationToken);
                if (users == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(users));
                    return methodResult;
                }
                var userIdentityHasTimeIds = users.Where(x => x.UserId.HasValue).Select(x => x.UserId!.Value).Distinct().ToList();


                // hợp nhất UserIds
                var userIds = userIdentityHasTimeIds.Concat(userPtTestHasTimeIds).Concat(courseHasTimeIds).Concat(userOrderIds).ToList();
                distinctUserIds = userIds.Distinct().ToList();
            }
            else
            {
                var user = await _humanRepository.Queryable.FirstOrDefaultAsync(x => x.Email != null && x.Email.ToLower().Trim() == request.Email.ToLower().Trim(), cancellationToken);

                if (user == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), $"{request.Email}");
                    return methodResult;
                }
                distinctUserIds.Add(user.UserId!.Value);
            }

            // phân trang
            var paging = distinctUserIds.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

            // lấy order
            var orders = await _orderService.GetOrderByStatusAsync(new GetOrderByStatusQueryModel { UserIds = paging, Status = true });
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

            // xoá những user không phải là client trong list user id
            paging = paging.Where(x => orderResults.Select(x => x.UserId).Contains(x)).ToList();

            // lấy Pt
            var courseIntegrationQueryModel = new CourseIntegrationQueryModel
            {
                UserIds = paging
            };

            var ptTests = await _lmsCourseService.GetPalcementTestResults(courseIntegrationQueryModel);
            var ptTestResults = ptTests.Content?.Result;

            // lấy unit lesson
            var units = await _lmsCourseService.GetUnitResults(courseIntegrationQueryModel);
            var unitResults = units.Content?.Result;

            // lấy user
            var userCombines = await _humanRepository.Queryable
                                                     .Include(x => x.User)
                                                     .Include(x => x.Student)
                                                     .ThenInclude(x => x!.ParentStudents)
                                                     .ThenInclude(x => x.Parent)
                                                     .ThenInclude(x => x!.Human)
                                                     .Where(x => x.UserId.HasValue && paging.Contains(x.UserId.Value))
                                                     .ToListAsync(cancellationToken);

            // Thông tin trường học
            var schoolIds = userCombines.Where(x => x.Student != null && x.Student.SchoolId.HasValue).Select(x => x.Student?.SchoolId ?? Guid.Empty).ToList();
            var school = await _systemService.GetSchoolByIds(schoolIds);
            var schoolResult = school.Content?.Result;

            // Lấy ra lần đăng nhập cuối cùng
            var featureAccessTime = await _systemService.GetFeatureAccessTimeByUserIds(paging);
            var featureAccessTimeResult = featureAccessTime.Content?.Result;

            #region Trả dữ liệu
            List<ClientsIntegrationModel> clientsIntegrations = new List<ClientsIntegrationModel>();
            clientsIntegrations = _mapper.Map<List<ClientsIntegrationModel>>(userCombines);

            clientsIntegrations.ForEach(item =>
            {
                var ptTestResult = ptTestResults?.FirstOrDefault(x => x.UserId == item.UserId);
                var unitResult = unitResults?.FirstOrDefault(x => x.UserId == item.UserId);
                item.LongPathSchool = schoolResult?.FirstOrDefault(x => x.Id == item.SchoolId)?.LongPath;
                item.LongPathLocation = schoolResult?.FirstOrDefault(x => x.Id == item.SchoolId)?.Location?.LongPath;
                item.LastDate = featureAccessTimeResult?.FirstOrDefault(x => x.CreatedUserId == item.UserId)?.LastVisited;
                item.PTLevel = ptTestResults?.FirstOrDefault(x => x.UserId == item.UserId)?.Level;
                item.PTLevel = ptTestResults?.FirstOrDefault(x => x.UserId == item.UserId)?.Level;
                item.CourseLevel = unitResult?.CourseLevel;
                item.StartCourse = unitResult?.StartCourse;
                item.EndCourse = unitResult?.EndCourse;

                var dateOrder = orderResults.Where(x => x.UserId == item.UserId).Max(c => c.CreatedDate > c.UpdatedDate ? c.CreatedDate : c.UpdatedDate);
                var dateUser = userCombines.FirstOrDefault(x => x.UserId == item.UserId)?.UpdatedDate != null ? userCombines.FirstOrDefault(x => x.UserId == item.UserId)?.UpdatedDate : userCombines.FirstOrDefault(x => x.UserId == item.UserId)?.CreatedDate;

                var dateEdits = new[] { dateOrder, ptTestResult?.DateEdit, unitResult?.DateEdit, dateUser };
                if (dateEdits != null && dateEdits.Any() && dateEdits.Any(x => x.HasValue))
                {
                    item.DateEdit = dateEdits.Where(d => d.HasValue).Max(d => d.Value);
                }

                if (ptTestResult != null)
                {
                    item.PTLevel = ptTestResult.Level;
                    item.PlacementTestResults = ptTestResult.PlacementTestResults;
                }

                item.CurrentUnit = unitResult?.Name;
                item.CurrentLesson = unitResult?.CurrentLesson;
                item.LessonCompleted = unitResult?.LessonCompleted;

                List<OrderIntegrationModel> orderIntegrations = new List<OrderIntegrationModel>();
                foreach (var order in orderResults.Where(x => x.UserId == item.UserId).ToList())
                {
                    var courseName = EnumCourseType.Ielts.ToString();
                    if (order.CourseName != null && (int)order.CourseName <= 5)
                    {
                        courseName = EnumCourseType.Academic.ToString();
                    }
                    var orderIntegration = new OrderIntegrationModel
                    {
                        OrderCode = order.Code,
                        StartDate = order.UpdatedDate ?? default,
                        EndDate = order.ExpireDate ?? default,
                        Program = courseName,
                        CourseLevel = order.CourseName.ToString(),
                        CoursePackage = order.MonthNumber ?? default,
                        PaymentMethod = order.PaymentMethod ?? default,
                        DiscountPrice = order.DiscountPrice,
                        TotalPrice = order.TotalPrice,
                        Status = order.StatusCourseResult,
                        RevenueType = order.RevenueType
                    };
                    orderIntegrations.Add(orderIntegration);
                }

                item.OrderIntegration = orderIntegrations;
            });
            #endregion

            int totalItem = distinctUserIds.Count;
            methodResult.Result = new PagingItemsModel<ClientsIntegrationModel>(clientsIntegrations, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
