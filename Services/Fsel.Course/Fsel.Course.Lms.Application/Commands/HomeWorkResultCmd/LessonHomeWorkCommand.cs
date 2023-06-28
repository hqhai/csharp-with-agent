// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.HomeWorkResultCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class LessonHomeWorkCommand : IRequest<MethodResult<HomeWorkResultModel>>
    {
        public Guid LessonResulttId { get; set; }
    }

    public class LessonHomeWorkCommandHandler : IRequestHandler<LessonHomeWorkCommand, MethodResult<HomeWorkResultModel>>
    {
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IMapper _mapper;
        private readonly IQuestionRepository _questionRepository;
        private readonly IHomeWorkAnswerRepository _homeWorkAnswerRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IHomeWorkQuestionRepository _homeWorkQuestionRepository;

        public LessonHomeWorkCommandHandler(IHomeWorkResultRepository homeWorkResultRepository,
            IMapper mapper,
            IQuestionRepository questionRepository,
            IHomeWorkAnswerRepository homeWorkAnswerRepository,
            IHomeWorkRepository homeWorkRepository,
            IHomeWorkQuestionRepository homeWorkQuestionRepository)
        {
            _homeWorkResultRepository = homeWorkResultRepository;
            _mapper = mapper;
            _questionRepository = questionRepository;
            _homeWorkAnswerRepository = homeWorkAnswerRepository;
            _homeWorkRepository = homeWorkRepository;
            _homeWorkQuestionRepository = homeWorkQuestionRepository;
        }

        public async Task<MethodResult<HomeWorkResultModel>> Handle(LessonHomeWorkCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HomeWorkResultModel> methodResult = new MethodResult<HomeWorkResultModel>();
            var homeWorkResult = await _homeWorkResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == request.LessonResulttId, cancellationToken: cancellationToken);
            if (homeWorkResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkResultErrorCode.HomeWorkResultNotExist), nameof(request.LessonResulttId), request.LessonResulttId);
                return methodResult;
            }
            var answerQuery = from hwr in _homeWorkResultRepository.Queryable
                              join hw in _homeWorkRepository.Queryable on hwr.HomeWorkId equals hw.Id
                              join hwas in _homeWorkAnswerRepository.Queryable on hwr.Id equals hwas.HomeWorkResultId
                              where hwr.LessonResultId == request.LessonResulttId
                              group new { hw, hwas } by hw.CourseSkill into g
                              select new
                              {
                                  Skill = g.Key,
                                  CorrectCount = g.Sum(x => x.hwas.CorrectCount)
                              };

            var questionQuery = from hw in _homeWorkRepository.Queryable
                                join hq in _homeWorkQuestionRepository.Queryable on hw.Id equals hq.HomeWorkId
                                join q in _questionRepository.Queryable on hq.QuestionId equals q.Id
                                where hw.Id == homeWorkResult.HomeWorkId
                                group new { hw, q } by hw.CourseSkill into g
                                select new
                                {
                                    Skill = g.Key,
                                    TotalCount = g.Sum(x => x.q.CorrectTotal)
                                };
            var questions = await questionQuery.ToListAsync(cancellationToken);
            var skills = Enum.GetValues(typeof(EnumCourseSkill)).Cast<EnumCourseSkill>();
            var scoreQuery = from skill in skills
                             join questionTimeCodeQ in questions on skill equals questionTimeCodeQ.Skill into questionTimeCodeQ_jointable
                             from questionTimeCodeQJ in questionTimeCodeQ_jointable.DefaultIfEmpty()
                             join answerTimeCodeQ in answerQuery on skill equals answerTimeCodeQ.Skill into answerTimeCodeQ_jointable
                             from answerTimeCodeQJ in answerTimeCodeQ_jointable.DefaultIfEmpty()
                             select new SkillScores
                             {
                                 Skill = skill,
                                 TotalCount = questionTimeCodeQJ != null ? questionTimeCodeQJ.TotalCount : default,
                                 CorrectCount = answerTimeCodeQJ != null ? answerTimeCodeQJ.CorrectCount : default,
                             };
            homeWorkResult.CorrectCount = await answerQuery.SumAsync(x => x.CorrectCount, cancellationToken);
            homeWorkResult.CorrectTotal = await questionQuery.SumAsync(x => x.TotalCount, cancellationToken);
            homeWorkResult.Status = EnumResultStatus.Done;
            homeWorkResult.SkillScores = scoreQuery.ToList();
            homeWorkResult.Percent = homeWorkResult.CorrectTotal != 0 ? (double)homeWorkResult.CorrectCount / homeWorkResult.CorrectTotal * 100 : 0;

            await _homeWorkResultRepository.ExecuteTransactionAsync(async () =>
            {
                homeWorkResult = _homeWorkResultRepository.Update(homeWorkResult);
                await _homeWorkResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<HomeWorkResultModel>(homeWorkResult);
                return methodResult;
            });
            return methodResult;
        }
    }
}
