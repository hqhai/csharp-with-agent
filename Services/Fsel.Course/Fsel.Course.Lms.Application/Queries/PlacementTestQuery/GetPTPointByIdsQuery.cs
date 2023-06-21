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

            var ptRs = await _placementTestResultRepository.Queryable.Where(p => request.StudentIds.Contains(p.StudentId)).Select(x => new StudentPTPointModel
            {
                StudentId = x.StudentId,
                PTPoint = (long)x.Percent
            }).ToListAsync(cancellationToken);

            methodResult.Result = ptRs;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
