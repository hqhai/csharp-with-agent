// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.Enums;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Entities.SkillScoresConfigs;
using Fsel.Course.Domain.Entities.V1i1;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class HomeWorkRepository : BaseRepository<HomeWork>, IHomeWorkRepository
    {
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IHomeWorkQuestionRepository _homeWorkQuestionRepository;
        private readonly IQuestionRepository _questionRepository;

        public HomeWorkRepository(CourseDbContext dbContext,
            CourseReadDbContext readDbContext,
            AuthContext authContext,
            IMapper mapper,
            ILessonModuleRepository lessonModuleRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            IHomeWorkQuestionRepository homeWorkQuestionRepository,
            IQuestionRepository questionRepository)
            : base(dbContext, readDbContext, authContext, mapper)
        {
            _lessonModuleRepository = lessonModuleRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _homeWorkQuestionRepository = homeWorkQuestionRepository;
            _questionRepository = questionRepository;
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
                        VersionType = x.VersionType,
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

        public async Task<IList<SkillScores>> GetSkillScoresAsync(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return new List<SkillScores>();
            }

            // 1) Multiplier theo số lần HomeWorkId xuất hiện
            var idCounts = ids
                .GroupBy(x => x)
                .ToDictionary(g => g.Key, g => g.Count());

            var distinctIds = idCounts.Keys.ToList();

            // 2) Query base: lấy rows theo HomeWorkId + Skill + Question
            //    Nếu Skill là navigation: có thể Join bảng Skill thay vì dùng navigation.
            var rows = await (
                from hw in ReadQueryable.AsNoTracking()
                join hwq in _homeWorkQuestionRepository.ReadQueryable.AsNoTracking()
                    on hw.Id equals hwq.HomeWorkId
                join q in _questionRepository.ReadQueryable.AsNoTracking()
                    on hwq.QuestionId equals q.Id
                where distinctIds.Contains(hw.Id)
                      && !q.Ungraded
                      && q.QuestionType != EnumQuestionType.ExercisePreparation
                select new
                {
                    HomeWorkId = hw.Id,
                    hw.CourseSkill,
                    hw.SkillId,
                    SkillName = hw.Skill != null ? hw.Skill.Name : string.Empty,
                    QuestionId = q.Id,
                    q.CorrectTotal
                }
            ).ToListAsync();

            if (rows.Count == 0)
            {
                return new List<SkillScores>();
            }

            // 3) Gom theo (HomeWorkId, Skill) => totals cho 1 lần xuất hiện homework
            //    rồi nhân theo số lần homework xuất hiện trong ids
            var perHomeWorkSkill = rows
                .GroupBy(x => new { x.HomeWorkId, x.SkillId, x.CourseSkill, x.SkillName })
                .Select(g =>
                {
                    var totalQuestion = g.Select(x => x.QuestionId).Distinct().Count();
                    var totalCount = g.Sum(x => x.CorrectTotal);

                    var multiplier = idCounts.TryGetValue(g.Key.HomeWorkId, out var m) ? m : 1;

                    return new SkillScores
                    {
                        SkillId = g.Key.SkillId,
                        Skill = g.Key.CourseSkill,
                        SkillName = g.Key.SkillName,
                        TotalQuestion = totalQuestion * multiplier,
                        TotalCount = totalCount * multiplier
                    };
                })
                .ToList();

            // 4) Gom cuối theo Skill
            return perHomeWorkSkill
                .GroupBy(x => new { x.SkillId, x.Skill, x.SkillName })
                .Select(g => new SkillScores
                {
                    SkillId = g.Key.SkillId,
                    Skill = g.Key.Skill,
                    SkillName = g.Key.SkillName,
                    TotalQuestion = g.Sum(x => x.TotalQuestion),
                    TotalCount = g.Sum(x => x.TotalCount)
                })
                .ToList();
        }
    }
}
