// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteGuestStudentByIdCommand : IRequest<VoidMethodResult>
    {
        public Guid? Id { get; set; }
    }

    public class DeleteStudentByIdCommandHandler : IRequestHandler<DeleteGuestStudentByIdCommand, VoidMethodResult>
    {
        private readonly IStudentRepository _studentRepository;

        public DeleteStudentByIdCommandHandler(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<VoidMethodResult> Handle(DeleteGuestStudentByIdCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();

            var student = await _studentRepository.Queryable.Where(x => x.Id == request.Id).FirstOrDefaultAsync(cancellationToken);

            await _studentRepository.DeleteAsync(student!);
            await _studentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
