// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.v1i1
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Common.Models;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Ordering.Application.Queries.OrderQuery;
    using Fsel.Ordering.Application.Queues.Publishers;
    using Fsel.Ordering.Application.Services.CourseService;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i1;
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
        private readonly IMediator _mediator;
        private readonly NotificationMessagePublisher _notificationMessagePublisher;
        private readonly IPackageRepository _packageRepository;
        private readonly AuthContext _authContext;
        private readonly ILmsCourseService _courseService;

        public CreateOrderCommandHandler(IMapper mapper, IOrderRepository orderRepository, IMediator mediator, NotificationMessagePublisher notificationMessagePublisher, IPackageRepository packageRepository, AuthContext authContext, ILmsCourseService courseService)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _mediator = mediator;
            _notificationMessagePublisher = notificationMessagePublisher;
            _packageRepository = packageRepository;
            _authContext = authContext;
            _courseService = courseService;
        }

        public async Task<MethodResult<OrderModel>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OrderModel>();

            var package = await _packageRepository.GetByIdAsync(request.PackageId);

            if (package == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(package));
                return methodResult;
            }

            var codeSend = await _mediator.Send(new GenerateRamdomOrderQuery { CourseLevel = request.CourseLevel, PackageId = package.Id }, cancellationToken).ConfigureAwait(false);
            var code = codeSend.Result?.Code;

            if (await _orderRepository.Queryable.AnyAsync(x => x.Code == code || (x.Status == EnumOrderStatus.New && x.UserId == _authContext.CurrentUserId), cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(code));
                return methodResult;
            }

            var courseResult = await _courseService.GetCourseByLevel(new BaseQueryModel()
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

            Order order = _mapper.Map<Order>(request);
            order.Country = EnumZoneRegion.Vietnam.ToString();
            order.Status = EnumOrderStatus.New;
            order.UserId = _authContext.CurrentUserId;
            order.Code = code;
            order.Price = package.Price;
            order.DiscountPercent = 0;
            order.DiscountPrice = (decimal)NumberHelper.ConvertDoublePercent(Convert.ToDouble(order.Price * order.DiscountPercent));
            order.TotalPrice = order.Price - order.DiscountPrice;
            order.CourseId = course!.Id;
            if (!order.IsValid())
            {
                methodResult.AddErrorBadRequest(order.ErrorMessages);
                return methodResult;
            }
            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                order = _orderRepository.Add(order);
                await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                await _notificationMessagePublisher.Publish(new NotificationSendingQueueModel
                {
                    Roles = new List<EnumRole> { EnumRole.Admin },
                    ObjectId = order.Id,
                    Type = EnumNotificationType.Text,
                    Content = EnumNotificationContent.OrderCreate,
                    SenderId = order.CreatedUserId,
                    PlatformCode = EnumPlatformCode.LMSAdmin
                }, cancellationToken);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<OrderModel>(order);
                return methodResult;
            });
            return methodResult;
        }
    }
}
