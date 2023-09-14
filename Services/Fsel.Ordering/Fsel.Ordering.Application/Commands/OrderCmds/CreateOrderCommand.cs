// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Ordering.Application.Queries.OrderQuery;
    using Fsel.Ordering.Application.Queues.Publishers;
    using Fsel.Ordering.Application.Services.TrainingService;
    using Fsel.Ordering.Application.Services.TrainingService.CommandModels;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateOrderCommand : CreateOrderCommandModel, IRequest<MethodResult<OrderModel>>
    {
    }

    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, MethodResult<OrderModel>>
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly MediatR.IMediator _mediator;
        private readonly IUserService _userService;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly ITrainingService _trainingService;
        private readonly IPackageRepository _packageRepository;

        public CreateOrderCommandHandler(IMapper mapper,
            IOrderRepository orderRepository,
            IMediator mediator,
            IUserService userService,
            NotificationMessagePublisher notificationMessagePublisher,
            ITrainingService trainingService,
            IPackageRepository packageRepository)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _mediator = mediator;
            _userService = userService;
            _notificationMessagePublisher = notificationMessagePublisher;
            _trainingService = trainingService;
            _packageRepository = packageRepository;
        }

        public async Task<MethodResult<OrderModel>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<OrderModel> methodResult = new MethodResult<OrderModel>();

            //var package = await _packageRepository.GetByIdAsync(request.PackageId);

            #region Pilot

            var package = await _packageRepository.Queryable.FirstOrDefaultAsync(x => x.Code == EnumPackageCode.BASIC, cancellationToken);

            #endregion Pilot

            if (package == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(package));
                return methodResult;
            }
            var codeSend = await _mediator.Send(new GenerateRamdomOrderQuery { CourseLevel = request.CourseLevel, PackageId = package.Id, UserId = request.UserId }, cancellationToken).ConfigureAwait(false);
            var code = codeSend.Result?.Code;
            if (await _orderRepository.Queryable.AnyAsync(x => x.UserId == request.UserId && x.Status != EnumOrderStatus.Reject && x.CreatedDate.AddDays(14) < DateTime.Now, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.CourseLevel));
                return methodResult;
            }

            if (await _orderRepository.Queryable.AnyAsync(x => x.Code == code, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(code));
                return methodResult;
            }

            //var classnew = await _trainingService.RegisterClassAsync(new RegisterClassCommandModel { Code = request.CodeClass, CourseId = request.CourseId, CourseLevel = request.CourseLevel, PackageId = request.PackageId, LiveDays = request.LiveDays, LiveTimeFrameId = request.LiveTimeFrameId });

            #region Pilot

            var classnew = await _trainingService.RegisterClassAsync(new RegisterClassCommandModel { UserId = request.UserId, Code = request.CodeCourse, CourseId = request.CourseId, CourseLevel = request.CourseLevel, PackageId = package.Id, LiveDays = request.LiveDays, LiveTimeFrameId = request.LiveTimeFrameId });

            #endregion Pilot

            if (!classnew.IsSuccessStatusCode)
            {
                methodResult.AddError(classnew.Error);
                return methodResult;
            }

            if (classnew?.Content?.Result == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classnew));
                return methodResult;
            }

            Order order = _mapper.Map<Order>(request);
            order.Status = EnumOrderStatus.New;
            order.UserId = request.UserId;
            order.Code = code;
            order.PackageId = package.Id;
            order.Price = package.Price;
            order.DiscountPercent = 5;
            order.DiscountPrice = (decimal)NumberHelper.ConvertDoublePercent(Convert.ToDouble(order.Price * order.DiscountPercent));
            order.TotalPrice = order.Price - order.DiscountPrice;
            order.ClassId = classnew.Content?.Result.Id ?? default;
            if (!order.IsValid())
            {
                methodResult.AddErrorBadRequest(order.ErrorMessages);
                return methodResult;
            }

            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                order = _orderRepository.Add(order);
                await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                await _notificationMessagePublisher.Publish(new NotificationQueueModel
                {
                    Roles = new List<EnumRole> { EnumRole.Admin },
                    ObjectId = order.Id,
                    Type = EnumNotificationType.Text,
                    Content = EnumNotificationContent.OrderCreate
                }, cancellationToken);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<OrderModel>(order);
                return methodResult;
            });
            return methodResult;
        }
    }
}
