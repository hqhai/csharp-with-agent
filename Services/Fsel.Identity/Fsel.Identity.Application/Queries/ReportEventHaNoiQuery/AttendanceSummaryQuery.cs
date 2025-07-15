// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.ReportEventHaNoiQuery
{
    using System.Text;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Common.Caching;
    using Fsel.Identity.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Identity.Infrastructure;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    public class AttendanceSummaryQuery : IRequest<MethodResult<OverallStudentModel>>
    {
        public IList<string>? DistrictIds { get; set; }

        public IList<string>? GradeLevels { get; set; }

        public IList<string>? SchoolIds { get; set; }

        public int? TargetGroup { get; set; }

        public DateTime? Date { get; set; }
    }

    public class AttendanceSummaryQueryHandler : IRequestHandler<AttendanceSummaryQuery, MethodResult<OverallStudentModel>>
    {
        private readonly UserDbContext _userDbContext;
        private readonly ICacheService<OverallStudentModel> _cacheService;
        private readonly AppSetting _appSetting;
        private readonly AuthContext _authContext;

        public AttendanceSummaryQueryHandler(UserDbContext userDbContext,
                                             ICacheService<OverallStudentModel> cacheService,
                                             AppSetting appSetting,
                                             AuthContext authContext)
        {
            _userDbContext = userDbContext;
            _cacheService = cacheService;
            _appSetting = appSetting;
            _authContext = authContext;
        }

        public async Task<MethodResult<OverallStudentModel>> Handle(AttendanceSummaryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<OverallStudentModel>();

            var keyCache = $"AttendanceSummaryQuery_{ConvertHelper.Serialize(request)}_{_authContext.CurrentUserId}";
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

            StringBuilder sbGradeLevel = new StringBuilder();
            if (request.GradeLevels != null)
            {
                foreach (var gradeLevel in request.GradeLevels)
                {
                    if (sbGradeLevel.Length > 0)
                    {
                        sbGradeLevel.Append(",");
                    }

                    sbGradeLevel.Append(gradeLevel);
                }
            }
            var gradeLevelsParam = sbGradeLevel.Length > 0 ? sbGradeLevel.ToString() : (object)DBNull.Value;

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

            var level = await _userDbContext.Set<OverallStudentModel>()
                                            .FromSqlRaw("EXEC AttendanceSummary @UserId, @DistrictIds, @GradeLevel, @SchoolIds, @TargetGroup, @Date",
                                                 new SqlParameter("@UserId", _authContext.CurrentUserId),
                                                 new SqlParameter("@DistrictIds", districtIdsParam),
                                                 new SqlParameter("@GradeLevel", gradeLevelsParam),
                                                 new SqlParameter("@SchoolIds", schoolIdsParam),
                                                 new SqlParameter("@TargetGroup", request.TargetGroup),
                                                 new SqlParameter("@Date", date))
                                            .AsNoTracking()
                                            .ToListAsync(cancellationToken);

            var overallStudent = level.FirstOrDefault();
            methodResult.Result = overallStudent;
            if (_appSetting.CacheConfig != null && _appSetting.CacheConfig.TurnOnCaching && overallStudent != null)
            {
                await _cacheService.SetAsync(keyCache, overallStudent, TimeSpan.FromSeconds(_appSetting.CacheConfig.CachingDuration));
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
