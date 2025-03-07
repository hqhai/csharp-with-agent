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
                var orderHasTimes = await _orderService.GetOrderByStatusAsync(new GetOrderByStatusQueryModel { StartDate = request.StartDate, EndDate = request.EndDate, Status = false });
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
                var userIdentityHasTimeIds = users.Select(x => x.UserId ?? Guid.Empty).ToList();

                // hợp nhất UserId chưa có order
                var userIds = userIdentityHasTimeIds.Concat(userPtTestHasTimeIds).Concat(userUnitResultHasTimeIds).Concat(userOrderIds).ToList();
                distinctUserIds = userIds.Distinct().ToList();
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

            // lấy unit lesson
            var units = await _lmsCourseService.GetUnitResults(courseIntegrationQueryModel);
            var unitResults = units.Content?.Result;

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
            var school = await _systemService.GetSchoolByIds(schoolIds);
            var schoolResult = school.Content?.Result;

            // lấy lần đăng nhập cuối cùng
            var featureAccessTime = await _systemService.GetFeatureAccessTimeByUserIds(paging);
            var featureAccessTimeResult = featureAccessTime.Content?.Result;

            #region SetData
            List<LeadsIntegrationModel> leadsIntegrations = new List<LeadsIntegrationModel>();
            leadsIntegrations = _mapper.Map(userCombines, leadsIntegrations);

            foreach (var item in leadsIntegrations)
            {
                var orderItem = orderResults?.Where(x => x.UserId == item.UserId).OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate).FirstOrDefault();
                if (orderItem != null && !orderItem.IsTrial)
                {
                    continue;
                }

                var ptTestResult = ptTestResults?.FirstOrDefault(x => x.UserId == item.UserId);
                var unitResult = unitResults?.FirstOrDefault(x => x.UserId == item.UserId);
                item.LastDate = featureAccessTimeResult?.FirstOrDefault(x => x.CreatedUserId == item.UserId)?.LastVisited;
                item.AccessTime = featureAccessTimeResult?.FirstOrDefault(x => x.CreatedUserId == item.UserId)?.AccessTime;
                item.Status = EnumIntegrationStatus.Register;
                item.LongPathSchool = schoolResult?.FirstOrDefault(x => x.Id == item.SchoolId)?.LongPath;
                item.LongPathLocation = schoolResult?.FirstOrDefault(x => x.Id == item.SchoolId)?.Location?.LongPath;
                item.CurrentUnit = unitResult?.Name;
                item.CurrentLesson = unitResult?.CurrentLesson;
                item.LessonCompleted = unitResult?.LessonCompleted;

                var dateOrder = orderItem?.UpdatedDate ?? orderItem?.CreatedDate;
                var dateUser = userCombines.FirstOrDefault(x => x.UserId == item.UserId)?.UpdatedDate != null ? userCombines.FirstOrDefault(x => x.UserId == item.UserId)?.UpdatedDate : userCombines.FirstOrDefault(x => x.UserId == item.UserId)?.CreatedDate;

                var dateEdits = new[] { dateOrder, ptTestResult?.DateEdit, unitResult?.DateEdit, dateUser };
                if (dateEdits.Any() && dateEdits.Any(x => x.HasValue))
                {
                    item.DateEdit = dateEdits.Where(d => d.HasValue).Max(d => d.Value);
                }

                if (ptTestResult != null)
                {
                    item.PTLevel = ptTestResult.Level;
                    item.Status = EnumIntegrationStatus.Placement;
                    item.PlacementTestResults = ptTestResult.PlacementTestResults;
                    if (ptTestResult.Status == "Done")
                    {
                        item.StatusPT = "Done";
                    }
                    else
                    {
                        item.StatusPT = "Process";
                    }
                }

                if (orderItem != null)
                {
                    if (orderItem.IsTrial)
                    {
                        item.Status = EnumIntegrationStatus.Trial;
                        item.StartTrial = orderItem.CreatedDate ?? null;
                        item.ExpireDate = orderItem.ExpireDate ?? null;
                    }
                    else if (orderItem.ExpireDate != null && !orderItem.IsTrial && orderItem.ExpireDate < DateTime.UtcNow)
                    {
                        item.Status = EnumIntegrationStatus.Expired;
                    }
                    else if (orderItem.Status == EnumOrderStatus.Fail)
                    {
                        item.Status = EnumIntegrationStatus.Fail;
                    }
                    else if (orderItem.Status == EnumOrderStatus.New)
                    {
                        item.Status = EnumIntegrationStatus.New;
                    }
                    else if (orderItem.Status == EnumOrderStatus.Reject)
                    {
                        item.Status = EnumIntegrationStatus.Reject;
                    }

                    item.CourseLevel = orderItem.CourseName.ToString() ?? string.Empty;
                }
            };
            #endregion

            leadsIntegrations = leadsIntegrations.OrderByDescending(x => x.Status).ToList();
            int totalItem = distinctUserIds.Count;
            methodResult.Result = new PagingItemsModel<LeadsIntegrationModel>(leadsIntegrations, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
