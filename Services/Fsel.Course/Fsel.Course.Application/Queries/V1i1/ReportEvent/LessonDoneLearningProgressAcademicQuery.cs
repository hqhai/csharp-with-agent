// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.V1i1.ReportEvent
{
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Caching;
    using Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Course.Infrastructure;
    using Fsel.Course.Infrastructure.ValueSettings;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    public class LessonDoneLearningProgressAcademicQuery : IRequest<MethodResult<IList<LessonDoneLearningProgressAcademicModel>>>
    {
        public int Target { get; set; } = 1;

        public int GroupByType { get; set; } = 1;

        public IList<string>? DistrictIds { get; set; }

        public IList<string>? GroupIds { get; set; }

        public IList<string>? SchoolIds { get; set; }

        public DateTime? Date { get; set; }
    }

    public class LessonDoneLearningProgressAcademicQueryHandler : IRequestHandler<LessonDoneLearningProgressAcademicQuery, MethodResult<IList<LessonDoneLearningProgressAcademicModel>>>
    {
        private readonly CourseDbContext _courseDbContext;
        private readonly ICacheService<IList<LessonDoneLearningProgressAcademicModel>> _cacheService;
        private readonly AppSetting _appSetting;
        private readonly AuthContext _authContext;

        public LessonDoneLearningProgressAcademicQueryHandler(CourseDbContext courseDbContext,
                                                              ICacheService<IList<LessonDoneLearningProgressAcademicModel>> cacheService,
                                                              AppSetting appSetting,
                                                              AuthContext authContext)
        {
            _courseDbContext = courseDbContext;
            _cacheService = cacheService;
            _appSetting = appSetting;
            _authContext = authContext;
        }

        public async Task<MethodResult<IList<LessonDoneLearningProgressAcademicModel>>> Handle(LessonDoneLearningProgressAcademicQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LessonDoneLearningProgressAcademicModel>>();

            var keyCache = $"LessonDoneLearningProgressAcademic_{ConvertHelper.Serialize(request)}";
            var data = await _cacheService.GetAsync(keyCache);
            if (data != null && _appSetting.CacheConfig != null && _appSetting.CacheConfig.TurnOnCaching)
            {
                methodResult.Result = data;
                return methodResult;
            }

            StringBuilder sbDistrict = new StringBuilder();
            if (request.DistrictIds != null)
            {
                foreach (var id in request.DistrictIds)
                {
                    if (sbDistrict.Length > 0)
                    {
                        sbDistrict.Append(",");
                    }

                    sbDistrict.Append(id);
                }
            }
            var districtIdsParam = sbDistrict.Length > 0 ? sbDistrict.ToString() : (object)DBNull.Value;

            StringBuilder sbGroup = new StringBuilder();
            if (request.GroupIds != null)
            {
                foreach (var id in request.GroupIds)
                {
                    if (sbGroup.Length > 0)
                    {
                        sbGroup.Append(",");
                    }

                    sbGroup.Append(id);
                }
            }
            var groupIdsParam = sbGroup.Length > 0 ? sbGroup.ToString() : (object)DBNull.Value;

            StringBuilder sbSchool = new StringBuilder();
            if (request.SchoolIds != null)
            {
                foreach (var id in request.SchoolIds)
                {
                    if (sbSchool.Length > 0)
                    {
                        sbSchool.Append(",");
                    }

                    sbSchool.Append(id);
                }
            }
            var schoolIdsParam = sbSchool.Length > 0 ? sbSchool.ToString() : (object)DBNull.Value;
            var date = request.Date != null ? request.Date : (object)DBNull.Value;

            var lessonDoneAca = await _courseDbContext.Database.SqlQueryRaw<LessonDoneLearningProgressAcademicModel>("EXEC LessonDoneLearningProgressAcademic @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @Date",
                                                           new SqlParameter("@Target", request.Target),
                                                           new SqlParameter("@GroupByType", request.GroupByType),
                                                           new SqlParameter("@DistrictIds", districtIdsParam),
                                                           new SqlParameter("@GroupIds", groupIdsParam),
                                                           new SqlParameter("@SchoolIds", schoolIdsParam),
                                                           new SqlParameter("@Date", date))
                                                      .AsNoTracking()
                                                      .ToListAsync(cancellationToken);

            methodResult.Result = lessonDoneAca;
            if (_appSetting.CacheConfig != null && _appSetting.CacheConfig.TurnOnCaching)
            {
                await _cacheService.SetAsync(keyCache, lessonDoneAca, TimeSpan.FromSeconds(_appSetting.CacheConfig.CachingDuration));
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
