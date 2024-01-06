// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.IRepositories;

    public class TeacherFreeTimeLiveRepository : BaseRepository<TeacherFreeTimeLive>, ITeacherFreeTimeLiveRepository
    {
        public TeacherFreeTimeLiveRepository(TrainingDbContext dbContext, AuthContext authContext, IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
