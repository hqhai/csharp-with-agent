// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    public class UserSchoolRepository : BaseRepository<UserSchool>, IUserSchoolRepository
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;

        public UserSchoolRepository(UserDbContext dbContext, IStudentRepository studentRepository, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
            _studentRepository = studentRepository;
            _mapper = mapper;
        }

        public async Task<Guid> GetSchoolIdAsync()
        {
            var targetRoles = new List<string> { EnumRole.AdminSchool.ToString(), EnumRole.TeacherCampus.ToString(), EnumRole.AdminCampus.ToString() };
            var hasMatchedRole = _authContext.Roles != null && _authContext.Roles.Any(r => targetRoles.Contains(r));
            if (hasMatchedRole)
            {
                var userSchool = await Queryable.FirstOrDefaultAsync(x => x.UserId == _authContext.CurrentUserId);
                return userSchool?.SchoolId ?? default;
            }
            return default;
        }

        public async Task<IList<StudentModel>> GetStudentsByRoleAdminSchoolAsync()
        {
            var targetRoles = new List<string> { EnumRole.AdminSchool.ToString(), EnumRole.TeacherCampus.ToString(), EnumRole.AdminCampus.ToString() };
            var hasMatchedRole = _authContext.Roles != null && _authContext.Roles.Any(r => targetRoles.Contains(r));
            if (hasMatchedRole)
            {
                var userSchool = await Queryable.FirstOrDefaultAsync(x => x.UserId == _authContext.CurrentUserId);
                if (userSchool == null)
                {
                    return new List<StudentModel>();
                }
                var students = await _studentRepository.Queryable.Where(x => x.SchoolId == userSchool.SchoolId).OrderByDescending(x => x.CreatedDate).ToListAsync();
                return _mapper.Map<IList<StudentModel>>(students);
            }
            return new List<StudentModel>();
        }

        public async Task<bool> CheckStudentToAdminSchoolAsync(Guid studentId)
        {
            var targetRoles = new List<string> { EnumRole.AdminSchool.ToString(), EnumRole.TeacherCampus.ToString(), EnumRole.AdminCampus.ToString() };
            var hasMatchedRole = _authContext.Roles != null && _authContext.Roles.Any(r => targetRoles.Contains(r));
            if (hasMatchedRole)
            {
                var userSchool = await Queryable.FirstOrDefaultAsync(x => x.UserId == _authContext.CurrentUserId);
                if (userSchool == null)
                {
                    return false;
                }
                var student = await _studentRepository.GetByIdAsync(studentId);
                return student?.SchoolId == userSchool.SchoolId;
            }
            return true;
        }
    }
}
