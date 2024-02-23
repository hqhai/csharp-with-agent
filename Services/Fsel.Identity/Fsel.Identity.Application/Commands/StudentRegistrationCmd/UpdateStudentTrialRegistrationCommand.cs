// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentRegistrationCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.StudentTrialRegistration;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStudentTrialRegistrationCommand : StudentTrialRegistrationCommandModel, IRequest<MethodResult<StudentTrialRegistration>>
    {
    }

    public class UpdateStudentTrialRegistrationCommandHandler : IRequestHandler<UpdateStudentTrialRegistrationCommand, MethodResult<StudentTrialRegistration>>
    {
        private readonly IMapper _mapper;
        private readonly IStudentTrialRegistrationRepository _studentTrialRegistrationRepository;

        public UpdateStudentTrialRegistrationCommandHandler(IMapper mapper, IStudentTrialRegistrationRepository studentTrialRegistrationRepository)
        {
            _mapper = mapper;
            _studentTrialRegistrationRepository = studentTrialRegistrationRepository;
        }

        public async Task<MethodResult<StudentTrialRegistration>> Handle(UpdateStudentTrialRegistrationCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentTrialRegistration> methodResult = new MethodResult<StudentTrialRegistration>();


            var studentTrialRegistrationResult = await _studentTrialRegistrationRepository.Queryable.FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

            if (studentTrialRegistrationResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            _mapper.Map(request, studentTrialRegistrationResult);


            await _studentTrialRegistrationRepository.ExecuteTransactionAsync(async () =>
            {
                _studentTrialRegistrationRepository.Update(studentTrialRegistrationResult);
                await _studentTrialRegistrationRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = studentTrialRegistrationResult;
                return methodResult;
            });

            return methodResult;
        }
    }
}
