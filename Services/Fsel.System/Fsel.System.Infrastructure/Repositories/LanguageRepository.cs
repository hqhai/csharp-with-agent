// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;

    public class LanguageRepository : BaseRepository<Language>, ILanguageRepository
    {
        public LanguageRepository(SystemDbContext dbContext, SystemReadDbContext readDbContext, AuthContext authContext, IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
