// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.FeatureAccessTimeQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
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
        private const int RANGE_WEEK_DAY = 7; // khoảng cách từ ngày đầu tuần đến ngày cuối tuần
        private const int MAX_HOUR = 24; // Giờ trong ngày

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
                .Where(x => x.CreatedUserId == _authContext.CurrentUserId && x.LessonId == null && x.UnitId == null)
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
                            CreateFeatureAccessTimeAWeek(featureAccessTimes.AsReadOnly(), learnFeatures, EnumFeatureBussinessType.Learn )
                        };
                    break;

            }

            methodResult.Result = chartData;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static FeatureAcessTimeChartModel CreateFeatureAccessTimeADay(ReadOnlyCollection<FeatureAccessTime> featureAccessTimes, EnumFeature[] features, EnumFeatureBussinessType type)
        {
            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumZoneRegion.Vietnam).Date;

            var featureGroup = featureAccessTimes
                .Where(f => f.LastVisited.HasValue && f.LastVisited.Value.ConvertTimeFromUtc(EnumZoneRegion.Vietnam).Date == currentDate && features.Contains(f.EnumFeature)).ToList();

            var featureAccessTimeResult = new List<FeatureAccessTimeByTypeModel>();

            for (var i = 0; i < MAX_HOUR; i++)
            {
                var featureGroupHour = featureGroup.FirstOrDefault(x => x.LastVisited!.Value.ConvertTimeFromUtc(EnumZoneRegion.Vietnam).Hour == i);
                var featureAccessTime = new FeatureAccessTimeByTypeModel();

                featureAccessTime.AccessTime = featureGroupHour?.AccessTime ?? 0;
                featureAccessTime.HourActive = featureGroupHour?.LastVisited!.Value.ConvertTimeFromUtc(EnumZoneRegion.Vietnam).Hour ?? i;
                featureAccessTime.DayActive = featureGroupHour?.LastVisited!.Value.DayOfWeek ?? DateTime.UtcNow.DayOfWeek;

                featureAccessTimeResult.Add(featureAccessTime);
            }

            var result = new FeatureAcessTimeChartModel
            {
                FeatureBussinessType = type,
                FeatureAccessTimes = featureAccessTimeResult
            };

            return result;

        }

        private static FeatureAcessTimeChartModel CreateFeatureAccessTimeAWeek(ReadOnlyCollection<FeatureAccessTime> featureAccessTimes, EnumFeature[] features, EnumFeatureBussinessType type)
        {
            var currentDayOfWeek = DateTime.UtcNow.ConvertTimeFromUtc(EnumZoneRegion.Vietnam);

            var startOfWeek = currentDayOfWeek.AddDays(-(int)currentDayOfWeek.DayOfWeek + (int)DayOfWeek.Monday).Date;
            var endOfWeek = startOfWeek.AddDays(RANGE_WEEK_DAY).Date;

            var featureGroup = featureAccessTimes
                                 .Where(f => f.LastVisited.HasValue &&
                                             f.LastVisited.Value.ConvertTimeFromUtc(EnumZoneRegion.Vietnam) >= startOfWeek &&
                                             f.LastVisited.Value.ConvertTimeFromUtc(EnumZoneRegion.Vietnam) < endOfWeek &&
                                             features.Contains(f.EnumFeature))
                                 .GroupBy(f => f.LastVisited!.Value.ConvertTimeFromUtc(EnumZoneRegion.Vietnam).Date)
                                 .Select(group => new FeatureAccessTime
                                 {
                                     LastVisited = group.Key, // Date
                                     AccessTime = group.Sum(f => f.AccessTime),
                                 })
                                 .ToList();

            var featureAccessTimeResult = new List<FeatureAccessTimeByTypeModel>();

            var daysOfWeek = EnumHelper.GetList<DayOfWeek>()!.Select(day => Enum.Parse<DayOfWeek>(day))
                          .ToList();

            for (var i = 0; i < daysOfWeek!.Count; i++)
            {
                var featureGroupHour = featureGroup.FirstOrDefault(x => x.LastVisited!.Value.ConvertTimeFromUtc(EnumZoneRegion.Vietnam).DayOfWeek == daysOfWeek[i]);
                var featureAccessTime = new FeatureAccessTimeByTypeModel();

                featureAccessTime.AccessTime = featureGroupHour?.AccessTime ?? 0;
                featureAccessTime.HourActive = featureGroupHour?.LastVisited!.Value.ConvertTimeFromUtc(EnumZoneRegion.Vietnam).Hour ?? 0;
                featureAccessTime.DayActive = featureGroupHour?.LastVisited!.Value.ConvertTimeFromUtc(EnumZoneRegion.Vietnam).DayOfWeek ?? daysOfWeek[i];
                featureAccessTimeResult.Add(featureAccessTime);
            }

            var result = new FeatureAcessTimeChartModel
            {
                FeatureBussinessType = type,
                FeatureAccessTimes = featureAccessTimeResult
            };

            return result;
        }

    }
}
