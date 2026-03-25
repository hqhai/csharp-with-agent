// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.IRepositories
{
    public interface IMasterBaseRepository<T> where T : class
    {
        IQueryable<T> Queryable { get; }
    }
}
