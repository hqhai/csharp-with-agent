// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CheckDonePTByStudentIdQuery : IRequest<MethodResult<EnumResultStatus>>
    {
        public Guid StudentId { get; set; }
    }

    public class CheckDonePTByStudentIdHandler : IRequestHandler<CheckDonePTByStudentIdQuery, MethodResult<EnumResultStatus>>
    {
        private readonly IRepository<TestGroupResult> _testGroupResult;

        public CheckDonePTByStudentIdHandler(IRepository<TestGroupResult> testGroupResult)
        {
            _testGroupResult = testGroupResult;
        }

        public async Task<MethodResult<EnumResultStatus>> Handle(CheckDonePTByStudentIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var testGroupResult = await _testGroupResult.Queryable.FirstOrDefaultAsync(x => x.StudentId == request.StudentId
                                                                                   && x.TestType == EnumTestType.PlacementTest,
                cancellationToken);

            return new MethodResult<EnumResultStatus> { StatusCode = StatusCodes.Status200OK, Result = testGroupResult?.Status ?? EnumResultStatus.NotStarted };
        }
    }
}
