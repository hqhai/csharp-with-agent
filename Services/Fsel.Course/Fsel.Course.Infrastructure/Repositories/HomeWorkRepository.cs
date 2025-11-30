// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.Enums;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Entities.V1i1;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class HomeWorkRepository : BaseRepository<HomeWork>, IHomeWorkRepository
    {
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;

        public HomeWorkRepository(CourseDbContext dbContext,
            CourseReadDbContext readDbContext,
            AuthContext authContext,
            IMapper mapper,
            ILessonModuleRepository lessonModuleRepository,
            IHomeWorkResultRepository homeWorkResultRepository)
            : base(dbContext, readDbContext, authContext, mapper)
        {
            _lessonModuleRepository = lessonModuleRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
        }

        public async Task<IDictionary<Guid, HomeWork>> GetHomeWorkDicAsync(IList<Guid>? originalIds)
        {
            if (originalIds == null || originalIds.Count == 0)
            {
                return new Dictionary<Guid, HomeWork>();
            }

            var homeWorks = await ReadQueryable.WhereBulkContains(originalIds, x => x.OriginalId)
                                            .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                                            .ToListAsync();

            return homeWorks.ToDictionary(x => x.OriginalId);
        }

        public async Task<(IDictionary<Guid, (HomeWork, LessonModule, HomeWorkResult)>, IDictionary<Guid, HomeWork>)>
        BuildHomeWorkLookupsAsync(LessonResult? lessonResult, IList<LessonModule> lessonModules)
        {
            var homeWorkOriginalIds = lessonModules
                .Where(x => x.LessonConfigType == EnumLessonConfigType.HomeWork)
                .Select(x => new { x.OriginalId, x.Id })
                .Distinct()
                .ToHashSet();

            if (!homeWorkOriginalIds.Any())
            {
                return (new Dictionary<Guid, (HomeWork, LessonModule, HomeWorkResult)>(),
                        new Dictionary<Guid, HomeWork>());
            }

            var homeWorkResultsByOriginalId = new Dictionary<Guid, (HomeWork, LessonModule, HomeWorkResult)>();
            var pendingHomeWorkOriginalIds = new List<Guid>();
            if (lessonResult != null)
            {
                var homeWorkResults = await (from baseQ in _homeWorkResultRepository.ReadQueryable
                                             where baseQ.LessonResultId == lessonResult.Id
                                             join lessonModule in _lessonModuleRepository.ReadQueryable
                                                 on baseQ.LessonModuleId equals lessonModule.Id

                                             join homeWork in ReadQueryable
                                                 on baseQ.HomeWorkId equals homeWork.Id
                                             select new
                                             {
                                                 LessonModule = lessonModule,
                                                 HomeWork = homeWork,
                                                 HomeWorkResult = baseQ
                                             }).ToListAsync();

                var homeWorkOriginalIdsHasResult = homeWorkResults
                                    .Where(x => x.HomeWork != null)
                                    .Select(x => new
                                    {
                                        x.HomeWork!.OriginalId,
                                        Id = x.LessonModule.Id
                                    })
                                    .ToHashSet();

                pendingHomeWorkOriginalIds = homeWorkOriginalIds
                            .Where(x => !homeWorkOriginalIdsHasResult.Contains(
                                new
                                {
                                    x.OriginalId,
                                    x.Id
                                }))
                            .Select(x => x.OriginalId)   // <- chọn ra Guid
                            .Distinct()
                            .ToList();

                homeWorkResultsByOriginalId = homeWorkResults
                   .Where(x => x.HomeWork != null)
                   .ToDictionary(
                       x => x.LessonModule.Id,
                       x => (HomeWork: x.HomeWork!, LessonModule: x.LessonModule, HomeWorkResult: x.HomeWorkResult));
            }
            else
            {
                pendingHomeWorkOriginalIds = homeWorkOriginalIds.Select(x => x.OriginalId)   // <- chọn ra Guid
                                                                .Distinct().ToList();
            }
            var homeWorkDics = await GetHomeWorkDicAsync(pendingHomeWorkOriginalIds);
            return (homeWorkResultsByOriginalId, homeWorkDics);
        }

        public async Task<bool> IsHomeWorkUsed(Guid? id)
        {
            return await (from baseQ in Queryable
                          join lessonModule in _lessonModuleRepository.Queryable on baseQ.OriginalId equals lessonModule.OriginalId
                          where baseQ.Id == id
                          select baseQ.Id).AnyAsync();
        }

        public async Task<HomeWorkModel?> GetIncludeAllAsync(Guid? id)
        {
            try
            {
                return await Queryable.Where(x => x.Id == id)
                    .Select(x => new HomeWorkModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        CreatedDate = x.CreatedDate,
                        UpdatedDate = x.UpdatedDate,
                        Code = x.Code,
                        MediaPost = x.MediaPost,
                        IsActive = x.LessonHomeWorks.Any(),
                        CourseLevel = x.CourseLevel,
                        CourseSkill = x.CourseSkill,
                        SkillId = x.SkillId,
                        SkillName = x.Skill != null ? x.Skill.Name : null,
                        ProgramId = x.ProgramId,
                        ProgramName = x.Program != null ? x.Program.Name : null,
                        LevelId = x.LevelId,
                        LevelName = x.Level != null ? x.Level.Name : null,
                        OriginalId = x.OriginalId,
                        MediaPostContentRuby = x.MediaPostContentRuby,
                        Questions = x.HomeWorkQuestions.Where(m => m.Question != null && !m.IsDeleted).Select(m => m.Question).OrderBy(x => x!.CreatedDate).Select(m => new QuestionModel()
                        {
                            Id = m!.Id,
                            QuestionType = m.QuestionType,
                            Explanation = m.Explanation,
                            Ungraded = m.Ungraded,
                            CorrectTotal = m.CorrectTotal,
                            Config = m.Config
                        }).ToList()
                    }).AsNoTracking().FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IList<HomeWork>> GetListAsync(LessonResult lessonResult)
        {
            try
            {
                return await Queryable.Include(x => x!.LessonHomeWorks.Where(x => x.LessonId == lessonResult.LessonId))
                                        .Include(x => x!.HomeWorkQuestions)
                                        .Include(x => x.HomeWorkResults.Where(x => x.LessonResultId == lessonResult.Id))
                                        .Where(x => x.LessonHomeWorks.Any(x => x.LessonId == lessonResult.LessonId))
                                        .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(IList<HomeWork>, IList<HomeWorkResult>)> GetModulesListAsync(LessonResult lessonResult, Guid? homeWorkId)
        {
            try
            {
                var query = from baseQ in Queryable.Include(x => x.HomeWorkQuestions)
                            where !homeWorkId.HasValue || baseQ.Id == homeWorkId

                            join lessonModule in _lessonModuleRepository.Queryable on baseQ.OriginalId equals lessonModule.OriginalId
                            where lessonModule.LessonId == lessonResult.LessonId && lessonModule.LessonConfigType == EnumLessonConfigType.HomeWork

                            join homeWorkResult in _homeWorkResultRepository.Queryable.Where(x => x.LessonResultId == lessonResult.Id)
                                    on lessonModule.Id equals homeWorkResult.LessonModuleId into homeWorkResultJoin
                            from homeWorkResult in homeWorkResultJoin.DefaultIfEmpty()
                            select new
                            {
                                HomeWork = baseQ,
                                HomeWorkResult = homeWorkResult,
                                DisplayOrder = lessonModule.DisplayOrder
                            };
                var data = await query.OrderBy(x => x.DisplayOrder).ToListAsync();
                return (data.Select(x => x.HomeWork).ToList(), data.Select(x => x.HomeWorkResult).ToList());
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<HomeWork?> GetAsync(HomeWorkResult homeWorkResult)
        {
            try
            {
                return await Queryable
                        .Include(x => x.HomeWorkQuestions)
                        .ThenInclude(x => x.Question)
                        .Include(x => x.HomeWorkQuestions)
                        .ThenInclude(x => x.HomeWorkAnswers.Where(n => n.HomeWorkResultId == homeWorkResult.Id))
                        .Where(x => x.Id == homeWorkResult.HomeWorkId)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> IsUsingByClient(Guid id)
        {
            return await DbContext.Set<HomeWorkResult>().AsQueryable()
                  .AnyAsync(x => x.HomeWorkId == id);
        }
    }
}
