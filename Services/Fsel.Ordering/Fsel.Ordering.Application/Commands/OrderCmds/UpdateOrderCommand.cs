// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Queues.Publishers;
    using Fsel.Ordering.Application.Services.TrainingService;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateOrderCommand : UpdateOrderCommandModel, IRequest<MethodResult<OrderModel>>
    {
    }

    public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, MethodResult<OrderModel>>
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly IMediator _mediator;
        private readonly IUserService _userService;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly ITrainingService _trainingService;
        private readonly IPackageRepository _packageRepository;
        private readonly AuthContext _authContext;
        private const int NumberTrialDays = 14; // số ngày dùng thử chương trình là 14 ngày.

        public UpdateOrderCommandHandler(IMapper mapper,
            IOrderRepository orderRepository,
            IMediator mediator,
            IUserService userService,
            NotificationMessagePublisher notificationMessagePublisher,
            ITrainingService trainingService,
            IPackageRepository packageRepository,
            AuthContext authContext)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _mediator = mediator;
            _userService = userService;
            _notificationMessagePublisher = notificationMessagePublisher;
            _trainingService = trainingService;
            _packageRepository = packageRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<OrderModel>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<OrderModel> methodResult = new MethodResult<OrderModel>();
            var order = _orderRepository.Queryable.FirstOrDefault(x => x.UserId == request.UserId);

            if (order == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            _mapper.Map(request, order);

            if (!order.IsValid())
            {
                methodResult.AddErrorBadRequest(order.ErrorMessages);
                return methodResult;
            }

            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                order = _orderRepository.Add(order);
                await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                await SendNotification(order, cancellationToken);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<OrderModel>(order);
                return methodResult;
            });
            return methodResult;
        }

        private async Task SendNotification(Order order, CancellationToken cancellationToken)
        {
            await _notificationMessagePublisher.Publish(new NotificationSendingQueueModel
            {
                Roles = new List<EnumRole> { EnumRole.Admin },
                ObjectId = order.Id,
                Type = EnumNotificationType.Text,
                Content = EnumNotificationContent.OrderCreate,
                SenderId = order.UserId,
                PlatformCode = EnumPlatformCode.LMSAdmin
            }, cancellationToken);
        }
    }
}
