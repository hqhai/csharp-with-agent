// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Commands.OrderCmds.V1i2
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Ordering.Application.Queries.OrderQuery;
    using Fsel.Ordering.Application.Queues.Publishers;
    using Fsel.Ordering.Application.Services.UserService;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Ordering.Domain.IRepositories;
    using Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i2;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateOrderForUserFromLeaderBoardCommand : CreateOrderForUserFromLeaderBoardCommandModel, IRequest<MethodResult<VoidMethodResult>>
    {
    }

    public class CreateOrderForUserFromLeaderBoardCommandHandler : IRequestHandler<CreateOrderForUserFromLeaderBoardCommand, MethodResult<VoidMethodResult>>
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly IMediator _mediator;
        private readonly IPackageRepository _packageRepository;
        private readonly IUserService _userService;
        private readonly IEventRepository _eventRepository;
        private readonly AddExpiredDateForStudentPublisher _addExpiredDateForStudentPublisher;

        public CreateOrderForUserFromLeaderBoardCommandHandler(IMapper mapper, IOrderRepository orderRepository, IMediator mediator, IPackageRepository packageRepository, IUserService userService, IEventRepository eventRepository, AddExpiredDateForStudentPublisher addExpiredDateForStudentPublisher)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _mediator = mediator;
            _packageRepository = packageRepository;
            _userService = userService;
            _eventRepository = eventRepository;
            _addExpiredDateForStudentPublisher = addExpiredDateForStudentPublisher;
        }

        public async Task<MethodResult<VoidMethodResult>> Handle(CreateOrderForUserFromLeaderBoardCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<VoidMethodResult>();

            var studentResult = await _userService.GetStudentByUserIdAsync(request.UserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }
            var student = studentResult.Content?.Result;

            if (await _orderRepository.Queryable.AnyAsync(p => !p.IsTrial && p.Status == EnumOrderStatus.Payment && p.UserId == request.UserId, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return methodResult;
            }

            if (string.IsNullOrEmpty(request.FullName) || string.IsNullOrEmpty(request.Email))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            if (!string.IsNullOrEmpty(request.PhoneNumber) && !request.PhoneNumber.IsValidPhoneNumber() || !request.Email.IsValidEmail())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            if (request.IsInvoice && (string.IsNullOrEmpty(request.CompanyName) || string.IsNullOrEmpty(request.CompanyAddress) || string.IsNullOrEmpty(request.CompanyTaxCode)))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            Package? package = null;

            if (request.Month.HasValue)
            {
                package = await _packageRepository.Queryable.FirstOrDefaultAsync(p => p.MonthNumber == request.Month.Value, cancellationToken);
            }
            else
            {
                package = await _packageRepository.Queryable.OrderByDescending(p => p.MonthNumber).FirstOrDefaultAsync(cancellationToken);
            }

            if (package == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(package));
                return methodResult;
            }

            var @event = await _eventRepository.Queryable.FirstOrDefaultAsync(p => p.Status == EnumEventPackageStatus.Active, cancellationToken);
            if (@event == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.EventNotExist), nameof(@event));
                return methodResult;
            }

            if (!@event.IsDefault)
            {
                var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
                if (@event.StartDate?.Date > currentDate.Date || @event.EndDate?.Date < currentDate.Date)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.EventHasExpired), nameof(@event));
                    return methodResult;
                }
            }

            var codeSend = await _mediator.Send(new GenerateRandomOrderQuery() { StudentCode = student?.User?.Code }, cancellationToken).ConfigureAwait(false);
            string code = codeSend.Result ?? string.Empty;

            if (string.IsNullOrEmpty(code) || await _orderRepository.Queryable.AnyAsync(x => x.Code == code, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(code));
                return methodResult;
            }

            var newOrder = _mapper.Map<Order>(request);
            newOrder.PackageId = package.Id;
            newOrder.EventId = @event.Id;
            newOrder.Price = package.Price;
            newOrder.Code = code;
            newOrder.DiscountPercent = 0;
            newOrder.DiscountPrice = (decimal)NumberHelper.ConvertDoublePercent(Convert.ToDouble(newOrder.Price * newOrder.DiscountPercent));
            newOrder.TotalPrice = newOrder.Price - newOrder.DiscountPrice;
            newOrder.UserId = request.UserId;
            newOrder.Status = EnumOrderStatus.Payment;
            newOrder.UpdatedDate = DateTime.UtcNow;
            newOrder.ExpireDate = DateTime.UtcNow.AddMonths(package.MonthNumber);
            newOrder.RevenueType = null;
            newOrder.OrderTransactions.Add(new OrderTransaction()
            {
                Status = EnumOrderTransactionStatus.Success,
                RequestBody = request,
                Type = EnumOrderTransactionType.BankTransfer
            });
            if (!newOrder.IsValid())
            {
                methodResult.AddErrorBadRequest(newOrder.ErrorMessages);
                return methodResult;
            }

            await _orderRepository.ExecuteTransactionAsync(async () =>
            {
                newOrder = _orderRepository.Add(newOrder);
                await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            await _addExpiredDateForStudentPublisher.Publish(new AddExpiredDateForStudentQueueModel()
            {
                StudentId = student!.Id,
                Month = request.ExpiredDate.HasValue ? null : package.MonthNumber,
                Day = request.ExpiredDate.HasValue ? null : 0,
                ExpiredDate = request.ExpiredDate.HasValue ? request.ExpiredDate.Value : DateTime.UtcNow,
            }, cancellationToken);

            return methodResult;
        }
    }
}
