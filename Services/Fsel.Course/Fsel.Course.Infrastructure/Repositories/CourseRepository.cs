// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using EntityCourse = Fsel.Course.Domain.Entities.Course;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class CourseRepository : BaseRepository<EntityCourse>, ICourseRepository
    {
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IMapper _mapper;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;
        private readonly IUnitLessonRepository _unitLessonRepository;
        private readonly ICourseResultRepository _courseResultRepository;

        public CourseRepository(CourseDbContext dbContext,
            CourseReadDbContext courseReadDbContext,
            IUnitResultRepository unitResultRepository,
            IFinalTestResultRepository finalTestResultRepository,
            IMockTestResultRepository mockTestResultRepository,
            AuthContext authContext,
            IUnitRepository unitRepository,
            ILessonResultRepository lessonResultRepository,
            IMapper mapper,
            ICourseUnitMockTestRepository courseUnitMockTestRepository,
            IUnitLessonRepository unitLessonRepository,
            ICourseResultRepository courseResultRepository) : base(dbContext, courseReadDbContext, authContext, mapper)
        {
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _mapper = mapper;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
            _unitLessonRepository = unitLessonRepository;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<bool> IsUnitUsed(Guid id)
        {
            return await Queryable
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted))
                .AnyAsync(x => x.Id == id && x.CourseUnitMockTests.Count > 0);
        }

        public override async Task<EntityCourse?> GetIncludeByIdAsync(Guid id)
        {
            try
            {
                return await Queryable
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted).OrderBy(x => x.DisplayOrder))
                .ThenInclude(x => x.Unit)
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted).OrderBy(x => x.DisplayOrder))
                .ThenInclude(x => x.FinalTest)
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted).OrderBy(x => x.DisplayOrder))
                .ThenInclude(x => x.MockTest)
                .Include(x => x.CourseTeachers.Where(c => !c.IsDeleted).OrderBy(x => x.CreatedDate))
                .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<EntityCourse?> GetIncludeLessonVideoByIdAsync(Guid id)
        {
            try
            {
                return await Queryable.Include(x => x.CourseResults)
                                        .Include(x => x.CourseUnitMockTests.Where(y => y.IsDeleted == false).OrderBy(x => x.DisplayOrder))
                                        .ThenInclude(x => x.Unit)
                                        .ThenInclude(x => x!.UnitLessons.Where(y => y.IsDeleted == false).OrderBy(x => x.DisplayOrder))
                                        .ThenInclude(x => x.Lesson)
                                        .ThenInclude(x => x!.LessonVideos.Where(y => y.IsDeleted == false).OrderBy(x => x.CreatedDate))
                                        .Where(x => x.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<EntityCourse?> GetIncludeCourseUnitMockTestByIdAsync(Guid id, Guid? studentId)
        {
            try
            {
                return await Queryable.Include(x => x.CourseUnitMockTests)
                                        .Include(x => x.CourseResults.Where(x => x.CourseId == id && x.StudentId == studentId))
                                        .Include(x => x.UnitResults.Where(x => x.CourseId == id && x.StudentId == studentId))
                                        .Include(x => x.MockTestResults.Where(x => x.CourseId == id && x.StudentId == studentId))
                                        .Include(x => x.FinalTestResults.Where(x => x.CourseId == id && x.StudentId == studentId))
                                        .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<EntityCourse?> GetAsync(Guid id, Guid? studentId)
        {
            try
            {
                return await Queryable.Include(x => x.CourseUnitMockTests.OrderBy(x => x.DisplayOrder))
                                      .Include(x => x.CourseResults.Where(x => x.StudentId == studentId))
                                      .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<CourseModel?> GetIncludeCourseResult(Guid id, Guid? studentId, string? classCode)
        {
            return await Queryable.Where(x => x.Id == id)
                         .AsNoTracking()
                         .Select(x => new CourseModel
                         {
                             Id = x.Id,
                             Code = x.Code,
                             CourseLevel = x.CourseLevel,
                             CourseType = x.CourseType,
                             InstructionContent = x.InstructionContent,
                             Name = x.Name,
                             Status = x.Status,
                             CourseResult = _mapper.Map<CourseResultModel>(x.CourseResults.FirstOrDefault(x => x.StudentId == studentId)),
                             CourseTeachers = _mapper.Map<IList<CourseTeacherModel>>(x.CourseTeachers),
                             CourseClass = new CourseClassModel
                             {
                                 Code = classCode,
                                 CourseId = x.Id
                             },
                             CourseUnitMockTests = x.CourseUnitMockTests.OrderBy(x => x!.DisplayOrder).ThenBy(x => x.CreatedDate).Select(x => new CourseUnitMockTestModel
                             {
                                 DisplayOrder = x.DisplayOrder,
                                 CourseId = x.CourseId,
                                 FinalTestId = x.FinalTestId,
                                 MockTestId = x.MockTestId,
                                 UnitId = x.UnitId,
                                 FinalTest = x.FinalTest != null ? new FinalTestModel
                                 {
                                     Id = x.FinalTest.Id,
                                     ExecutionTime = x.FinalTest.ExecutionTime,
                                     FinalTestLevel = x.FinalTest.FinalTestLevel,
                                     Name = x.FinalTest.Name,
                                     FinalTestResult = _mapper.Map<FinalTestResultModel>(x.FinalTest.FinalTestResults.AsQueryable().Include(x => x.SectionGroupResults)
                                                                 .Include(x => x.FinalTest)
                                                                 .ThenInclude(x => x!.FinalTestSections)
                                                                 .Where(y => y.StudentId == studentId && y.CourseId == x.CourseId)
                                                                 .AsNoTracking().FirstOrDefault()),
                                 } : null,
                                 MockTest = x.MockTest != null ? new MockTestModel
                                 {
                                     Id = x.MockTest.Id,
                                     ExecutionTime = x.MockTest.ExecutionTime,
                                     MockTestType = x.MockTest.MockTestType,
                                     Name = x.MockTest.Name,
                                     MockTestResult = _mapper.Map<MockTestResultModel>(x.MockTest.MockTestResults.AsQueryable().Include(x => x.SectionGroupResults)
                                                                 .Include(x => x.MockTest)
                                                                 .ThenInclude(x => x!.MockTestSections)
                                                                 .Where(y => y.StudentId == studentId && y.CourseId == x.CourseId)
                                                                 .AsNoTracking().FirstOrDefault()),
                                 } : null,
                                 Unit = x.Unit != null ? new UnitModel
                                 {
                                     Id = x.Unit.Id,
                                     Name = x.Unit.Name,
                                     Code = x.Unit.Code,
                                     CourseLevel = x.Unit.CourseLevel,
                                     UnitResult = _mapper.Map<UnitResultModel>(x.Unit.UnitResults.AsQueryable().Include(x => x.Unit).ThenInclude(x => x!.LessonResults.Where(x => x.StudentId == studentId))
                                                                 .Include(x => x.Unit).ThenInclude(x => x!.UnitLessons)
                                                                 .Where(y => y.StudentId == studentId && y.CourseId == x.CourseId)
                                                                 .AsNoTracking().FirstOrDefault()),
                                 } : null,
                             }).ToList()
                         })
                         .FirstOrDefaultAsync();
        }

        public async Task<(int, int)> GetDisplayOrder(CourseResultModel courseResult, DateTime? arrivalDate = default)
        {
            ArgumentNullException.ThrowIfNull(courseResult);

            var displayOrderLesson = 1;
            var displayOrderUnit = 1;
            var unitResult = await _unitResultRepository.Queryable.Include(x => x.Unit).ThenInclude(x => x!.CourseUnitMockTests)
                .Where(x => !arrivalDate.HasValue || (x!.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date)
                .Where(x => x.CourseResultId == courseResult.Id && x.Status != EnumResultStatus.Unfinished)
                .OrderByDescending(x => x.CreatedDate)
                .ThenByDescending(x => x.UpdatedDate)
                .FirstOrDefaultAsync();
            if (unitResult != null)
            {
                displayOrderUnit = unitResult.Unit?.CourseUnitMockTests.FirstOrDefault()?.DisplayOrder ?? default;
                var lessonResults = await _lessonResultRepository.Queryable.Where(x => !arrivalDate.HasValue || (x!.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date)
                                    .Where(x => x.StudentId == courseResult.StudentId && x.UnitResultId == unitResult.Id)
                                    .ToListAsync();
                if (lessonResults != null && lessonResults.Any())
                {
                    if (lessonResults.All(x => x.Status == EnumResultStatus.Done))
                    {
                        displayOrderLesson = lessonResults.Count;
                    }
                    else
                    {
                        displayOrderLesson = lessonResults.Where(x => x.Status == EnumResultStatus.Done).Count() + 1;
                    }
                }
            }
            return (displayOrderUnit, displayOrderLesson);
        }

        public async Task<List<UnitCurrentPositionModel>> GetPostionUnit(IList<CourseResultModel>? courseResults)
        {
            var unitCurrentPositions = new List<UnitCurrentPositionModel>();
            if (courseResults == null)
            {
                return unitCurrentPositions;
            }
            var query = from baseQ in _courseResultRepository.Queryable
                        join cum in _courseUnitMockTestRepository.Queryable on baseQ.CourseId equals cum.CourseId
                        join ur in _unitResultRepository.Queryable on baseQ.Id equals ur.CourseResultId
                        join ul in _unitLessonRepository.Queryable on ur.UnitId equals ul.UnitId
                        join lr in _lessonResultRepository.Queryable on ur.Id equals lr.UnitResultId
                        where courseResults.Select(x => x.StudentId).Contains(baseQ.StudentId) && baseQ.WorkingStatus == Shared.Enums.EnumWorkingStatus.Active
                        group new { baseQ, lr, ur, cum, ul } by new { baseQ.CourseId, baseQ.StudentId } into g
                        select new UnitCurrentPositionModel
                        {
                            StudentId = g.Key.StudentId,
                        };
            var b = await query.ToListAsync();
            return unitCurrentPositions;
        }

        public async Task<List<UnitCurrentPositionModel>> GetDisplayOrder(IList<CourseResultModel>? courseResults, DateTime? arrivalDate = default)
        {
            var unitCurrentPositions = new List<UnitCurrentPositionModel>();
            if (courseResults == null)
            {
                return unitCurrentPositions;
            }
            var courseIds = courseResults.Select(x => x.CourseId).ToList();
            var courseResultIds = courseResults.Select(x => x.Id).ToList();
            var studentIds = courseResults.Select(x => x.StudentId).ToList();
            var lessonResults = new List<LessonResult>();

            var unitResults = await _unitResultRepository.Queryable.Include(x => x.Unit).ThenInclude(x => x!.CourseUnitMockTests.Where(x => courseIds.Contains(x.CourseId)))
                 .Where(x => !arrivalDate.HasValue || (x!.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date)
                 .Where(x => x.Status != EnumResultStatus.Unfinished)
                 .Where(x => courseResultIds.Contains(x.CourseResultId ?? Guid.Empty))
                 .GroupBy(x => x.StudentId)
                 .Where(x => x.Any())
                 .Select(x => x.OrderByDescending(x => x.UpdatedDate).ThenByDescending(x => x.CreatedDate).FirstOrDefault()!)
                 .ToListAsync();
            if (unitResults != null)
            {
                lessonResults = await _lessonResultRepository.Queryable.Where(x => !arrivalDate.HasValue || (x!.UpdatedDate ?? x.CreatedDate).Date <= arrivalDate.Value.Date)
                                       .Where(x => courseResultIds.Contains(x.CourseResultId ?? Guid.Empty) && studentIds.Contains(x.StudentId))
                                       .ToListAsync();

                lessonResults = lessonResults.Join(unitResults,
                                                 lessonResult => new { lessonResult.CourseId, lessonResult.UnitId, lessonResult.StudentId },
                                                 unitResult => new { unitResult!.CourseId, unitResult.UnitId, unitResult.StudentId },
                                                 (lessonResult, unitResult) => lessonResult).ToList();
            }
            foreach (var courseResult in courseResults)
            {
                var unitCurrentPositionModel = new UnitCurrentPositionModel()
                {
                    StudentId = courseResult.StudentId,
                };
                var unitResult = unitResults?.FirstOrDefault(x => x.StudentId == courseResult.StudentId && x.CourseId == courseResult.CourseId);
                var displayUnit = unitResult?.Unit?.CourseUnitMockTests.FirstOrDefault(x => x.CourseId == courseResult.CourseId)?.Number;
                if (unitResult != null && displayUnit.HasValue)
                {
                    unitCurrentPositionModel.DisplayUnit = displayUnit.Value;
                    var lessonUnitResults = lessonResults?.Where(x => x.StudentId == courseResult.StudentId).ToList();
                    if (lessonUnitResults == null || !lessonUnitResults.Any())
                    {
                        unitCurrentPositions.Add(unitCurrentPositionModel);
                        continue;
                    }
                    if (lessonUnitResults.All(x => x.Status == EnumResultStatus.Done))
                    {
                        unitCurrentPositionModel.DisplayLesson = lessonUnitResults.Count;
                    }
                    else
                    {
                        unitCurrentPositionModel.DisplayLesson = lessonUnitResults.Where(x => x.Status == EnumResultStatus.Done).Count() + 1;
                    }
                }
                unitCurrentPositions.Add(unitCurrentPositionModel);
            }
            return unitCurrentPositions;
        }
    }
}
