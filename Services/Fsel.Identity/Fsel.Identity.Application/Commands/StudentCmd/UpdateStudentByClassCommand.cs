// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Identity.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

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

            var student = await _studentRepository.Queryable.Include(x => x.Human)
                                                  .FirstOrDefaultAsync(x => x.Id == request.StudentId, cancellationToken: cancellationToken);

            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumStudentErrorCode.StudentNull), nameof(request.StudentId), request.StudentId);
                return methodResult;
            }
            student.ClassId = request.ClassId;
            student.PackageId = request.PackageId;
            await _studentRepository.ExecuteTransactionAsync(async () =>
            {
                student = _studentRepository.Update(student);
                await _studentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<StudentModel>(student);
                return methodResult;
            });
            return methodResult;
        }
    }
}
