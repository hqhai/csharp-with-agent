// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.DashboardQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLessonOverviewQuery : IRequest<MethodResult<LessonDashboardModel>>
    {
    }

    public class GetLessonDashboardQueryHandler : IRequestHandler<GetLessonOverviewQuery, MethodResult<LessonDashboardModel>>
    {
        private readonly IUserService _userService;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ITimeCodeExerciseRepository _timeCodeExerciseRepository;
        private readonly IExerciseQuestionRepository _exerciseQuestionRepository;
        private readonly IVideoTimeCodeAnswerRepository _videoTimeCodeAnswerRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;

        public GetLessonDashboardQueryHandler(IUserService userService,
            ILessonResultRepository lessonResultRepository,
            IVideoResultRepository videoResultRepository,
            IExerciseRepository exerciseRepository,
            IVideoRepository videoRepository,
            IQuestionRepository questionRepository,
            ITimeCodeExerciseRepository timeCodeExerciseRepository,
            IExerciseQuestionRepository exerciseQuestionRepository,
            IVideoTimeCodeAnswerRepository videoTimeCodeAnswerRepository,
            IVideoTimeCodeRepository videoTimeCodeRepository,
            IMapper mapper,
            AuthContext authContext)
        {
            _userService = userService;
            _lessonResultRepository = lessonResultRepository;
            _videoResultRepository = videoResultRepository;
            _exerciseRepository = exerciseRepository;
            _videoRepository = videoRepository;
            _questionRepository = questionRepository;
            _timeCodeExerciseRepository = timeCodeExerciseRepository;
            _exerciseQuestionRepository = exerciseQuestionRepository;
            _videoTimeCodeAnswerRepository = videoTimeCodeAnswerRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _mapper = mapper;
            _authContext = authContext;
        }

        public async Task<MethodResult<LessonDashboardModel>> Handle(GetLessonOverviewQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonDashboardModel> methodResult = new MethodResult<LessonDashboardModel>();

            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentsResult));
                return methodResult;
            }
            var studentId = studentsResult.Content?.Result?.Id;
            var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.Lesson)
                                                                    .ThenInclude(x => x!.LessonInstructions)
                                                                    .Include(x => x.VideoResult)
                                                                    .Include(x => x.HomeWorkResults.Where(x => x.StudentId == studentId))
                                                                    .Include(x => x.ClassForumResults.Where(x => x.StudentId == studentId))
                                                                    .Where(x => x.StudentId == studentId && (x.Status == EnumResultStatus.New || x.Status == EnumResultStatus.Process))
                                                                    .AsNoTracking()
                                                                    .FirstOrDefaultAsync(cancellationToken);

            if (lessonResult == null)
            {
                lessonResult = await _lessonResultRepository.Queryable.Include(x => x.Unit)
                                                                  .ThenInclude(x => x!.UnitSkillMockTests)
                                                                  .Include(x => x.Lesson)
                                                                  .ThenInclude(x => x!.LessonInstructions)
                                                                  .Include(x => x.VideoResult)
                                                                  .Include(x => x.HomeWorkResults.Where(x => x.StudentId == studentId))
                                                                  .Include(x => x.ClassForumResults.Where(x => x.StudentId == studentId))
                                                                  .Where(x => x.StudentId == studentId && x.Status == EnumResultStatus.Done)
                                                                  .OrderByDescending(x => x.CreatedDate)
                                                                  .AsNoTracking()
                                                                  .FirstOrDefaultAsync(cancellationToken);
            }
            if (lessonResult == null)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var lesson = lessonResult.Lesson;
            if (lesson == null)
            {
                methodResult.Result = null;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }

            var lessonDashBoard = new LessonDashboardModel
            {
                Id = lesson.Id,
                Name = lesson.Name,
                CourseLevel = lesson.CourseLevel,
                InstructionContent = lesson.InstructionContent,
                LessonInstructions = _mapper.Map<IList<LessonInstructionModel>>(lesson.LessonInstructions.OrderBy(x => x.CreatedDate).ToList()),
                LessonResult = _mapper.Map<LessonResultModel>(lessonResult),
            };
            var mockTestId = lessonResult.Unit?.UnitSkillMockTests.FirstOrDefault()?.MockTestId ?? null;
            if (mockTestId != null)
            {
                lessonDashBoard.MockTestId = mockTestId;
            }
            var baseQuery = from lr in _lessonResultRepository.Queryable
                            join vr in _videoResultRepository.Queryable on lr.Id equals vr.LessonResultId
                            where lr.Id == lessonResult.Id
                            select vr;

            var answerQuery = from baseQ in baseQuery
                              join vtca in _videoTimeCodeAnswerRepository.Queryable on baseQ.Id equals vtca.VideoResultId
                              join e in _exerciseRepository.Queryable on vtca.ExerciseId equals e.Id
                              join te in _timeCodeExerciseRepository.Queryable on e.Id equals te.ExerciseId
                              join vt in _videoTimeCodeRepository.Queryable on te.VideoTimeCodeId equals vt.Id
                              where vt.TimeCodeType == EnumTimeCodeType.Standalone
                              group new { vt, vtca } by new { vt.TimeCodeType, e.CourseSkill } into g
                              select new
                              {
                                  Type = g.Key.TimeCodeType,
                                  Skill = g.Key.CourseSkill,
                                  CorrectCount = g.Sum(x => x.vtca.CorrectCount)
                              };
            var questionQuery = from baseQ in baseQuery
                                join v in _videoRepository.Queryable on baseQ.VideoId equals v.Id
                                join vt in _videoTimeCodeRepository.Queryable on v.Id equals vt.VideoId
                                join te in _timeCodeExerciseRepository.Queryable on vt.Id equals te.VideoTimeCodeId
                                join e in _exerciseRepository.Queryable on te.ExerciseId equals e.Id
                                join eq in _exerciseQuestionRepository.Queryable on e.Id equals eq.ExerciseId
                                join q in _questionRepository.Queryable on eq.QuestionId equals q.Id
                                where q.Ungraded == false && vt.TimeCodeType == EnumTimeCodeType.Standalone
                                group new { vt, q } by new { vt.TimeCodeType, e.CourseSkill } into g
                                select new
                                {
                                    Type = g.Key.TimeCodeType,
                                    Skill = g.Key.CourseSkill,
                                    TotalCount = g.Sum(x => x.q.CorrectTotal)
                                };

            var questions = await questionQuery.ToListAsync(cancellationToken);
            var skills = Enum.GetValues(typeof(EnumCourseSkill)).Cast<EnumCourseSkill>();
            var scoreQuery = from skill in skills
                             join questionQ in questions on skill equals questionQ.Skill into questionQ_jointable
                             from questionQJ in questionQ_jointable.DefaultIfEmpty()
                             join answerQ in answerQuery on skill equals answerQ.Skill into answerQ_jointable
                             from answerQJ in answerQ_jointable.DefaultIfEmpty()
                             select new SkillScores
                             {
                                 Skill = skill,
                                 TotalCount = questionQJ != null ? questionQJ.TotalCount : default,
                                 CorrectCount = answerQJ != null ? answerQJ.CorrectCount : default,
                             };

            var correctCount = scoreQuery.Select(x => x.CorrectCount).Sum();
            var totalCount = scoreQuery.Select(x => x.TotalCount).Sum();
            if (totalCount != 0)
            {
                lessonDashBoard.Percent = (correctCount / (double)totalCount) * 100;
            }
            var homeWorks = lessonResult.HomeWorkResults.Where(x => x.StudentId == studentId && x.LessonResultId == lessonResult.Id).ToList();
            var classForumResult = lessonResult.ClassForumResults.FirstOrDefault(x => x.StudentId == studentId && x.LessonResultId == lessonResult.Id);
            var (statusVideo, numberVideo) = GetStatusVideo(lessonResult.VideoResult);
            var (statusClassForum, numberClassForum) = GetStatusClassForums(classForumResult, lessonDashBoard.StatusVideo);
            var (statusHomeWork, numberHomeWork) = GetStatusHomeWorks(homeWorks, lessonDashBoard.StatusClassForum);
            lessonDashBoard.StatusVideo = statusVideo;
            lessonDashBoard.StatusClassForum = statusClassForum;
            lessonDashBoard.StatusHomeWork = statusHomeWork;
            var numbers = new List<int> { numberClassForum, numberVideo, numberHomeWork };
            lessonDashBoard.PercentProgress = Math.Round((double)numbers.Average() * 100, 0);
            methodResult.Result = lessonDashBoard;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public (EnumResultStatus, int) GetStatusVideo(VideoResult? videoResult)
        {
            if (videoResult != null)
            {
                if (videoResult.Status == EnumResultStatus.Process || videoResult.Status == EnumResultStatus.New)
                {
                    return (EnumResultStatus.Process, 0);
                }
                else
                {
                    return (EnumResultStatus.Done, 1);
                }
            }
            return (EnumResultStatus.Unfinished, 0);
        }

        public (EnumResultStatus, int) GetStatusHomeWorks(IList<HomeWorkResult>? homeWorkResults, EnumResultStatus status)
        {
            var statusHomeWork = EnumResultStatus.Unfinished;
            if (status == EnumResultStatus.Done)
            {
                statusHomeWork = EnumResultStatus.Process;
            }
            if (homeWorkResults != null && homeWorkResults.Count > 0)
            {
                if (homeWorkResults.All(x => x.Status == EnumResultStatus.Done))
                {
                    return (EnumResultStatus.Done, 1);
                }
                else if (homeWorkResults.All(x => x.Status == EnumResultStatus.Unfinished))
                {
                    return (EnumResultStatus.Unfinished, 0);
                }
                else if (homeWorkResults.Any(x => x.Status == EnumResultStatus.Process))
                {
                    return (EnumResultStatus.Process, 0);
                }
            }
            return (statusHomeWork, 0);
        }

        public (EnumResultStatus, int) GetStatusClassForums(ClassForumResult? classForumResult, EnumResultStatus status)
        {
            var statusClassForum = EnumResultStatus.Unfinished;
            if (status == EnumResultStatus.Done)
            {
                statusClassForum = EnumResultStatus.Process;
            }
            if (classForumResult != null)
            {
                if (classForumResult.Status == EnumClassForumResultStatus.Graded)
                {
                    return (EnumResultStatus.Done, 1);
                }
                else if (classForumResult.Status == EnumClassForumResultStatus.Pending || classForumResult.Status == EnumClassForumResultStatus.PendingForGrading)
                {
                    return (EnumResultStatus.Process, 0);
                }
            }
            return (statusClassForum, 0);
        }
    }
}
