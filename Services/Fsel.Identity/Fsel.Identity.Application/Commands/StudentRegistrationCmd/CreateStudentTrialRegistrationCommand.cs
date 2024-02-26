// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentRegistrationCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class CreateStudentTrialRegistrationCommand : IRequest<MethodResult<StudentTrialRegistration>>
    {
    }

    public class CreateStudentRegistrationCommandHandler : IRequestHandler<CreateStudentTrialRegistrationCommand, MethodResult<StudentTrialRegistration>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentTrialRegistrationRepository _studentTrialRegistrationRepository;
        private readonly AuthContext _authContext;

        public CreateStudentRegistrationCommandHandler(IMapper mapper, IStudentTrialRegistrationRepository studentTrialRegistrationRepository, AuthContext authContext)
        {
            _mapper = mapper;
            _studentTrialRegistrationRepository = studentTrialRegistrationRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<StudentTrialRegistration>> Handle(CreateStudentTrialRegistrationCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentTrialRegistration> methodResult = new MethodResult<StudentTrialRegistration>();

            StudentTrialRegistration studentTrialRegistration = new StudentTrialRegistration()
            {
                UserId = _authContext.CurrentUserId,
                Status = EnumTrialRegistrationStatus.Trial
            };

            var checkExistTrial = _studentTrialRegistrationRepository.Queryable.Any(x => x.UserId == _authContext.CurrentUserId);
            if (checkExistTrial)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist));
                return methodResult;
            }


            await _studentTrialRegistrationRepository.ExecuteTransactionAsync(async () =>
            {
                _studentTrialRegistrationRepository.Add(studentTrialRegistration);
                await _studentTrialRegistrationRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = studentTrialRegistration;
                return methodResult;
            });

            return methodResult;
        }
    }
}
