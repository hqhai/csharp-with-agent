// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.IntegrationQuery
{
    using System.Threading;
    using System.Threading.Tasks;
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

        public ClientIntegrationQueryHandler(IOrderService orderService,
                                            IHumanRepository humanRepository,
                                            ILmsCourseService lmsCourseService,
                                            ISystemService systemService)
        {
            _orderService = orderService;
            _humanRepository = humanRepository;
            _lmsCourseService = lmsCourseService;
            _systemService = systemService;
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

            #region Order
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
            #endregion

            #region PTTestResult
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
            #endregion

            #region Identity
            var users = await _humanRepository.Queryable
                                              .Include(x => x.User)
                                              .Include(x => x.Student)
                                              .ThenInclude(x => x!.ParentStudents)
                                              .ThenInclude(x => x.Parent)
                                              .ThenInclude(x => x!.Human)
                                              .Where(x => x.UpdatedDate == null ? (x.CreatedDate.Date >= request.StartDate.Date && x.CreatedDate.Date <= request.EndDate.Date) : (x.UpdatedDate.Value.Date >= request.StartDate.Date && x.UpdatedDate.Value.Date <= request.EndDate.Date))
                                              .Select(x => new ClientsIntegrationModel
                                              {
                                                  UserId = x.UserId ?? default,
                                                  FullName = x.FullName,
                                                  UserName = x.User!.UserName,
                                                  StudentEmail = x.Email,
                                                  StudentPhone = x.PhoneNumber,
                                                  EnumGender = x.Gender,
                                                  Birthday = x.Birthday,
                                                  Address = x.Address,
                                                  SchoolId = x.Student!.SchoolId,
                                                  ParentName = x.Student!.ParentStudents.Select(x => x.Parent).FirstOrDefault()!.Human!.FullName,
                                                  ParentPhone = x.Student!.ParentStudents.Select(x => x.Parent).FirstOrDefault()!.Human!.PhoneNumber,
                                                  ParentEmail = x.Student!.ParentStudents.Select(x => x.Parent).FirstOrDefault()!.Human!.Email,
                                                  ParentGender = x.Student!.ParentStudents.Select(x => x.Parent).FirstOrDefault()!.Human!.Gender
                                              })
                                              .ToListAsync(cancellationToken);
            if (users == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(users));
                return methodResult;
            }
            var userIdentityIds = users.Select(x => x.UserId).ToList();
            #endregion

            #region SetData
            // hợp list UserId và lấy ra UserId duy nhất
            var listUserIds = userIdentityIds.Concat(userPtTestIds).Concat(userOrderIds).ToList();
            var distinctUserIds = listUserIds.Distinct().ToList();

            #region School
            var schoolIds = users.Where(x => x.SchoolId.HasValue).Select(x => x.SchoolId!.Value).ToList();
            var school = await _systemService.GetSchoolByIds(schoolIds);
            var schoolResult = school.Content?.Result;
            #endregion

            #region LastTime
            var featureAccessTime = await _systemService.GetFeatureAccessTimeByUserIds(distinctUserIds);
            var featureAccessTimeResult = featureAccessTime.Content?.Result;
            #endregion

            // gán dữ liệu
            List<ClientsIntegrationModel> leadsIntegrations = new List<ClientsIntegrationModel>();
            foreach (var item in distinctUserIds)
            {
                var user = users.FirstOrDefault(x => x.UserId == item);
                var ptTestResult = ptTestResults.FirstOrDefault(x => x.UserId == item);
                var orderItems = orderResults.Where(x => x.UserId == item).ToList();
                var locationId = schoolResult?.FirstOrDefault(x => x.Id == user?.SchoolId)?.LocationId;
                var lastTime = featureAccessTimeResult?.FirstOrDefault(x => x.CreatedUserId == item)?.LastVisited;
                if (user == null)
                {
                    user = await _humanRepository.Queryable
                                                 .Include(x => x.User)
                                                 .Include(x => x.Student)
                                                 .ThenInclude(x => x!.ParentStudents)
                                                 .ThenInclude(x => x.Parent)
                                                 .ThenInclude(x => x!.Human)
                                                 .Where(x => x.UserId == item)
                                                 .Select(x =>
                                                 new ClientsIntegrationModel
                                                 {
                                                     UserId = x.UserId ?? default,
                                                     FullName = x.FullName,
                                                     UserName = x.User!.UserName,
                                                     StudentEmail = x.Email,
                                                     StudentPhone = x.PhoneNumber,
                                                     EnumGender = x.Gender,
                                                     Birthday = x.Birthday,
                                                     Address = x.Address,
                                                     SchoolId = x.Student!.SchoolId,
                                                     ParentName = x.Student!.ParentStudents.Select(x => x.Parent).FirstOrDefault()!.Human!.FullName,
                                                     ParentPhone = x.Student!.ParentStudents.Select(x => x.Parent).FirstOrDefault()!.Human!.PhoneNumber,
                                                     ParentEmail = x.Student!.ParentStudents.Select(x => x.Parent).FirstOrDefault()!.Human!.Email,
                                                     ParentGender = x.Student!.ParentStudents.Select(x => x.Parent).FirstOrDefault()!.Human!.Gender
                                                 }).FirstOrDefaultAsync(cancellationToken);
                }

                List<OrderIntegrationModel> orderIntegrations = new List<OrderIntegrationModel>();
                foreach (var order in orderItems)
                {
                    var courseName = string.Empty;
                    if (order.CourseName != null && (int)order.CourseName <= 5)
                    {
                        courseName = EnumCourseType.Academic.ToString();
                    }
                    else
                    {
                        courseName = EnumCourseType.Ielts.ToString();
                    }
                    var orderIntegration = new OrderIntegrationModel
                    {
                        OrderCode = order.Code,
                        StartDate = order.UpdatedDate ?? default,
                        EndDate = order.ExpireDate ?? default,
                        Program = courseName,
                        CourseLever = order.CourseName.ToString(),
                        CoursePackage = order.MonthNumber,
                        PaymentMethod = order.PaymentMethod ?? default,
                        DiscountPrice = order.DiscountPrice,
                        TotalPrice = order.TotalPrice,
                        Status = order.StatusCourseResult
                    };
                    orderIntegrations.Add(orderIntegration);
                }

                var leadsIntegration = new ClientsIntegrationModel
                {
                    UserId = item,
                    FullName = user?.FullName,
                    UserName = user?.UserName,
                    StudentEmail = user?.StudentEmail,
                    StudentPhone = user?.StudentPhone,
                    EnumGender = user?.EnumGender,
                    Birthday = user?.Birthday,
                    Address = user?.Address,
                    ParentName = user?.ParentName,
                    ParentPhone = user?.ParentPhone,
                    ParentEmail = user?.ParentEmail,
                    ParentGender = user?.ParentGender,
                    LocationId = locationId,
                    SchoolId = user?.SchoolId,
                    PTLevel = ptTestResult?.Level,
                    OrderIntegration = orderIntegrations,
                    LastDate = lastTime
                };
                leadsIntegrations.Add(leadsIntegration);
            }
            #endregion 

            int totalItem = leadsIntegrations.Count;
            var lists = leadsIntegrations
                    .ApplySortAndPaging(request)
                    .ToList();

            methodResult.Result = new PagingItemsModel<ClientsIntegrationModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
