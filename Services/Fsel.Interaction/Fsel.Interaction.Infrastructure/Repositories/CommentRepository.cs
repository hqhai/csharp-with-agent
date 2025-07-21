// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Interaction.Domain.IRepositories;

    public class CommentRepository : BaseRepository<Comment>, ICommentRepository
    {
        public CommentRepository(InteractionDbContext dbContext, InteractionReadDbContext readDbContext, AuthContext authContext, AutoMapper.IMapper mapper)
            : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
