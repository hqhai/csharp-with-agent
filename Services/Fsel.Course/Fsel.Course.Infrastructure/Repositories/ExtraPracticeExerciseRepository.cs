// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;

    public class ExtraPracticeExerciseRepository : BaseRepository<ExtraPracticeExercise>, IExtraPracticeExerciseRepository
    {
        public ExtraPracticeExerciseRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
