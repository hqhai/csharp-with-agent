// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentTrialRegistrationQuery

{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CheckStudentTrialRegistrationQuery : IRequest<MethodResult<bool>>
    {
        public Guid UserId { get; set; }
    }

    public class CheckStudentTrialRegistrationQueryHandler : IRequestHandler<CheckStudentTrialRegistrationQuery, MethodResult<bool>>
    {
        private readonly IStudentTrialRegistrationRepository _studentTrialRegistrationRepository;

        public CheckStudentTrialRegistrationQueryHandler(IStudentTrialRegistrationRepository studentTrialRegistrationRepository)
        {
            _studentTrialRegistrationRepository = studentTrialRegistrationRepository;
        }

        public async Task<MethodResult<bool>> Handle(CheckStudentTrialRegistrationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var checkStudentRegistration = await _studentTrialRegistrationRepository.Queryable.AnyAsync(x => x.UserId == request.UserId, cancellationToken);

            methodResult.Result = checkStudentRegistration;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
