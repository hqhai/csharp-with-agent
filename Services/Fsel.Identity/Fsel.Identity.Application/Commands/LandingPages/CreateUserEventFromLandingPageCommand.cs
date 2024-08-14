// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.LandingPages
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.StudentRanking;
    using Fsel.Identity.Infrastructure.Repositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CreateUserEventFromLandingPageCommand : CreateUserEventFromLandingPageCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateUserEventFromLandingPageCommandHandler : IRequestHandler<CreateUserEventFromLandingPageCommand, MethodResult<bool>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentRankingEventsRepository _studentRankingEventsRepository;
        private readonly UserManager<User> _userManager;
        private readonly IPlatformRepository _platformRepository;
        private const string DefaultPassword = "Fsel@2024";

        public CreateUserEventFromLandingPageCommandHandler(ICompetitionEventsRepository competitionEventsRepository, IStudentRankingEventsRepository studentRankingEventsRepository, UserManager<User> userManager, IPlatformRepository platformRepository)
        {
            _competitionEventsRepository = competitionEventsRepository;
            _studentRankingEventsRepository = studentRankingEventsRepository;
            _userManager = userManager;
            _platformRepository = platformRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateUserEventFromLandingPageCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var @event = await _competitionEventsRepository.Queryable.FirstOrDefaultAsync(p => p.EventCode.ToLower() == request.EventCode.ToLower(), cancellationToken);
            if (@event == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(p => p.UserName.ToLower() == request.Email.ToLower() || p.Email.ToLower() == request.Email.ToLower(), cancellationToken);
            if (user == null)
            {
                var platform = await _platformRepository.GetPlatformAsync(EnumPlatformCode.LMS, cancellationToken);
                if (platform == null)
                {
                    methodResult.AddErrorBadRequest("Platform null");
                    return methodResult;
                }

                Microsoft.AspNetCore.Identity.IdentityResult identityStudentResult;
                user = new User()
                {
                    UserName = request.Email,
                    FullName = request.FirstName + " " + request.LastName,
                    Email = request.Email,
                    EmailConfirmed = true,
                    PhoneNumber = request.PhoneNumber,
                    PhoneNumberConfirmed = false,
                    Human = new Human()
                    {
                        FullName = request.FirstName + " " + request.LastName,
                        Birthday = request.BirthDay,
                        PhoneNumber = request.PhoneNumber,
                        Email = request.Email,
                        Student = new Student()
                        {
                            Occupation = "Student",
                            CourseLevel = EnumCourseLevel.A1,
                            CreatedByParent = false
                        }
                    },
                    UserPlatforms = new List<UserPlatform>()
                {
                    new UserPlatform()
                    {
                        PlatformId = platform.Id
                    }
                },
                    UserSettings = new List<UserSetting>()
                    {
                       new UserSetting(true)
                    }
                };

                var passwordValidator = new Microsoft.AspNetCore.Identity.PasswordValidator<User>();
                var validPassword = await passwordValidator.ValidateAsync(_userManager, user, DefaultPassword);
                if (!validPassword.Succeeded)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.PasswordIsNotValid));
                    return methodResult;
                }

                if (!user.IsValid())
                {
                    methodResult.AddError(user.ErrorMessages);
                    return methodResult;
                }

                identityStudentResult = await _userManager.CreateAsync(user, DefaultPassword);
                if (!identityStudentResult.Succeeded)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate));
                    return methodResult;
                }
                await _userManager.AddToRoleAsync(user, EnumRole.Student.ToString());

                _studentRankingEventsRepository.Add(new StudentRankingEvents()
                {
                    StudentId = user.Human.Student.Id,
                    CompetitionRankingId = @event.Id
                });
            }
            return methodResult;
        }
    }
}
