// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;

    public class PostRepository : BaseRepository<Post>, IPostRepository
    {
        public PostRepository(InteractionDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
