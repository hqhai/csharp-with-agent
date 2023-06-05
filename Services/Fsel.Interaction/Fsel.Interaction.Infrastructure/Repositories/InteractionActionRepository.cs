// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;

    public class InteractionActionRepository : BaseRepository<InteractionAction>, IInteractionActionRepository
    {
        public InteractionActionRepository(InteractionDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
