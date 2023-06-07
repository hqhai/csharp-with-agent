// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class ExtraPracticeRepository : BaseRepository<ExtraPractice>, IExtraPracticeRepository
    {
        public ExtraPracticeRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public override async Task<ExtraPractice?> GetIncludeByIdAsync(Guid id, int? siteId = null)
        {
            try
            {
                return await Queryable.Include(x => x.ExtraPracticeChapters.Where(n => !n.IsDeleted))
                                    .ThenInclude(x => x.ExtraPracticeExercises.Where(n => !n.IsDeleted))
                                    .ThenInclude(x => x.Exercise)
                                    .ThenInclude(x => x!.ExerciseQuestions.Where(n => !n.IsDeleted))
                                    .ThenInclude(x => x.Question)
                                    .Include(x => x.LessonExtraPractices.Where(n => !n.IsDeleted))
                                    .Include(x => x.Video)
                                    .ThenInclude(x => x!.VideoTimeCodes.Where(x => !x.IsDeleted))
                                    .ThenInclude(x => x.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null))
                                    .ThenInclude(x => x.Exercise)
                                    .ThenInclude(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted))
                                    .ThenInclude(x => x.Question)
                                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
