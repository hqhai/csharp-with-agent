// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ReportEventHaNoiQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi;
    using Fsel.Course.Infrastructure;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;

    public class SchoolSummaryQuery : IRequest<MethodResult<PagingItemsModel<SchoolSummaryModel>>>
    {
        public int FilterType { get; set; }
        public IList<string>? FilterValues { get; set; }
        public int GroupByType { get; set; } = 1;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int UseParentEvent { get; set; } = 1;
        public IList<string>? DistrictIds { get; set; }
        public IList<string>? GroupIds { get; set; }
        public IList<string>? SchoolIds { get; set; }
        public int CheckByGroup { get; set; }
    }

    public class SchoolSummaryQueryHandler : IRequestHandler<SchoolSummaryQuery, MethodResult<PagingItemsModel<SchoolSummaryModel>>>
    {
        private readonly CourseDbContext _courseDbContext;

        public SchoolSummaryQueryHandler(CourseDbContext courseDbContext)
        {
            _courseDbContext = courseDbContext;
        }

        public async Task<MethodResult<PagingItemsModel<SchoolSummaryModel>>> Handle(SchoolSummaryQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<SchoolSummaryModel>>();

            var districtIdsParam = request.DistrictIds != null ? string.Join(",", request.DistrictIds) : (object)DBNull.Value;
            var groupIdsParam = request.GroupIds != null ? string.Join(",", request.GroupIds) : (object)DBNull.Value;
            var schoolIdsParam = request.SchoolIds != null ? string.Join(",", request.SchoolIds) : (object)DBNull.Value;
            var filterValuesParam = request.FilterValues != null ? string.Join(",", request.FilterValues) : (object)DBNull.Value;

            var result = await _courseDbContext.Set<SchoolSummaryModel>()
                                               .FromSqlRaw("EXEC SchoolSummary @FilterType, @FilterValue, @GroupByType, @PageNumber, @PageSize, @UseParentEvent, @DistrictIds, @GroupIds, @SchoolIds, @CheckByGroup",
                                                   new SqlParameter("@FilterType", request.FilterType),
                                                   new SqlParameter("@FilterValue", filterValuesParam),
                                                   new SqlParameter("@GroupByType", request.GroupByType),
                                                   new SqlParameter("@PageNumber", request.PageNumber),
                                                   new SqlParameter("@PageSize", request.PageSize),
                                                   new SqlParameter("@UseParentEvent", request.UseParentEvent),
                                                   new SqlParameter("@DistrictIds", districtIdsParam),
                                                   new SqlParameter("@GroupIds", groupIdsParam),
                                                   new SqlParameter("@SchoolIds", schoolIdsParam),
                                                   new SqlParameter("@CheckByGroup", request.CheckByGroup))
                                               .AsNoTracking()
                                               .ToListAsync(cancellationToken);

            methodResult.Result = new PagingItemsModel<SchoolSummaryModel>
            {
                Items = result,
                PagingInfo = new PagingInfoModel { Page = request.PageNumber, PageSize = request.PageSize, TotalItems = result.FirstOrDefault()?.TotalItem ?? default }
            };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
