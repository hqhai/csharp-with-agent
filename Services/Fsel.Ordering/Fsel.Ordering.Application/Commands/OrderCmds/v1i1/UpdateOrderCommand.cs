// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.v1i1
{
    using System;
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Models;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Ordering.Application.Queries.OrderQuery;
    using Fsel.Ordering.Application.Services.CourseService;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i1;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateOrderCommand : CreateOrderCommandModel, IRequest<MethodResult<OrderModel>>
    {
    }

    public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, MethodResult<OrderModel>>
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IMediator _mediator;
        private readonly AuthContext _authContext;
        private readonly ILmsCourseService _courseService;

        public UpdateOrderCommandHandler(IMapper mapper, IOrderRepository orderRepository, IPackageRepository packageRepository, MediatR.IMediator mediator, AuthContext authContext, ILmsCourseService courseService)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _packageRepository = packageRepository;
            _mediator = mediator;
            _authContext = authContext;
            _courseService = courseService;
        }

        public async Task<MethodResult<OrderModel>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OrderModel>();

            var order = await _orderRepository.Queryable.FirstOrDefaultAsync(p => p.UserId == _authContext.CurrentUserId, cancellationToken);
            if (order == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

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

            order.Status = EnumOrderStatus.New;
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
                order = _orderRepository.Update(order);
                await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<OrderModel>(order);
                return methodResult;
            });
            return methodResult;
        }
    }
}
