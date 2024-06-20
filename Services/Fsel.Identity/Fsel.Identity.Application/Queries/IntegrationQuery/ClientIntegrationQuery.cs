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
    using Fsel.Identity.Domain.Enums.ErrorCodes;
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

            var check = request.EndDate.Date - request.StartDate.Date;
            if (check.TotalDays > 7)
            {
                methodResult.AddErrorBadRequest(nameof(EnumIntegrationErrorCode.TotalDaysGreater7), nameof(check));
                return methodResult;
            }

            // lấy order
            var queryOrder = new GetOrderByStatusQueryModel
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = true
            };
            var orders = await _orderService.GetOrderByStatusAsync(queryOrder);
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
            var userOrderIds = orderResults.Select(x => x.UserId).ToList();

            // lấy pt
            var queryPtTest = new GetPTTestModel
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
            };
            var ptTest = await _lmsCourseService.GetPalcementTestResults(queryPtTest);
            if (!ptTest.IsSuccessStatusCode)
            {
                methodResult.AddError(ptTest.Error);
                return methodResult;
            }
            var ptTestResults = ptTest.Content?.Result;
            if (ptTestResults == null)
            {
                methodResult.AddError(orders.Error);
                return methodResult;
            }
            var userPtTestIds = ptTestResults.Select(x => x.UserId).ToList();

            // lấy user
            var users = await _humanRepository.Queryable
                                              .Include(x => x.User)
                                              .Include(x => x.Student)
                                              .ThenInclude(x => x!.ParentStudents)
                                              .ThenInclude(x => x.Parent)
                                              .ThenInclude(x => x!.Human)
                                              .Where(x => x.UpdatedDate == null ? (x.CreatedDate.Date >= request.StartDate.Date && x.CreatedDate.Date <= request.EndDate.Date) : (x.UpdatedDate.Value.Date >= request.StartDate.Date && x.UpdatedDate.Value.Date <= request.EndDate.Date))
                                              .ToListAsync(cancellationToken);
            if (users == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(users));
                return methodResult;
            }
            var userIdentityIds = users.Select(x => x.UserId ?? Guid.Empty).ToList();

            // Hợp nhất UserId
            var userIds = userIdentityIds.Concat(userPtTestIds).Concat(userOrderIds).ToList();
            var distinctUserIds = userIds.Distinct().ToList();

            // Thông tin trường học
            var schoolIds = users.Where(x => x.Student != null && x.Student.SchoolId.HasValue).Select(x => x.Student?.SchoolId ?? Guid.Empty).ToList();
            var school = await _systemService.GetSchoolByIds(schoolIds);
            var schoolResult = school.Content?.Result;

            // Lấy ra lần đăng nhập cuối cùng
            var featureAccessTime = await _systemService.GetFeatureAccessTimeByUserIds(distinctUserIds);
            var featureAccessTimeResult = featureAccessTime.Content?.Result;

            #region Trả dữ liệu
            List<ClientsIntegrationModel> clientsIntegrations = new List<ClientsIntegrationModel>();
            clientsIntegrations = _mapper.Map<List<ClientsIntegrationModel>>(users);

            clientsIntegrations.ForEach(item =>
            {
                item.LongPathSchool = schoolResult?.FirstOrDefault(x => x.Id == item.SchoolId)?.LongPath;
                item.LongPathLocation = schoolResult?.FirstOrDefault(x => x.Id == item.SchoolId)?.Location?.LongPath;
                item.LastDate = featureAccessTimeResult?.FirstOrDefault(x => x.CreatedUserId == item.UserId)?.LastVisited;
                item.PTLevel = ptTestResults.FirstOrDefault(x => x.UserId == item.UserId)?.Level;

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
                        CoursePackage = order.MonthNumber,
                        PaymentMethod = order.PaymentMethod ?? default,
                        DiscountPrice = order.DiscountPrice,
                        TotalPrice = order.TotalPrice,
                        Status = order.StatusCourseResult
                    };
                    orderIntegrations.Add(orderIntegration);
                }

                item.OrderIntegration = orderIntegrations;
            });
            #endregion

            int totalItem = clientsIntegrations.Count;
            var lists = clientsIntegrations
                    .ApplySortAndPaging(request)
                    .ToList();

            methodResult.Result = new PagingItemsModel<ClientsIntegrationModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
