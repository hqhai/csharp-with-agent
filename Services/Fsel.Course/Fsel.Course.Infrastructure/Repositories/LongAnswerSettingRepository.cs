// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using AutoMapper;
    using Core.Base;
    using Domain.Entities.LongAnswerConfig;
    using Domain.IRepositories;

    public class LongAnswerSettingRepository : BaseRepository<LongAnswerSetting>, ILongAnswerSettingRepository
    {
        public LongAnswerSettingRepository(BaseDbContext masterDbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper)
            : base(masterDbContext, readDbContext,authContext, mapper)
        {
        }
    }
}
