// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.OtherQuery
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MassTransit.Initializers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class ExportAllCoursesQuestionsExcelQuery : IRequest<MethodResult<Stream>>
    {
    }

    public class ExportAllCoursesQuestionsExcelQueryHandler : IRequestHandler<ExportAllCoursesQuestionsExcelQuery, MethodResult<Stream>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly ITimeCodeExerciseRepository _timeCodeExerciseRepository;
        private readonly IExerciseQuestionRepository _exerciseQuestionRepository;
        private readonly ILessonHomeWorkRepository _lessonHomeWorkRepository;
        private readonly ILessonVideoRepository _lessonVideoRepository;

        public ExportAllCoursesQuestionsExcelQueryHandler(ICourseRepository courseRepository,
            IUnitRepository unitRepository,
            IMockTestRepository mockTestRepository,
            SectionGroupConverter sectionGroupConverter,
            IFinalTestRepository finalTestRepository,
            IVideoRepository videoRepository,
            IVideoTimeCodeRepository videoTimeCodeRepository,
            ITimeCodeExerciseRepository timeCodeExerciseRepository,
            IExerciseQuestionRepository exerciseQuestionRepository,
            ILessonHomeWorkRepository lessonHomeWorkRepository,
            ILessonVideoRepository lessonVideoRepository)

        {
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _mockTestRepository = mockTestRepository;
            _sectionGroupConverter = sectionGroupConverter;
            _finalTestRepository = finalTestRepository;
            _videoRepository = videoRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _timeCodeExerciseRepository = timeCodeExerciseRepository;
            _exerciseQuestionRepository = exerciseQuestionRepository;
            _lessonHomeWorkRepository = lessonHomeWorkRepository;
            _lessonVideoRepository = lessonVideoRepository;
        }

        public async Task<MethodResult<Stream>> Handle(ExportAllCoursesQuestionsExcelQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();
            var courseGroupLevels = await _courseRepository.Queryable.Where(x => x.Status == Shared.Enums.EnumCourseStatus.Active).GroupBy(x => x.CourseLevel).Select(x => new
            {
                CourseLevel = x.Key,
                UnitIds = x.SelectMany(x => x.CourseUnitMockTests).Where(x => x.UnitId.HasValue).Select(x => x.UnitId ?? default).ToList(),
                MockTestIds = x.SelectMany(x => x.CourseUnitMockTests).Where(x => x.MockTestId.HasValue).Select(x => x.MockTestId ?? default).ToList(),
                FinalTestIds = x.SelectMany(x => x.CourseUnitMockTests).Where(x => x.FinalTestId.HasValue).Select(x => x.FinalTestId ?? default).ToList(),
            }).ToListAsync(cancellationToken);

            var unitIds = courseGroupLevels.SelectMany(x => x.UnitIds).ToList();
            var units = await _unitRepository.Queryable.Where(x => unitIds.Contains(x.Id))
                .Select(x => new
                {
                    Id = x.Id,
                    LessonIds = x.UnitLessons.Select(x => x.LessonId).ToList(),
                    MockTestId = x.UnitSkillMockTests.Select(x => x.MockTestId).FirstOrDefault(),
                })
                .ToListAsync(cancellationToken);
            List<CourseModuleQuestionExportModel> courseModuleQuestionExports = new List<CourseModuleQuestionExportModel>();
            foreach (var courseGroupLevel in courseGroupLevels)
            {
                var unitCourses = courseGroupLevel.UnitIds.Select(x => units.FirstOrDefault(y => y.Id == x)).ToList();
                var lessonIds = unitCourses.Where(x => x != null).SelectMany(x => x!.LessonIds).ToList();
                var totalFullMockTest = await GetTotalQuestionMockTestAsync(courseGroupLevel.MockTestIds);
                var totalFinalTest = await GetTotalQuestionFinalTestAsync(courseGroupLevel.FinalTestIds);
                var totalHomeWork = await GetTotalQuestionHomeWorkAsync(lessonIds);

                var courseModuleQuestionExport = new CourseModuleQuestionExportModel
                {
                    CourseLevel = courseGroupLevel.CourseLevel,
                    TotalQuestionFinalTest = totalFinalTest,
                    TotalQuestionFullMockTest = totalFullMockTest,
                    TotalQuestionHomeWork = totalHomeWork,
                };
                var skillMockTestIds = unitCourses.Where(x => x != null && x.MockTestId != Guid.Empty).Select(x => x!.MockTestId).ToList();
                if (skillMockTestIds.Any())
                {
                    courseModuleQuestionExport.TotalQuestionSkillMockTest = await GetTotalQuestionMockTestAsync(skillMockTestIds);
                }
                await GetTotalQuestionVideosAsync(courseModuleQuestionExport, lessonIds);
                courseModuleQuestionExports.Add(courseModuleQuestionExport);
            }
            methodResult.Result = courseModuleQuestionExports.OrderBy(x => x.CourseLevel).ToList().ExportExcel();
            return methodResult;
        }

        private async Task GetTotalQuestionVideosAsync(CourseModuleQuestionExportModel courseModuleQuestionExport, IList<Guid> lessonIds)
        {
            var videoLessons = await _lessonVideoRepository.Queryable.Where(x => lessonIds.Contains(x.LessonId))
                .Select(x => new
                {
                    VideoId = x.VideoId,
                    LessonId = x.LessonId
                })
                .ToListAsync();

            var videoTotalQuestions = await (from baseQ in _videoRepository.Queryable
                                             join vtc in _videoTimeCodeRepository.Queryable on baseQ.Id equals vtc.VideoId
                                             join vtce in _timeCodeExerciseRepository.Queryable on vtc.Id equals vtce.VideoTimeCodeId
                                             join eq in _exerciseQuestionRepository.Queryable on vtce.ExerciseId equals eq.ExerciseId
                                             where videoLessons.Select(x => x.VideoId).Contains(baseQ.Id)
                                             group eq by new { vtc.VideoId, vtc.TimeCodeType } into g
                                             select new
                                             {
                                                 VideoId = g.Key.VideoId,
                                                 Type = g.Key.TimeCodeType,
                                                 TotalQuestion = g.Select(x => x.Id).Distinct().Count(),
                                             }).ToListAsync();

            var timeCodeTypes = ConvertHelper.EnumToList<EnumTimeCodeType>().ToList();
            // Dictionary để lưu kết quả tính toán
            var totalQuestionsByType = new Dictionary<EnumTimeCodeType, int>();

            // Tính toán tổng câu hỏi cho từng loại TimeCodeType
            foreach (var type in timeCodeTypes)
            {
                totalQuestionsByType[type] = lessonIds
                    .Select(id => videoLessons.FirstOrDefault(x => x.LessonId == id)?.VideoId ?? default)
                    .Sum(videoId => videoTotalQuestions.FirstOrDefault(x => x.VideoId == videoId && x.Type == type)?.TotalQuestion ?? 0);
            }

            // Gán kết quả vào courseModuleQuestionExport
            courseModuleQuestionExport.TotalQuestionStandalone = totalQuestionsByType[EnumTimeCodeType.Standalone];
            courseModuleQuestionExport.TotalQuestionSkillTest = totalQuestionsByType[EnumTimeCodeType.SkillTest];
            courseModuleQuestionExport.TotalQuestionUnitTest = totalQuestionsByType[EnumTimeCodeType.UnitTest];
        }

        private async Task<long> GetTotalQuestionHomeWorkAsync(IList<Guid> lessonIds)
        {
            var lessonHomeworks = await _lessonHomeWorkRepository.Queryable.Where(x => lessonIds.Contains(x.LessonId))
                                                      .GroupBy(x => x.LessonId)
                                                      .Select(x => new
                                                      {
                                                          LessonId = x.Key,
                                                          TotalQuestion = x.Select(x => x.HomeWork).SelectMany(x => x.HomeWorkQuestions).Count(),
                                                      }).ToListAsync();

            var totalHomeWork = lessonIds.Select(lessonId =>
             {
                 return lessonHomeworks.FirstOrDefault(x => x.LessonId == lessonId)?.TotalQuestion ?? default;
             }).Sum();
            return totalHomeWork;
        }

        private async Task<long> GetTotalQuestionMockTestAsync(IList<Guid> mockTestIds)
        {
            var mockTests = await _mockTestRepository.Queryable.Where(x => mockTestIds.Contains(x.Id))
                                                               .Include(x => x.MockTestSections)
                                                               .ThenInclude(x => x.SectionGroup)
                                                               .ThenInclude(x => x.Sections)
                                                               .ThenInclude(x => x.SectionParts)
                                                               .ThenInclude(x => x.SectionQuestions)
                                                               .Include(x => x.MockTestSections)
                                                               .ThenInclude(x => x.SectionGroup)
                                                               .ThenInclude(x => x.Sections)
                                                               .ThenInclude(x => x.SectionTimeCodes)
                                                               .Include(x => x.MockTestSections)
                                                               .ThenInclude(x => x.SectionGroup)
                                                               .ThenInclude(x => x.Sections)
                                                               .ThenInclude(x => x.SectionQuestions)
                                                               .ThenInclude(x => x.Question)
                                                               .AsNoTracking()
                                                               .ToListAsync();

            return mockTestIds.Select(mockTestId =>
            {
                var mockTest = mockTests.FirstOrDefault(x => x.Id == mockTestId);
                return mockTest?.MockTestSections.Select(x => x.SectionGroup).Select(x => _sectionGroupConverter.GetTotalQuestion(x)).Sum() ?? default;
            }).Sum();
        }

        private async Task<long> GetTotalQuestionFinalTestAsync(IList<Guid> finalTestIds)
        {
            var finalTests = await _finalTestRepository.Queryable.Where(x => finalTestIds.Contains(x.Id))
                                                               .Include(x => x.FinalTestSections)
                                                               .ThenInclude(x => x.SectionGroup)
                                                               .ThenInclude(x => x.Sections)
                                                               .ThenInclude(x => x.SectionQuestions)
                                                               .ThenInclude(x => x.Question)
                                                               .AsNoTracking()
                                                               .ToListAsync();
            return finalTestIds.Select(finalTestId =>
            {
                var finalTest = finalTests.FirstOrDefault(x => x.Id == finalTestId);
                return finalTest?.FinalTestSections.Select(x => x.SectionGroup).Select(x => _sectionGroupConverter.GetTotalQuestion(x)).Sum() ?? default;
            }).Sum();
        }
    }
}
