// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class UpdateStudentByClassCommand : UpdateStudentByClassCommandModel, IRequest<MethodResult<StudentModel>>
    {
    }

    public class UpdateStudentByClassCommandHandler : IRequestHandler<UpdateStudentByClassCommand, MethodResult<StudentModel>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;

        public UpdateStudentByClassCommandHandler(
            IStudentRepository studentRepository,
            IMapper mapper)
        {
            _studentRepository = studentRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<StudentModel>> Handle(UpdateStudentByClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentModel> methodResult = new MethodResult<StudentModel>();

            var student = await _studentRepository.GetByIdAsync(request.StudentId);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            student.CourseLevel = request.CourseLevel;
            student.ClassId = request.ClassId;
            student.CourseId = request.CourseId;
            if (request.PackageId.HasValue)
            {
                student.PackageId = request.PackageId.Value;
            }
            if (request.NumberOfShield.HasValue)
            {
                student.NumberOfShield += request.NumberOfShield.Value;
            }
            await _studentRepository.ExecuteTransactionAsync(async () =>
            {
                student = _studentRepository.Update(student);
                await _studentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<StudentModel>(student);
                return methodResult;
            });
            return methodResult;
        }
    }
}
