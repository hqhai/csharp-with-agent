// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteStudentFromClassCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteStudentFromClassCommandHandler : IRequestHandler<DeleteStudentFromClassCommand, MethodResult<bool>>
    {
        private readonly IHumanRepository _humanRepository;
        private readonly IStudentRepository _studentRepository;

        public DeleteStudentFromClassCommandHandler(IHumanRepository humanRepository, IStudentRepository studentRepository)
        {
            _humanRepository = humanRepository;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteStudentFromClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var student = await _studentRepository.GetByIdAsync(request.Id);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHumanErrorCode.StudentNotExist));
                return methodResult;
            }
            await _studentRepository.ExecuteTransactionAsync(async () =>
            {
                student.ClassId = null;
                student = _studentRepository.Update(student);
                await _studentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
