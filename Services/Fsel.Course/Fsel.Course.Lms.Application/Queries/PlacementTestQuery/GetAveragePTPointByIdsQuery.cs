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
            List<AveragePTPointModel> average = new List<AveragePTPointModel>();

            List<Guid>? studentIds = request.PointByIdQueryModels.SelectMany(x => x.StudentIds!).ToList();

            var ptResult = await _placementTestResultRepository.Queryable.Where(x => studentIds.Contains(x.StudentId)).OrderByDescending(p => p.CreatedDate).ToListAsync(cancellationToken);

            foreach (var item in request.PointByIdQueryModels)
            {
                var averagePT = new AveragePTPointModel();
                averagePT.ClassId = item.ClassId;

                double percent = 0;
                foreach (var student in item.StudentIds!)
                {
                    var pt = ptResult.FirstOrDefault(p => p.StudentId == student)!;
                    percent += pt?.Percent ?? default;
                }
                averagePT.AveragePTPoint = item.StudentIds.Count == 0 ? 0 : percent / item.StudentIds.Count;
                average.Add(averagePT);
            }
            methodResult.Result = average;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
