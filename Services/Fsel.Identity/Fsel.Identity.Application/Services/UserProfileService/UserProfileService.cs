// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.UserProfileService
{
    using System;
    using System.Data;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Fsel.Common.Constants;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.InteractionService;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.Constants;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Shared.Enums;
    using IdentityModel;
    using IdentityServer4;
    using IdentityServer4.AspNetIdentity;
    using IdentityServer4.Extensions;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class UserProfileService : ProfileService<User>, IProfileService
    {
        private readonly Core.Base.Managers.UserManager<User> _userManager;
        private readonly Core.Base.Managers.RoleManager<Role> _roleManager;
        private readonly IInteractionService _interactionService;
        private readonly ITrainingService _trainingService;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly IOrderService _orderService;

        public UserProfileService(Core.Base.Managers.UserManager<User> usermanager, Core.Base.Managers.RoleManager<Role> roleManager, IUserClaimsPrincipalFactory<User> userClaimsPrincipalFactory, IInteractionService interactionService, ITrainingService trainingService, ILmsCourseService lmsCourseService, IOrderService orderService)
            : base(usermanager, userClaimsPrincipalFactory)
        {
            _userManager = usermanager;
            _roleManager = roleManager;
            _interactionService = interactionService;
            _trainingService = trainingService;
            _lmsCourseService = lmsCourseService;
            _orderService = orderService;
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var userId = _userManager.GetUserId(context.Subject).Parse<Guid>();
            var user = await _userManager.Users.Include(x => x.UserSchools).Include(x => x.Student).FirstOrDefaultAsync(x => x.Id == userId);

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

                if (context.RequestedResources.ParsedScopes.Any(x => x.ParsedName == IdentityServerConstants.StandardScopes.Profile))
                {
                    claims.Add(new Claim(JwtClaimNames.UserId, user.Id.ToString()));
                    claims.Add(new Claim(JwtClaimNames.UserName, user.UserName ?? string.Empty, ClaimValueTypes.String));
                    claims.Add(new Claim(JwtClaimNames.FullName, user.FullName ?? string.Empty, ClaimValueTypes.String));
                    claims.Add(new Claim(JwtClaimNames.Surname, user.LastName ?? string.Empty, ClaimValueTypes.String));
                    claims.Add(new Claim(JwtClaimNames.GivenName, user.FirstName ?? string.Empty, ClaimValueTypes.String));

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
                        claims.Add(new Claim(IdentityServerSettings.JwtApiClaimNames.ClassId, classId.ToString() ?? string.Empty, ClaimValueTypes.String));
                        claims.Add(new Claim(IdentityServerSettings.JwtApiClaimNames.ClassCode, classCode ?? string.Empty, ClaimValueTypes.String));
                        claims.Add(new Claim(IdentityServerSettings.JwtApiClaimNames.IsPlacementTest, isPlacementTest?.ToString() ?? string.Empty, ClaimValueTypes.Boolean));
                        claims.Add(new Claim(IdentityServerSettings.JwtApiClaimNames.IsSurvey, isSurvey?.ToString() ?? string.Empty, ClaimValueTypes.Boolean));
                        claims.Add(new Claim(IdentityServerSettings.JwtApiClaimNames.IsOrder, isOrder.ToString(), ClaimValueTypes.Boolean));
                    }
                    else if (roles.Contains(EnumRole.AdminSchool.ToString()))
                    {
                        var schoolId = user.UserSchools.FirstOrDefault()?.SchoolId;
                        if (schoolId.HasValue)
                        {
                            claims.Add(new Claim(IdentityServerSettings.JwtApiClaimNames.SchoolId, schoolId.Value.ToString()));
                        }
                    }

                    #endregion
                }

                if (context.RequestedResources.ParsedScopes.Any(x => x.ParsedName == IdentityServerConstants.StandardScopes.Email))
                {
                    claims.Add(new Claim(JwtClaimTypes.Email, user.Email ?? string.Empty, ClaimValueTypes.String));
                    claims.Add(new Claim(JwtClaimTypes.EmailVerified, user.EmailConfirmed.ToString(), ClaimValueTypes.Boolean));
                }

                if (context.RequestedResources.ParsedScopes.Any(x => x.ParsedName == IdentityServerConstants.StandardScopes.Phone))
                {
                    claims.Add(new Claim(JwtClaimTypes.PhoneNumber, user.PhoneNumber ?? string.Empty, ClaimValueTypes.String));
                    claims.Add(new Claim(JwtClaimTypes.PhoneNumberVerified, user.PhoneNumberConfirmed.ToString(), ClaimValueTypes.Boolean));
                }

                if (context.RequestedResources.ParsedScopes.Any(x => x.ParsedName == IdentityServerConstants.StandardScopes.OpenId))
                {
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

            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            var active = (user != null && (!user.LockoutEnabled || user.LockoutEnd == null)) ||
                         (user != null && user.LockoutEnabled && user.LockoutEnd != null &&
                          DateTime.UtcNow > user.LockoutEnd);

            context.IsActive = active;
        }
    }
}
