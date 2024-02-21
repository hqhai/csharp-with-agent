// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentRegistrationCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.StudentTrialRegistration;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateStudentTrialRegistrationCommand : StudentTrialRegistrationCommandModel, IRequest<MethodResult<StudentTrialRegistration>>
    {
    }

    public class CreateStudentRegistrationCommandHandler : IRequestHandler<CreateStudentTrialRegistrationCommand, MethodResult<StudentTrialRegistration>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentTrialRegistrationRepository _studentTrialRegistrationRepository;

        public CreateStudentRegistrationCommandHandler(IMapper mapper, IStudentTrialRegistrationRepository studentTrialRegistrationRepository)
        {
            _mapper = mapper;
            _studentTrialRegistrationRepository = studentTrialRegistrationRepository;
        }

        public async Task<MethodResult<StudentTrialRegistration>> Handle(CreateStudentTrialRegistrationCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentTrialRegistration> methodResult = new MethodResult<StudentTrialRegistration>();

            StudentTrialRegistration studentTrialRegistration = new StudentTrialRegistration();

            studentTrialRegistration = _mapper.Map<StudentTrialRegistration>(request);


            await _studentTrialRegistrationRepository.ExecuteTransactionAsync(async () =>
            {
                _studentTrialRegistrationRepository.Add(studentTrialRegistration);
                await _studentTrialRegistrationRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<StudentTrialRegistration>(_studentTrialRegistrationRepository);
                return methodResult;
            });

            return methodResult;
        }
    }
}
