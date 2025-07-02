//// Copyright (c) Atlantic. All rights reserved.

//namespace Fsel.Ordering.Application.Commands.OrderCmds.v1i1
//{
//    using System.Threading;
//    using System.Threading.Tasks;
//    using AutoMapper;
//    using Fsel.Common.ActionResults;
//    using Fsel.Common.Enums;
//    using Fsel.Common.Enums.ErrorCodes;
//    using Fsel.Common.Models;
//    using Fsel.Core.Base;
//    using Fsel.Core.Base.BaseModels;
//    using Fsel.Ordering.Application.Queries.OrderQuery;
//    using Fsel.Ordering.Application.Queues.Publishers;
//    using Fsel.Ordering.Application.Services.CourseService;
//    using Fsel.Ordering.Application.Services.TrainingService;
//    using Fsel.Ordering.Application.Services.TrainingService.CommandModels;
//    using Fsel.Ordering.Application.Services.UserService;
//    using Fsel.Ordering.Application.Services.UserService.Models;
//    using Fsel.Ordering.Domain.Entities;
//    using Fsel.Ordering.Domain.Enums.ErrorCodes;
//    using Fsel.Ordering.Domain.IRepositories;
//    using Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i1;
//    using Fsel.Ordering.Domain.Models.EntityModels;
//    using Fsel.Shared.Constants;
//    using Fsel.Shared.Enums;
//    using Fsel.Shared.Helpers;
//    using Fsel.Shared.Models.ShareModels;
//    using MediatR;
//    using Microsoft.AspNetCore.Http;
//    using Microsoft.EntityFrameworkCore;

//    public class CreateOrderCommand : CreateOrderCommandModel, IRequest<MethodResult<OrderModel>>
//    {
//    }

//    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, MethodResult<OrderModel>>
//    {
//        private readonly IMapper _mapper;
//        private readonly IOrderRepository _orderRepository;
//        private readonly IMediator _mediator;
//        private readonly NotificationMessagePublisher _notificationMessagePublisher;
//        private readonly IPackageRepository _packageRepository;
//        private readonly AuthContext _authContext;
//        private readonly ILmsCourseService _courseService;
//        private readonly IUserService _userService;
//        private readonly ITrainingService _trainingService;

//        public CreateOrderCommandHandler(IMapper mapper, IOrderRepository orderRepository, IMediator mediator, NotificationMessagePublisher notificationMessagePublisher, IPackageRepository packageRepository, AuthContext authContext, ILmsCourseService courseService, IUserService userService, ITrainingService trainingService)
//        {
//            _mapper = mapper;
//            _orderRepository = orderRepository;
//            _mediator = mediator;
//            _notificationMessagePublisher = notificationMessagePublisher;
//            _packageRepository = packageRepository;
//            _authContext = authContext;
//            _courseService = courseService;
//            _userService = userService;
//            _trainingService = trainingService;
//        }

//        public async Task<MethodResult<OrderModel>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
//        {
//            ArgumentNullException.ThrowIfNull(request);
//            var methodResult = new MethodResult<OrderModel>();

//            if (await _orderRepository.Queryable.AnyAsync(x =>
//            x.Status == EnumOrderStatus.Payment &&
//            ((x.IsTrial && request.IsTrial) ||
//            (!x.IsTrial && x.ExpireDate > DateTime.UtcNow)) &&
//            x.UserId == _authContext.CurrentUserId, cancellationToken))
//            {
//                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.IsTrial));
//                return methodResult;
//            }

//            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
//            if (!studentResult.IsSuccessStatusCode)
//            {
//                methodResult.AddError(studentResult.Error);
//                return methodResult;
//            }
//            var student = studentResult.Content?.Result;

//            Guid? courseId = null;
//            if (request.CourseLevel == null)
//            {
//                var orderPayment = await _orderRepository.Queryable.Where(p => (p.Status == EnumOrderStatus.Payment || p.Status == EnumOrderStatus.New) && p.UserId == _authContext.CurrentUserId).OrderByDescending(p => p.CreatedDate).FirstOrDefaultAsync(cancellationToken);
//                courseId = orderPayment?.CourseId;

//                //request.CourseLevel = student?.CourseLevel;
//                var courseResult = await _courseService.GetCourseByLevel(new BaseQueryModel()
//                {
//                    Filters = new List<GenericFilterModel>()
//                    {
//                        new GenericFilterModel()
//                        {
//                            Property = "Id",
//                            Operator = EnumFilterOperator.Equal,
//                            Value = courseId
//                        }
//                    }
//                });

//                if (!courseResult.IsSuccessStatusCode)
//                {
//                    methodResult.AddError(courseResult.Error);
//                    return methodResult;
//                }
//                request.CourseLevel = courseResult?.Content?.Result?.CourseLevel;
//            }
//            else
//            {
//                //request.CourseLevel = student?.CourseLevel;
//                var courseResult = await _courseService.GetCourseByLevel(new BaseQueryModel()
//                {
//                    Filters = new List<GenericFilterModel>()
//                    {
//                        new GenericFilterModel()
//                        {
//                            Property = "Status",
//                            Operator = EnumFilterOperator.Equal,
//                            Value = "Active"
//                        },
//                        new GenericFilterModel()
//                        {
//                            Property = "CourseLevel",
//                            Operator = EnumFilterOperator.Equal,
//                            Value = request.CourseLevel.ToString()
//                        }
//                    }
//                });

//                if (!courseResult.IsSuccessStatusCode)
//                {
//                    methodResult.AddError(courseResult.Error);
//                    return methodResult;
//                }
//                var course = courseResult.Content?.Result;
//                courseId = course?.Id;
//            }
//            if (!courseId.HasValue)
//            {
//                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseId));
//                return methodResult;
//            }

