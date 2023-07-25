// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ExtraPracticeQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetReportMockTestExtraPracticeQuery : IRequest<MethodResult<ExtraPracticeResultModel>>
    {
        public Guid ExtraPracticeId { get; set; }
        public Guid ExtraPracticeResultId { get; set; }
    }

    public class GetReportMockTestExtraPracticeQueryHandler : IRequestHandler<GetReportMockTestExtraPracticeQuery, MethodResult<ExtraPracticeResultModel>>
    {
        private readonly IExtraPracticeAnswerRepository _extraPracticeAnswerRepository;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;
        private readonly ISectionGroupRepository _sectionGroupRepository;
        private readonly ISectionPartRepository _sectionPartRepository;

        public GetReportMockTestExtraPracticeQueryHandler(IExtraPracticeAnswerRepository extraPracticeAnswerRepository
            , IExtraPracticeResultRepository extraPracticeResultRepository
            , ISectionGroupRepository sectionGroupRepository
            , ISectionPartRepository sectionPartRepository)
        {
            _extraPracticeAnswerRepository = extraPracticeAnswerRepository;
            _extraPracticeResultRepository = extraPracticeResultRepository;
            _sectionGroupRepository = sectionGroupRepository;
            _sectionPartRepository = sectionPartRepository;
        }

        public async Task<MethodResult<ExtraPracticeResultModel>> Handle(GetReportMockTestExtraPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ExtraPracticeResultModel> methodResult = new MethodResult<ExtraPracticeResultModel>();

            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.ExtraPracticeResultId, cancellationToken: cancellationToken);
            if (extraPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumExtraPracticeErrorCode.ExtraPracticesNotExist), nameof(request.ExtraPracticeResultId), request.ExtraPracticeResultId);
                return methodResult;
            }

            var answerQuery = from baseQ in _extraPracticeResultRepository.Queryable
                              join ea in _extraPracticeAnswerRepository.Queryable on baseQ.Id equals ea.ExtraPracticeResultId
                              join e in sec.Queryable on vtca.ExerciseId equals e.Id
                              join te in _timeCodeExerciseRepository.Queryable on e.Id equals te.ExerciseId
                              join vt in _videoTimeCodeRepository.Queryable on te.VideoTimeCodeId equals vt.Id
                              where baseQ.Id == videoResult.Id
                              group new { vt, vtca } by new { vt.TimeCodeType, e.CourseSkill } into g
                              select new
                              {
                                  Type = g.Key.TimeCodeType,
                                  Skill = g.Key.CourseSkill,
                                  CorrectCount = g.Sum(x => x.vtca.CorrectCount),
                                  TotalAnswer = g.Select(x => x.vtca).Count()
                              };

            var questionQuery = from baseQ in _videoResultRepository.Queryable
                                join v in _videoRepository.Queryable on baseQ.VideoId equals v.Id
                                join vt in _videoTimeCodeRepository.Queryable on v.Id equals vt.VideoId
                                join te in _timeCodeExerciseRepository.Queryable on vt.Id equals te.VideoTimeCodeId
                                join e in _exerciseRepository.Queryable on te.ExerciseId equals e.Id
                                join eq in _exerciseQuestionRepository.Queryable on e.Id equals eq.ExerciseId
                                join q in _questionRepository.Queryable on eq.QuestionId equals q.Id
                                where baseQ.Id == videoResult.Id && q.QuestionType != EnumQuestionType.ExercisePreparation
                                group new { vt, q } by new { vt.TimeCodeType, e.CourseSkill } into g
                                select new
                                {
                                    Type = g.Key.TimeCodeType,
                                    Skill = g.Key.CourseSkill,
                                    TotalCount = g.Sum(x => x.q.CorrectTotal),
                                    TotalQuestion = g.Select(x => x.q).Count()
                                };
            var questions = await questionQuery.ToListAsync(cancellationToken);
            var answers = await answerQuery.ToListAsync(cancellationToken);
            if (questions.Sum(x => x.TotalQuestion) != answers.Sum(x => x.TotalAnswer))
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoResultErrorCode.NotEnoughQuestions));
                return methodResult;
            }
            var skills = Enum.GetValues(typeof(EnumCourseSkill)).Cast<EnumCourseSkill>();
            var types = Enum.GetValues(typeof(EnumTimeCodeType)).Cast<EnumTimeCodeType>();
            var scoreQuery = from type in types
                             select new VideoSkillScores
                             {
                                 Type = type,
                                 SkillScores = (from skill in skills
                                                join questionTimeCodeQ in questions on skill equals questionTimeCodeQ.Skill into questionTimeCodeQ_jointable
                                                from questionTimeCodeQJ in questionTimeCodeQ_jointable.DefaultIfEmpty()
                                                join answerTimeCodeQ in answerQuery on skill equals answerTimeCodeQ.Skill into answerTimeCodeQ_jointable
                                                from answerTimeCodeQJ in answerTimeCodeQ_jointable.DefaultIfEmpty()
                                                where questionTimeCodeQJ != null && answerTimeCodeQJ != null && questionTimeCodeQJ.Type == type && answerTimeCodeQJ.Type == type
                                                select new SkillScores
                                                {
                                                    Skill = skill,
                                                    TotalCount = questionTimeCodeQJ != null ? questionTimeCodeQJ.TotalCount : default,
                                                    CorrectCount = answerTimeCodeQJ != null ? answerTimeCodeQJ.CorrectCount : default,
                                                }).ToList()
                             };
            videoResult.CorrectCount = (int)scoreQuery.Where(x => x.Type == EnumTimeCodeType.Standalone).SelectMany(x => x.SkillScores!).Sum(x => x.CorrectCount);
            videoResult.CorrectTotal = (int)scoreQuery.Where(x => x.Type == EnumTimeCodeType.Standalone).SelectMany(x => x.SkillScores!).Sum(x => x.TotalCount);
            videoResult.Status = EnumResultStatus.Done;
            videoResult.Percent = videoResult.CorrectTotal != 0 ? (double)videoResult.CorrectCount / videoResult.CorrectTotal * 100 : 0;
            videoResult.VideoSkillScores = scoreQuery.ToList();
            await _videoResultRepository.ExecuteTransactionAsync(async () =>
            {
                videoResult = _videoResultRepository.Update(videoResult);
                await _videoResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<VideoResultModel>(videoResult);
                return methodResult;
            });
            return methodResult;
        }
    }
}
