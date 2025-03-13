// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.ReportEventHaNoiQuery
{
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Course.Infrastructure;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    public class GetSchoolInfoQuery : IRequest<MethodResult<IList<SchoolInfoModel>>>
    {
        public int Type { get; set; } = 1;

        public IList<string>? GroupIds { get; set; }

        public IList<string>? DistrictIds { get; set; }

        public IList<string>? Levels { get; set; }
    }

    public class GetSchoolInfoQueryHandler : IRequestHandler<GetSchoolInfoQuery, MethodResult<IList<SchoolInfoModel>>>
    {
        private readonly CourseDbContext _courseDbContext;

        public GetSchoolInfoQueryHandler(CourseDbContext courseDbContext)
        {
            _courseDbContext = courseDbContext;
        }

        public async Task<MethodResult<IList<SchoolInfoModel>>> Handle(GetSchoolInfoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<SchoolInfoModel>> methodResult = new MethodResult<IList<SchoolInfoModel>>();

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

            StringBuilder sbLevel = new StringBuilder();
            if (request.Levels != null)
            {
                foreach (var id in request.Levels)
                {
                    if (sbLevel.Length > 0)
                    {
                        sbLevel.Append(",");
                    }

                    sbLevel.Append(id);
                }
            }
            var levelsParam = sbLevel.Length > 0 ? sbLevel.ToString() : (object)DBNull.Value;

            var schoolInfos = await _courseDbContext.Set<SchoolInfoModel>()
                                                    .FromSqlRaw("EXEC SchoolInfoQuery @Type, @GroupIds, @DistrictIds, @Level",
                                                        new SqlParameter("@Type", request.Type),
                                                        new SqlParameter("@GroupIds", groupIdsParam),
                                                        new SqlParameter("@DistrictIds", districtIdsParam),
                                                        new SqlParameter("@Level", levelsParam))
                                                    .AsNoTracking()
                                                    .ToListAsync(cancellationToken);

            methodResult.Result = schoolInfos;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
