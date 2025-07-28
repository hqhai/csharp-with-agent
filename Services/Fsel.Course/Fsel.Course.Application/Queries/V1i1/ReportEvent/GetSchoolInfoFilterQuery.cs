// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.V1i1.ReportEvent
{
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Course.Infrastructure;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    public class GetSchoolInfoFilterQuery : IRequest<MethodResult<IList<SchoolInfoFilterModel>>>
    {
        public IList<string>? DistrictIds { get; set; }

        public IList<string>? GradeLevels { get; set; }
    }

    public class GetSchoolInfoFilterQueryHandler : IRequestHandler<GetSchoolInfoFilterQuery, MethodResult<IList<SchoolInfoFilterModel>>>
    {
        private readonly CourseDbContext _courseDbContext;

        public GetSchoolInfoFilterQueryHandler(CourseDbContext courseDbContext)
        {
            _courseDbContext = courseDbContext;
        }

        public async Task<MethodResult<IList<SchoolInfoFilterModel>>> Handle(GetSchoolInfoFilterQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<SchoolInfoFilterModel>> methodResult = new MethodResult<IList<SchoolInfoFilterModel>>();

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

            var totalDetail = await _courseDbContext.Set<SchoolInfoFilterModel>()
                                                    .FromSqlRaw("EXEC SchoolInfoFilter @DistrictIds, @GradeLevel",
                                                        new SqlParameter("@DistrictIds", districtIdsParam),
                                                        new SqlParameter("@GradeLevel", gradeLevelsParam))
                                                    .AsNoTracking()
                                                    .ToListAsync(cancellationToken);

            methodResult.Result = totalDetail;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
