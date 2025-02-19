// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetPTPointByIdsQuery : IRequest<MethodResult<List<StudentPTPointModel>>>
    {
        public IList<Guid>? StudentIds { get; set; }
    }

    public class GetPTPointByIdsQueryHandler : IRequestHandler<GetPTPointByIdsQuery, MethodResult<List<StudentPTPointModel>>>
    {
        private readonly IPlacementTestResultRepository _placementTestResultRepository;

        public GetPTPointByIdsQueryHandler(IPlacementTestResultRepository placementTestResultRepository)
        {
            _placementTestResultRepository = placementTestResultRepository;
        }

        public async Task<MethodResult<List<StudentPTPointModel>>> Handle(GetPTPointByIdsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<List<StudentPTPointModel>> methodResult = new MethodResult<List<StudentPTPointModel>>();

            if (request.StudentIds == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestResultErrorCode.IdsNull));
                return methodResult;
            }
            List<StudentPTPointModel> studentPTPoints = new List<StudentPTPointModel>();
            var ptRs = await _placementTestResultRepository.Queryable
                                                           .WhereBulkContains(request.StudentIds, p => p.StudentId)
                                                           .OrderByDescending(x => x.CreatedDate)
                                                           .ToListAsync(cancellationToken);
            foreach (var item in request.StudentIds)
            {
                studentPTPoints.Add(
                    new StudentPTPointModel
                    {
                        StudentId = item,
                        PTPoint = ptRs.FirstOrDefault(p => p.StudentId == item) == null ? 0 : ptRs.FirstOrDefault(p => p.StudentId == item)!.Percent
                    });
            }
            methodResult.Result = studentPTPoints;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
