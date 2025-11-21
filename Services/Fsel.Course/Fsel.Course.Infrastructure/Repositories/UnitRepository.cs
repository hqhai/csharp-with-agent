// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.Enums;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Entities.V1i1;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class UnitRepository : BaseRepository<Unit>, IUnitRepository
    {
        private readonly IMapper _mapper;
        private readonly IUnitResultRepository _unitResultRepository;

        public UnitRepository(CourseDbContext dbContext,
            CourseReadDbContext readDbContext,
            AuthContext authContext,
            IMapper mapper,
            IUnitResultRepository unitResultRepository) : base(dbContext, readDbContext, authContext, mapper)
        {
            _mapper = mapper;
            _unitResultRepository = unitResultRepository;
        }

        public override async Task<Unit?> GetIncludeByIdAsync(Guid id)
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

        public async Task<Unit?> GetIncludeAsync(Guid? id, Guid courseId, Guid? studentId)
        {
            try
            {
                return await Queryable.Include(x => x.UnitSkillMockTests)
                                    .Include(x => x.MockTestResults.Where(x => x.StudentId == studentId && x.CourseId == courseId))
                                    .Include(x => x.UnitLessons)
                                    .Include(x => x.LessonResults.Where(x => x.StudentId == studentId && x.CourseId == courseId))
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

        public async Task<bool> IsUnitUsed(Guid id)
        {
            return await Queryable
                .Include(x => x.CourseUnitMockTests.Where(n => !n.IsDeleted))
                .AnyAsync(x => x.Id == id && x.CourseUnitMockTests.Count > 0);
        }

        public async Task<bool> IsUsingByClient(Guid id)
        {
            return await DbContext.Set<UnitResult>().AsQueryable()
                  .AnyAsync(x => x.UnitId == id);
        }

        public async Task<(IDictionary<Guid, (Unit, UnitResult)>, IDictionary<Guid, Unit>)> BuildUnitLookupsAsync(CourseResult courseResult, IList<CourseModule> courseModules)
        {
            var unitOriginalIds = courseModules
                .Where(x => x.CourseConfigType == EnumCourseConfigType.Unit)
                .Select(x => x.OriginalId)
                .Distinct()
                .ToList();

            if (!unitOriginalIds.Any())
            {
                return (new Dictionary<Guid, (Unit, UnitResult)>(),
                        new Dictionary<Guid, Unit>());
            }

            var unitResults = await (from baseQ in _unitResultRepository.ReadQueryable
                                     where baseQ.CourseResultId == courseResult.Id
                                     join unit in ReadQueryable on baseQ.UnitId equals unit.Id
                                     select new
                                     {
                                         Unit = unit,
                                         UnitResult = baseQ
                                     }).ToListAsync();

            var unitOriginalIdsHasResult = unitResults
                .Where(x => x.Unit != null)
                .Select(x => x.Unit!.OriginalId)
                .Distinct();

            var pendingUnitOriginalIds = unitOriginalIds
                .Except(unitOriginalIdsHasResult)
                .ToList();

            var unitDics = await GetUnitDicAsync(pendingUnitOriginalIds);

            var unitResultsByOriginalId = unitResults
                .Where(x => x.Unit != null)
                .ToDictionary(
                    x => x.Unit!.OriginalId,
                    x => (Unit: x.Unit!, UnitResult: x.UnitResult));

            return (unitResultsByOriginalId, unitDics);
        }

        public async Task<IDictionary<Guid, Unit>> GetUnitDicAsync(IList<Guid>? originalIds)
        {
            if (originalIds == null || originalIds.Count == 0)
            {
                return new Dictionary<Guid, Unit>();
            }

            var units = await ReadQueryable.WhereBulkContains(originalIds, x => x.OriginalId)
                                         .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                                         .ToListAsync();

            return units.ToDictionary(x => x.OriginalId);
        }
    }
}
