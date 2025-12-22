// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentTrialRegistrationQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Shared.Enums;

    public class CheckStudentTrialRegistrationQuery : IRequest<MethodResult<bool>>
    {
    }

    public class CheckStudentTrialRegistrationQueryHandler : IRequestHandler<CheckStudentTrialRegistrationQuery, MethodResult<bool>>
    {
        private readonly IStudentTrialRegistrationRepository _studentTrialRegistrationRepository;
        private readonly AuthContext _authContext;

        public CheckStudentTrialRegistrationQueryHandler(IStudentTrialRegistrationRepository studentTrialRegistrationRepository, AuthContext authContext)
        {
            _studentTrialRegistrationRepository = studentTrialRegistrationRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<bool>> Handle(CheckStudentTrialRegistrationQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var checkStudentRegistration = await _studentTrialRegistrationRepository.Queryable.AnyAsync(x => x.UserId == _authContext.CurrentUserId && (x.Status == EnumTrialRegistrationStatus.Trial || x.Status == EnumTrialRegistrationStatus.Expired || x.Status == EnumTrialRegistrationStatus.Finished), cancellationToken);

            methodResult.Result = checkStudentRegistration;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
