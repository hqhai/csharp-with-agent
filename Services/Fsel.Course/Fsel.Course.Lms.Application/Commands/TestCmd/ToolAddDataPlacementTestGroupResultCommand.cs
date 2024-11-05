// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.TestCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ToolAddDataPlacementTestGroupResultCommand : IRequest<MethodResult<bool>>
    {
    }

    public class ToolAddDataPlacementTestGroupResultCommandHandler : IRequestHandler<ToolAddDataPlacementTestGroupResultCommand, MethodResult<bool>>
    {
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IPlacementTestGroupResultRepository _placementTestGroupResultRepository;

        public ToolAddDataPlacementTestGroupResultCommandHandler(IPlacementTestResultRepository placementTestResultRepository, IPlacementTestGroupResultRepository placementTestGroupResultRepository)
        {
            _placementTestResultRepository = placementTestResultRepository;
            _placementTestGroupResultRepository = placementTestGroupResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(ToolAddDataPlacementTestGroupResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var placementTestGroupResultNews = new List<PlacementTestGroupResult>();
            var placementTestGroupResults = await _placementTestGroupResultRepository.Queryable.ToListAsync(cancellationToken);
            var placementTestResults = await _placementTestResultRepository.Queryable
                                            .GroupBy(x => x.StudentId)
                                            .Select(x => x.OrderByDescending(x => x.UpdatedDate).ThenByDescending(x => x.CreatedDate).FirstOrDefault())
                                            .ToListAsync(cancellationToken);
            var placementTestResultCurrents = await _placementTestResultRepository.Queryable
                                .GroupBy(x => x.StudentId)
                                .Select(x => x.OrderBy(x => x.CreatedDate).FirstOrDefault())
                                .ToListAsync(cancellationToken);
            foreach (var item in placementTestResults)
            {
                if (item == null)
                {
                    continue;
                }
                if (item.StudentId == Guid.Empty)
                {
                    continue;
                }

                var placementTestGroupResult = placementTestGroupResults.FirstOrDefault(x => x.StudentId == item.StudentId);
                var placementTestResultCurrent = placementTestResultCurrents.FirstOrDefault(x => x.StudentId == item.StudentId);
                if (placementTestResultCurrent == null)
                {
                    continue;
                }
                var (levelCompleted, isLock) = item.Level.GetLevelInScore(item.Percent, IeltsScoreHelper.GetInitialAge(placementTestResultCurrent.Level));
                placementTestGroupResult = new PlacementTestGroupResult
                {
                    StudentId = item.StudentId,
                    ProcessDate = placementTestResultCurrent.CreatedDate,
                    ProcessLevel = placementTestResultCurrent.Level,
                    Status = Domain.Enums.EnumResultStatus.Process
                };
                if (isLock)
                {
                    placementTestGroupResult.Percent = item.Percent;
                    placementTestGroupResult.CompletionDate = item.UpdatedDate ?? item.CreatedDate;
                    placementTestGroupResult.CompletionLevel = item.Level;
                    placementTestGroupResult.Status = Domain.Enums.EnumResultStatus.Done;
                    placementTestGroupResult.SuggetLevel = levelCompleted;
                }
                placementTestGroupResultNews.Add(placementTestGroupResult);
            }
            await _placementTestGroupResultRepository.ExecuteTransactionAsync(async () =>
            {
                await _placementTestGroupResultRepository.AddList(placementTestGroupResultNews);
                await _placementTestGroupResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });
            return methodResult;
        }
    }
}
