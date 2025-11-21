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

        public async Task<(IDictionary<Guid, (HomeWork, HomeWorkResult)>, IDictionary<Guid, HomeWork>)>
        BuildHomeWorkLookupsAsync(LessonResult lessonResult, IList<LessonModule> lessonModules)
        {
            var homeWorkOriginalIds = lessonModules
                .Where(x => x.LessonConfigType == EnumLessonConfigType.HomeWork)
                .Select(x => x.OriginalId)
                .Distinct()
                .ToList();

            if (!homeWorkOriginalIds.Any())
            {
                return (new Dictionary<Guid, (HomeWork, HomeWorkResult)>(),
                        new Dictionary<Guid, HomeWork>());
            }

            var homeWorkResults = await (from baseQ in _homeWorkResultRepository.ReadQueryable
                                         where baseQ.LessonResultId == lessonResult.Id
                                         join homeWork in ReadQueryable
                                             on baseQ.HomeWorkId equals homeWork.Id
                                         select new
                                         {
                                             HomeWork = homeWork,
                                             HomeWorkResult = baseQ
                                         }).ToListAsync();

            var homeWorkOriginalIdsHasResult = homeWorkResults
                .Where(x => x.HomeWork != null)
                .Select(x => x.HomeWork!.OriginalId)
                .Distinct();

            var pendingHomeWorkOriginalIds = homeWorkOriginalIds
                .Except(homeWorkOriginalIdsHasResult)
                .ToList();

            var homeWorkDics = await GetHomeWorkDicAsync(pendingHomeWorkOriginalIds);

            var homeWorkResultsByOriginalId = homeWorkResults
                .Where(x => x.HomeWork != null)
                .ToDictionary(
                    x => x.HomeWork!.OriginalId,
                    x => (HomeWork: x.HomeWork!, HomeWorkResult: x.HomeWorkResult));

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
