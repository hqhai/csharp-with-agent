// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.v1i1
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Queries.OrderQuery;
    using Fsel.Ordering.Application.Queues.Publishers;
    using Fsel.Ordering.Application.Services.CourseService;
    using Fsel.Ordering.Application.Services.TrainingService;
    using Fsel.Ordering.Application.Services.TrainingService.CommandModels;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Application.Services.UserService.Models;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i1;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateOrderByUserIdCommand : CreateOrderToUserIdCommandModel, IRequest<MethodResult<OrderModel>>
    {
    }

    public class CreateOrderByUserIdCommandHandler : IRequestHandler<CreateOrderByUserIdCommand, MethodResult<OrderModel>>
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IMediator _mediator;
        private readonly IPackageRepository _packageRepository;
        private readonly ILmsCourseService _courseService;
        private readonly IUserService _userService;
        private readonly ITrainingService _trainingService;
        private readonly AddExpiredDateForStudentPublisher _addExpiredDateForStudentPublisher;

        public CreateOrderByUserIdCommandHandler(IMapper mapper,
            IOrderRepository orderRepository,
            IEventRepository eventRepository,
            IMediator mediator,
            IPackageRepository packageRepository,
            ILmsCourseService courseService,
            IUserService userService,
            ITrainingService trainingService,
            AddExpiredDateForStudentPublisher addExpiredDateForStudentPublisher)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _eventRepository = eventRepository;
            _mediator = mediator;
            _packageRepository = packageRepository;
            _courseService = courseService;
            _userService = userService;
            _trainingService = trainingService;
            _addExpiredDateForStudentPublisher = addExpiredDateForStudentPublisher;
        }

        public async Task<MethodResult<OrderModel>> Handle(CreateOrderByUserIdCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OrderModel>();

            #region Validate Student

            var studentResult = await _userService.GetStudentByUserIdAsync(request.UserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            if (!student.CourseLevel.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            if (!request.CourseLevel.IsCheckCourseLevel(student.CourseLevel.Value))
            {
                methodResult.AddErrorBadRequest(nameof(EnumOrderErrorCode.YouChoseTheWrongLevel), nameof(request.CourseLevel));
                return methodResult;
            }

            if (string.IsNullOrEmpty(student.User?.FullName) || string.IsNullOrEmpty(student.User?.Email))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student.User.FullName), nameof(student.User.Email));
                return methodResult;
            }

            #endregion Validate Student

            var package = await _packageRepository.GetByIdAsync(request.PackageId ?? default);
            if (package == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(package));
                return methodResult;
            }

            var existsOrder = await _orderRepository.Queryable.OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync(x => x.UserId == request.UserId && x.Status == EnumOrderStatus.Payment, cancellationToken);
            var courseResult = await _courseService.GetCourseByIdAsync(request.CourseId);
            if (!courseResult.IsSuccessStatusCode)
            {
                methodResult.AddError(courseResult.Error);
                return methodResult;
            }
            var course = courseResult.Content?.Result;
            var newOrder = await _orderRepository.Queryable.FirstOrDefaultAsync(p => p.UserId == request.UserId && p.Status == EnumOrderStatus.New, cancellationToken);
            var isOrderEmpty = newOrder == null;
            string code = string.Empty;
            do
            {
                var codeSend = await _mediator.Send(new GenerateRandomOrderQuery() { StudentCode = student.User?.Code }, cancellationToken).ConfigureAwait(false);
                code = codeSend.Result ?? string.Empty;
            } while (await _orderRepository.Queryable.AnyAsync(x => x.Code == code, cancellationToken) && (newOrder == null || newOrder.Code != code));
            if (newOrder != null)
            {
                newOrder = _mapper.Map(request, newOrder);
            }
            else
            {
                newOrder = _mapper.Map<Order>(request);
            }
            newOrder.EventId = _eventRepository.Queryable.Include(x => x.PackageEvents).FirstOrDefault(x => x.PackageEvents.Any(y => y.PackageId == package.Id))?.Id;
            AddDataIntoOrder(newOrder, code, package.Price, existsOrder?.CourseId ?? course!.Id, student);
            if (!newOrder.IsValid())
            {
                methodResult.AddErrorBadRequest(newOrder.ErrorMessages);
                return methodResult;
            }

            if (request.IsTrialRegistration)
            {
                newOrder.IsTrial = request.IsTrialRegistration;
                newOrder.ExpireDate = DateTime.UtcNow.AddDays(ValueSettings.AmountTrialDays);
                newOrder.Status = EnumOrderStatus.Payment;
                newOrder.RevenueType = EnumPaymentRevenueType.NotRevenue;
                await _userService.CreateStudentTrialRegistration();
                await _addExpiredDateForStudentPublisher.Publish(new AddExpiredDateForStudentQueueModel()
                {
                    StudentId = student.Id,
                    ExpiredDate = newOrder.ExpireDate
                }, cancellationToken);
            }
            var numberOfShield = package.Code.HasValue ? (int)package.Code.Value : default;
            var addStudentIntoClassResult = await _trainingService.AddStudentIntoClass(new AddStudentIntoClassCommandModel() { UserId = request.UserId, CourseId = request.CourseId, PackageId = newOrder.PackageId ?? default, NumberOfShield = numberOfShield });
            if (!addStudentIntoClassResult.IsSuccessStatusCode)
            {
                methodResult.AddError(addStudentIntoClassResult.Error);
                return methodResult;
            }

            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                if (isOrderEmpty)
                {
                    newOrder = _orderRepository.Add(newOrder);
                }
                else
                {
                    newOrder = _orderRepository.Update(newOrder);
                }
                await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<OrderModel>(newOrder);
                return methodResult;
            });
            if (!request.IsTrialRegistration)
            {
                Thread.Sleep(3000);
                var changeStatusOrderResult = await _mediator.Send(new ChangeStatusOrderCommand
                {
                    OrderId = newOrder.Id,
                    OrderStatus = EnumOrderStatus.Payment,
                    Type = EnumOrderTransactionType.BankTransfer,
                    RevenueType = request.RevenueType,
                    IsSendEmail = request.IsSendEmail
                }, cancellationToken);

                if (!changeStatusOrderResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(changeStatusOrderResult.ErrorMessages);
                    return methodResult;
                }
            }

            return methodResult;
        }

        private static void AddDataIntoOrder(Order order, string? code, decimal price, Guid courseId, StudentModel student)
        {
            order.FullName = student.User?.FullName;
            order.PhoneNumber = student.User?.PhoneNumber;
            order.Email = student.User?.Email;
            order.Status = EnumOrderStatus.New;
            order.UserId = student?.UserId ?? default;
            order.Code = code;
            order.Price = price;
            order.DiscountPrice = (decimal)NumberHelper.ConvertDoublePercent(Convert.ToDouble(order.Price * order.DiscountPercent));
            order.TotalPrice = order.Price - order.DiscountPrice;
            order.CourseId = courseId;
        }
    }
}
