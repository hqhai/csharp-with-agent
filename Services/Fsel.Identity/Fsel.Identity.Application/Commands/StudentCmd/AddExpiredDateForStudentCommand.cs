// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Application.Commands.StudentEditHistoryCmd;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class AddExpiredDateForStudentCommand : AddExpiredDateForStudentCommandModel, IRequest<VoidMethodResult>
    {
    }

    public class AddExpiredDateForStudentCommandHandler : IRequestHandler<AddExpiredDateForStudentCommand, VoidMethodResult>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IMediator _mediator;

        public AddExpiredDateForStudentCommandHandler(IStudentRepository studentRepository, IMediator mediator)
        {
            _studentRepository = studentRepository;
            _mediator = mediator;
        }

        public async Task<VoidMethodResult> Handle(AddExpiredDateForStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new VoidMethodResult();

            var student = await _studentRepository.GetByIdAsync(request.StudentId);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            DateTime? oldExpiredDate = null;
            DateTime? newExpiredDate = null;

            await _studentRepository.ExecuteTransactionAsync(async () =>
            {
                oldExpiredDate = student.ExpiredDate;

                if (request.Month.HasValue || request.Day.HasValue)
                {
                    if (!student.ExpiredDate.HasValue || student.ExpiredDate.Value < currentDate)
                    {
                        if (request.Month.HasValue && request.Day.HasValue)
                        {
                            student.ExpiredDate = currentDate.AddMonths(request.Month.Value);
                            student.ExpiredDate = student.ExpiredDate.Value.AddDays(request.Day.Value);
                        }
                        else if (request.Month.HasValue)
                        {
                            student.ExpiredDate = currentDate.AddMonths(request.Month.Value);
                        }
                        else if (request.Day.HasValue)
                        {
                            student.ExpiredDate = currentDate.AddDays(request.Day.Value);
                        }
                    }
                    else
                    {
                        if (request.Month.HasValue)
                        {
                            student.ExpiredDate = student.ExpiredDate.Value.AddMonths(request.Month.Value);
                        }
                        if (request.Day.HasValue)
                        {
                            student.ExpiredDate = student.ExpiredDate.Value.AddDays(request.Day.Value);
                        }
                    }
                }
                else if (request.ExpiredDate.HasValue)
                {
                    if (!student.ExpiredDate.HasValue)
                    {
                        student.ExpiredDate = request.ExpiredDate.Value;
                    }
                    else if (student.ExpiredDate.HasValue && student.ExpiredDate.Value.Date < request.ExpiredDate.Value.Date)
                    {
                        student.ExpiredDate = request.ExpiredDate.Value;
                    }
                }

                newExpiredDate = student.ExpiredDate;

                student = _studentRepository.Update(student);
                await _studentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                await _mediator.Send(new CreateStudentEditHistoryCommand()
                {
                    Type = request.StudentEditHistoryType,
                    StudentId = student.Id,
                    Description = request.Description,
                    EditDetail = new StudentEditHistoryDetailModel()
                    {
                        OldExpiredDate = oldExpiredDate,
                        NewExpiredDate = newExpiredDate
                    }
                });

                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });
            return methodResult;
        }
    }
}
