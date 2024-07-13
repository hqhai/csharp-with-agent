// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class AddExpiredDateForStudentCommand : AddExpiredDateForStudentCommandModel, IRequest<VoidMethodResult>
    {
    }

    public class AddExpiredDateForStudentCommandHandler : IRequestHandler<AddExpiredDateForStudentCommand, VoidMethodResult>
    {
        private readonly IStudentRepository _studentRepository;

        public AddExpiredDateForStudentCommandHandler(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
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
            await _studentRepository.ExecuteTransactionAsync(async () =>
            {
                if (!student.ExpiredDate.HasValue || student.ExpiredDate.Value.Date < DateTime.UtcNow.Date)
                {
                    if (request.Month.HasValue && request.Day.HasValue)
                    {
                        student.ExpiredDate = DateTime.UtcNow.AddMonths(request.Month.Value);
                        student.ExpiredDate = student.ExpiredDate.Value.AddDays(request.Day.Value);
                    }
                    else if (request.Month.HasValue)
                    {
                        student.ExpiredDate = DateTime.UtcNow.AddMonths(request.Month.Value);
                    }
                    else if (request.Day.HasValue)
                    {
                        student.ExpiredDate = DateTime.UtcNow.AddDays(request.Day.Value);
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
                student = _studentRepository.Update(student);
                await _studentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });
            return methodResult;
        }
    }
}
