// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestQuery
{
    using System;
    using System.Threading.Tasks;
    using Domain.Models.EntityModels.PlacementTestModels;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CheckDonePtByStudentIdQuery : IRequest<MethodResult<PtStateModel>>
    {
        public Guid StudentId { get; set; }
    }

    public class CheckDonePtByStudentIdHandler : IRequestHandler<CheckDonePtByStudentIdQuery, MethodResult<PtStateModel>>
    {
        private readonly IRepository<TestGroupResult> _testGroupResult;

        public CheckDonePtByStudentIdHandler(IRepository<TestGroupResult> testGroupResult)
        {
            _testGroupResult = testGroupResult;
        }

        public async Task<MethodResult<PtStateModel>> Handle(CheckDonePtByStudentIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var testGroupResult = await _testGroupResult.Queryable
                .Include(x => x.CurrentLevel)
                .FirstOrDefaultAsync(x => x.StudentId == request.StudentId
                                          && x.TestType == EnumTestType.PlacementTest,
                    cancellationToken);

            var ptState = new PtStateModel
            {
                FlowId = testGroupResult?.FlowId,
                TestGroupResultId = testGroupResult?.Id,
                StudentId = request.StudentId,
                Level = testGroupResult?.CurrentLevel?.Name,
                LevelId = testGroupResult?.CurrentLevel?.Id,
                Status = testGroupResult?.Status ?? EnumResultStatus.NotStarted
            };

            return new MethodResult<PtStateModel> { StatusCode = StatusCodes.Status200OK, Result = ptState };
        }
    }
}
