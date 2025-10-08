// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.CampusCmd.Classes
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class DeleteSchoolClassCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteSchoolClassCommandHandler : IRequestHandler<DeleteSchoolClassCommand, MethodResult<bool>>
    {
        private readonly ISchoolClassRepository _schoolClassRepository;
        private readonly IMediator _mediator;
        private readonly IStudentRepository _studentRepository;

        public DeleteSchoolClassCommandHandler(ISchoolClassRepository schoolClassRepository, IMediator mediator, IStudentRepository studentRepository)
        {
            _schoolClassRepository = schoolClassRepository;
            _mediator = mediator;
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<bool>> Handle(DeleteSchoolClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var schoolClass = await _schoolClassRepository.GetByIdAsync(request.Id);
            if (schoolClass == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(schoolClass), request.Id);
                return methodResult;
            }

            var studentIds = await _studentRepository.Queryable.Where(p => p.SchoolClassId == schoolClass.Id).Select(x => x.Id).ToListAsync(cancellationToken);

            await _schoolClassRepository.ExecuteTransactionAsync(async () =>
            {
                var deleteResult = await _mediator.Send(new DeleteStudentsInClassCommand() { StudentIds = studentIds, SchoolClassId = schoolClass.Id }, cancellationToken);
                if (!deleteResult.IsOK)
                {
                    methodResult.AddError(deleteResult.ErrorMessages);
                    return methodResult;
                }

                await _schoolClassRepository.DeleteAsync(schoolClass);
                await _schoolClassRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
