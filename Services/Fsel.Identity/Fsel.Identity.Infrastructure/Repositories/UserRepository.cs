// Copyright (c) Atlantic. All rights reserved.

using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly IStudentRepository _studentRepository;
        private readonly IParentRepository _parentRepository;

        public UserRepository(UserManager<User> userManager, IStudentRepository studentRepository, IParentRepository parentRepository)
        {
            _userManager = userManager;
            _studentRepository = studentRepository;
            _parentRepository = parentRepository;
        }

        public async Task<User> GenerateUserDataAsync(User user, EnumRoleRegister role)
        {
            if (user == null)
            {
                return new User();
            }

            var currentDate = DateTime.UtcNow;
            var weekNumber = (currentDate.DayOfYear - 1) / 7 + 1;

            if (role == EnumRoleRegister.Student)
            {
                var stt = await _studentRepository.Queryable.CountAsync();
                var lastDigitOfYear = currentDate.Year % 10;
                var lastOfBirthDay = user.Birthday!.Value.Year % 100;

                user.Code = $"HN_{weekNumber}{lastDigitOfYear}{lastOfBirthDay}{stt:000}";
                if (await _studentRepository.Queryable.Include(x => x.User).AnyAsync(x => x!.User!.Code == user.Code))
                {
                    user.Code = $"HN_{weekNumber}{lastDigitOfYear}{2}{lastOfBirthDay}{stt:000}";
                }

                var level = EnumCourseLevel.A2;
                int age = Shared.Helpers.DateTimeHelper.GetYearOld(user.Birthday);
                if (age >= 14)
                {
                    level = EnumCourseLevel.B1;
                }

                user.Student = new Student
                {
                    UserId = user.Id,
                    CreatedByParent = false,
                    Occupation = nameof(Student),
                    CourseLevel = level
                };
            }
            else if (role == EnumRoleRegister.Parent)
            {
                var stt = await _parentRepository.Queryable.CountAsync();
                user.Parent = new Parent
                {
                    UserId = user.Id,
                };
                user.Code = $"PH_{weekNumber}{stt:0000}";
            }

            return user;
        }
    }
}
