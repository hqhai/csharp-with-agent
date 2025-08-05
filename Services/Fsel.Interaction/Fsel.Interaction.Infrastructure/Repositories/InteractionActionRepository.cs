// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;

    public class InteractionActionRepository : BaseRepository<InteractionAction>, IInteractionActionRepository
    {
        public InteractionActionRepository(InteractionDbContext dbContext, InteractionReadDbContext readDbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
