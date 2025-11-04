// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.UserProfileService
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Fsel.Common.Constants;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Identity.Application.Services.InteractionService;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.Constants;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using IdentityModel;
    using IdentityServer4;
    using IdentityServer4.AspNetIdentity;
    using IdentityServer4.Extensions;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using static Fsel.Identity.Domain.Constants.IdentityServerSettings;

    public class UserProfileService : ProfileService<User>, IProfileService
    {
        private Core.Base.Managers.UserManager<User> _userManager;
        private Core.Base.Managers.RoleManager<Role> _roleManager;
        private readonly IInteractionService _interactionService;
        private readonly ITrainingService _trainingService;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly IOrderService _orderService;
        private readonly ISystemConfigRepository _systemConfigRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IUserSchoolRepository _userSchoolRepository;

        public UserProfileService(Core.Base.Managers.UserManager<User> usermanager, Core.Base.Managers.RoleManager<Role> roleManager, IUserClaimsPrincipalFactory<User> userClaimsPrincipalFactory, IInteractionService interactionService, ITrainingService trainingService, ILmsCourseService lmsCourseService, IOrderService orderService, ISystemConfigRepository systemConfigRepository, IServiceProvider serviceProvider, ICompetitionEventsRepository competitionEventsRepository, IUserSchoolRepository userSchoolRepository)
            : base(usermanager, userClaimsPrincipalFactory)
        {
            _userManager = usermanager;
            _roleManager = roleManager;
            _interactionService = interactionService;
            _trainingService = trainingService;
            _lmsCourseService = lmsCourseService;
            _orderService = orderService;
            _systemConfigRepository = systemConfigRepository;
            _serviceProvider = serviceProvider;
            _competitionEventsRepository = competitionEventsRepository;
            _userSchoolRepository = userSchoolRepository;
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var userId = _userManager.GetUserId(context.Subject);

            var tenantProvider = _serviceProvider.GetService<ITenantProvider>();
            var tenant = tenantProvider != null ? await tenantProvider.GetTenantAsync(string.Empty, userId.Parse<Guid>()) : null;
            if (tenantProvider != null)
            {
                _userManager = await tenantProvider.CreateUserManagerAsync<User>(userId: userId.Parse<Guid>()) ?? _userManager;
                _roleManager = await tenantProvider.CreateRoleManagerAsync<Role>(userId: userId.Parse<Guid>()) ?? _roleManager;
            }

            var user = await _userManager.Users.Include(x => x.UserSchools).Include(x => x.Student).FirstOrDefaultAsync(x => x.Id ==  userId.Parse<Guid>());
            if (user != null)
            {
                var claims = (await _userManager.GetClaimsAsync(user)).ToList();
                var roles = await _userManager.GetRolesAsync(user);

                foreach (var role in roles)
                {
                    claims.Add(new Claim(JwtClaimTypes.Role, role));

                    var roleEntity = await _roleManager.FindByNameAsync(role);
                    if (roleEntity != null)
                    {
                        var roleClaims = await _roleManager.GetClaimsAsync(roleEntity);
                        claims.AddRange(roleClaims);
                    }
                }

                var isEnabledExtra = await _systemConfigRepository.Queryable.Select(x => x.IsEnabled).FirstOrDefaultAsync();
                if (context.RequestedResources.ParsedScopes.Any(x => x.ParsedName == IdentityServerConstants.StandardScopes.Profile))
                {
                    claims.Add(new Claim(JwtClaimNames.UserId, user.Id.ToString(), ClaimValueTypes.String));
                    claims.Add(new Claim(JwtClaimNames.UserName, user.UserName ?? string.Empty, ClaimValueTypes.String));
                    claims.Add(new Claim(JwtClaimNames.FullName, user.FullName ?? string.Empty, ClaimValueTypes.String));
                    claims.Add(new Claim(JwtClaimNames.Surname, user.LastName ?? string.Empty, ClaimValueTypes.String));
                    claims.Add(new Claim(JwtClaimNames.GivenName, user.FirstName ?? string.Empty, ClaimValueTypes.String));
                    claims.Add(new Claim(JwtApiClaimNames.IsEnabledExtra, isEnabledExtra.ToString(), ClaimValueTypes.Boolean));

                    var schoolId = user.UserSchools.FirstOrDefault()?.SchoolId;
                    if (schoolId.HasValue)
                    {
                        claims.Add(new Claim(IdentityServerSettings.JwtApiClaimNames.SchoolId, schoolId.Value.ToString()));
                    }

                    #region Custom Profile

                    if (roles.Contains(EnumRole.Student.ToString()))
                    {
                        var student = user.Student;
                        var classStudentResult = await _trainingService.GetClassByStudentId(student?.Id ?? default);
                        var @class = classStudentResult?.Content?.Result;
                        var isPlacementTestResult = await _lmsCourseService.IsPlacementTestAsync(student?.Id ?? default);
                        var isSurveyResult = await _interactionService.IsSurveyCompleted(user.Id);

                        var classId = student?.ClassId;
                        var classCode = @class?.Code;
                        var isPlacementTest = isPlacementTestResult?.Content?.Result;
                        bool? isSurvey = null;
                        bool isOrder = false;

                        if (@class != null)
                        {
                            var order = await _orderService.GetStatusAsync(new GetStatusByUserCommandModel { CourseId = @class.CourseId, UserId = user.Id });
                            isOrder = order?.Content?.Result == EnumOrderStatus.Payment;
                        }
                        if (isSurveyResult.IsSuccessStatusCode)
                        {
                            isSurvey = isSurveyResult?.Content?.Result;
                        }

                        claims.Add(new Claim(IdentityServerSettings.JwtApiClaimNames.Code, user.Code ?? string.Empty, ClaimValueTypes.String));
                        claims.Add(new Claim(IdentityServerSettings.JwtApiClaimNames.Status, user.Status?.ToString() ?? string.Empty, ClaimValueTypes.String));
                        claims.Add(new Claim(IdentityServerSettings.JwtApiClaimNames.ClassId, classId.ToString() ?? string.Empty, ClaimValueTypes.String));
                        claims.Add(new Claim(IdentityServerSettings.JwtApiClaimNames.ClassCode, classCode ?? string.Empty, ClaimValueTypes.String));
                        claims.Add(new Claim(IdentityServerSettings.JwtApiClaimNames.IsPlacementTest, isPlacementTest?.ToString(CultureInfo.InvariantCulture) ?? string.Empty, ClaimValueTypes.Boolean));
                        claims.Add(new Claim(IdentityServerSettings.JwtApiClaimNames.IsSurvey, isSurvey?.ToString(CultureInfo.InvariantCulture) ?? string.Empty, ClaimValueTypes.Boolean));
                        claims.Add(new Claim(IdentityServerSettings.JwtApiClaimNames.IsOrder, isOrder.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Boolean));
                    }
                    else if (roles.Contains(EnumRole.AdminSchool.ToString()))
                    {
                        claims.Add(new Claim(IdentityServerSettings.JwtApiClaimNames.EventCode, await _competitionEventsRepository.GetEventCodeAsync(schoolId)));
                    }

                    #endregion Custom Profile
                }

                if (context.RequestedResources.ParsedScopes.Any(x => x.ParsedName == IdentityServerConstants.StandardScopes.Email))
                {
                    claims.Add(new Claim(JwtClaimTypes.Email, user.Email ?? string.Empty, ClaimValueTypes.String));
                    claims.Add(new Claim(JwtClaimTypes.EmailVerified, user.EmailConfirmed.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Boolean));
                }

                if (context.RequestedResources.ParsedScopes.Any(x => x.ParsedName == IdentityServerConstants.StandardScopes.Phone))
                {
                    claims.Add(new Claim(JwtClaimTypes.PhoneNumber, user.PhoneNumber ?? string.Empty, ClaimValueTypes.String));
                    claims.Add(new Claim(JwtClaimTypes.PhoneNumberVerified, user.PhoneNumberConfirmed.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Boolean));
                }

                if (context.RequestedResources.ParsedScopes.Any(x => x.ParsedName == IdentityServerConstants.StandardScopes.OpenId))
                {
                    claims.Add(new Claim(JwtClaimNames.TenantId, tenant?.Id.ToString() ?? string.Empty, ClaimValueTypes.String));
                    claims.Add(new Claim(JwtClaimNames.UserName, user.UserName ?? string.Empty, ClaimValueTypes.String));
                    claims.Add(new Claim(JwtClaimNames.UserId, user.Id.ToString()));
                }

                context.IssuedClaims.AddRange(claims);
            }

            await base.GetProfileDataAsync(context);
        }

        public override async Task IsActiveAsync(IsActiveContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            var userId = _userManager.GetUserId(context.Subject);

            var tenantProvider = _serviceProvider.GetService<ITenantProvider>();
            _userManager = tenantProvider != null ? await tenantProvider.CreateUserManagerAsync<User>(userId: userId.Parse<Guid>()) ?? _userManager : _userManager;

            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            var active = (user != null && (!user.LockoutEnabled || user.LockoutEnd == null)) ||
                         (user != null && user.LockoutEnabled && user.LockoutEnd != null &&
                          DateTime.UtcNow > user.LockoutEnd);

            context.IsActive = active;
        }
    }
}
