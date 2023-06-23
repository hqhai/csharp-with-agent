// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Services.TrainingService;
    using Fsel.Ordering.Application.Services.TrainingService.CommandModels;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders;
    using Fsel.Ordering.Domain.Models.EntityModels;
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
        private readonly AuthContext _authContext;
        private readonly ITrainingService _trainingService;
        private readonly IPackageRepository _packageRepository;

        public CreateOrderCommandHandler(IMapper mapper,
            IOrderRepository orderRepository,
            AuthContext authContext,
            ITrainingService trainingService,
            IPackageRepository packageRepository)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _authContext = authContext;
            _trainingService = trainingService;
            _packageRepository = packageRepository;
        }

        public async Task<MethodResult<OrderModel>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<OrderModel> methodResult = new MethodResult<OrderModel>();

            var package = await _packageRepository.GetByIdAsync(request.PackageId);
            if (package == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOrderErrorCode.PackageNotExist));
                return methodResult;
            }

            if (await _orderRepository.Queryable.AnyAsync(x => x.Code == request.Code, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumOrderErrorCode.CodeOrderAlreadyExist));
                return methodResult;
            }

            var classnew = await _trainingService.RegisterClassAsync(new RegisterClassCommandModel { Code = request.CodeClass, CourseId = request.CourseId, CourseLevel = request.CourseLevel, PackageId = request.PackageId, LiveDays = request.LiveDays, LiveTimeFrameId = request.LiveTimeFrameId });
            if (!classnew.IsSuccessStatusCode)
            {
                methodResult.AddError(classnew.Error?.Content, classnew.StatusCode);
                return methodResult;
            }
            if (classnew?.Content?.Result == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumOrderErrorCode.ClassNotExist));
                return methodResult;
            }

            Order order = _mapper.Map<Order>(request);
            order.Status = EnumOrderStatus.New;
            order.UserId = _authContext.CurrentUserId;
            order.Price = package.Price;
            order.DiscountPercent = 5;
            order.DiscountPrice = order.Price * order.DiscountPercent / 100;
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

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<OrderModel>(order);
                return methodResult;
            });
            return methodResult;
        }
    }
}
