// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.v1i1
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Models;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Ordering.Application.Services.CourseService;
    using Fsel.Ordering.Application.Services.TrainingService;
    using Fsel.Ordering.Application.Services.TrainingService.CommandModels;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class ChooseLevelForAccountCRMCommand : IRequest<MethodResult<bool>>
    {
        public EnumCourseLevel CourseLevel { get; set; }
    }

    public class ChooseLevelForAccountCRMCommandHandler : IRequestHandler<ChooseLevelForAccountCRMCommand, MethodResult<bool>>
    {
        private readonly ITrainingService _trainingService;
        private readonly IOrderRepository _orderRepository;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly AuthContext _authContext;
        private readonly IPackageRepository _packageRepository;

        public ChooseLevelForAccountCRMCommandHandler(ITrainingService trainingService, IOrderRepository orderRepository, ILmsCourseService lmsCourseService, AuthContext authContext, IPackageRepository packageRepository)
        {
            _trainingService = trainingService;
            _orderRepository = orderRepository;
            _lmsCourseService = lmsCourseService;
            _authContext = authContext;
            _packageRepository = packageRepository;
        }

        public async Task<MethodResult<bool>> Handle(ChooseLevelForAccountCRMCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var courseResult = await _lmsCourseService.GetCourseByLevel(new BaseQueryModel()
            {
                Filters = new List<GenericFilterModel>()
                {
                    new GenericFilterModel()
                    {
                        Property = "Status",
                        Operator = EnumFilterOperator.Equal,
                        Value = "Active"
                    },
                    new GenericFilterModel()
                    {
                        Property = "CourseLevel",
                        Operator = EnumFilterOperator.Equal,
                        Value = request.CourseLevel.ToString()
                    }
                }
            });

            if (!courseResult.IsSuccessStatusCode)
            {
                methodResult.AddError(courseResult.Error);
                return methodResult;
            }

            var course = courseResult.Content?.Result;
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var order = await _orderRepository.Queryable.Include(ot => ot.OrderTransactions).Where(p => p.Status == EnumOrderStatus.Payment && p.IsTrial && p.ExpireDate == null).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(cancellationToken);
            if (order == null || order.OrderTransactions.FirstOrDefault()?.RequestBodyStr != "CRM")
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var package = await _packageRepository.GetByIdAsync(order.PackageId ?? default);
            if (package == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var numberOfShield = package.Code.HasValue ? (int)package.Code.Value : default;

            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                var addStudentIntoClassResult = await _trainingService.AddStudentIntoClass(new AddStudentIntoClassCommandModel() { UserId = _authContext.CurrentUserId, CourseId = course.Id, PackageId = order.PackageId ?? default, NumberOfShield = numberOfShield });
                if (!addStudentIntoClassResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(addStudentIntoClassResult.Error);
                    return methodResult;
                }
                order.ExpireDate = DateTime.UtcNow.AddMonths(package.MonthNumber);
                order = _orderRepository.Update(order);
                await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                return methodResult;
            });
            return methodResult;
        }
    }
}
