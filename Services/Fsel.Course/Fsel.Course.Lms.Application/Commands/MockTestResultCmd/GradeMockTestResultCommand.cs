// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.MockTestResultCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.MockTestResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GradeMockTestResultCommand : GradeMockTestResultCommandModel, IRequest<MethodResult<List<MockTestScoreModel>>>
    {
    }

    public class GradeMockTestResultCommandHandler : IRequestHandler<GradeMockTestResultCommand, MethodResult<List<MockTestScoreModel>>>
    {
        private readonly IMapper _mapper;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;

        public GradeMockTestResultCommandHandler(IMapper mapper, IMockTestResultRepository mockTestResultRepository, ISectionGroupRepository sectionGroupRepository)
        {
            _mapper = mapper;
            _mockTestResultRepository = mockTestResultRepository;
            _sectionGroupRepository = sectionGroupRepository;
        }

        public async Task<MethodResult<List<MockTestScoreModel>>> Handle(GradeMockTestResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<List<MockTestScoreModel>> methodResult = new MethodResult<List<MockTestScoreModel>>();

            if (request.MockTestScores == null || request.MockTestScores.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestResultErrorCode.MockTestScoresNull));
                return methodResult;
            }
            var mockTestResult = await _mockTestResultRepository.Queryable.Where(e => e.Id == request.MockTestResultId).FirstOrDefaultAsync(cancellationToken);
            if (mockTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassForumResultErrorCode.ClassForumResultNotExist));
                return methodResult;
            }
            var sectionGroupIds = request.MockTestScores.Select(y => y.SectionGroupId).Distinct().ToList();
            var sectionGroups = await _sectionGroupRepository.Queryable.Where(x => sectionGroupIds.Contains(x.Id)).ToListAsync(cancellationToken);
            if (sectionGroups.Count != sectionGroupIds.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumMockTestResultErrorCode.SectionGroupsNotExist));
                return methodResult;
            }

            foreach (var item in request.MockTestScores)
            {
                if (item.Score > 9)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumMockTestResultErrorCode.ScoreMustLessThan9), nameof(item.Score), new
                    {
                        Score = item.Score,
                    });
                    return methodResult;
                }
                MockTestScore mockTestScore = _mapper.Map<MockTestScore>(item);
                if (!mockTestScore.IsValid())
                {
                    methodResult.AddErrorBadRequest(mockTestScore.ErrorMessages);
                    return methodResult;
                }
                mockTestResult.MockTestScores.Add(mockTestScore);
            }

            await _mockTestResultRepository.ExecuteTransactionAsync(async () =>
            {
                _mockTestResultRepository.Update(mockTestResult);
                await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<List<MockTestScoreModel>>(mockTestResult.MockTestScores);
                return methodResult;
            });

            return methodResult;
        }
    }
}
