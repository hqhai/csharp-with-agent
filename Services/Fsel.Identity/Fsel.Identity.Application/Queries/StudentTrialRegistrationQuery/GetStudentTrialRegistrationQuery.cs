// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentTrialRegistrationQuery

{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentTrialRegistrationQuery : IRequest<MethodResult<bool>>
    {
        public Guid StudentId { get; set; }
    }

    public class GetStudentTrialRegistrationQueryHandler : IRequestHandler<GetStudentTrialRegistrationQuery, MethodResult<bool>>
    {
        private readonly IStudentTrialRegistrationRepository _studentTrialRegistrationRepository;

        public GetStudentTrialRegistrationQueryHandler(IStudentTrialRegistrationRepository studentTrialRegistrationRepository)
        {
            _studentTrialRegistrationRepository = studentTrialRegistrationRepository;
        }

        public async Task<MethodResult<bool>> Handle(GetStudentTrialRegistrationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var checkStudentRegistration = await _studentTrialRegistrationRepository.Queryable.AnyAsync(x => x.StudentId == request.StudentId && x.Status == EnumTrialRegistrationStatus.Trial, cancellationToken);

            methodResult.Result = checkStudentRegistration;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
