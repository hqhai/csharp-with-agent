// Copyright (c) Atlantic. All rights reserved.

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Constants;
using Fsel.Common.Helpers;
using Fsel.Common.ValueSettings;
using Fsel.Core.Base.Interfaces;
using Fsel.Core.Base.Managers;
using Fsel.Core.Entities;
using Fsel.Core.Extensions;
using Fsel.Identity.Application.Services.InteractionService;
using Fsel.Identity.Application.Services.LmsCourseService;
using Fsel.Identity.Application.Services.OrderService;
using Fsel.Identity.Application.Services.OrderService.Model;
using Fsel.Identity.Application.Services.TrainingService;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Identity.Infrastructure;
using Fsel.Identity.Infrastructure.ValueSettings;
using Fsel.Shared.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Fsel.Identity.Application.Commands.AuthCmd
{
    public class GenerateTokenCommand : IRequest<MethodResult<TokenModel>>
    {
        public Guid? Id { get; set; }
        public string? UserName { get; set; }
    }

    public class GenerateTokenCommandHandler : IRequestHandler<GenerateTokenCommand, MethodResult<TokenModel>>
    {
        private UserManager<User> _userManager;
        private IInteractionService _interactionService;
        private ITrainingService _trainingService;
        private ILmsCourseService _lmsCourseService;
        private IUserTokenRepository _userTokenRepository;
        private IOrderService _orderService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppSetting _appSetting;
        private readonly IRoleClaimRepository _roleClaimRepository;
        private readonly RoleManager<Role> _roleManager;
        private readonly IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;

        public GenerateTokenCommandHandler(UserManager<User> userManager,
            IInteractionService interactionService,
            ITrainingService trainingService,
            ILmsCourseService lmsCourseService,
            IUserTokenRepository userTokenRepository,
            IOrderService orderService,
            AppSetting appSetting,
            IHttpContextAccessor httpContextAccessor,
            IRoleClaimRepository roleClaimRepository,
            RoleManager<Role> roleManager,
            IMapper mapper,
            IServiceProvider serviceProvider)
        {
            _userManager = userManager;
            _interactionService = interactionService;
            _trainingService = trainingService;
            _lmsCourseService = lmsCourseService;
            _userTokenRepository = userTokenRepository;
            _orderService = orderService;
            _appSetting = appSetting;
            _httpContextAccessor = httpContextAccessor;
            _roleClaimRepository = roleClaimRepository;
            _roleManager = roleManager;
            _mapper = mapper;
            _serviceProvider = serviceProvider;
        }

        public async Task<MethodResult<TokenModel>> Handle(GenerateTokenCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TokenModel> methodResult = new MethodResult<TokenModel>();

            var tenantProvider = _serviceProvider.GetService<ITenantProvider>();
            var tenant = tenantProvider != null ? await tenantProvider.GetTenantAsync(request.UserName) : null;
            if (tenantProvider != null)
            {
                _userManager = await tenantProvider.CreateUserManagerAsync<User>(request.UserName) ?? _userManager;
                _userTokenRepository = await tenantProvider.CreateRepositoryAsync<IUserTokenRepository>(request.UserName) ?? _userTokenRepository;
                _interactionService = await tenantProvider.CreateServiceAsync<IInteractionService>(_appSetting.Services?.InteractionApiUrl, request.UserName) ?? _interactionService;
                _trainingService = await tenantProvider.CreateServiceAsync<ITrainingService>(_appSetting.Services?.TrainingApiUrl, request.UserName) ?? _trainingService;
                _lmsCourseService = await tenantProvider.CreateServiceAsync<ILmsCourseService>(_appSetting.Services?.LmsCourseApiUrl, request.UserName) ?? _lmsCourseService;
                _orderService = await tenantProvider.CreateServiceAsync<IOrderService>(_appSetting.Services?.OrderApiUrl, request.UserName) ?? _orderService;
            }

            var user = await _userManager.Users.Include(x => x.UserSchools).Include(x => x.Student).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (user == null)
            {
                methodResult.StatusCode = StatusCodes.Status401Unauthorized;
                return methodResult;
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var jti = Guid.NewGuid().ToString();
            var authClaims = new List<Claim>
            {
                new Claim(JwtClaimNames.UserName, user.UserName ?? string.Empty),
                new Claim(JwtClaimNames.FullName, user.FullName ?? string.Empty),
                new Claim(JwtClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtClaimNames.UserId, user.Id.ToString()),
                new Claim(JwtClaimNames.Sub, _appSetting.Jwt?.Subject ?? string.Empty),
                new Claim(JwtClaimNames.Jti, jti),
                new Claim(JwtClaimNames.TenantId, tenant?.Id.ToString() ?? string.Empty),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(JwtClaimNames.Role, userRole));
            }

            var role = await _roleManager.FindByNameAsync(userRoles.FirstOrDefault() ?? string.Empty);
            if (role == null)
            {
                methodResult.StatusCode = StatusCodes.Status401Unauthorized;
                return methodResult;
            }

            var roleClaims = await _roleClaimRepository.GetClaimsByRole(role.Id, cancellationToken);
            authClaims.AddRange(_mapper.Map<IList<Claim>>(roleClaims));

            var secretKeyBytes = Encoding.ASCII.GetBytes(_appSetting.Jwt?.SecretKey ?? string.Empty);
            var signin = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _appSetting.Jwt?.Issuer ?? string.Empty,
                _appSetting.Jwt?.Audience ?? string.Empty,
                authClaims,
                expires: DateTime.UtcNow.AddMinutes(_appSetting.Jwt?.TokenValidityInMinutes ?? default),
                signingCredentials: signin
                );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = TokenHelper.GenerateRefreshToken();
            var forwarded = _httpContextAccessor.HttpContext?.Request?.Headers["X-Forwarded-For"];

            await _userTokenRepository.AddAsync(new UserToken
            {
                Name = jti,
                Value = accessToken,
                RefreshToken = refreshToken,
                LoginProvider = JwtBearerDefaults.AuthenticationScheme,
                UserId = user.Id,
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_appSetting.Jwt?.RefreshTokenValidityInDays ?? default),
                IpAddress = forwarded?.ToString()
            });

            var tokenLogin = new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Expiration = token.ValidTo.ConvertTimeFromUtc(TimeZoneInfo.Local),
                FullName = user.FullName,
                Roles = userRoles.ToList(),
                Code = user.Code
            };

            if (userRoles.Contains(EnumRole.Student.ToString()))
            {
                var student = user.Student;
                tokenLogin.IsOrder = false;
                tokenLogin.ClassId = student?.ClassId;
                var classStudent = await _trainingService.GetClassToStudentId(student?.Id ?? default);
                var @class = classStudent?.Content?.Result;
                var isPlacementTest = await _lmsCourseService.IsPlacementTestAsync(student?.Id ?? default);
                var isSurvey = await _interactionService.IsSurveyCompleted(request.Id ?? default);
                tokenLogin.IsPlacementTest = isPlacementTest?.Content?.Result;
                if (@class != null)
                {
                    var order = await _orderService.GetStatusAsync(new GetStatusByUserCommandModel { CourseId = @class.CourseId, UserId = request.Id });

                    tokenLogin.ClassCode = @class.Code;
                    tokenLogin.IsOrder = order?.Content?.Result == EnumOrderStatus.Payment;
                }
                if (isSurvey.IsSuccessStatusCode)
                {
                    tokenLogin.IsSurvey = isSurvey?.Content?.Result;
                }
            }
            if (userRoles.Contains(EnumRole.AdminSchool.ToString()))
            {
                tokenLogin.SchoolId = user.UserSchools.FirstOrDefault()?.SchoolId;
            }

            methodResult.Result = tokenLogin;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
