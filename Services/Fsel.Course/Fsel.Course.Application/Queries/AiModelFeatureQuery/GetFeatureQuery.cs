// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.AiModelFeatureQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.CommandModels.AiModelFeature;
    using Fsel.Course.Domain.Models.QueryModels.AiModelFeature;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.OpenApi.Extensions;

    public class GetFeatureQuery : IRequest<MethodResult<IReadOnlyList<GetFeatureAiModelQuery>>>
    {
    }

    public class GetFeatureQueryHandler : IRequestHandler<GetFeatureQuery, MethodResult<IReadOnlyList<GetFeatureAiModelQuery>>>
    {
        public async Task<MethodResult<IReadOnlyList<GetFeatureAiModelQuery>>> Handle(GetFeatureQuery request, CancellationToken cancellationToken)
        {
            MethodResult<IReadOnlyList<GetFeatureAiModelQuery>> methodResult = new MethodResult<IReadOnlyList<GetFeatureAiModelQuery>>();
            var featue = Enum.GetValues<EnumFeatureAi>()
                .Select(x =>
                {
                    var types = FeatureModel.FeatureTypes.TryGetValue(x, out var type)
                    ? type.Select(t => new GetTypeFeatureQuery((int)t, t.ToString(), t.DisplayName())) : null;

                    var featureModel = new GetFeatureAiModelQuery(
                        Id: (int)x,
                        Key: x.ToString(),
                        Label: ((Enum)x).DisplayName(),
                        Group: ((Enum)x).GroupName(),
                        Order: ((Enum)x).Order(),
                        Types: types
                        );
                    return featureModel;
                })
                .OrderBy(x => x.Order)
                .ToList()
                .AsReadOnly();

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = featue;

            return methodResult;
        }
    }
}
