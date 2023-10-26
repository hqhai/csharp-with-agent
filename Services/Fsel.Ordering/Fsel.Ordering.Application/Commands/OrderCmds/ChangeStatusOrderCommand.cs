// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Queues.Publishers;
    using Fsel.Ordering.Application.Services.CourseService;
    using Fsel.Ordering.Application.Services.TrainingService;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Application.Services.UserService.Models;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ChangeStatusOrderCommand : ChangeStatusOrderCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class ChangeStatusOrderCommandHandler : IRequestHandler<ChangeStatusOrderCommand, MethodResult<bool>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ITrainingService _trainingService;
        private readonly IUserService _userService;
        private readonly IPackageRepository _packageRepository;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly AuthContext _authContext;

        public ChangeStatusOrderCommandHandler(IOrderRepository orderRepository
            , ITrainingService trainingService
            , IUserService userService
            , IPackageRepository packageRepository
            , ILmsCourseService lmsCourseService
            , NotificationMessagePublisher notificationMessagePublisher
            , AuthContext authContext)
        {
            _orderRepository = orderRepository;
            _trainingService = trainingService;
            _userService = userService;
            _packageRepository = packageRepository;
            _lmsCourseService = lmsCourseService;
            _notificationMessagePublisher = notificationMessagePublisher;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(ChangeStatusOrderCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(order));
                return methodResult;
            }
            if (order.Status != EnumOrderStatus.New)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOrderErrorCode.OrderStatusIsNotNew));
                return methodResult;
            }
            var student = await _userService.GetStudentByUserIdAsync(order.UserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var classes = await _trainingService.GetNewClassByStudentId(student.Content!.Result!.Id);
            if (!classes.IsSuccessStatusCode)
            {
                methodResult.AddError(classes.Error);
                return methodResult;
            }

            var courseResults = await _lmsCourseService.GetCoursesByIdsAsync(new List<Guid> { order.CourseId });
            if (!courseResults.IsSuccessStatusCode)
            {
                methodResult.AddError(courseResults.Error);
                return methodResult;
            }
            var package = await _packageRepository.GetByIdAsync(order.PackageId);
            var numberOfShield = (package != null && package.Code.HasValue) ? (int)package.Code.Value : default;
            var course = courseResults.Content?.Result?.FirstOrDefault();
            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                if (request.OrderStatus == EnumOrderStatus.Reject)
                {
                    var classStudent = await _trainingService.DeleteStudentFromClass(order.UserId);
                    if (!classStudent.IsSuccessStatusCode)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumOrderErrorCode.UpdateNotSuccess));
                        return methodResult;
                    }
                }
                else if (request.OrderStatus == EnumOrderStatus.Payment)
                {
                    var updateStudentByClass = await _userService.UpdateStudentByClassAsync(new UpdateStudentByClassIdModel { ClassId = classes.Content?.Result?.Id, StudentId = student.Content!.Result!.Id, PackageId = request.PackageId, NumberOfShield = numberOfShield, CourseLevel = course?.CourseLevel });
                    if (!updateStudentByClass.IsSuccessStatusCode)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumOrderErrorCode.UpdateNotSuccess));
                        return methodResult;
                    }

                    var updateStudentStatusInClass = await _trainingService.UpdateStatusStudentInClass(student.Content!.Result!.Id);
                    if (!updateStudentStatusInClass.IsSuccessStatusCode)
                    {
                        methodResult.AddError(updateStudentStatusInClass.Error);
                        return methodResult;
                    }

                    await _notificationMessagePublisher.Publish(new NotificationQueueModel
                    {
                        UserId = order.CreatedUserId,
                        ObjectId = order.Id,
                        ParamsMessage = new List<object> { course?.Name ?? string.Empty },
                        Type = EnumNotificationType.Text,
                        Content = EnumNotificationContent.OrderChangeStatus,
                        SenderId = _authContext.CurrentUserId
                    }, cancellationToken);
                }
                order.Status = request.OrderStatus;
                order = _orderRepository.Update(order);
                await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                var orders = await _orderRepository.Queryable.Where(p => p.ClassId == order.ClassId && p.Status == EnumOrderStatus.Payment).ToListAsync(cancellationToken);
                if (orders.Count == 12)
                {
                    var activeClassResult = await _trainingService.ActiveClass(order.ClassId);
                    if (!activeClassResult.IsSuccessStatusCode)
                    {
                        methodResult.AddError(activeClassResult.Error);
                        return methodResult;
                    }
                }

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
