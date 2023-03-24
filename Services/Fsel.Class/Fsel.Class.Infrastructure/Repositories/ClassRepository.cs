// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Class.Infrastructure.Repositories
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Class.Doman.IRepositories;
    using Fsel.Core.Base;
    using Classes = Fsel.Class.Doman.Entities.Class;

    public class ClassRepository : BaseRepository<Classes>, IClassRepository
    {
        public ClassRepository(ClassDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
