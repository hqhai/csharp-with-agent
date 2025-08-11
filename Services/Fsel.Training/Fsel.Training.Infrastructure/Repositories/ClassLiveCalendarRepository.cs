// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.IRepositories;

    public class ClassLiveCalendarRepository : BaseRepository<ClassLiveCalendar>, IClassLiveCalendarRepository
    {
        public ClassLiveCalendarRepository(TrainingDbContext dbContext, TrainingReadDbContext readDbContext, AuthContext authContext, AutoMapper.IMapper mapper)
            : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
