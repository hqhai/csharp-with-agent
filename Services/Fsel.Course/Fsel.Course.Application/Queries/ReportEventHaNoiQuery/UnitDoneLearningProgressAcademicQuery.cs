// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.ReportEventHaNoiQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Course.Infrastructure;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    public class UnitDoneLearningProgressAcademicQuery : IRequest<MethodResult<IList<UnitDoneLearningProgressAcademicModel>>>
    {
        public int Target { get; set; } = 1;

        public int GroupByType { get; set; } = 1;

        public IList<string>? DistrictIds { get; set; }

        public IList<string>? GroupIds { get; set; }

        public IList<string>? SchoolIds { get; set; }

        public DateTime? Date { get; set; }
    }

    public class UnitDoneLearningProgressAcademicQueryHandler : IRequestHandler<UnitDoneLearningProgressAcademicQuery, MethodResult<IList<UnitDoneLearningProgressAcademicModel>>>
    {
        private readonly CourseDbContext _courseDbContext;

        public UnitDoneLearningProgressAcademicQueryHandler(CourseDbContext courseDbContext)
        {
            _courseDbContext = courseDbContext;
        }

        public async Task<MethodResult<IList<UnitDoneLearningProgressAcademicModel>>> Handle(UnitDoneLearningProgressAcademicQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<UnitDoneLearningProgressAcademicModel>>();

            var districtIdsParam = request.DistrictIds != null ? string.Join(",", request.DistrictIds) : (object)DBNull.Value;
            var groupIdsParam = request.GroupIds != null ? string.Join(",", request.GroupIds) : (object)DBNull.Value;
            var schoolIdsParam = request.SchoolIds != null ? string.Join(",", request.SchoolIds) : (object)DBNull.Value;
            var date = request.Date != null ? request.Date : (object)DBNull.Value;

            var unitDoneAca = await _courseDbContext.Set<UnitDoneLearningProgressAcademicModel>()
                                                    .FromSqlRaw("EXEC UnitDoneLearningProgressAcademic @Target, @GroupByType, @DistrictIds, @GroupIds, @SchoolIds, @Date",
                                                       new SqlParameter("@Target", request.Target),
                                                       new SqlParameter("@GroupByType", request.GroupByType),
                                                       new SqlParameter("@DistrictIds", districtIdsParam),
                                                       new SqlParameter("@GroupIds", groupIdsParam),
                                                       new SqlParameter("@SchoolIds", schoolIdsParam),
                                                       new SqlParameter("@Date", date))
                                                    .AsNoTracking()
                                                    .ToListAsync(cancellationToken);

            methodResult.Result = unitDoneAca;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
