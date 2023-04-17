// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Training.Doman.Entities;
    using Fsel.Training.Doman.IRepositories;

    public class ClassStudentRepository : BaseRepository<ClassStudent>, IClassStudentRepository
    {
        public ClassStudentRepository(TrainingDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
