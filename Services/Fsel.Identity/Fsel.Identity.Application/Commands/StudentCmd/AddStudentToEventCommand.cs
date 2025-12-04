// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class AddStudentToEventCommand : IRequest<MethodResult<bool>>
    {
        public Guid StudentId { get; set; }
        public string? EventCode { get; set; }
    }

    public class AddStudentToEventCommandHandler : IRequestHandler<AddStudentToEventCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly IOrderService _orderService;

        public AddStudentToEventCommandHandler(IStudentRepository studentRepository,
                                               ICompetitionEventsRepository competitionEventsRepository,
                                               IStudentCompetitionEventsRepository studentCompetitionEventsRepository,
                                               IOrderService orderService)
        {
            _studentRepository = studentRepository;
            _competitionEventsRepository = competitionEventsRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _orderService = orderService;
        }

        public async Task<MethodResult<bool>> Handle(AddStudentToEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.EventCode);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var competitionEvent = await _competitionEventsRepository.Queryable.FirstOrDefaultAsync(x => x.EventCode != null && x.EventCode.Trim() == request.EventCode.Trim(), cancellationToken);
            if (competitionEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumEventErrorCode.EventNotExist), nameof(competitionEvent));
                return methodResult;
            }

            var checkStudentCompetitionEvent = await _studentCompetitionEventsRepository.Queryable.AnyAsync(x => x.StudentId == request.StudentId, cancellationToken);
            if (checkStudentCompetitionEvent)
            {
                methodResult.AddErrorBadRequest(nameof(EnumStudentErrorCode.StudentAlreadyAnotherEvent), nameof(checkStudentCompetitionEvent));
                return methodResult;
            }

            var student = await _studentRepository.Queryable
                                                  .Include(x => x.User)
                                                  .FirstOrDefaultAsync(x => x.Id == request.StudentId, cancellationToken);
            if (student == null || student.User == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var paymentDate = competitionEvent.EventContent?.PaymentDate;
            if (paymentDate == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(paymentDate));
                return methodResult;
            }

            var addOrder = await _orderService.CreateOrderEventByStudent(new CreateOrderForStudentEventCommandModel
            {
                ExpiredDate = paymentDate.Value,
                Student = new CreateOrderForStudentsEventCommandModel
                {
                    Email = student.User.Email,
                    FullName = student.User.FullName,
                    PhoneNumber = student.User.PhoneNumber,
                    StudentCode = student.User.Code,
                    StudentId = student.Id,
                    UserId = student.UserId
                }
            });
            if (!addOrder.IsSuccessStatusCode)
            {
                methodResult.AddError(addOrder.Error);
                return methodResult;
            }

            var newStudentCompetitionEvent = new StudentCompetitionEvent
            {
                StudentId = request.StudentId,
                CompetitionEventId = competitionEvent.Id
            };

            await _studentCompetitionEventsRepository.ExecuteTransactionAsync(async () =>
            {
                _studentCompetitionEventsRepository.Add(newStudentCompetitionEvent);
                await _studentCompetitionEventsRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            return methodResult;
        }
    }
}
