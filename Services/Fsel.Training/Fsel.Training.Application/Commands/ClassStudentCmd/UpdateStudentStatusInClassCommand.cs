// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassStudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStudentStatusInClassCommand : BaseCommandModel, IRequest<MethodResult<bool>>
    {
    }
    public class UpdateStudentStatusInClassCommandHandler : IRequestHandler<UpdateStudentStatusInClassCommand, MethodResult<bool>>
    {
        private readonly IClassStudentRepository _classStudentRepository;

        public UpdateStudentStatusInClassCommandHandler(IClassStudentRepository classStudentRepository)
        {
            _classStudentRepository = classStudentRepository;
        }

        public async Task<MethodResult<bool>> Handle(UpdateStudentStatusInClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var classStudent = await _classStudentRepository.Queryable.FirstOrDefaultAsync(p => p.StudentId == request.Id, cancellationToken);
            if (classStudent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.StudentNotExistInClass));
                return methodResult;
            }
            await _classStudentRepository.ExecuteTransactionAsync(async () =>
            {
                classStudent.IsActive = true;
                _classStudentRepository.Update(classStudent);
                await _classStudentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
