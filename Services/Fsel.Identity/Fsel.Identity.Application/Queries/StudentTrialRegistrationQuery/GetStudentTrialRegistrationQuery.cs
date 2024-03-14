// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentTrialRegistrationQuery

{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentTrialRegistrationQuery : IRequest<MethodResult<StudentTrialRegistration>>
    {
        public Guid UserId { get; set; }
    }

    public class GetStudentTrialRegistrationQueryHandler : IRequestHandler<GetStudentTrialRegistrationQuery, MethodResult<StudentTrialRegistration>>
    {
        private readonly IStudentTrialRegistrationRepository _studentTrialRegistrationRepository;

        public GetStudentTrialRegistrationQueryHandler(IStudentTrialRegistrationRepository studentTrialRegistrationRepository)
        {
            _studentTrialRegistrationRepository = studentTrialRegistrationRepository;
        }

        public async Task<MethodResult<StudentTrialRegistration>> Handle(GetStudentTrialRegistrationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<StudentTrialRegistration> methodResult = new MethodResult<StudentTrialRegistration>();
            var checkStudentRegistration = await _studentTrialRegistrationRepository.Queryable.FirstOrDefaultAsync(x => x.UserId == request.UserId && (x.Status == EnumTrialRegistrationStatus.Trial || x.Status == EnumTrialRegistrationStatus.Expired || x.Status == EnumTrialRegistrationStatus.Finished), cancellationToken);

            methodResult.Result = checkStudentRegistration;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
