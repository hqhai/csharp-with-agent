// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.IRepositories;

    public class ClassLiveCalendarRepository : BaseRepository<ClassLiveCalendar>, IClassLiveCalendarRepository
    {
        public ClassLiveCalendarRepository(TrainingDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
