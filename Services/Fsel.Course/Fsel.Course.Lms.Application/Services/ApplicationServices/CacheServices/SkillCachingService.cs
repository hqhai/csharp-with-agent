// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices
{
    using AutoMapper;
    using Fsel.Common.Caching;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.SkillModels;
    using Microsoft.EntityFrameworkCore;

    public interface ISkillCachingService : IEntityCachingService<Skill>
    {
        Task<IList<SkillModel>> GetSkillsPtByProgramIdAsync(Guid programId, CancellationToken cancellationToken = default);
    }

    public class SkillCachingService : EntityCachingService<Skill>, ISkillCachingService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICategoryTestBankRepository _categoryTestBankRepository;
        private readonly ITestRepository _testRepository;
        private readonly ITestSectionRepository _testSectionRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IMapper _mapper;

        public SkillCachingService(ICacheService<Skill> cacheService,
            ICategoryRepository categoryRepository,
            ICategoryTestBankRepository categoryTestBankRepository,
            ITestRepository testRepository,
            ITestSectionRepository testSectionRepository,
            ISkillRepository skillRepository,
            IMapper mapper) : base(cacheService)

        {
            _categoryRepository = categoryRepository;
            _categoryTestBankRepository = categoryTestBankRepository;
            _testRepository = testRepository;
            _testSectionRepository = testSectionRepository;
            _skillRepository = skillRepository;
            _mapper = mapper;
        }

        public override List<string> Tags => new List<string>
        {
            "CourseService",
            "Entity",
            nameof(Skill),
        };

        public override string Prefix => $"CourseService:Entity:{nameof(Skill)}";

        public async Task<IList<SkillModel>> GetSkillsPtByProgramIdAsync(Guid programId, CancellationToken cancellationToken = default)
        {
            var skills = await GetOrSetAsync($"skills-pt-{programId}", async (ctx, _) =>
            {
                var category = await _categoryRepository.ReadQueryable
                            .FirstOrDefaultAsync(x => x.Id == programId && x.Type == Shared.Enums.EnumTypeCategory.Program, cancellationToken);

                if (category != null && category.TestMode.HasValue && new List<EnumTestMode> { EnumTestMode.Default, EnumTestMode.Not }.Contains(category.TestMode.Value))
                {
                    category = await _categoryRepository.ReadQueryable
                            .Where(x => x.TestMode == EnumTestMode.Custom)
                            .Where(x => x.IsTestDefault)
                            .FirstOrDefaultAsync(x => x.ParentId == category.ParentId && x.Type == Shared.Enums.EnumTypeCategory.Program, cancellationToken);
                }
                if (category == null)
                {
                    return new List<Skill>();
                }

                var query = from baseQ in _categoryTestBankRepository.ReadQueryable
                            join t in _testRepository.ReadQueryable on baseQ.TestOriginalId equals t.OriginalId
                            join ts in _testSectionRepository.ReadQueryable on t.Id equals ts.TestId
                            join s in _skillRepository.ReadQueryable on ts.SkillId equals s.Id
                            where baseQ.ProgramId == category.Id
                            && ts.ParentId == null
                            && t.VersionStatus == Common.Enums.EnumVersionStatus.LastVersion
                            select s;

                return await query.Distinct().OrderBy(x => x.Name).ToListAsync(cancellationToken);
            }, token: cancellationToken);

            return _mapper.Map<IList<SkillModel>>(skills);
        }
    }
}