//            if (request.CourseLevel.HasValue && !request.CourseLevel.Value.IsCheckCourseLevel(student?.CourseLevel ?? default))
//            {
//                methodResult.AddErrorBadRequest(nameof(EnumOrderErrorCode.YouChoseTheWrongLevel), nameof(request.CourseLevel));
//                return methodResult;
//            }

//            if (string.IsNullOrEmpty(student?.User?.FullName) || string.IsNullOrEmpty(student?.User?.Email))
//            {
//                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student.User.FullName), nameof(student.User.Email));
//                return methodResult;
//            }

//            Package? package;
//            if (!request.IsTrial)
//            {
//                package = await _packageRepository.GetByIdAsync(request.PackageId ?? default);
//            }
//            else
//            {
//                package = await _packageRepository.Queryable.FirstOrDefaultAsync(cancellationToken);
//            }
//            if (package == null)
//            {
//                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(package));
//                return methodResult;
//            }

//            var codeSend = await _mediator.Send(new GenerateRandomOrderQuery { StudentCode = student.User.Code }, cancellationToken).ConfigureAwait(false);

//            string code = codeSend.Result ?? string.Empty;

//            if (await _orderRepository.Queryable.AnyAsync(x => x.Code == code, cancellationToken))
//            {
//                methodResult.AddErrorBadRequest(nameof(EnumOrderErrorCode.TryAgainInOneMinute), nameof(code));
//                return methodResult;
//            }

//            var order = await _orderRepository.Queryable.FirstOrDefaultAsync(p => p.UserId == _authContext.CurrentUserId && p.Status == EnumOrderStatus.New && !p.IsTrial, cancellationToken);

//            if (order != null)
//            {
//                var updateOrderResult = await _mediator.Send(new UpdateOrderCommand()
//                {
//                    Order = order,
//                    Package = package,
//                    Code = code,
//                    FullName = student?.User?.FullName,
//                    PhoneNumber = request.PhoneNumber,
//                    Email = student?.User?.Email,
//                    Address = request.Address,
//                    PaymentMethod = request.PaymentMethod,
//                    ProvinceId = request.ProvinceId,
//                    DistrictId = request.DistrictId,
//                }, cancellationToken).ConfigureAwait(false);

//                if (!updateOrderResult.IsOK)
//                {
//                    methodResult.AddError(updateOrderResult.ErrorMessages);
//                    return methodResult;
//                }

//                methodResult.Result = updateOrderResult.Result;
//                return methodResult;
//            }

//            Order newOrder = _mapper.Map<Order>(request);

//            AddDataIntoOrder(newOrder, code, package.Price, courseId ?? default, student);

//            if (!newOrder.IsValid())
//            {
//                methodResult.AddErrorBadRequest(newOrder.ErrorMessages);
//                return methodResult;
//            }

//            if (request.IsTrial)
//            {
//                newOrder.IsTrial = request.IsTrial;
//                newOrder.ExpireDate = DateTime.UtcNow.AddDays(ValueSettings.AmountTrialDays);
//                newOrder.Status = EnumOrderStatus.Payment;
//                newOrder.Price = 0;
//                newOrder.DiscountPercent = 0;
//                newOrder.DiscountPrice = 0;
//                newOrder.TotalPrice = 0;
//                newOrder.UserId = _authContext.CurrentUserId;

//                await _userService.CreateStudentTrialRegistration();
//                var numberOfShield = package.Code.HasValue ? (int)package.Code.Value : default;
//                var addStudentIntoClassResult = await _trainingService.AddStudentIntoClass(new AddStudentIntoClassCommandModel() { UserId = _authContext.CurrentUserId, CourseId = newOrder.CourseId ?? default, PackageId = newOrder.PackageId ?? default, NumberOfShield = numberOfShield });
//                if (!addStudentIntoClassResult.IsSuccessStatusCode)
//                {
//                    methodResult.AddError(addStudentIntoClassResult.Error);
//                    return methodResult;
//                }
//            }

//            await _orderRepository.ExecuteTransactionAsync(async () =>
//            {
//                newOrder = _orderRepository.Add(newOrder);
//                await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
//                SendNotify(newOrder.Id, newOrder.UserId, cancellationToken);
//                methodResult.StatusCode = StatusCodes.Status201Created;
//                methodResult.Result = _mapper.Map<OrderModel>(newOrder);
//                return methodResult;
//            });
//            return methodResult;
//        }

//        private void AddDataIntoOrder(Order order, string? code, decimal price, Guid courseId, StudentModel student)
//        {
//            order.FullName = student.User?.FullName;
//            order.Email = student.User?.Email;
//            order.Status = EnumOrderStatus.New;
//            order.UserId = _authContext.CurrentUserId;
//            order.Code = code;
//            order.Price = price;
//            order.DiscountPercent = 0;
//            order.DiscountPrice = (decimal)NumberHelper.ConvertDoublePercent(Convert.ToDouble(order.Price * order.DiscountPercent));
//            order.TotalPrice = order.Price - order.DiscountPrice;
//            order.CourseId = courseId;
//        }

//        private async void SendNotify(Guid orderId, Guid senderId, CancellationToken cancellationToken)
//        {
//            await _notificationMessagePublisher.Publish(new NotificationSendingQueueModel
//            {
//                Roles = new List<EnumRole> { EnumRole.Admin },
//                ObjectId = orderId,
//                Type = EnumNotificationType.Text,
//                Content = EnumNotificationContent.OrderCreate,
//                SenderId = senderId,
//                PlatformCode = EnumPlatformCode.LMSAdmin
//            }, cancellationToken);
//        }
//    }
//}
