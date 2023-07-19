// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoResultCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.VideoResults;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ReviewLessonVideoCommand : ReviewLessonVideoCommandModel, IRequest<MethodResult<VideoResultModel>>
    {
    }

    public class ReviewLessonVideoCommandHandler : IRequestHandler<ReviewLessonVideoCommand, MethodResult<VideoResultModel>>
    {
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly ITimeCodeExerciseRepository _timeCodeExerciseRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IExerciseQuestionRepository _exerciseQuestionRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IMapper _mapper;

        public ReviewLessonVideoCommandHandler(IVideoResultRepository videoResultRepository,
            IMapper mapper,
            IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository,
            ITimeCodeExerciseRepository timeCodeExerciseRepository,
            IExerciseRepository exerciseRepository,
            IVideoRepository videoRepository,
            IVideoTimeCodeRepository videoTimeCodeRepository,
            IExerciseQuestionRepository exerciseQuestionRepository,
            IQuestionRepository questionRepository)
        {
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _timeCodeExerciseRepository = timeCodeExerciseRepository;
            _exerciseRepository = exerciseRepository;
            _videoRepository = videoRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _exerciseQuestionRepository = exerciseQuestionRepository;
            _questionRepository = questionRepository;
            _videoResultRepository = videoResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<VideoResultModel>> Handle(ReviewLessonVideoCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoResultModel> methodResult = new MethodResult<VideoResultModel>();

            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == request.LessonResulttId, cancellationToken: cancellationToken);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoResultErrorCode.VideoResultNotExist), nameof(request.LessonResulttId), request.LessonResulttId);
                return methodResult;
            }

            _mapper.Map(request, videoResult);
            if (!videoResult.IsValid())
            {
                methodResult.AddErrorBadRequest(videoResult.ErrorMessages);
                return methodResult;
            }

            var answerQuery = from baseQ in _videoResultRepository.Queryable
                              join vtca in _videoTimeCodeAnswerRepository.Queryable on baseQ.Id equals vtca.VideoResultId
                              join e in _exerciseRepository.Queryable on vtca.ExerciseId equals e.Id
                              join te in _timeCodeExerciseRepository.Queryable on e.Id equals te.ExerciseId
                              join vt in _videoTimeCodeRepository.Queryable on te.VideoTimeCodeId equals vt.Id
                              where baseQ.Id == videoResult.Id
                              group new { vt, vtca } by new { vt.TimeCodeType, e.CourseSkill } into g
                              select new
                              {
                                  Type = g.Key.TimeCodeType,
                                  Skill = g.Key.CourseSkill,
                                  CorrectCount = g.Sum(x => x.vtca.CorrectCount)
                              };

            var questionQuery = from baseQ in _videoResultRepository.Queryable
                                join v in _videoRepository.Queryable on baseQ.VideoId equals v.Id
                                join vt in _videoTimeCodeRepository.Queryable on v.Id equals vt.VideoId
                                join te in _timeCodeExerciseRepository.Queryable on vt.Id equals te.VideoTimeCodeId
                                join e in _exerciseRepository.Queryable on te.ExerciseId equals e.Id
                                join eq in _exerciseQuestionRepository.Queryable on e.Id equals eq.ExerciseId
                                join q in _questionRepository.Queryable on eq.QuestionId equals q.Id
                                where baseQ.Id == videoResult.Id
                                group new { vt, q } by new { vt.TimeCodeType, e.CourseSkill } into g
                                select new
                                {
                                    Type = g.Key.TimeCodeType,
                                    Skill = g.Key.CourseSkill,
                                    TotalCount = g.Sum(x => x.q.CorrectTotal)
                                };
            var questions = await questionQuery.ToListAsync(cancellationToken);
            var answers = await answerQuery.ToListAsync(cancellationToken);
            if (questions.Count != answers.Count)
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
