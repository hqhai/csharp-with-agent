// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.OtherFeatureQuery
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;

    public class GetParamOverallScoreQuery : IRequest<MethodResult<object>>
    {
        public Guid StudentId { get; set; }
        public Guid? UnitId { get; set; }
        public string? Type { get; set; }
    }

    public class GetParamOverallScoreQueryHandler : IRequestHandler<GetParamOverallScoreQuery, MethodResult<object>>
    {
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitRepository _unitRepository;

        public GetParamOverallScoreQueryHandler(IFinalTestResultRepository finalTestResultRepository,
            IClassForumResultRepository classForumResultRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            IVideoResultRepository videoResultRepository,
            ICourseResultRepository courseResultRepository,
            ILessonResultRepository lessonResultRepository,
            IUnitRepository unitRepository)
        {
            _finalTestResultRepository = finalTestResultRepository;
            _classForumResultRepository = classForumResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _videoResultRepository = videoResultRepository;
            _courseResultRepository = courseResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _unitRepository = unitRepository;
        }

        public async Task<MethodResult<object>> Handle(GetParamOverallScoreQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<object> methodResult = new MethodResult<object>();

            if (request.Type == nameof(Course))
            {
                methodResult.Result = await GetCourseResult(request);
            }
            else if (request.Type == nameof(Domain.Entities.Unit) && request.UnitId.HasValue)
            {
                methodResult.Result = await GetUnitSkillScores(request);
            }
            return methodResult;
        }

        private async Task<object> GetCourseResult(GetParamOverallScoreQuery request)
        {
            ArgumentNullException.ThrowIfNull(request);
            var courseResult = await _courseResultRepository.Queryable.Include(x => x.Course).Where(x => x.StudentId == request.StudentId && x.WorkingStatus == EnumWorkingStatus.Active).FirstOrDefaultAsync();
            var courseType = courseResult?.Course?.CourseType;
            if (courseResult == null || courseResult.Course == null)
            {
                return new object();
            }

            var units = await _unitRepository.Queryable.Include(x => x.LessonResults.Where(x => x.StudentId == request.StudentId && x.CourseId == courseResult.CourseId))
                                                       .Where(x => x.CourseUnitMockTests.Any(y => y.CourseId == courseResult.CourseId))
                                                       .ToListAsync();
            var unitIds = units.Select(x => x.Id).ToList();
            var lessonResultIds = units.SelectMany(x => x.LessonResults).Where(x => x.StudentId == courseResult.StudentId && x.CourseId == courseResult.CourseId).Select(x => x.Id).ToList();

            var (videoSkillScores, percentVideo) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.Standalone, courseType: courseType);
            var (homeWorkSkillScores, percentHomeWork) = await GetHomeWordsSkillScores(lessonResultIds, courseType: courseType);
            var (classForumSkillScores, percentClassForum) = await GetClassForumSkillScores(lessonResultIds, courseType: courseType);

            var percents = new List<double> { percentClassForum, percentHomeWork, percentVideo };
            List<SkillScores> mergedSkillScores = videoSkillScores.Concat(homeWorkSkillScores).Concat(classForumSkillScores).ToList();
            if (courseType == EnumCourseType.Academic)
            {
                var (unitSkillScores, percentUnitSkill, listPercentSkillTest) = await GetSkillScoreByCourses(courseResult.CourseId, unitIds, courseResult.StudentId, EnumTimeCodeType.UnitTest, OverallPercentCourse.OverallAcaPercentUnitTest);
                if (!unitSkillScores.Any())
                {
                    percentUnitSkill = OverallPercentCourse.OverallAcaPercentUnitTest;
                }
                var (skillSkillScores, percentSkill, listPercentUnitTest) = await GetSkillScoreByCourses(courseResult.CourseId, unitIds, courseResult.StudentId, EnumTimeCodeType.SkillTest, OverallPercentCourse.OverallAcaPercentSkillTest);
                if (!skillSkillScores.Any())
                {
                    percentSkill = OverallPercentCourse.OverallAcaPercentSkillTest;
                }
                var (finalTestSkillScores, percentFinalTest) = await GetFinalTestSkillScore(courseResult.Course, courseResult.StudentId);
                return new
                {
                    VideoSkillScores = videoSkillScores,
                    PercentVideo = percentVideo,
                    HomeWorkSkillScores = homeWorkSkillScores,
                    PercentHomeWork = percentHomeWork,
                    ClassForumSkillScores = classForumSkillScores,
                    PercentClassForum = percentClassForum,
                    UnitSkillScores = unitSkillScores,
                    PercentUnitSkill = percentUnitSkill,
                    SkillTestSkillScores = skillSkillScores,
                    PercentSkillTest = percentSkill,
                    FinalTestSkillScores = finalTestSkillScores,
                    PercentFinalTest = percentFinalTest,
                    OverallPercentUnitTest = listPercentUnitTest,
                    OverallPercentSkillTest = listPercentSkillTest,
                };
            }
            if (courseType == EnumCourseType.EnglishFoundation)
            {
                var (unitSkillScores, percentUnitSkill, listPercentUnitTest) = await GetSkillScoreByCourses(courseResult.CourseId, unitIds, courseResult.StudentId, EnumTimeCodeType.UnitTest, OverallPercentCourse.OverallRFIPercentUnitTest);
                if (!unitSkillScores.Any())
                {
                    percentUnitSkill = OverallPercentCourse.OverallRFIPercentUnitTest;
                }
                var (finalTestSkillScores, percentFinalTest) = await GetFinalTestSkillScore(courseResult.Course, courseResult.StudentId);
                return new
                {
                    VideoSkillScores = videoSkillScores,
                    PercentVideo = percentVideo,
                    HomeWorkSkillScores = homeWorkSkillScores,
                    PercentHomeWork = percentHomeWork,
                    ClassForumSkillScores = classForumSkillScores,
                    PercentClassForum = percentClassForum,
                    UnitSkillScores = unitSkillScores,
                    PercentUnitSkill = percentUnitSkill,
                    FinalTestSkillScores = finalTestSkillScores,
                    PercentFinalTest = percentFinalTest,
                    OverallPercentUnitTest = listPercentUnitTest,
                };
            }
            return new
            {
                VideoSkillScores = videoSkillScores,
                PercentVideo = percentVideo,
                HomeWorkSkillScores = homeWorkSkillScores,
                PercentHomeWork = percentHomeWork,
                ClassForumSkillScores = classForumSkillScores,
                PercentClassForum = percentClassForum,
            };
        }

        private async Task<object> GetUnitSkillScores(GetParamOverallScoreQuery request)
        {
            var courseResult = await _courseResultRepository.Queryable.Include(x => x.Course).Where(x => x.StudentId == request.StudentId && x.WorkingStatus == EnumWorkingStatus.Active).FirstOrDefaultAsync();
            var courseType = courseResult?.Course?.CourseType;
            if (courseResult == null || courseResult.Course == null)
            {
                return new object();
            }
            var lessonResultIds = await _lessonResultRepository.Queryable.Where(x => request.UnitId.HasValue && x.StudentId == courseResult.StudentId && x.UnitId == request.UnitId && x.CourseId == courseResult.CourseId).Select(x => x.Id).ToListAsync();
            if (courseType == EnumCourseType.Academic)
            {
                var (videoSkillScores, percentVideo) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.Standalone, OverallPercentUnit.OverallAcaPercentVideo);
                var (unitTestSkillScores, percentUnitTest) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.UnitTest, OverallPercentUnit.OverallAcaPercentUnitTest);
                if (!unitTestSkillScores.Any())
                {
                    percentUnitTest = OverallPercentUnit.OverallAcaPercentUnitTest;
                }
                var (skillTestSkillScores, percentSkillTest) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.SkillTest, OverallPercentUnit.OverallAcaPercentSkillTest);
                if (!skillTestSkillScores.Any())
                {
                    percentSkillTest = OverallPercentUnit.OverallAcaPercentSkillTest;
                }
                var (homeWorkSkillScores, percentHomeWork) = await GetHomeWordsSkillScores(lessonResultIds, OverallPercentUnit.OverallAcaPercentHomeWork);
                var (classForumSkillScores, percentClassForum) = await GetClassForumSkillScores(lessonResultIds, OverallPercentUnit.OverallAcaPercentClassForum);
                return new
                {
                    VideoSkillScores = videoSkillScores,
                    PercentVideo = percentVideo,
                    UnitTestSkillScores = unitTestSkillScores,
                    PercentUnitTest = percentUnitTest,
                    SkillTestSkillScores = skillTestSkillScores,
                    PercentSkillTest = percentSkillTest,
                    HomeWorkSkillScores = homeWorkSkillScores,
                    PercentHomeWork = percentHomeWork,
                    ClassForumSkillScores = classForumSkillScores,
                    PercentClassForum = percentClassForum,
                };
            }
            else if (courseType == EnumCourseType.Ielts)
            {
                var (videoSkillScores, percentVideo) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.Standalone, courseType: courseType);
                var (homeWorkSkillScores, percentHomeWork) = await GetHomeWordsSkillScores(lessonResultIds, courseType: courseType);
                var (classForumSkillScores, percentClassForum) = await GetClassForumSkillScores(lessonResultIds, courseType: courseType);
                return new
                {
                    VideoSkillScores = videoSkillScores,
                    PercentVideo = percentVideo,
                    HomeWorkSkillScores = homeWorkSkillScores,
                    PercentHomeWork = percentHomeWork,
                    ClassForumSkillScores = classForumSkillScores,
                    PercentClassForum = percentClassForum,
                };
            }
            else if (courseType == EnumCourseType.EnglishFoundation)
            {
                var (videoSkillScores, percentVideo) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.Standalone, OverallPercentUnit.OverallRFIPercentVideo);
                var (unitTestSkillScores, percentUnitTest) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.UnitTest, OverallPercentUnit.OverallRFIPercentUnitTest);
                if (!unitTestSkillScores.Any())
                {
                    percentUnitTest = OverallPercentUnit.OverallRFIPercentUnitTest;
                }
                var (homeWorkSkillScores, percentHomeWork) = await GetHomeWordsSkillScores(lessonResultIds, OverallPercentUnit.OverallRFIPercentHomeWork);
                var (classForumSkillScores, percentClassForum) = await GetClassForumSkillScores(lessonResultIds, OverallPercentUnit.OverallRFIPercentClassForum);
                return new
                {
                    VideoSkillScores = videoSkillScores,
                    PercentVideo = percentVideo,
                    UnitTestSkillScores = unitTestSkillScores,
                    PercentUnitTest = percentUnitTest,
                    HomeWorkSkillScores = homeWorkSkillScores,
                    PercentHomeWork = percentHomeWork,
                    ClassForumSkillScores = classForumSkillScores,
                    PercentClassForum = percentClassForum,
                };
            }

            return new object();
        }

        public async Task<(List<SkillScores>, double)> GetVideoSkillScores(IList<Guid>? lessonResultIds, EnumTimeCodeType type, int percentSkill = default, EnumCourseType? courseType = null)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            List<SkillScores> skillScores = new List<SkillScores>();
            var videoResults = await _videoResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && lessonResultIds.Contains(x.LessonResultId)).ToListAsync();
            if (videoResults != null && videoResults.Any())
            {
                skillScores = videoResults.Where(x => x.VideoSkillScores != null && x.VideoSkillScores.Any()).SelectMany(x => x.VideoSkillScores!)
                    .Where(x => x.Type == type && x.SkillScores != null && x.SkillScores.Any()).SelectMany(x => x.SkillScores!).GroupBy(x => x.Skill).Select(x => GetSkillScore(x)).ToList();
            }

            if (courseType.HasValue && courseType == EnumCourseType.Ielts)
            {
                return (skillScores, skillScores.Any() ? NumberHelper.ConvertDoublePercent(skillScores.Sum(x =>
                {
                    if (x.Skill == EnumCourseSkill.Writing || x.Skill == EnumCourseSkill.Speaking)
                    {
                        return NumberHelper.ConvertRound(x.Percent * OverallPercentCourse.SkillSWIELTSPercentVideo);
                    }
                    return NumberHelper.ConvertRound(x.Percent * OverallPercentCourse.SkillIELTSPercentVideo);
                })) : default);
            }
            if (courseType.HasValue)
            {
                switch (courseType.Value)
                {
                    case EnumCourseType.Academic:
                        percentSkill = OverallPercentCourse.OverallAcaPercentVideo;
                        break;

                    case EnumCourseType.EnglishFoundation:
                        percentSkill = OverallPercentCourse.OverallRFIPercentVideo;
                        break;
                }
            }
            return (skillScores, GetDoublePercent(skillScores, percentSkill));
        }

        public async Task<(List<SkillScores>, double)> GetClassForumSkillScores(IList<Guid>? lessonResultIds, int percentSkill = default, EnumCourseType? courseType = null)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            List<SkillScores> skillScores = new List<SkillScores>();
            var classForumResults = await _classForumResultRepository.Queryable.Where(x => x.Status.HasValue).Where(x => lessonResultIds.Contains(x.LessonResultId)).ToListAsync();
            if (classForumResults != null && classForumResults.Any())
            {
                skillScores = classForumResults.Where(x => x.SkillScores != null && x.SkillScores.Any())
                                               .SelectMany(x => x.SkillScores!)
                                               .GroupBy(x => x.Skill)
                                               .Select(x => GetSkillScore(x))
                                               .ToList();
            }
            if (courseType.HasValue)
            {
                switch (courseType.Value)
                {
                    case EnumCourseType.Academic:
                        percentSkill = OverallPercentCourse.OverallAcaPercentClassForum;
                        break;

                    case EnumCourseType.Ielts:
                        percentSkill = OverallPercentCourse.OverallIELTSPercentClassForum;
                        break;

                    case EnumCourseType.EnglishFoundation:
                        percentSkill = OverallPercentCourse.OverallRFIPercentClassForum;
                        break;
                }
            }
            return (skillScores, GetDoublePercent(skillScores, percentSkill));
        }

        public async Task<(List<SkillScores>, double)> GetHomeWordsSkillScores(IList<Guid>? lessonResultIds, int percentSkill = default, EnumCourseType? courseType = null)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            List<SkillScores> skillScores = new List<SkillScores>();
            var homeWorkResults = await _homeWorkResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && lessonResultIds.Contains(x.LessonResultId)).ToListAsync();
            if (homeWorkResults != null)
            {
                skillScores = homeWorkResults.Where(x => x.SkillScores != null && x.SkillScores.Any()).SelectMany(x => x.SkillScores!).GroupBy(x => x.Skill).Select(x => GetSkillScore(x)).ToList();
            }
            if (courseType.HasValue)
            {
                switch (courseType.Value)
                {
                    case EnumCourseType.Academic:
                        percentSkill = OverallPercentCourse.OverallAcaPercentHomeWork;
                        break;

                    case EnumCourseType.Ielts:
                        percentSkill = OverallPercentCourse.OverallIELTSPercentHomeWork;
                        break;

                    case EnumCourseType.EnglishFoundation:
                        percentSkill = OverallPercentCourse.OverallRFIPercentHomeWork;
                        break;
                }
            }
            return (skillScores, GetDoublePercent(skillScores, percentSkill));
        }

        private static double GetDoublePercent(IList<SkillScores>? skillScores, int percentOccupy, int numberOfElements = default)
        {
            if (skillScores == null || !skillScores.Any())
            {
                return default;
            }
            if (numberOfElements != default)
            {
                return NumberHelper.ConvertDoublePercent(skillScores.Average(x => x.Percent * percentOccupy / numberOfElements));
            }
            else
            {
                return NumberHelper.ConvertDoublePercent(skillScores.Sum(x => x.Percent * percentOccupy / skillScores.Count));
            }
        }

        public static SkillScores GetSumSkillScore(IGrouping<EnumCourseSkill, SkillScores>? group)
        {
            if (group != null)
            {
                return new SkillScores
                {
                    Skill = group.Key,
                    Scores = group.Average(x => x.Scores),
                    TotalCount = group.Sum(x => x.TotalCount),
                    CorrectCount = group.Sum(x => x.CorrectCount),
                    CountQuestion = group.Sum(x => x.CountQuestion),
                    TotalQuestion = group.Sum(x => x.TotalQuestion),
                };
            }
            return new SkillScores();
        }

        public static SkillScores GetSkillScore(IGrouping<EnumCourseSkill, SkillScores>? x)
        {
            SkillScores skillScores = new SkillScores();
            if (x != null)
            {
                skillScores.Skill = x.Key;
                skillScores.TotalQuestion = x.Sum(x => x.TotalQuestion);
                skillScores.CountQuestion = x.Sum(x => x.CountQuestion);
                skillScores.TotalCount = x.Sum(x => x.TotalCount);
                skillScores.CorrectCount = x.Sum(x => x.CorrectCount);
                return skillScores;
            };
            return skillScores;
        }

        private async Task<(List<SkillScores>, double, IList<double>)> GetSkillScoreByCourses(Guid courseId, IList<Guid>? unitIds, Guid studentId, EnumTimeCodeType type, int percentSkill = default)
        {
            ArgumentNullException.ThrowIfNull(unitIds);
            var skillScorePercents = new List<(List<SkillScores>, double)>();
            var units = await _unitRepository.Queryable.Include(x => x.LessonResults.Where(x => x.StudentId == studentId && x.CourseId == courseId && x.Status == EnumResultStatus.Done))
                                                      .Where(x => unitIds.Contains(x.Id))
                                                      .ToListAsync();
            units = units.OrderBy(x => unitIds.IndexOf(x.Id)).ToList();
            var listLessonResultId = units.SelectMany(x => x.LessonResults).Where(x => x.StudentId == studentId && x.CourseId == courseId && x.Status == EnumResultStatus.Done)
                                       .Select(x => x.Id).ToList();

            var videoResults = await _videoResultRepository.Queryable.Where(x => listLessonResultId.Contains(x.LessonResultId) && x.Status == EnumResultStatus.Done).ToListAsync();
            foreach (var unit in units)
            {
                var lessonResultIds = unit.LessonResults.Where(x => x.StudentId == studentId && x.CourseId == courseId && x.Status == EnumResultStatus.Done).Select(x => x.Id).ToList();
                if (lessonResultIds == null || !lessonResultIds.Any())
                {
                    continue;
                }
                var listVideoResults = videoResults.Where(x => lessonResultIds.Contains(x.LessonResultId)).ToList();
                var videoSkillScore = listVideoResults.Where(x => x.VideoSkillScores != null && x.VideoSkillScores.Any())
                                                      .SelectMany(x => x.VideoSkillScores!)
                                                      .FirstOrDefault(x => x.Type == type && x.SkillScores != null && x.SkillScores.Any());
                if (videoSkillScore != null && videoSkillScore.SkillScores != null && videoSkillScore.SkillScores.Any())
                {
                    var skillScores = videoSkillScore.SkillScores.GroupBy(x => x.Skill)
                       .Select(x => GetSkillScore(x))
                       .ToList();
                    var percent = GetDoublePercent(skillScores, percentSkill, unitIds.Count);
                    skillScorePercents.Add((skillScores, percent));
                }
            }
            if (skillScorePercents.Any())
            {
                var skillScoreSkills = skillScorePercents.SelectMany(x => x.Item1).GroupBy(x => x.Skill).Select(x => GetSkillScore(x)).ToList();
                return (skillScoreSkills, NumberHelper.ConvertRound(skillScorePercents.Sum(x => x.Item2)), skillScorePercents.Select(x => x.Item2).ToList());
            }

            return (new List<SkillScores>(), default, skillScorePercents.Select(x => x.Item2).ToList());
        }

        private async Task<(List<SkillScores>, double)> GetFinalTestSkillScore(Course course, Guid studentId)
        {
            var finalTestResult = await _finalTestResultRepository.Queryable.Where(x => x.CourseId == course.Id).FirstOrDefaultAsync(x => x.StudentId == studentId && x.Status == EnumResultStatus.Done);
            var skillScores = finalTestResult?.SkillScores?.GroupBy(x => x.Skill).Select(x => GetSkillScore(x)).ToList() ?? new List<SkillScores>();
            var percentSkill = 0;
            if (course.CourseType == EnumCourseType.Academic)
            {
                percentSkill = OverallPercentCourse.OverallAcaPercentFinalTest;
            }
            else if (course.CourseType == EnumCourseType.EnglishFoundation)
            {
                percentSkill = OverallPercentCourse.OverallRFIPercentFinalTest;
            }
            return (skillScores, NumberHelper.ConvertDoublePercent((finalTestResult?.Percent ?? default) * percentSkill));
        }
    }
}
