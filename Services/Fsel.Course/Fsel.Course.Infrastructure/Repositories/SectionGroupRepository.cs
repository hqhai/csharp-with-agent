// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    public class SectionGroupRepository : BaseRepository<SectionGroup>, ISectionGroupRepository
    {
        public SectionGroupRepository(CourseDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }

        public async Task<SectionGroup?> GetAsync(Guid id, Guid? objectResultId, string? type, EnumCourseSkill? enumCourseSkill)
        {
            if (string.IsNullOrEmpty(type))
            {
                return default;
            }
            switch (type)
            {
                case nameof(FinalTest):
                    return await Queryable.Include(x => x.Sections)
                                    .ThenInclude(x => x.SectionQuestions)
                                  .Include(x => x.Sections)
                                    .ThenInclude(x => x.SectionQuestions)
                                    .ThenInclude(x => x.FinalTestAnswers.Where(x => x.FinalTestResultId == objectResultId))
                                  .Where(x => x.Id == id)
                                  .FirstOrDefaultAsync();

                case nameof(MockTest):
                    return await Queryable.Include(x => x.Sections)
                                    .ThenInclude(x => x.SectionQuestions)
                                  .Include(x => x.Sections)
                                    .ThenInclude(x => x.SectionQuestions)
                                    .ThenInclude(x => x.MockTestAnswers)
                                  .Where(x => x.Id == id)
                                  .FirstOrDefaultAsync();

                default:
                    return default;
            }
        }
    }
}
