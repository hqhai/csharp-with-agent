// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.FeatureAccessTimeQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Collections.Generic;
    using global::System.Collections.ObjectModel;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetFeatureAccessTimeChartQuery : IRequest<MethodResult<IList<FeatureAcessTimeChartModel>>>
    {
        public EnumFeatureTimeType? Type { get; set; }
    }

    public class GetFeatureAccessTimeChartQueryHandler : IRequestHandler<GetFeatureAccessTimeChartQuery, MethodResult<IList<FeatureAcessTimeChartModel>>>
    {
        private readonly IFeatureAccessTimeRepository _featureAccessTimeRepository;
        private readonly AuthContext _authContext;

        public GetFeatureAccessTimeChartQueryHandler(IFeatureAccessTimeRepository featureAccessTimeRepository, AuthContext authContext)
        {
            _featureAccessTimeRepository = featureAccessTimeRepository;
            _authContext = authContext;
        }

        public async Task<MethodResult<IList<FeatureAcessTimeChartModel>>> Handle(GetFeatureAccessTimeChartQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<FeatureAcessTimeChartModel>> methodResult = new MethodResult<IList<FeatureAcessTimeChartModel>>();

            var featureAccessTimes = await _featureAccessTimeRepository.Queryable
                .Where(x => x.CreatedUserId == _authContext.CurrentUserId)
                .OrderByDescending(x => x.LastVisited)
                .ToListAsync(cancellationToken);

            List<FeatureAcessTimeChartModel> chartData = new List<FeatureAcessTimeChartModel>();

            switch (request.Type)
            {
                case EnumFeatureTimeType.Day:
                    chartData.Add(CaculateFeatureAccessTimeADay(featureAccessTimes.AsReadOnly()));
                    break;
                case EnumFeatureTimeType.Week:
                    // Implement logic for week here
                    break;
                default:
                    break;
            }

            methodResult.Result = chartData;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public static FeatureAcessTimeChartModel CaculateFeatureAccessTimeADay(ReadOnlyCollection<FeatureAccessTime> featureAccessTimes)
        {

            FeatureAcessTimeChartModel chartModel = new FeatureAcessTimeChartModel();

            if (featureAccessTimes == null || featureAccessTimes.Count == 0)
            {
                return chartModel;
            }

            chartModel.FeatureBussinessType = EnumFeatureBussinessType.Learn;
            chartModel.Label = EnumFeatureBussinessType.Learn.ToString();

            var groupedData = featureAccessTimes!
                .Where(f => f.LastVisited.HasValue)
                .GroupBy(f => f.LastVisited!.Value.Hour)
                .ToDictionary(g => g.Key, g => g.Sum(f => f.AccessTime));

            chartModel.ChartData = new double[24];

            for (int i = 0; i < 24; i++)
            {
                if (groupedData.ContainsKey(i))
                {
                    chartModel.ChartData[i] = groupedData[i];
                }
                else
                {
                    chartModel.ChartData[i] = 0;
                }
            }

            return chartModel;
        }

    }
}
