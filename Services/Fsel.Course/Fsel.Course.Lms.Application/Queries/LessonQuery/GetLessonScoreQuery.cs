// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
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
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly AuthContext _authContext;

        public GetLessonScoreQueryHandler(
            AuthContext authContext,
            ILessonResultRepository lessonResultRepository,
            IVideoResultRepository videoResultRepository,
            IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository,
            IExerciseRepository exerciseRepository,
            IQuestionRepository questionRepository
            )
        {
            _authContext = authContext;
            _lessonResultRepository = lessonResultRepository;
            _videoResultRepository = videoResultRepository;
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _exerciseRepository = exerciseRepository;
            _questionRepository = questionRepository;
        }

        public async Task<MethodResult<LessonScoreModel>> Handle(GetLessonScoreQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<LessonScoreModel>();

            var lessonScore = new LessonScoreModel();

            var lessonSkillScoreQuery = from lr in _lessonResultRepository.Queryable
                                        join vr in _videoResultRepository.Queryable on lr.Id equals vr.LessonResultId
                                        join vtca in _videoTimeCodeAnswerRepository.Queryable on vr.Id equals vtca.VideoResultId
                                        join e in _exerciseRepository.Queryable on vtca.ExerciseId equals e.Id
                                        join q in _questionRepository.Queryable on vtca.QuestionId equals q.Id
                                        where lr.CourseId == request.CourseId &&
                                              lr.UnitId == request.UnitId &&
                                              lr.LessonId == request.LessonId &&
                                              lr.StudentId == _authContext.CurrentUserId
                                        group new { q, vtca } by e.CourseSkill into g
                                        select new LessonSkillScoreModel
                                        {
                                            Skill = g.Key,
                                            TotalCount = g.Select(x => x.q).Sum(x => x.CorrectTotal),
                                            CorrectCount = g.Select(x => x.vtca).Sum(x => x.CorrectCount)
                                        };
            var correctCount = lessonSkillScoreQuery.Select(x => x.CorrectCount).Sum();
            var totalCount = lessonSkillScoreQuery.Select(x => x.TotalCount).Sum();
            if (totalCount != 0)
            {
                lessonScore.Percent = (correctCount / totalCount) * 100;
            }

            lessonScore.LessonSkillScores = await lessonSkillScoreQuery.ToListAsync(cancellationToken);
            methodResult.Result = lessonScore;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
