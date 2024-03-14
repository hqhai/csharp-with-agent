// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.StudentProgressQuery
{
    using System.Collections.Generic;
    using System.Linq.Dynamic.Core;
    using System.Linq.Expressions;
    using System.Reflection.Metadata;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.TrainingServices.Models;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentsProgressCoursesQuery : IRequest<MethodResult<IList<CompetitionStudentProgressModel>>>
    {
        public IList<Guid>? StudentIds { get; set; }

        public EnumCourseType CourseType { get; set; }
    }

    public class GetStudentsProgressCoursesQueryHandler : IRequestHandler<GetStudentsProgressCoursesQuery, MethodResult<IList<CompetitionStudentProgressModel>>>
    {
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ITrainingService _trainingService;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IFinalTestResultRepository _finalTestResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private const int TotalProcess = 217; // tổng số tiến trình hiện có
        private const int TotalProcessIelsts = 106; // tổng số tiến trình hiện có của Ielts

        public GetStudentsProgressCoursesQueryHandler(ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, ITrainingService trainingService, IVideoResultRepository videoResultRepository, IHomeWorkResultRepository homeWorkResultRepository, IFinalTestResultRepository finalTestResultRepository, IClassForumResultRepository classForumResultRepository)
        {
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
            _trainingService = trainingService;
            _videoResultRepository = videoResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _finalTestResultRepository = finalTestResultRepository;
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task<MethodResult<IList<CompetitionStudentProgressModel>>> Handle(GetStudentsProgressCoursesQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CompetitionStudentProgressModel>> methodResult = new MethodResult<IList<CompetitionStudentProgressModel>>();
            IList<CompetitionStudentProgressModel> courseProgress = new List<CompetitionStudentProgressModel>();

            double videoLessonRatio = request.CourseType == EnumCourseType.Academic ? ValueSettings.AcademicStudentResultRatio.VideoRatio : ValueSettings.IeltsStudentResultRatio.VideoRatio;
            double homeWorkRatio = request.CourseType == EnumCourseType.Academic ? ValueSettings.AcademicStudentResultRatio.HomeWorkRatio : ValueSettings.IeltsStudentResultRatio.HomeWorkRatio;
            double classForumRatio = request.CourseType == EnumCourseType.Academic ? ValueSettings.AcademicStudentResultRatio.ClassForumRatio : ValueSettings.IeltsStudentResultRatio.ClassForumRatio;

            var studentIds = request.StudentIds;

            #region validate
            if (studentIds == null || studentIds.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentIds));
                return methodResult;
            }
            #endregion

            #region Progress
            var classStudentResults = await _trainingService.GetListClassBySpecificStudentIdsAsync(new GetClassListBySpecificStudentIdsModel { StudentIds = request.StudentIds });

            var classStudentResultsContent = classStudentResults?.Content?.Result;
            if (classStudentResultsContent != null && classStudentResultsContent.Any())
            {
                var courseIds = classStudentResultsContent.Select(x => x.CourseId).ToList();
                var studentCourseIds = classStudentResultsContent.Select(x => x.StudentId).ToList();
                var courses = await _courseRepository.GetByIdsAsync(courseIds);
                if (courses == null || !courses.Any())
                {
                    methodResult.StatusCode = StatusCodes.Status200OK;
                    return methodResult;
                }
                var courseQuery = _courseResultRepository.Queryable.Include(x => x.Course).Where(x => studentCourseIds.Contains(x.StudentId)).ToList();

                foreach (var (item, studentId) in courses.SelectMany(course => studentCourseIds.Select(sid => (course, sid))))
                {
                    var courseResult = courseQuery.FirstOrDefault(x => x.CourseId == item.Id && x.StudentId == studentId);

                    if (courseResult == null)
                    {
                        continue;
                    }
                    CompetitionStudentProgressModel courseStudentProgress = new CompetitionStudentProgressModel();

                    var courseResultModel = new CourseResultModel
                    {
                        CourseType = courseResult.Course?.CourseType,
                        CourseId = courseResult.CourseId,
                        StudentId = studentId
                    };
                    var (currentProgress, progress) = await _courseRepository.GetContentComplete(courseResultModel);

                    double progressPercentage = ((float)currentProgress / TotalProcessIelsts) * 100;
                    // Update số lượng process do trên dữ liệu chưa nhập đủ
                    if (courseResult.Course?.CourseType == EnumCourseType.Academic)
                    {
                        progressPercentage = ((float)currentProgress / TotalProcess) * 100;
                    }

                    courseStudentProgress.ContentCompleted = Math.Round(progressPercentage, 2);
                    courseStudentProgress.CourseName = item.Code;
                    courseStudentProgress.CourseId = item.Id;
                    courseStudentProgress.StudentId = studentId;
                    courseProgress.Add(courseStudentProgress);
                }
            }
            #endregion

            #region Video
            var videoResults = CompetitionAverageScores(_videoResultRepository, videoLessonRatio, studentIds, EnumLearnType.Video);
            #endregion

            #region UnitsTest
            var videoResultCompetition = _videoResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && studentIds.Contains(x.StudentId));
            var unitTestGroupByStudentId = request.CourseType == EnumCourseType.Ielts ? new List<StudentCompetitionAverageScore>() : videoResultCompetition
                .GroupBy(vr => vr.StudentId)
                .AsEnumerable()
                .Select(group =>
                {
                    var denominator = CountExitsResult(group.Select(vr => vr.VideoSkillScoresStr ?? string.Empty).ToList(), EnumTimeCodeType.UnitTest);
                    double countUnitTestResultValid = CountComfort(group.Select(vr => vr.VideoSkillScoresStr ?? string.Empty).ToList(), EnumTimeCodeType.UnitTest);

                    return new StudentCompetitionAverageScore
                    {
                        StudentId = group.Key,
                        LearnRatio = countUnitTestResultValid == 0 ? 0 : ValueSettings.AcademicStudentResultRatio.UnitTestsRatio / countUnitTestResultValid,
                        TotalRecords = denominator,
                        AverageScoreByType = CountOverralUnitSkillTest(group.Select(vr => vr.VideoSkillScoresStr ?? string.Empty).ToList(), EnumTimeCodeType.UnitTest),
                        LearnType = EnumLearnType.UnitTests
                    };
                }).ToList();
            #endregion

            #region SkillsTest
            var skillTestGroupByStudentId = request.CourseType == EnumCourseType.Ielts ? new List<StudentCompetitionAverageScore>() : videoResultCompetition
                .GroupBy(vr => vr.StudentId)
                .AsEnumerable()
                .Select(group =>
                {
                    var denominator = CountExitsResult(group.Select(vr => vr.VideoSkillScoresStr ?? string.Empty).ToList(), EnumTimeCodeType.SkillTest);

                    double countSkillTestResultValid = CountComfort(group.Select(vr => vr.VideoSkillScoresStr ?? string.Empty).ToList(), EnumTimeCodeType.SkillTest);
                    return new StudentCompetitionAverageScore
                    {
                        StudentId = group.Key,
                        LearnRatio = countSkillTestResultValid == 0 ? 0 : ValueSettings.AcademicStudentResultRatio.SkillsTestsRatio / countSkillTestResultValid,
                        TotalRecords = denominator,
                        AverageScoreByType = CountOverralUnitSkillTest(group.Select(vr => vr.VideoSkillScoresStr ?? string.Empty).ToList(), EnumTimeCodeType.SkillTest),
                        LearnType = EnumLearnType.SkillsTests
                    };
                }).ToList();

            #endregion

            #region HomeWork
            var homeWorkResults = CompetitionAverageScores(_homeWorkResultRepository, homeWorkRatio, studentIds, EnumLearnType.HomeWork);
            #endregion

            #region ClassForum
            var classForumResultCompetion = _classForumResultRepository.Queryable
                .Include(x => x.ClassForum)
                .Where(x => (x.Status == EnumClassForumResultStatus.PendingForGrading || x.Status == EnumClassForumResultStatus.Graded)
                            && studentIds.Contains(x.StudentId)
                            && x.ClassForum != null
                            && EF.Functions.DataLength(x.WordContent) >= x.ClassForum.TaggetWordLimit);
            int countClassForumDistinct = classForumResultCompetion.Select(x => x.StudentId).Distinct().Count();

            var classForumnResultGroupByStudentId = classForumResultCompetion
                .GroupBy(vr => vr.StudentId)
                .Select(group =>
                 new StudentCompetitionAverageScore
                 {
                     StudentId = group.Key,
                     LearnRatio = countClassForumDistinct == 0 ? 0 : classForumRatio / group.Count(),
                     TotalRecords = group.Count(),
                     AverageScoreByType = group.Count(),
                     LearnType = EnumLearnType.ClassForum
                 }).ToList();
            ;


            #endregion

            #region FinalTest
            var finalResults = request.CourseType == EnumCourseType.Ielts ? new List<StudentCompetitionAverageScore>() : CompetitionAverageScores(_finalTestResultRepository, ValueSettings.AcademicStudentResultRatio.FinalTestRatio, studentIds, EnumLearnType.FinalTest);

            List<List<StudentCompetitionAverageScore>> allResults = new List<List<StudentCompetitionAverageScore>>
                {
                    videoResults,
                    skillTestGroupByStudentId,
                    unitTestGroupByStudentId,
                    classForumnResultGroupByStudentId,
                    finalResults,
                    homeWorkResults
                };

            List<StudentCompetitionOverallModel> overallResults = new List<StudentCompetitionOverallModel>();

            overallResults = CalculateOverallOfAcademicStudent(studentIds, allResults);
            #endregion

            #region Result

            var result = (from progress in courseProgress
                          join overall in overallResults
                          on progress.StudentId equals overall.StudentId into gj
                          from overall in gj.DefaultIfEmpty(new StudentCompetitionOverallModel())
                          select new CompetitionStudentProgressModel
                          {
                              StudentId = progress.StudentId,
                              CourseId = progress.CourseId,
                              CourseName = progress.CourseName,
                              ContentCompleted = progress.ContentCompleted,
                              TotalScore = NumberHelper.RoundNumberDouble(overall.TotalScore)
                          }).ToList();
            #endregion

            methodResult.Result = result;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }



        #region Caculatator
        private static double CountOverralUnitSkillTest(List<string>? listStr, EnumTimeCodeType timCodeType)
        {
            double overallScore = 0;
            if (listStr != null && listStr.Count > 0)
            {
                foreach (var item in listStr)
                {
                    var tempList = ConvertHelper.Deserialize<IList<VideoSkillScores>>(item);
                    if (tempList != null && tempList.Any(x => x.Type == timCodeType))
                    {
                        double totalCorrectCount = tempList.FirstOrDefault(x => x.Type == timCodeType)?.SkillScores?.Sum(score => score.CorrectCount) ?? 0;
                        double totalTotalCount = tempList.FirstOrDefault(x => x.Type == timCodeType)?.SkillScores?.Sum(score => score.TotalCount) ?? 0;
                        double overallPercent = totalTotalCount == 0 ? 0 : Math.Round((double)totalCorrectCount / totalTotalCount, 2);

                        overallScore += overallPercent;
                    }
                }
            }
            return overallScore;
        }

        private static double CountComfort(List<string>? listStr, EnumTimeCodeType timCodeType)
        {
            int countComfortableType = 0;
            if (listStr != null && listStr.Count > 0)
            {
                foreach (var item in listStr)
                {
                    var tempList = ConvertHelper.Deserialize<IList<VideoSkillScores>>(item);
                    if (tempList != null && tempList.Any(x => x.Type == timCodeType))
                    {
                        countComfortableType += 1;
                    }


                }
            }
            return countComfortableType;
        }

        private static int CountExitsResult(List<string>? listStr, EnumTimeCodeType timCodeType)
        {
            int quantity = 0;
            if (listStr != null && listStr.Count > 0)
            {
                foreach (var item in listStr)
                {
                    var tempList = ConvertHelper.Deserialize<IList<VideoSkillScores>>(item);
                    if (tempList != null && tempList.Any(x => x.Type == timCodeType))
                    {
                        quantity += 1;
                    }


                }
            }

            return quantity;
        }

        private static double CaculateNumerator(StudentCompetitionAverageScore? item)
        {
            if (item == null)
            {
                return 0;
            }
            double result = item.AverageScoreByType * item.LearnRatio;
            return result;
        }

        private static double CaculateDonomerator(StudentCompetitionAverageScore? item)
        {
            if (item == null)
            {
                return 0;
            }
            double result = item.TotalRecords * item.LearnRatio;
            return result;
        }

        private static List<StudentCompetitionOverallModel> CalculateOverallOfAcademicStudent(IList<Guid>? studentIds, List<List<StudentCompetitionAverageScore>> allResults)
        {
            // Thực hiện tính toán trên tất cả các kết quả ở đây

            List<StudentCompetitionOverallModel> joinedList = studentIds!
                           .GroupJoin(allResults[0], studentId => studentId, itemA => itemA.StudentId, (studentId, itemsA) => new { StudentId = studentId, ItemsA = itemsA.DefaultIfEmpty() })
                           .GroupJoin(allResults[1], studentId => studentId.StudentId, itemB => itemB.StudentId, (result, itemsB) => new { result.StudentId, result.ItemsA, ItemsB = itemsB.DefaultIfEmpty() })
                           .GroupJoin(allResults[2], studentId => studentId.StudentId, itemC => itemC.StudentId, (result, itemsC) => new { result.StudentId, result.ItemsA, result.ItemsB, ItemsC = itemsC.DefaultIfEmpty() })
                           .GroupJoin(allResults[3], studentId => studentId.StudentId, itemD => itemD.StudentId, (result, itemsD) => new { result.StudentId, result.ItemsA, result.ItemsB, result.ItemsC, ItemsD = itemsD.DefaultIfEmpty() })
                           .GroupJoin(allResults[4], studentId => studentId.StudentId, itemE => itemE.StudentId, (result, itemsE) => new { result.StudentId, result.ItemsA, result.ItemsB, result.ItemsC, result.ItemsD, ItemsE = itemsE.DefaultIfEmpty() })
                           .GroupJoin(allResults[5], studentId => studentId.StudentId, itemF => itemF.StudentId, (result, itemsF) => new { result.StudentId, result.ItemsA, result.ItemsB, result.ItemsC, result.ItemsD, result.ItemsE, ItemsF = itemsF.DefaultIfEmpty() })
                           .SelectMany(result => result.ItemsA
                               .SelectMany(itemA => result.ItemsB
                                   .SelectMany(itemB => result.ItemsC
                                       .SelectMany(itemC => result.ItemsD
                                        .SelectMany(itemD => result.ItemsE
                                           .SelectMany(itemE => result.ItemsF
                                               .Select(itemF => new StudentCompetitionOverallModel
                                               {
                                                   StudentId = result.StudentId,
                                                   TotalScore = (CaculateDonomerator(itemA) + CaculateDonomerator(itemB) + CaculateDonomerator(itemC) + CaculateDonomerator(itemD) + CaculateDonomerator(itemE) + CaculateDonomerator(itemF)) != 0 ? ((CaculateNumerator(itemA) + CaculateNumerator(itemB) + CaculateNumerator(itemC) + CaculateNumerator(itemD) + CaculateNumerator(itemE) + CaculateNumerator(itemF)) * 100) / (CaculateDonomerator(itemA) + CaculateDonomerator(itemB) + CaculateDonomerator(itemC) + CaculateDonomerator(itemD) + CaculateDonomerator(itemE) + CaculateDonomerator(itemF)) : 0
                                               })
                                           )
                                       )
                                   )
                               )
                           )
                        ).ToList();

            return joinedList;
        }

        #endregion


        #region QueryResult
        private static List<StudentCompetitionAverageScore> CompetitionAverageScores<T>(IRepository<T> repository, double ratioResult, IList<Guid>? studentIds, EnumLearnType learnType)
            where T : BaseResult
        {
            List<Func<T, bool>> additionalConditions = new List<Func<T, bool>>
                {
                    x => x.Status == EnumResultStatus.Done
                };

            var resultCompetition = GetResultCommon(repository.Queryable, studentIds!, additionalConditions);
            int numberOfRecordValid = resultCompetition.Select(x => x.StudentId).Distinct().Count();
            List<StudentCompetitionAverageScore> result = resultCompetition
                        .GroupBy(vr => vr.StudentId)
                        .AsEnumerable()
                        .Select(group =>
                        {
                            return new StudentCompetitionAverageScore
                            {
                                StudentId = group.Key,
                                LearnRatio = numberOfRecordValid == 0 ? 0 : ratioResult / group.Count(),
                                TotalRecords = group.Count(),
                                AverageScoreByType = group.Sum(vr => vr.CorrectTotal == 0 ? 0 : (double)vr.CorrectCount / vr.CorrectTotal),
                                LearnType = learnType
                            };
                        }).ToList();

            return result;
        }

        private static IQueryable<T> GetResultCommon<T>(IQueryable<T>? repositoryQueryable, IList<Guid> studentIds, List<Func<T, bool>> additionalConditions) where T : BaseResult
        {
            var query = repositoryQueryable!.Where(x => studentIds.Contains(x.StudentId));

            foreach (var condition in additionalConditions)
            {
                query = query.Where(condition).AsQueryable();
            }

            return query;
        }



        #endregion
    }

}

