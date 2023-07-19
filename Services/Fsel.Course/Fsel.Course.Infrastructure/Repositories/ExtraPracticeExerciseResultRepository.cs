// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;

    public class ExtraPracticeExerciseResultRepository : BaseRepository<ExtraPracticeExerciseResult>, IExtraPracticeExerciseResultRepository
    {
        public ExtraPracticeExerciseResultRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
