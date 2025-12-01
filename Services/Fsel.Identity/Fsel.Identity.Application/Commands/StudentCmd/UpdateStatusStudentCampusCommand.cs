// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using AutoMapper;
    using Common.ActionResults;
    using Common.Enums.ErrorCodes;
    using Domain.IRepositories;
    using Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Shared.Enums;

    public class UpdateStatusStudentCampusCommand : IRequest<MethodResult<StudentModel>>
    {
        public Guid StudentId { get; set; }
        public EnumStatusStudentCampus StatusStudentGoal { get; set; }
    }

    public class UpdateStatusStudentCampusCommandHandler : IRequestHandler<UpdateStatusStudentCampusCommand, MethodResult<StudentModel>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;

        public UpdateStatusStudentCampusCommandHandler(IStudentRepository studentRepository, IMapper mapper)
        {
            _studentRepository = studentRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<StudentModel>> Handle(UpdateStatusStudentCampusCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentModel>();

            var student = await _studentRepository.GetByIdAsync(request.StudentId);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            student.StatusStudentCampus = request.StatusStudentGoal;

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
