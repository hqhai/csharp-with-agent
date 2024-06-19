// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.LevelSettingQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Domain.Helpers;
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetAllEnumCourseLevelQuery : IRequest<MethodResult<object>>
    {
    }

    public class GetAllEnumCourseLevelQueryHandler : IRequestHandler<GetAllEnumCourseLevelQuery, MethodResult<object>>
    {
        public GetAllEnumCourseLevelQueryHandler()
        {
        }

        public async Task<MethodResult<object>> Handle(GetAllEnumCourseLevelQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<object> methodResult = new MethodResult<object>();

            methodResult.Result = EnumCourseLevelHelper.GetEnumCourseLevels();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
