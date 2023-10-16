// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;

    public class SupportCategoryRepository : BaseRepository<SupportCategory>, ISupportCategoryRepository
    {
        public SupportCategoryRepository(InteractionDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
