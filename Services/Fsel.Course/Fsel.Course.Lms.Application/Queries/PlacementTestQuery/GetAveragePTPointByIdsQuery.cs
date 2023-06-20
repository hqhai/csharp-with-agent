// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestQuery
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.QueryModels.PlacementTests;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetAveragePTPointByIdsQuery : List<GetAveragePTPointByIdsQueryModel>, IRequest<MethodResult<List<AveragePTPointModel>>>
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
            List<AveragePTPointModel> average = new List<AveragePTPointModel>();
            foreach (var item in request)
            {
                var ptResult = await _placementTestResultRepository.Queryable.Where(x => item.StudentIds!.Contains(x.StudentId)).OrderByDescending(p => p.CreatedDate).ToListAsync(cancellationToken);
                var averagePT = new AveragePTPointModel();
                averagePT.ClassId = item.ClassId;
                averagePT.AveragePTPoint = 0;
                foreach (var student in item.StudentIds!)
                {
                    var pt = ptResult.FirstOrDefault(p => p.StudentId == student)!;

                    averagePT.MaxPTPoint = pt == null ? 0 : pt.CorrectTotal;
                    averagePT.AveragePTPoint += pt == null ? 0 : pt.CorrectCount;
                }
                averagePT.AveragePTPoint = item.StudentIds.Count == 0 ? 0 : averagePT.AveragePTPoint / item.StudentIds.Count;
                average.Add(averagePT);
            }
            methodResult.Result = average;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
