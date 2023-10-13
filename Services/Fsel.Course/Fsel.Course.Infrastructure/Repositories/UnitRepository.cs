// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Shared.Enums;
using Fsel.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class UnitRepository : BaseRepository<Unit>, IUnitRepository
    {
        private readonly IMapper _mapper;
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly ILessonVideoRepository _lessonVideoRepository;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IClassForumRepository _classForumRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly ILessonHomeWorkRepository _lessonHomeWorkRepository;
        private readonly IUnitLessonRepository _unitLessonRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseUnitMockTestRepository _courseUnitMockTestRepository;

        public UnitRepository(CourseDbContext dbContext, AuthContext authContext, IMapper mapper, IVideoRepository videoRepository, IVideoTimeCodeRepository videoTimeCodeRepository, IVideoResultRepository videoResultRepository, ILessonVideoRepository lessonVideoRepository, IVideoTimeCodeResultRepository videoTimeCodeResultRepository, IClassForumResultRepository classForumResultRepository, IClassForumRepository classForumRepository, IHomeWorkResultRepository homeWorkResultRepository, ILessonHomeWorkRepository lessonHomeWorkRepository, IUnitLessonRepository unitLessonRepository, ILessonRepository lessonRepository, ILessonResultRepository lessonResultRepository, IUnitResultRepository unitResultRepository, ICourseUnitMockTestRepository courseUnitMockTestRepository) : base(dbContext, authContext)
        {
            _mapper = mapper;
            _videoRepository = videoRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _videoResultRepository = videoResultRepository;
            _lessonVideoRepository = lessonVideoRepository;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _classForumResultRepository = classForumResultRepository;
            _classForumRepository = classForumRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _lessonHomeWorkRepository = lessonHomeWorkRepository;
            _unitLessonRepository = unitLessonRepository;
            _lessonRepository = lessonRepository;
            _lessonResultRepository = lessonResultRepository;
            _unitResultRepository = unitResultRepository;
            _courseUnitMockTestRepository = courseUnitMockTestRepository;
        }

        public override async Task<Unit?> GetIncludeByIdAsync(Guid id, int? siteId = null)
        {
            try
            {
                return await Queryable
                .Include(x => x.UnitSkillMockTests.Where(n => !n.IsDeleted))
                .ThenInclude(x => x.MockTest)
                .ThenInclude(x => x!.MockTestSections.Where(n => !n.IsDeleted))
                .ThenInclude(x => x.SectionGroup)
                .Include(x => x.UnitLessons.Where(n => !n.IsDeleted))
                .ThenInclude(x => x.Lesson)
                .ThenInclude(x => x!.LessonInstructions.Where(n => !n.IsDeleted))
                .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Unit>?> GetListAsync(IList<Guid>? ids, Guid? studentId)
        {
            if (ids == null || !ids.Any())
            {
                return null;
            }
            return await Queryable.Include(x => x.UnitSkillMockTests).Include(x => x.UnitLessons).Where(x => ids.Contains(x.Id)).ToListAsync();
        }

        public async Task<IList<UnitModel>> GetListModelAsync(Guid? studentId, Guid courseId, EnumLearnProcessType type)
        {
            if (type == EnumLearnProcessType.LessonVideo)
            {
                var query = from baseQ in Queryable
                            join ur in _unitResultRepository.Queryable on baseQ.Id equals ur.UnitId
                            join cumt in _courseUnitMockTestRepository.Queryable on baseQ.Id equals cumt.UnitId
                            join ul in _unitLessonRepository.Queryable on baseQ.Id equals ul.UnitId
                            join lr in _lessonResultRepository.Queryable on baseQ.Id equals lr.UnitId
                            where cumt.CourseId == courseId && lr.StudentId == studentId && ur.StudentId == studentId
                            group new { ul, lr, ur } by baseQ into g
                            select new
                            {
                                Unit = g.Key,
                                UnitResult = g.Select(x => x.ur).FirstOrDefault(),
                                CountDone = g.Select(x => x.lr).Where(x => x.Status == EnumResultStatus.Done).Count(),
                                TotalDone = g.Select(x => x.ul).Count(),
                            };
                var list = await query.ToListAsync();
                return list.Select(x => GetUnitModel(x.Unit, x.UnitResult, courseId, x.CountDone, x.TotalDone)).ToList();
            }
            else if (type == EnumLearnProcessType.HomeWork)
            {
                var query = from baseQ in Queryable
                            join ur in _unitResultRepository.Queryable on baseQ.Id equals ur.UnitId
                            join cumt in _courseUnitMockTestRepository.Queryable on baseQ.Id equals cumt.UnitId
                            join ul in _unitLessonRepository.Queryable on baseQ.Id equals ul.UnitId
                            join l in _lessonRepository.Queryable on ul.LessonId equals l.Id
                            join lh in _lessonHomeWorkRepository.Queryable on l.Id equals lh.LessonId
                            join lr in _lessonResultRepository.Queryable on baseQ.Id equals lr.UnitId
                            join hr in _homeWorkResultRepository.Queryable on lr.Id equals hr.LessonResultId
                            where cumt.CourseId == courseId && lr.StudentId == studentId && ur.StudentId == studentId
                            group new { hr, lh, ur } by baseQ into g
                            select new
                            {
                                Unit = g.Key,
                                UnitResult = g.Select(x => x.ur).FirstOrDefault(),
                                CountDone = g.Select(x => x.hr).Where(x => x.Status == EnumResultStatus.Done).Count(),
                                TotalDone = g.Select(x => x.lh).Count(),
                            };
                var list = await query.ToListAsync();
                return list.Select(x => GetUnitModel(x.Unit, x.UnitResult, courseId, x.CountDone, x.TotalDone)).ToList();
            }
            else if (type == EnumLearnProcessType.ClassForum)
            {
                var query = from baseQ in Queryable
                            join ur in _unitResultRepository.Queryable on baseQ.Id equals ur.UnitId
                            join cumt in _courseUnitMockTestRepository.Queryable on baseQ.Id equals cumt.UnitId
                            join ul in _unitLessonRepository.Queryable on baseQ.Id equals ul.UnitId
                            join l in _lessonRepository.Queryable on ul.LessonId equals l.Id
                            join cf in _classForumRepository.Queryable on l.Id equals cf.LessonId
                            join lr in _lessonResultRepository.Queryable on baseQ.Id equals lr.UnitId
                            join cfr in _classForumResultRepository.Queryable on lr.Id equals cfr.LessonResultId
                            where cumt.CourseId == courseId && lr.StudentId == studentId && ur.StudentId == studentId
                            group new { cfr, cf, ur } by baseQ into g
                            select new
                            {
                                Unit = g.Key,
                                UnitResult = g.Select(x => x.ur).FirstOrDefault(),
                                CountDone = g.Select(x => x.cfr).Where(x => x.Status == EnumClassForumResultStatus.Graded || x.Status == EnumClassForumResultStatus.PendingForGrading).Count(),
                                TotalDone = g.Select(x => x.cf).Count(),
                            };
                var list = await query.ToListAsync();
                return list.Select(x => GetUnitModel(x.Unit, x.UnitResult, courseId, x.CountDone, x.TotalDone)).ToList();
            }
            else
            {
                var query = from baseQ in Queryable
                            join ur in _unitResultRepository.Queryable on baseQ.Id equals ur.UnitId
                            join cumt in _courseUnitMockTestRepository.Queryable on baseQ.Id equals cumt.UnitId
                            join ul in _unitLessonRepository.Queryable on baseQ.Id equals ul.UnitId
                            join l in _lessonRepository.Queryable on ul.LessonId equals l.Id
                            join lv in _lessonVideoRepository.Queryable on l.Id equals lv.LessonId
                            join v in _videoRepository.Queryable on lv.VideoId equals v.Id
                            join vtc in _videoTimeCodeRepository.Queryable on v.Id equals vtc.VideoId
                            join lr in _lessonResultRepository.Queryable on baseQ.Id equals lr.UnitId
                            join vr in _videoResultRepository.Queryable on lr.Id equals vr.LessonResultId
                            join vtcr in _videoTimeCodeResultRepository.Queryable on vr.Id equals vtcr.VideoResultId
                            where cumt.CourseId == courseId && lr.StudentId == studentId && ur.StudentId == studentId
                            group new { vtcr, vtc, ur } by baseQ into g
                            select new
                            {
                                Unit = g.Key,
                                UnitResult = g.Select(x => x.ur).FirstOrDefault(),
                                CountDone = g.Select(x => x.vtcr).Where(x => x.Status == EnumResultStatus.Done).Count(),
                                TotalDone = g.Select(x => x.vtc).Count(),
                            };
                var list = await query.ToListAsync();
                return list.Select(x => GetUnitModel(x.Unit, x.UnitResult, courseId, x.CountDone, x.TotalDone)).ToList();
            }
        }

        private UnitModel GetUnitModel(Unit x, UnitResult unitResult, Guid courseId, double totalDone, double countDone)
        {
            var displayOrder = x.CourseUnitMockTests.FirstOrDefault(y => y.CourseId == courseId && y.UnitId == x.Id)?.DisplayOrder ?? default;
            var unitModel = new UnitModel
            {
                Id = x.Id,
                Code = x.Code,
                CourseLevel = x.CourseLevel,
                CreatedDate = x.CreatedDate,
                CreatedFullName = x.CreatedFullName,
                CreatedUserId = x.CreatedUserId,
                IsActive = x.CourseUnitMockTests.Any(),
                Name = x.Name,
                DisplayOrder = displayOrder,
                UpdatedDate = x.UpdatedDate,
                UpdatedFullName = x.UpdatedFullName,
                UpdatedUserId = x.UpdatedUserId,
                UnitResult = _mapper.Map<UnitResultModel>(unitResult),
                Percent = totalDone > 0 ? NumberHelper.ConvertPercentDouble(countDone / totalDone) : default,
            };
            return unitModel;
        }

        public async Task<bool> IsUnitUsed(Guid id)
        {
            return await Queryable
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted))
                .AnyAsync(x => x.Id == id && x.CourseUnitMockTests.Count > 0);
        }
    }
}
