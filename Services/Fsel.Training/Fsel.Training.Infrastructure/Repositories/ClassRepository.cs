// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Repositories
{
    using Fsel.Training.Doman.IRepositories;
    using Fsel.Core.Base;
    using Fsel.Training.Doman.Entities;

    public class ClassRepository : BaseRepository<Class>, IClassRepository
    {
        public ClassRepository(TrainingDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
