// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.IntegrationQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Application.Services.InteractionService;
    using Fsel.Identity.Application.Services.InteractionService.Models;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.LmsCourseService.Model;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
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
        private readonly IInteractionService _interactionService;

        public LeadsIntegrationQueryHandler(IOrderService orderService,
                                            IHumanRepository humanRepository,
                                            ILmsCourseService lmsCourseService,
                                            IInteractionService interactionService)
        {
            _orderService = orderService;
            _humanRepository = humanRepository;
            _lmsCourseService = lmsCourseService;
            _interactionService = interactionService;
        }
        public async Task<MethodResult<PagingItemsModel<LeadsIntegrationModel>>> Handle(LeadsIntegrationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<LeadsIntegrationModel>>();

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
                Status = false
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

            #region Survey
            var querySurvey = new GetSchollByQuestionSurveyQueryModel
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate,
            };
            var schools = await _interactionService.GetSchoolByQuestion(querySurvey);
            if (!schools.IsSuccessStatusCode)
            {
                methodResult.AddError(schools.Error);
                return methodResult;
            }
            var schoolResults = schools.Content?.Result;
            if (schoolResults == null)
            {
                methodResult.AddError(schools.Error);
                return methodResult;
            }
            var userSchoolIds = schoolResults.Select(x => x.UserId).ToList();
            #endregion

            #region Identity
            var users = await _humanRepository.Queryable
                                              .Include(x => x.User)
                                              .Include(x => x.Student)
                                              .ThenInclude(x => x!.ParentStudents)
                                              .ThenInclude(x => x.Parent)
                                              .ThenInclude(x => x!.Human)
                                              .Where(x => x.UpdatedDate == null ? (x.CreatedDate >= request.StartDate && x.CreatedDate <= request.EndDate) : (x.UpdatedDate >= request.StartDate && x.UpdatedDate <= request.EndDate))
                                              .Select(x => new LeadsIntegrationModel
                                              {
                                                  UserId = x.UserId ?? default,
                                                  FullName = x.FullName,
                                                  UserName = x.User!.UserName,
                                                  StudentEmail = x.Email,
                                                  StudentPhone = x.PhoneNumber,
                                                  EnumGender = x.Gender,
                                                  Birthday = x.Birthday,
                                                  Address = x.Address,
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

            // hợp list Id và lấy ra Id duy nhất
            var listUserIds = userIdentityIds.Concat(userSchoolIds).Concat(userPtTestIds).Concat(userOrderIds).ToList();
            var distinctUserIds = userIdentityIds.Distinct().ToList();

            // gán dữ liệu
            List<LeadsIntegrationModel> leadsIntegrations = new List<LeadsIntegrationModel>();
            foreach (var item in distinctUserIds)
            {
                var user = users.FirstOrDefault(x => x.UserId == item);
                var survey = schoolResults.FirstOrDefault(x => x.UserId == item);
                var ptTestResult = ptTestResults.FirstOrDefault(x => x.UserId == item);
                var orderItems = orderResults.Where(x => x.UserId == item).ToList();
                EnumIntegrationStatus status = EnumIntegrationStatus.Register;
                var statusDetail = string.Empty;
                int learningPackage = 0;
                if (orderItems != null)
                {
                    foreach (var orderItem in orderItems)
                    {
                        if (orderItem.IsTrial)
                        {
                            status = EnumIntegrationStatus.Trial;
                            statusDetail = orderItem.ExpireDate.ToString();
                        }
                        else
                        {
                            learningPackage = orderItem.MonthNumber;
                        }
                    }
                }
                else if (ptTestResult != null)
                {
                    status = EnumIntegrationStatus.Placement;
                    if (ptTestResult.Status == "Done")
                    {
                        statusDetail = "Done";
                    }
                    else
                    {
                        statusDetail = "Process";
                    }
                }
                var leadsIntegration = new LeadsIntegrationModel
                {
                    UserId = item,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    StudentEmail = user.StudentEmail,
                    StudentPhone = user.StudentPhone,
                    EnumGender = user.EnumGender,
                    Birthday = user.Birthday,
                    Address = user.Address,
                    ParentName = user.ParentName,
                    ParentPhone = user.ParentPhone,
                    ParentEmail = user.ParentEmail,
                    ParentGender = user.ParentGender,
                    Status = status,
                    StatusDetail = statusDetail,
                    SchoolName = survey?.Name,
                    PTLever = ptTestResult?.Lever,
                    LearningPackage = learningPackage
                };
                leadsIntegrations.Add(leadsIntegration);
            }

            int totalItem = leadsIntegrations.Count;
            var lists = leadsIntegrations
                    .ApplySortAndPaging(request)
                    .ToList();

            methodResult.Result = new PagingItemsModel<LeadsIntegrationModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
