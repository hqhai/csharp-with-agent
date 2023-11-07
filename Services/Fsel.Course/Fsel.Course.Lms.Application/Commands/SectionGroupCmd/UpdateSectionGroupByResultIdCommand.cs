// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.SectionGroupCmd
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateSectionGroupByResultIdCommand : CompleteTestWhenTimeOutModel, IRequest<MethodResult<bool>>
    {
    }

    public class UpdateSectionGroupByObjectResultCommandHandler : IRequestHandler<UpdateSectionGroupByResultIdCommand, MethodResult<bool>>
    {
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;

        public UpdateSectionGroupByObjectResultCommandHandler(ISectionGroupResultRepository sectionGroupResultRepository, IPlacementTestResultRepository placementTestResultRepository, SectionGroupConverter sectionGroupConverter, IMockTestResultRepository mockTestResultRepository, IFinalTestResultRepository finalTestResultRepository)
        {
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _placementTestResultRepository = placementTestResultRepository;
            _sectionGroupConverter = sectionGroupConverter;
            _mockTestResultRepository = mockTestResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(UpdateSectionGroupByResultIdCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Include(x => x.SectionGroup).Where(x => x.Id == request.ObjectResultId).FirstOrDefaultAsync(cancellationToken);
            var sectionGroup = sectionGroupResult?.SectionGroup;
            if (sectionGroupResult == null || sectionGroup == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var objectResultId = sectionGroupResult.FinalTestResultId ?? sectionGroupResult.MockTestResultId ?? sectionGroupResult.MockTestResultId;

            return methodResult;
        }

        private async Task UpdateSectionGroupByFinalTest(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            ArgumentNullException.ThrowIfNull(sectionGroupResult);
            await _sectionGroupConverter.UpdatePlacementTestAnswers(sectionGroup, sectionGroupResult);

            _sectionGroupResultRepository.Update(sectionGroupResult);
            await _sectionGroupResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task UpdateFinalTestResult(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            ArgumentNullException.ThrowIfNull(sectionGroupResult);
            var maxSkillFullMockTest = 3;
            await UpdateSectionGroupByFinalTest(sectionGroup, sectionGroupResult, cancellationToken);

            var sectionGroupResults = await _sectionGroupResultRepository.Queryable.Where(x => x.FinalTestResultId == sectionGroupResult.FinalTestResultId).ToListAsync(cancellationToken);
            if (sectionGroupResults != null && sectionGroupResults.Count == maxSkillFullMockTest && sectionGroupResults.All(x => x.Status == EnumResultStatus.Done))
            {
                var finalTestResult = await _finalTestResultRepository.GetByIdAsync(sectionGroupResult.FinalTestResultId ?? default);
                if (finalTestResult != null)
                {
                    finalTestResult = GetFinalTestResult(sectionGroupResults.SelectMany(x => x.SkillScores!).ToList(), finalTestResult);
                    _finalTestResultRepository.Update(finalTestResult);
                    await _finalTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task UpdateSectionGroupByPlacementTest(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            ArgumentNullException.ThrowIfNull(sectionGroupResult);
            await _sectionGroupConverter.UpdatePlacementTestAnswers(sectionGroup, sectionGroupResult);

            _sectionGroupResultRepository.Update(sectionGroupResult);
            await _sectionGroupResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task UpdatePlacementTestResult(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            ArgumentNullException.ThrowIfNull(sectionGroupResult);
            await UpdateSectionGroupByPlacementTest(sectionGroup, sectionGroupResult, cancellationToken);
            var maxSkillTest = 4;
            var sectionGroupResults = await _sectionGroupResultRepository.Queryable.Where(x => x.PlacementTestResultId == sectionGroupResult.PlacementTestResultId).ToListAsync(cancellationToken);
            if (sectionGroupResults != null && sectionGroupResults.Count == maxSkillTest && sectionGroupResults.All(x => x.Status == EnumResultStatus.Done))
            {
                var placementTestResult = await _placementTestResultRepository.GetByIdAsync(sectionGroupResult.FinalTestResultId ?? default);
                if (placementTestResult != null)
                {
                    placementTestResult = GetPlacementTestResult(sectionGroupResults.SelectMany(x => x.SkillScores!).ToList(), placementTestResult);
                    _placementTestResultRepository.Update(placementTestResult);
                    await _finalTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task UpdateSectionGroupByMockTest(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            ArgumentNullException.ThrowIfNull(sectionGroupResult);
            await _sectionGroupConverter.UpdatePlacementTestAnswers(sectionGroup, sectionGroupResult);

            _sectionGroupResultRepository.Update(sectionGroupResult);
            await _sectionGroupResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task UpdateMockTestResult(SectionGroup sectionGroup, SectionGroupResult sectionGroupResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(sectionGroup);
            ArgumentNullException.ThrowIfNull(sectionGroupResult);
            await UpdateSectionGroupByPlacementTest(sectionGroup, sectionGroupResult, cancellationToken);
            var maxSkillFinalTest = 4;
            var mockTestResult = await _mockTestResultRepository.Queryable.Include(x => x.MockTest).FirstOrDefaultAsync(x => x.Id == sectionGroupResult.MockTestResultId, cancellationToken);
            var mockTest = mockTestResult?.MockTest;
            if (mockTest == null || mockTestResult == null)
            {
                return;
            }

            _sectionGroupResultRepository.Update(sectionGroupResult);
            await _sectionGroupResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            if (mockTest.MockTestType == EnumMockTestType.SkillMockTest)
            {
                mockTestResult = GetMockTestResult(sectionGroupResult.SkillScores, mockTestResult);
            }
            else
            {
                var sectionGroupResults = await _sectionGroupResultRepository.Queryable.Where(x => x.FinalTestResultId == sectionGroupResult.FinalTestResultId).ToListAsync(cancellationToken);
                if (sectionGroupResults != null && sectionGroupResults.Count == maxSkillFinalTest && sectionGroupResults.All(x => x.Status == EnumResultStatus.Done))
                {
                    mockTestResult = GetMockTestResult(sectionGroupResults.SelectMany(x => x.SkillScores!).ToList(), mockTestResult);
                    _mockTestResultRepository.Update(mockTestResult);
                    await _finalTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            _mockTestResultRepository.Update(mockTestResult);
            await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
        }

        private static MockTestResult GetMockTestResult(IList<SkillScores>? skillScores, MockTestResult mockTestResult)
        {
            ArgumentNullException.ThrowIfNull(skillScores);
            mockTestResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
            mockTestResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
            mockTestResult.Status = EnumResultStatus.Done;
            mockTestResult.SkillScores = skillScores;
            return mockTestResult;
        }

        private static FinalTestResult GetFinalTestResult(IList<SkillScores>? skillScores, FinalTestResult finalTestResult)
        {
            ArgumentNullException.ThrowIfNull(skillScores);
            finalTestResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
            finalTestResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
            finalTestResult.Status = EnumResultStatus.Done;
            finalTestResult.SkillScores = skillScores;
            return finalTestResult;
        }

        private static PlacementTestResult GetPlacementTestResult(IList<SkillScores>? skillScores, PlacementTestResult placementTestResult)
        {
            ArgumentNullException.ThrowIfNull(skillScores);
            placementTestResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
            placementTestResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
            placementTestResult.Status = EnumResultStatus.Done;
            placementTestResult.SkillScores = skillScores;
            return placementTestResult;
        }
    }
}
