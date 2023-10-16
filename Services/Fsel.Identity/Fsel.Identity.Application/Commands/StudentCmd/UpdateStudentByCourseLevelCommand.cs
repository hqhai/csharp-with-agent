// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStudentByCourseLevelCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
        public EnumCourseLevel Level { get; set; }
    }

    public class UpdateStudentByCourseLevelCommandHandler : IRequestHandler<UpdateStudentByCourseLevelCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;

        public UpdateStudentByCourseLevelCommandHandler(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<bool>> Handle(UpdateStudentByCourseLevelCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var student = await _studentRepository.Queryable.Include(x => x.Human)
                                                  .FirstOrDefaultAsync(x => x.Human!.UserId == request.Id, cancellationToken: cancellationToken);

            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            student.CourseLevel = request.Level;

            await _studentRepository.ExecuteTransactionAsync(async () =>
            {
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
