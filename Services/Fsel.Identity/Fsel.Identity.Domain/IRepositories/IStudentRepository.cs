// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.IRepositories
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Identity.Domain.Entities;

    public interface IStudentRepository : IRepository<Student>
    {
        Task<IList<Student>> GetIncludeByIdsAsync(IList<Guid> ids, int? siteId = null);
    }
}
