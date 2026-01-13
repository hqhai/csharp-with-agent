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
        public int? Year { get; set; }
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
                .Where(x => x.CreatedUserId == _authContext.CurrentUserId)
                .OrderByDescending(x => x.LastVisited)
                .ToListAsync(cancellationToken);

            var featureAccessTimesByMonth = await _featureAccessTimeRepository.Queryable
                .Where(x => x.CreatedUserId == _authContext.CurrentUserId && x.LastVisited!.Value.Year == request.Year)
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

                case EnumFeatureTimeType.Month:
                    chartData = new List<FeatureAcessTimeChartModel>
                        {
                            CreateFeatureAccessTimeAMonth(featureAccessTimesByMonth.AsReadOnly(), socialFeatures, EnumFeatureBussinessType.Social),
                            CreateFeatureAccessTimeAMonth(featureAccessTimesByMonth.AsReadOnly(), new[] { EnumFeature.Other }, EnumFeatureBussinessType.Other),
                            CreateFeatureAccessTimeAMonth(featureAccessTimesByMonth.AsReadOnly(), learnFeatures, EnumFeatureBussinessType.Learn)
                        };
                    break;
            }

            methodResult.Result = chartData;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static FeatureAcessTimeChartModel CreateFeatureAccessTimeADay(ReadOnlyCollection<FeatureAccessTime> featureAccessTimes, EnumFeature[] features, EnumFeatureBussinessType type)
        {
            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date;

            var featureGroup = featureAccessTimes
                .Where(f => f.LastVisited.HasValue && f.LastVisited.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date == currentDate && features.Contains(f.EnumFeature)).ToList();

            var featureAccessTimeResult = new List<FeatureAccessTimeByTypeModel>();

            for (var i = 0; i < MAX_HOUR; i++)
            {
                var totalAccessTime = featureGroup
                                     .Where(x => x.LastVisited!.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Hour == i)
                                     .Sum(x => x.AccessTime);

                var firstMatchingFeature = featureGroup
                    .FirstOrDefault(x => x.LastVisited!.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Hour == i);

                featureAccessTimeResult.Add(new FeatureAccessTimeByTypeModel
                {
                    AccessTime = totalAccessTime,
                    HourActive = firstMatchingFeature?.LastVisited!.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Hour ?? i,
                    DayActive = firstMatchingFeature?.LastVisited!.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).DayOfWeek ?? DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam).DayOfWeek
                });
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
            var currentDayOfWeek = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            // Tính ngày bắt đầu của tuần (ngày thứ hai)
            DateTime startOfWeek = currentDayOfWeek.AddDays(-(int)currentDayOfWeek.DayOfWeek + (int)DayOfWeek.Monday);
            if (currentDayOfWeek.DayOfWeek == DayOfWeek.Sunday)
            {
                startOfWeek = startOfWeek.AddDays(-RANGE_WEEK_DAY);
            }
            startOfWeek = startOfWeek.Date;

            DateTime endOfWeek = startOfWeek.AddDays(RANGE_WEEK_DAY);

            var featureGroup = featureAccessTimes
                                 .Where(f => f.LastVisited.HasValue &&
                                             f.LastVisited.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam) >= startOfWeek &&
                                             f.LastVisited.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam) < endOfWeek &&
                                             features.Contains(f.EnumFeature))
                                 .GroupBy(f => f.LastVisited!.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Date)
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
                var featureGroupHour = featureGroup.FirstOrDefault(x => x.LastVisited!.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).DayOfWeek == daysOfWeek[i]);
                var featureAccessTime = new FeatureAccessTimeByTypeModel();

                featureAccessTime.AccessTime = featureGroupHour?.AccessTime ?? 0;
                featureAccessTime.HourActive = featureGroupHour?.LastVisited!.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).Hour ?? 0;
                featureAccessTime.DayActive = featureGroupHour?.LastVisited!.Value.ConvertTimeFromUtc(EnumCountryKey.Vietnam).DayOfWeek ?? daysOfWeek[i];
                featureAccessTimeResult.Add(featureAccessTime);
            }

            var result = new FeatureAcessTimeChartModel
            {
                FeatureBussinessType = type,
                FeatureAccessTimes = featureAccessTimeResult
            };

            return result;
        }

        private static FeatureAcessTimeChartModel CreateFeatureAccessTimeAMonth(ReadOnlyCollection<FeatureAccessTime> featureAccessTimesByMonth, EnumFeature[] features, EnumFeatureBussinessType type)
        {
            var featureAccessTimeByTypeMonthResult = new List<FeatureAccessTimeByTypeModel>();
            for (int i = 1; i <= 12; i++)
            {
                var featureAccessTime = new FeatureAccessTimeByTypeModel();

                featureAccessTime.TotalHourActive = featureAccessTimesByMonth
                     .Where(f => f.LastVisited.HasValue &&
                                 f.LastVisited.Value.Month == i &&
                                 features.Contains(f.EnumFeature))
                     .Sum(f => f.AccessTime);
                featureAccessTime.MonthActive = i;

                featureAccessTimeByTypeMonthResult.Add(featureAccessTime);
            }

            var result = new FeatureAcessTimeChartModel
            {
                FeatureBussinessType = type,
                FeatureAccessTimes = featureAccessTimeByTypeMonthResult
            };

            return result;
        }
    }
}
