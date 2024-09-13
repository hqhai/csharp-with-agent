// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Ordering.Application.Commands.OrderCmds.v1i1;
    using Fsel.Ordering.Application.Commands.OrderCmds.V1i2;
    using Fsel.Ordering.Application.Commands.UserRefferalCmd;
    using Fsel.Ordering.Application.Commands.VoucherCmds;
    using Fsel.Ordering.Application.Queues.Publishers;
    using Fsel.Ordering.Application.Services.CourseService;
    using Fsel.Ordering.Application.Services.SenderService;
    using Fsel.Ordering.Application.Services.SystemService;
    using Fsel.Ordering.Application.Services.SystemService.Models;
    using Fsel.Ordering.Application.Services.TrainingService;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Ordering.Domain.Enums.ErrorCodes;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders;
    using Fsel.Ordering.Infrastructure.Repositories;
    using Fsel.Ordering.Infrastructure.ValueSettings;
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
        private readonly ChangeStatusOrderPublisher _changeStatusOrderPublisher;
        private readonly AuthContext _authContext;
        private readonly ILmsCourseService _courseService;
        private readonly ISenderServices _senderServices;
        private readonly AppSetting _appSetting;
        private readonly AddExpiredDateForStudentPublisher _addExpiredDateForStudentPublisher;
        private readonly ISystemService _systemService;
        private readonly IMediator _mediator;
        private readonly IPackageEventRepository _packageEventRepository;
        private readonly IUserVoucherLockRepository _userVoucherLockRepository;

        public ChangeStatusOrderCommandHandler(IOrderRepository orderRepository
            , ITrainingService trainingService
            , IUserService userService
            , IPackageRepository packageRepository
            , ILmsCourseService lmsCourseService
            , NotificationMessagePublisher notificationMessagePublisher
            , AuthContext authContext
            , ILmsCourseService courseService,
ISenderServices senderServices,
AppSetting appSetting,
AddExpiredDateForStudentPublisher addExpiredDateForStudentPublisher,
ISystemService systemService,
IMediator mediator,
IPackageEventRepository packageEventRepository,
ChangeStatusOrderPublisher changeStatusOrderPublisher,
IUserVoucherLockRepository userVoucherLockRepository)
        {
            _orderRepository = orderRepository;
            _trainingService = trainingService;
            _userService = userService;
            _packageRepository = packageRepository;
            _lmsCourseService = lmsCourseService;
            _notificationMessagePublisher = notificationMessagePublisher;
            _authContext = authContext;
            _courseService = courseService;
            _senderServices = senderServices;
            _appSetting = appSetting;
            _changeStatusOrderPublisher = changeStatusOrderPublisher;
            _addExpiredDateForStudentPublisher = addExpiredDateForStudentPublisher;
            _systemService = systemService;
            _mediator = mediator;
            _packageEventRepository = packageEventRepository;
            _userVoucherLockRepository = userVoucherLockRepository;
        }

        public async Task<MethodResult<bool>> Handle(ChangeStatusOrderCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            bool allowOpenNextUnit = false;

            var order = await _orderRepository.Queryable.Include(ot => ot.OrderTransactions).FirstOrDefaultAsync(p => p.Id == request.OrderId, cancellationToken);
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

            var studentResult = await _userService.GetStudentByUserIdAsync(order.UserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult.Content?.Result;

            var package = await _packageRepository.GetByIdAsync(order.PackageId ?? default);
            if (package == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(package));
                return methodResult;
            }

            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                if (request.OrderStatus == EnumOrderStatus.Reject)
                {
                    //var classStudent = await _trainingService.DeleteStudentFromClass(order.UserId);
                    //if (!classStudent.IsSuccessStatusCode)
                    //{
                    //    methodResult.AddErrorBadRequest(nameof(EnumOrderErrorCode.UpdateNotSuccess));
                    //    return methodResult;
                    //}
                }
                else if (request.OrderStatus == EnumOrderStatus.Payment)
                {
                    order.RevenueType = request.RevenueType;

                    allowOpenNextUnit = true;

                    var packageEvent = await _packageEventRepository.Queryable.FirstOrDefaultAsync(p => p.PackageId == package.Id && p.EventId == order.EventId, cancellationToken);

                    if (packageEvent == null)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.EventNotExist), nameof(packageEvent));
                        return methodResult;
                    }

                    order.ExpireDate = DateTime.UtcNow.AddMonths(package.MonthNumber + packageEvent.MonthBonus);
                    order.ExpireDate = order.ExpireDate.Value.AddDays(packageEvent.DayBonus);

                    await _addExpiredDateForStudentPublisher.Publish(new AddExpiredDateForStudentQueueModel()
                    {
                        StudentId = student!.Id,
                        Month = package.MonthNumber + packageEvent.MonthBonus,
                        Day = packageEvent.DayBonus
                    }, cancellationToken);

                    if (order.IsInvoice)
                    {
                        await _systemService.AddPaymentInfoToGoogleSheet(new AddPaymentInfoToGoogleSheetModel()
                        {
                            Code = order.Code,
                            CreatedDate = order.CreatedDate.ToString("dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture),
                            Price = order.Price.ToString(CultureInfo.InvariantCulture),
                            FullName = order.FullName,
                            StudentEmail = student?.Human?.Email,
                            BillingEmail = order.Email,
                            CompanyTaxCode = order.CompanyTaxCode,
                            CompanyAddress = order.CompanyAddress,
                            CompanyName = order.CompanyName,
                            CompanyEmail = order.CompanyEmail,
                        });
                    }

                    var courseResults = await _lmsCourseService.GetCoursesByIdsAsync(new List<Guid> { student?.CourseId ?? order.CourseId ?? default });
                    if (!courseResults.IsSuccessStatusCode)
                    {
                        methodResult.AddError(courseResults.Error);
                        return methodResult;
                    }
                    var course = courseResults.Content?.Result?.FirstOrDefault();
                    if (course != null)
                    {
                        await _notificationMessagePublisher.Publish(new NotificationSendingQueueModel
                        {
                            UserIds = new List<Guid>() { order.UserId },
                            ObjectId = order.Id,
                            ParamsMessage = new List<object> { course.Name ?? string.Empty },
                            Type = EnumNotificationType.Text,
                            Content = EnumNotificationContent.OrderChangeStatus,
                            SenderId = _authContext.CurrentUserId,
                            PlatformCode = EnumPlatformCode.LMS
                        }, cancellationToken);
                    }
                }
                order.OrderTransactions.Add(new OrderTransaction()
                {
                    Status = request.OrderStatus == EnumOrderStatus.Payment ? EnumOrderTransactionStatus.Success : EnumOrderTransactionStatus.Fail,
                    ResponseBody = request.Receipt,
                    Type = request.Type ?? EnumOrderTransactionType.BankTransfer
                });

                order.Status = request.OrderStatus;
                order = _orderRepository.Update(order);
                await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                #region Gửi mail thanh toán

                if (order.Status == EnumOrderStatus.Payment)
                {
                    await _mediator.Send(new AddFeatureMissionCommand()
                    {
                        ReceiverId = order.UserId,
                        FeatureUserReferral = EnumFeatureUserReferral.Payment
                    }, cancellationToken).ConfigureAwait(false);

                    var createDate = order.CreatedDate.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
                    var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

                    Guid voucherId = default;

                    if (!order.VoucherId.HasValue && _appSetting.VoucherConfigs?.VoucherForRetail?.StartDate <= createDate && _appSetting.VoucherConfigs.VoucherForRetail.EndDate >= createDate && currentDate < _appSetting.VoucherConfigs.VoucherForRetail.ExpiredDate)
                    {
                        var voucher = await _mediator.Send(new CreateVoucherForRetailCommand()
                        {
                            UserId = order.UserId,
                            PackageId = order.PackageId ?? default,
                        }, cancellationToken).ConfigureAwait(false);
                        voucherId = voucher.Result?.Id ?? default;

                        await _mediator.Send(new SendMailPaymentWithVoucherCommand() { OrderId = order.Id, VoucherId = voucherId }).ConfigureAwait(false);
                    }
                    else
                    {
                        await _mediator.Send(new SendMailPaymentCommand() { OrderId = order.Id });
                    }

                    await ResetUserVoucherLockAsync(order.UserId, cancellationToken).ConfigureAwait(false);
                }

                #endregion Gửi mail thanh toán

                #region bắn socket thanh toán

                await _changeStatusOrderPublisher.Publish(new OrderQueueModel()
                {
                    OrderId = order.Id,
                    UserId = order.UserId,
                    Status = order.Status,
                }, cancellationToken);

                #endregion bắn socket thanh toán

                // Mở Unit tiếp theo.

                //var orders = await _orderRepository.Queryable.Where(p => p.ClassId == order.ClassId && p.Status == EnumOrderStatus.Payment).ToListAsync(cancellationToken);
                //if (orders.Count == 12)
                //{
                //    var activeClassResult = await _trainingService.ActiveClass(order.ClassId);
                //    if (!activeClassResult.IsSuccessStatusCode)
                //    {
                //        methodResult.AddError(activeClassResult.Error);
                //        return methodResult;
                //    }
                //}

                #region for pilot

                //if (orders.Count == 100)
                //{
                //    var activeClassResult = await _trainingService.ActiveClass(order.ClassId);
                //    if (!activeClassResult.IsSuccessStatusCode)
                //    {
                //        methodResult.AddError(activeClassResult.Error);
                //        return methodResult;
                //    }
                //}

                #endregion for pilot

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            if (allowOpenNextUnit)
            {
                var updateNextUnitResult = await _courseService.UpdateNextUnit(order.UserId);
                if (!updateNextUnitResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(updateNextUnitResult.Error);
                    return methodResult;
                }
            }
            return methodResult;
        }

        private bool IsInteger(double number)
        {
            return number == (int)number;
        }

        private async Task ResetUserVoucherLockAsync(Guid userId, CancellationToken cancellationToken)
        {
            var userVoucherLock = await _userVoucherLockRepository.Queryable.FirstOrDefaultAsync(p => p.CreatedUserId == userId, cancellationToken);
            if (userVoucherLock != null)
            {
                userVoucherLock.Count = 0;
                userVoucherLock.ExpiredDate = null;
                userVoucherLock.IsLockForever = false;
                _userVoucherLockRepository.Update(userVoucherLock);
                await _userVoucherLockRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
