// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestResultQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class IntegrationPlacementTestResultsQuery : IRequest<MethodResult<IList<object>>>
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class IntegrationPlacementTestResultsQueryHandler : IRequestHandler<IntegrationPlacementTestResultsQuery, MethodResult<IList<object>>>
    {
        private readonly IPlacementTestResultRepository _placementTestResultRepository;

        public IntegrationPlacementTestResultsQueryHandler(IPlacementTestResultRepository placementTestResultRepository)
        {
            _placementTestResultRepository = placementTestResultRepository;
        }
        public async Task<MethodResult<IList<object>>> Handle(IntegrationPlacementTestResultsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<object>>();

            var querys = await _placementTestResultRepository.Queryable
                                                             .Where(x => x.UpdatedDate == null ? (x.CreatedDate.Date >= request.StartDate.Date && x.CreatedDate.Date <= request.EndDate.Date) : (x.UpdatedDate.Value.Date >= request.StartDate.Date && x.UpdatedDate.Value.Date <= request.EndDate.Date))
                                                             .GroupBy(x => x.CreatedUserId)
                                                             .Select(x => x.OrderByDescending(x => x.CreatedDate).FirstOrDefault())
                                                             .ToListAsync(cancellationToken);
            List<object> results = new();
            foreach (var item in querys)
            {
                var query = new
                {
                    UserId = item!.CreatedUserId,
                    Status = item.Status.ToString(),
                    Lever = item.Level.ToString()
                };
                results.Add(query);
            }

            methodResult.Result = results;
            return methodResult;
        }
    }
}
