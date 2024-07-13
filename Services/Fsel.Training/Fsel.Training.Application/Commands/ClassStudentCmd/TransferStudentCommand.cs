// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassStudentCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Application.Services.UserServices.Models;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class TransferStudentCommand : IRequest<MethodResult<bool>>
    {
        public Guid ClassId { get; set; }
        public Guid StudentId { get; set; }
        public Guid PackageId { get; set; }
    }

    public class TransferStudentCommandHandler : IRequestHandler<TransferStudentCommand, MethodResult<bool>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IClassStudentRepository _classStudentRepository;
        private readonly IUserService _userService;

        public TransferStudentCommandHandler(IClassRepository classRepository, IClassStudentRepository classStudentRepository, IUserService userService)
        {
            _classRepository = classRepository;
            _classStudentRepository = classStudentRepository;
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(TransferStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var classes = await _classRepository.GetByIdAsync(request.ClassId);
            if (classes == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classes));
                return methodResult;
            }
            var student = await _classStudentRepository.Queryable.FirstOrDefaultAsync(p => p.ClassId == request.ClassId && p.StudentId == request.StudentId, cancellationToken);
            if (student != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.StudentIsAlreadyInTheClass));
                return methodResult;
            }
            var countStudent = await _classStudentRepository.Queryable.Where(p => p.ClassId == request.ClassId).ToListAsync(cancellationToken);
            //if (countStudent.Count >= 12)
            //{
            //    methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassHasTooManyStudents));
            //    return methodResult;
            //}

            #region for pilot

            if (countStudent.Count >= 100)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassHasTooManyStudents));
                return methodResult;
            }

            #endregion for pilot

            var classStudent = await _classStudentRepository.Queryable.FirstOrDefaultAsync(p => p.StudentId == request.StudentId, cancellationToken);
            if (classStudent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.StudentNotExistInClass));
                return methodResult;
            }
            await _classStudentRepository.ExecuteTransactionAsync(async () =>
            {
                var studentResult = await _userService.UpdateStudentByClassAsync(new UpdateStudentByClassIdModel
                {
                    StudentId = request.StudentId,
                    ClassId = request.ClassId,
                    PackageId = request.PackageId,
                    CourseId = classes.CourseId
                });
                if (!studentResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(studentResult.Error);
                    return methodResult;
                }
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
