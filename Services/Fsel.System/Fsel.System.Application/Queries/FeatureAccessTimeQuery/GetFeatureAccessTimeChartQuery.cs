// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.FeatureAccessTimeQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
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
        private const int MaxHour = 24;
        private const int MaxWeek = 7;

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
                .Where(x => x.CreatedUserId == _authContext.CurrentUserId && x.ObjectId == null && x.LessonId == null)
                .OrderByDescending(x => x.LastVisited)
                .ToListAsync(cancellationToken);

            List<FeatureAcessTimeChartModel> chartData = new List<FeatureAcessTimeChartModel>();
            var socialFeatures = new[] { EnumFeature.ClassForum, EnumFeature.DiscussionBoard };
            var learnFeatures = Enum.GetValues(typeof(EnumFeature)).Cast<EnumFeature>().Except(socialFeatures).Except(new[] { EnumFeature.Other }).ToArray();

            switch (request.Type)
            {
                case EnumFeatureTimeType.Day:

                    chartData = new List<FeatureAcessTimeChartModel>
                        {
                            CreateFeatureAccessTimeADay(featureAccessTimes.AsReadOnly(), socialFeatures, EnumFeatureBussinessType.Social),
                            CreateFeatureAccessTimeADay(featureAccessTimes.AsReadOnly(), new[] { EnumFeature.Other }, EnumFeatureBussinessType.Other),
                            CreateFeatureAccessTimeADay(featureAccessTimes.AsReadOnly(), learnFeatures, EnumFeatureBussinessType.Learn)
                        };
                    break;
                case EnumFeatureTimeType.Week:
                    chartData = new List<FeatureAcessTimeChartModel>
                        {
                            CreateFeatureAccessTimeAWeek(featureAccessTimes.AsReadOnly(), socialFeatures, EnumFeatureBussinessType.Social),
                            CreateFeatureAccessTimeAWeek(featureAccessTimes.AsReadOnly(), new[] { EnumFeature.Other }, EnumFeatureBussinessType.Other),
                            CreateFeatureAccessTimeAWeek(featureAccessTimes.AsReadOnly(), learnFeatures, EnumFeatureBussinessType.Learn)
                        };
                    break;

            }

            methodResult.Result = chartData;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static FeatureAcessTimeChartModel CreateFeatureAccessTimeADay(ReadOnlyCollection<FeatureAccessTime> featureAccessTimes, EnumFeature[] features, EnumFeatureBussinessType type)
        {
            var chartModel = new FeatureAcessTimeChartModel();

            var currentDate = DateTime.UtcNow.Date;

            var groupedData = featureAccessTimes
                .Where(f =>
                    f.LastVisited.HasValue &&
                    f.LastVisited.Value.Date == currentDate &&
                    features.Contains(f.EnumFeature))
                .GroupBy(f => f.LastVisited!.Value.Hour)
                .ToDictionary(g => g.Key, g => g.Sum(f => f.AccessTime));

            chartModel.FeatureBussinessType = type;
            chartModel.Label = type.ToString();
            chartModel.ChartData = new double[MaxHour];

            for (int i = 0; i < MaxHour; i++)
            {
                chartModel.ChartData[i] = groupedData.ContainsKey(i) ? groupedData[i] : 0;
            }

            return chartModel;
        }

        private static FeatureAcessTimeChartModel CreateFeatureAccessTimeAWeek(ReadOnlyCollection<FeatureAccessTime> featureAccessTimes, EnumFeature[] features, EnumFeatureBussinessType type)
        {
            var chartModel = new FeatureAcessTimeChartModel();

            var groupedData = featureAccessTimes
                .Where(f => f.LastVisited.HasValue && features.Contains(f.EnumFeature))
                .GroupBy(f => f.LastVisited!.Value.DayOfWeek)
                .ToDictionary(g => g.Key, g => g.Sum(f => f.AccessTime));

            chartModel.FeatureBussinessType = type;
            chartModel.Label = type.ToString();
            chartModel.ChartData = new double[MaxWeek];

            var daysOfWeek = EnumHelper.GetList<DayOfWeek>()!.Select(day => Enum.Parse<DayOfWeek>(day))
                           .ToList();

            for (int i = 0; i < daysOfWeek!.Count; i++)
            {
                chartModel.ChartData[i] = groupedData.ContainsKey(daysOfWeek[i]) ? groupedData[daysOfWeek[i]] : 0;
            }

            return chartModel;
        }

    }
}
