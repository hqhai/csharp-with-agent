// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.EntityModels;

    public interface IUserSchoolRepository : IRepository<UserSchool>
    {
        Task<Guid?> GetSchoolIdAsync();

        Task<IList<StudentModel>> GetStudentsByRoleAdminSchoolAsync();

        Task<bool> CheckStudentToAdminSchoolAsync(Guid studentId);
    }
}
