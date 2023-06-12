// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLessonScoreQuery : IRequest<MethodResult<LessonScoreModel>>
    {
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
        public Guid LessonId { get; set; }
    }

    public class GetLessonScoreQueryHandler : IRequestHandler<GetLessonScoreQuery, MethodResult<LessonScoreModel>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly ITimeCodeExerciseRepository _timeCodeExerciseRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IExerciseQuestionRepository _exerciseQuestionRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;

        public GetLessonScoreQueryHandler(
            AuthContext authContext,
            IVideoRepository videoRepository,
            IVideoTimeCodeRepository videoTimeCodeRepository,
            ITimeCodeExerciseRepository timeCodeExerciseRepository,
            ILessonResultRepository lessonResultRepository,
            IVideoResultRepository videoResultRepository,
            IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository,
            IExerciseRepository exerciseRepository,
            IExerciseQuestionRepository exerciseQuestionRepository,
            IQuestionRepository questionRepository,
            IUserService userService)
        {
            _authContext = authContext;
            _lessonResultRepository = lessonResultRepository;
            _videoRepository = videoRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _timeCodeExerciseRepository = timeCodeExerciseRepository;
            _videoResultRepository = videoResultRepository;
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _exerciseRepository = exerciseRepository;
            _exerciseQuestionRepository = exerciseQuestionRepository;
            _questionRepository = questionRepository;
            _userService = userService;
        }

        public async Task<MethodResult<LessonScoreModel>> Handle(GetLessonScoreQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<LessonScoreModel>();
            var lessonScore = new LessonScoreModel();

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.UserNotExist));
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;

            var baseQuery = from lr in _lessonResultRepository.Queryable
                            join vr in _videoResultRepository.Queryable on lr.Id equals vr.LessonResultId
                            where lr.CourseId == request.CourseId &&
                                    lr.UnitId == request.UnitId &&
                                    lr.LessonId == request.LessonId &&
                                    lr.StudentId == studentId
                            select vr;

            var answerQuery = from baseQ in baseQuery
                              join vtca in _videoTimeCodeAnswerRepository.Queryable on baseQ.Id equals vtca.VideoResultId
                              join e in _exerciseRepository.Queryable on vtca.ExerciseId equals e.Id
                              join te in _timeCodeExerciseRepository.Queryable on e.Id equals te.ExerciseId
                              join vt in _videoTimeCodeRepository.Queryable on te.VideoTimeCodeId equals vt.Id
                              group new { vt, vtca } by new { vt.TimeCodeType, e.CourseSkill } into g
                              select new
                              {
                                  Type = g.Key.TimeCodeType,
                                  Skill = g.Key.CourseSkill,
                                  CorrectCount = g.Sum(x => x.vtca.CorrectCount)
                              };

            var answerTimeCodeQuery = answerQuery.Where(x => x.Type != EnumTimeCodeType.Standalone);
            var answerStaderlonQuery = answerQuery.Where(x => x.Type == EnumTimeCodeType.Standalone);

            var questionQuery = from baseQ in baseQuery
                                join v in _videoRepository.Queryable on baseQ.VideoId equals v.Id
                                join vt in _videoTimeCodeRepository.Queryable on v.Id equals vt.VideoId
                                join te in _timeCodeExerciseRepository.Queryable on vt.Id equals te.VideoTimeCodeId
                                join e in _exerciseRepository.Queryable on te.ExerciseId equals e.Id
                                join eq in _exerciseQuestionRepository.Queryable on e.Id equals eq.ExerciseId
                                join q in _questionRepository.Queryable on eq.QuestionId equals q.Id
                                group new { vt, q } by new { vt.TimeCodeType, e.CourseSkill } into g
                                select new
                                {
                                    Type = g.Key.TimeCodeType,
                                    Skill = g.Key.CourseSkill,
                                    TotalCount = g.Sum(x => x.q.CorrectTotal)
                                };

            var questions = await questionQuery.Where(x => x.Type == EnumTimeCodeType.Standalone).ToListAsync(cancellationToken);
            var questionTimeCodes = await questionQuery.Where(x => x.Type != EnumTimeCodeType.Standalone).ToListAsync(cancellationToken);

            var skills = Enum.GetValues(typeof(EnumCourseSkill)).Cast<EnumCourseSkill>();
            var types = Enum.GetValues(typeof(EnumTimeCodeType)).Cast<EnumTimeCodeType>().Where(x => x != EnumTimeCodeType.Standalone);
            var scoreQuery = from skill in skills
                             join questionQ in questions on skill equals questionQ.Skill into questionQ_jointable
                             from questionQJ in questionQ_jointable.DefaultIfEmpty()
                             join answerQ in answerStaderlonQuery on skill equals answerQ.Skill into answerQ_jointable
                             from answerQJ in answerQ_jointable.DefaultIfEmpty()
                             select new LessonSkillScoreModel
                             {
                                 Skill = skill,
                                 TotalCount = questionQJ != null ? questionQJ.TotalCount : default,
                                 CorrectCount = answerQJ != null ? answerQJ.CorrectCount : default,
                             };

            var scoreTimeCodeQuery = from type in types
                                     select new TimeCodeScoreModel
                                     {
                                         Type = type,
                                         SkillScores = (from skill in skills
                                                        join questionTimeCodeQ in questionTimeCodes on skill equals questionTimeCodeQ.Skill into questionTimeCodeQ_jointable
                                                        from questionTimeCodeQJ in questionTimeCodeQ_jointable.DefaultIfEmpty()
                                                        join answerTimeCodeQ in answerTimeCodeQuery on skill equals answerTimeCodeQ.Skill into answerTimeCodeQ_jointable
                                                        from answerTimeCodeQJ in answerTimeCodeQ_jointable.DefaultIfEmpty()
                                                        where questionTimeCodeQJ != null && answerTimeCodeQJ != null && questionTimeCodeQJ.Type == type && answerTimeCodeQJ.Type == type
                                                        select new LessonSkillScoreModel
                                                        {
                                                            Skill = skill,
                                                            TotalCount = questionTimeCodeQJ != null ? questionTimeCodeQJ.TotalCount : default,
                                                            CorrectCount = answerTimeCodeQJ != null ? answerTimeCodeQJ.CorrectCount : default,
                                                        }).ToList()
                                     };
            var timeCodeScores = scoreTimeCodeQuery.ToList();
            foreach (var item in timeCodeScores)
            {
                if (item.SkillScores != null && item.SkillScores.Count > 0)
                {
                    var correctCountTimeCode = item.SkillScores.Select(x => x.CorrectCount).Sum();
                    var totalCountTimeCode = item.SkillScores.Select(x => x.TotalCount).Sum();
                    if (totalCountTimeCode != 0)
                    {
                        item.Percent = (correctCountTimeCode / (double)totalCountTimeCode) * 100;
                    }
                }
            }
            var correctCount = scoreQuery.Select(x => x.CorrectCount).Sum();
            var totalCount = scoreQuery.Select(x => x.TotalCount).Sum();
            if (totalCount != 0)
            {
                lessonScore.Percent = (correctCount / (double)totalCount) * 100;
            }

            lessonScore.LessonSkillScores = scoreQuery.ToList();
            lessonScore.TimeCodeScores = timeCodeScores;
            methodResult.Result = lessonScore;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
