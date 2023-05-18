// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.PlacementTestResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdatePlacementTestResultCommand : UpdatePlacementTestResultCommandModel, IRequest<MethodResult<PlacementTestResultModel>>
    {
    }

    public class UpdatePlacementTestResultCommandHandler : IRequestHandler<UpdatePlacementTestResultCommand, MethodResult<PlacementTestResultModel>>
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IMapper _mapper;
        private readonly IPlacementTestAnswerRepository _placementTestAnswerRepository;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly ISectionQuestionRepository _sectionQuestionRepository;

        public UpdatePlacementTestResultCommandHandler(
            IMapper mapper,
            IPlacementTestAnswerRepository placementTestAnswerRepository,
            IPlacementTestResultRepository placementTestResultRepository,
            ISectionQuestionRepository sectionQuestionRepository,
            IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository;
            _mapper = mapper;
            _placementTestAnswerRepository = placementTestAnswerRepository;
            _placementTestResultRepository = placementTestResultRepository;
            _sectionQuestionRepository = sectionQuestionRepository;
        }

        public async Task<MethodResult<PlacementTestResultModel>> Handle(UpdatePlacementTestResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PlacementTestResultModel> methodResult = new MethodResult<PlacementTestResultModel>();

            var placementTestResult = await _placementTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.PlacementTestResultId, cancellationToken: cancellationToken);
            if (placementTestResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestResultErrorCode.PlacementTestResultNotExist), nameof(request.PlacementTestResultId), request.PlacementTestResultId);
                return methodResult;
            }

            _mapper.Map(request, placementTestResult);
            if (!placementTestResult.IsValid())
            {
                methodResult.AddErrorBadRequest(placementTestResult.ErrorMessages);
                return methodResult;
            }

            var answerQuery = from pta in _placementTestAnswerRepository.Queryable
                              where pta.PlacementTestResultId == placementTestResult.Id
                              select pta.CorrectCount;
            IQueryable<int>? placementTestResultQuery = from pr in _placementTestResultRepository.Queryable
                                                        join pa in _placementTestAnswerRepository.Queryable on pr.Id equals pa.PlacementTestResultId
                                                        join sq in _sectionQuestionRepository.Queryable on pa.SectionQuestionId equals sq.Id
                                                        join q in _questionRepository.Queryable on sq.QuestionId equals q.Id
                                                        where pr.Id == placementTestResult.Id
                                                        select q.CorrectTotal;

            placementTestResult.CorrectCount = await answerQuery.SumAsync(cancellationToken);
            placementTestResult.CorrectTotal = await placementTestResultQuery.SumAsync(cancellationToken);
            placementTestResult.Status = EnumResultStatus.Done;
            placementTestResult.Percent = (double)placementTestResult.CorrectCount / placementTestResult.CorrectTotal * 100;
            await _placementTestResultRepository.ExecuteTransactionAsync(async () =>
            {
                placementTestResult = _placementTestResultRepository.Update(placementTestResult);
                await _placementTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<PlacementTestResultModel>(placementTestResult);
                return methodResult;
            });
            return methodResult;
        }
    }
}
