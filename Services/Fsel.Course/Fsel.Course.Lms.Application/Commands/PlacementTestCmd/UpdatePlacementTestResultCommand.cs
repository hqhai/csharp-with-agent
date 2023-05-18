// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.PlacementTestCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.PlacementTestResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
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
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ISectionPartRepository _sectionPartRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly ISectionQuestionRepository _sectionQuestionRepository;

        public UpdatePlacementTestResultCommandHandler(
            IMapper mapper,
            IPlacementTestAnswerRepository placementTestAnswerRepository,
            IPlacementTestResultRepository placementTestResultRepository,
            ISectionGroupRepository sectionGroupRepository,
            ISectionPartRepository sectionPartRepository,
            ISectionRepository sectionRepository,
            ISectionQuestionRepository sectionQuestionRepository,
            IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository;
            _mapper = mapper;
            _placementTestAnswerRepository = placementTestAnswerRepository;
            _placementTestResultRepository = placementTestResultRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _sectionPartRepository = sectionPartRepository;
            _sectionRepository = sectionRepository;
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
                              join sq in _sectionQuestionRepository.Queryable on pta.SectionQuestionId equals sq.Id
                              join s in _sectionRepository.Queryable on sq.SectionId equals s.Id
                              join sg in _sectionGroupRepository.Queryable on s.SectionGroupId equals sg.Id
                              join q in _questionRepository.Queryable on sq.QuestionId equals q.Id
                              where pta.PlacementTestResultId == placementTestResult.Id
                              group pta by sg.CourseSkill into g
                              select new SkillScores
                              {
                                  Skill = g.Key,
                                  Scores = g.Sum(x => x.CorrectCount),
                              };
            IQueryable<SkillScores>? skillScoresQuery = null;
            if (placementTestResult.Level != EnumPlacementTestLevel.IELTS)
            {
                skillScoresQuery = from pr in _placementTestResultRepository.Queryable
                                   join pa in _placementTestAnswerRepository.Queryable on pr.Id equals pa.PlacementTestResultId
                                   join sq in _sectionQuestionRepository.Queryable on pa.SectionQuestionId equals sq.Id
                                   join s in _sectionRepository.Queryable on sq.SectionId equals s.Id
                                   join sg in _sectionGroupRepository.Queryable on s.SectionGroupId equals sg.Id
                                   join q in _questionRepository.Queryable on sq.QuestionId equals q.Id
                                   where pr.Id == placementTestResult.Id
                                   group q by sg.CourseSkill into g
                                   select new SkillScores
                                   {
                                       Skill = g.Key,
                                       Total = g.Sum(x => x.CorrectTotal),
                                   };
            }
            else
            {
                skillScoresQuery = from pr in _placementTestResultRepository.Queryable
                                   join pa in _placementTestAnswerRepository.Queryable on pr.Id equals pa.PlacementTestResultId
                                   join sq in _sectionQuestionRepository.Queryable on pa.SectionQuestionId equals sq.Id
                                   join sp in _sectionPartRepository.Queryable on sq.SectionPartId equals sp.Id
                                   join s in _sectionRepository.Queryable on sp.SectionId equals s.Id
                                   join sg in _sectionGroupRepository.Queryable on s.SectionGroupId equals sg.Id
                                   join q in _questionRepository.Queryable on sq.QuestionId equals q.Id
                                   where pr.Id == placementTestResult.Id
                                   group q by sg.CourseSkill into g
                                   select new SkillScores
                                   {
                                       Skill = g.Key,
                                       Total = g.Sum(x => x.CorrectTotal),
                                   };
            }

            var skillScoreTotals = await skillScoresQuery.ToListAsync(cancellationToken);
            var skillScoreScores = await answerQuery.ToListAsync(cancellationToken);
            foreach (var skillScore in skillScoreTotals)
            {
                skillScore.Scores = skillScoreScores.FirstOrDefault(x => x.Skill == skillScore.Skill)!.Scores;
            }
            placementTestResult.CorrectCount = Convert.ToInt32(skillScoreTotals.Sum(x => x.Scores));
            placementTestResult.CorrectTotal = Convert.ToInt32(skillScoreTotals.Sum(x => x.Total));
            placementTestResult.Status = EnumResultStatus.Done;
            placementTestResult.SkillScores = skillScoreTotals;
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
