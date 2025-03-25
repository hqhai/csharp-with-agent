// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestQuery
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.PlacementTests;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetAveragePTPointByIdsQuery : GetAveragePTPointByIdsQueryModel, IRequest<MethodResult<List<AveragePTPointModel>>>
    {
    }

    public class GetAveragePTPointByIdsQueryHandler : IRequestHandler<GetAveragePTPointByIdsQuery, MethodResult<List<AveragePTPointModel>>>
    {
        private readonly IPlacementTestResultRepository _placementTestResultRepository;

        public GetAveragePTPointByIdsQueryHandler(IPlacementTestResultRepository placementTestResultRepository)
        {
            _placementTestResultRepository = placementTestResultRepository;
        }

        public async Task<MethodResult<List<AveragePTPointModel>>> Handle(GetAveragePTPointByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<List<AveragePTPointModel>>();
            List<Guid> studentIds = request.PointByIdQueryModels.Where(x => x.StudentIds != null && x.StudentIds.Any()).SelectMany(x => x.StudentIds!).ToList();

            var ptResult = await (from baseQ in _placementTestResultRepository.Queryable.WhereBulkContains(studentIds, x => x.StudentId).Where(x => x.Status == Domain.Enums.EnumResultStatus.Done)
                                  group baseQ by baseQ.StudentId into g
                                  select g.OrderByDescending(x => x.CreatedDate).FirstOrDefault()).ToListAsync(cancellationToken);

            var average = request.PointByIdQueryModels
                                 .Select(item => new AveragePTPointModel
                                 {
                                     ClassId = item.ClassId,
                                     AveragePTPoint = item.StudentIds != null && item.StudentIds.Any()
                                            ? (from studentId in item.StudentIds
                                               join pt in ptResult on studentId equals pt.StudentId into joinedPts
                                               from pt in joinedPts.DefaultIfEmpty()
                                               select pt?.Percent ?? 0).Sum() / item.StudentIds.Count
                                            : 0
                                 }).ToList();

            methodResult.Result = average;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
