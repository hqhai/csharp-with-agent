// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Repositories
{
    using Fsel.Training.Doman.IRepositories;
    using Fsel.Core.Base;
    using Fsel.Training.Doman.Entities;

    public class TrainingRepository : BaseRepository<Class>, ITrainingRepository
    {
        public TrainingRepository(TrainingDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
