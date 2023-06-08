// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassStudentCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteStudentFromClassCommand : IRequest<MethodResult<bool>>
    {
        public Guid StudentId { get; set; }
    }

    public class DeleteStudentFromClassCommandHandler : IRequestHandler<DeleteStudentFromClassCommand, MethodResult<bool>>
    {
        private readonly IClassStudentRepository _classStudentRepository;
        private readonly IUserService _userService;
        public DeleteStudentFromClassCommandHandler(IClassStudentRepository classStudentRepository, IUserService userService)
        {
            _classStudentRepository = classStudentRepository;
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(DeleteStudentFromClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var classStudent = await _classStudentRepository.Queryable.FirstOrDefaultAsync(p => p.StudentId == request.StudentId, cancellationToken);
            if (classStudent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassStudentNotExist));
                return methodResult;
            }
            await _classStudentRepository.ExecuteTransactionAsync(async () =>
            {
                var student = await _userService.DeleteStudentFromClass(request.StudentId);
                if (!student.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.DeleteClassInStudentNotSuccess));
                    return methodResult;
                }
                await _classStudentRepository.DeleteAsync(classStudent);
                await _classStudentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;

        }
    }
}
