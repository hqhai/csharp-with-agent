// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Ordering.Application.Services.TrainingService;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Application.Services.UserService.Models;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ChangeStatusOrderCommand : ChangeStatusOrderCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class ChangeStatusOrderCommandHandler : IRequestHandler<ChangeStatusOrderCommand, MethodResult<bool>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ITrainingService _trainingService;
        private readonly IUserService _userService;

        public ChangeStatusOrderCommandHandler(IOrderRepository orderRepository, ITrainingService trainingService, IUserService userService)
        {
            _orderRepository = orderRepository;
            _trainingService = trainingService;
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(ChangeStatusOrderCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var order = await _orderRepository.GetByIdAsync(request.OderId);
            if (order == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOrderErrorCode.OrderNotExist));
                return methodResult;
            }
            if (order.Status != EnumOrderStatus.New)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOrderErrorCode.OrderStatusIsNotNew));
                return methodResult;
            }

            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                order.Status = request.OderStatus;
                order = _orderRepository.Update(order);
                await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                if (request.OderStatus == EnumOrderStatus.Reject)
                {
                    var student = await _trainingService.DeleteStudentFromClass(order.UserId);
                    if (!student.IsSuccessStatusCode)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumOrderErrorCode.UpdateNotSuccess));
                        return methodResult;
                    }
                }
                if (order.Status != EnumOrderStatus.Payment)
                {
                    //var student = await _userService.UpdateStudentByClassAsync(new UpdateStudentByClassIdModel { ClassId = classnew.Id, StudentId = studentId, PackageId = request.PackageId });
                }
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
