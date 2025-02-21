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
        public object? FilterValue { get; set; }
        public int GroupByType { get; set; } = 1;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int UseParentEvent { get; set; } = 1;
        public object? DistrictIds { get; set; }
        public object? GroupIds { get; set; }
        public object? SchoolIds { get; set; }
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

            var result = await _courseDbContext.Set<SchoolSummaryModel>()
                                               .FromSqlRaw("EXEC SchoolSummary @FilterType, @FilterValue, @GroupByType, @PageNumber, @PageSize, @UseParentEvent, @DistrictIds, @GroupIds, @SchoolIds, @CheckByGroup",
                                                   new SqlParameter("@FilterType", request.FilterType),
                                                   new SqlParameter("@FilterValue", request.FilterValue ?? (object)DBNull.Value),
                                                   new SqlParameter("@GroupByType", request.GroupByType),
                                                   new SqlParameter("@PageNumber", request.PageNumber),
                                                   new SqlParameter("@PageSize", request.PageSize),
                                                   new SqlParameter("@UseParentEvent", request.UseParentEvent),
                                                   new SqlParameter("@DistrictIds", request.DistrictIds ?? (object)DBNull.Value),
                                                   new SqlParameter("@GroupIds", request.GroupIds ?? (object)DBNull.Value),
                                                   new SqlParameter("@SchoolIds", request.SchoolIds ?? (object)DBNull.Value),
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
