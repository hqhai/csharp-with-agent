// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassStudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class TransferStudentCommand : IRequest<MethodResult<bool>>
    {
        public Guid ClassId { get; set; }
        public Guid StudentId { get; set; }
    }

    public class TransferStudentCommandHandler : IRequestHandler<TransferStudentCommand, MethodResult<bool>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IClassStudentRepository _classStudentRepository;

        public TransferStudentCommandHandler(IClassRepository classRepository, IClassStudentRepository classStudentRepository)
        {
            _classRepository = classRepository;
            _classStudentRepository = classStudentRepository;
        }

        public async Task<MethodResult<bool>> Handle(TransferStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var classes = await _classRepository.GetByIdAsync(request.ClassId);
            if (classes == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassesNotExits), nameof(request.ClassId), request.ClassId);
                return methodResult;
            }
            var student = await _classStudentRepository.Queryable.FirstOrDefaultAsync(p => p.ClassId == request.ClassId && p.StudentId == request.StudentId, cancellationToken);
            if (student != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.StudentIsAlreadyInTheClass));
                return methodResult;
            }
            var classStudents = await _classStudentRepository.Queryable.Where(p => p.ClassId == request.ClassId).ToListAsync(cancellationToken);
            if (classStudents.Count >= 12)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassHasTooManyStudents));
                return methodResult;
            }

            var classStudent = await _classStudentRepository.Queryable.FirstOrDefaultAsync(p => p.StudentId == request.StudentId, cancellationToken);
            if (classStudent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.StudentNotExistInClass));
                return methodResult;
            }
            await _classStudentRepository.ExecuteTransactionAsync(async () =>
            {
                classStudent.ClassId = request.ClassId;
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
