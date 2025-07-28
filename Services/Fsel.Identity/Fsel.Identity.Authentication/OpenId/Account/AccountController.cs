// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Globalization;
using System.Security.Claims;
using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Caching;
using Fsel.Common.Constants;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Common.Helpers;
using Fsel.Identity.Application.Commands.SenderCmd;
using Fsel.Identity.Application.Commands.UserOtpCodeCmd;
using Fsel.Identity.Application.Handlers.Interfaces;
using Fsel.Identity.Authentication.Auth;
using Fsel.Identity.Authentication.OpenId.Base;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.CommandModels.OpenId;
using Fsel.Identity.Domain.Models.EntityModels;
using Fsel.Identity.Infrastructure.ValueSettings;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using static IdentityServer4.IdentityServerConstants;

namespace Fsel.Identity.Authentication.OpenId.Account
{
    /// <summary>
    /// This sample controller implements a typical login/logout/provision workflow for local and external accounts.
    /// The login service encapsulates the interactions with the user data store. This data store is in-memory only and cannot be used for production!
    /// The interaction service provides a way for the UI to communicate with identityserver for validation and context retrieval
    /// </summary>
    [AllowAnonymous]
    public class AccountController : BaseController
    {
        protected IUserSession UserSession { get; private set; }

        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
        private readonly Core.Base.Managers.SignInManager<User> _signInManager;
        private readonly Core.Base.Managers.UserManager<User> _userManager;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly AppSetting _appSetting;
        private readonly ILogger<AccountController> _logger;
        private readonly IParentRepository _parentRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IUserOtpCodeRepository _userOtpRepository;
        private readonly IPlatformRepository _platformRepository;
        private readonly IUserRegisterHandler _userRegisterHandler;
        private readonly IUserRepository _userRepository;
        private readonly Core.Base.AuthContext _languageContext;
        private readonly IStringLocalizer _localizer;
        private readonly ICacheService<UserOtpCodeModel> _userOtpCache;

        public AccountController(
            IUserSession userSession,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            Core.Base.Managers.SignInManager<User> signInManager,
            Core.Base.Managers.UserManager<User> userManager,
            IMediator mediator,
            IMapper mapper,
            AppSetting appSetting,
            ILogger<AccountController> logger,
            IParentRepository parentRepository,
            IStudentRepository studentRepository,
            IUserOtpCodeRepository userOtpRepository,
            IUserRepository userRepository,
            Core.Base.AuthContext languageContext,
            IStringLocalizer localizer,
            ICacheService<UserOtpCodeModel> userOtpCache,
            IPlatformRepository platformRepository,
            IUserRegisterHandler userRegisterHandler)
        {
            // if the TestUserStore is not in DI, then we'll just use the global users collection
            // this is where you would plug in your own custom identity management library (e.g. ASP.NET Identity)

            UserSession = userSession;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
            _signInManager = signInManager;
            _userManager = userManager;
            _mediator = mediator;
            _mapper = mapper;
            _appSetting = appSetting;
            _logger = logger;
            _parentRepository = parentRepository;
            _studentRepository = studentRepository;
            _userOtpRepository = userOtpRepository;
            _userRepository = userRepository;
            _languageContext = languageContext;
            _localizer = localizer;
            _userOtpCache = userOtpCache;
            _platformRepository = platformRepository;
            _userRegisterHandler = userRegisterHandler;
        }

        /// <summary>
        /// Verify otp for sample user login
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> VerifyOtp(string? phoneNumber, string? returnUrl, string? type)
        {
            TempData[nameof(VerifyOtp)] = type;

            if (type == nameof(Register))
            {
                return View(new VerifyOtpModel
                {
                    Type = type,
                    PhoneNumber = phoneNumber,
                    ReturnUrl = returnUrl,
                    ExpiredTime = DateTime.UtcNow.Add(OtpSetting.MinimumBetweenTwoSendsDuration)
                });
            }
            else
            {
                var forgotModel = GetFromTempData(nameof(ForgotModel))?.ToString().Deserialize<ForgotModel>();

                var email = forgotModel?.Email;
                var user = await _userManager.FindByEmailAsync(email ?? string.Empty);

                var entry = await _userOtpCache.GetAsync($"{nameof(SendOtpAsync)}.{user?.Id}");

                return View(new VerifyOtpModel
                {
                    Type = type,
                    ReturnUrl = returnUrl,
                    ExpiredTime = entry?.ExpiredTime
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpModel? request, string? button)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!string.IsNullOrEmpty(button))
            {
                await ResendOtp(request);
            }
            else if (ModelState.IsValid)
            {
                if (request.Type == nameof(Register))
                {
                    if (string.IsNullOrEmpty(request.Otp))
                    {
                        ModelState.AddModelError(string.Empty, _localizer["i18n_OTP_cannot_be_empty"]);
                    }
                    var (isSuccessfully, message) = await _userRegisterHandler.VerifyUserAsync(request.PhoneNumber ?? string.Empty, request.Otp);
                    if (isSuccessfully)
                    {
                        await _userRegisterHandler.CreateUserAsync(request.PhoneNumber, request.Otp);
                        return await LoginWithoutPassword(request.PhoneNumber, request.ReturnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError(message.Value.Key, message.Value.Value);
                    }
                }
                else if (request.Type == nameof(Forgot))
                {
                    var forgotModel = GetFromTempData(nameof(ForgotModel))?.ToString().Deserialize<ForgotModel>();

                    var email = forgotModel?.Email;
                    var user = await _userManager.FindByEmailAsync(email ?? string.Empty);

                    var entry = await _userOtpCache.GetAsync($"{nameof(SendOtpAsync)}.{user?.Id}");
                    request.ExpiredTime = entry?.ExpiredTime;

                    if (forgotModel == null)
                    {
                        ModelState.AddModelError(string.Empty, _localizer["i18n_Data_does_not_exist"]);
                    }
                    else if (user == null || (!user.EmailConfirmed && (user.Student == null || user.Teacher == null || user.CSO == null || user.Parent == null)))
                    {
                        ModelState.AddModelError(string.Empty, _localizer["i18n_User_does_not_exist"]);
                    }
                    else if (string.IsNullOrEmpty(request.Otp))
                    {
                        ModelState.AddModelError(string.Empty, _localizer["i18n_OTP_cannot_be_empty"]);
                    }
                    else
                    {
                        var verify = await VerifyOtpAsync(user, request.Otp);
                        if (verify.Result)
                        {
                            TempData[nameof(ForgotPasswordModel)] = new ForgotPasswordModel
                            {
                                VerifyId = Guid.NewGuid(),
                                Email = email,
                                ReturnUrl = request.ReturnUrl,
                            }.Serialize();
                            return RedirectToAction(nameof(ForgotPassword), new { request.ReturnUrl });
                        }
                        else if (verify.ErrorMessages.Any(x => x.ErrorCode == nameof(EnumUserOtpCodeErrorCode.OtpInvalid)))
                        {
                            ModelState.AddModelError(nameof(request.Otp), _localizer["i18n_OTP_is_not_valid"]);
                        }
                        else if (verify.ErrorMessages.Any(x => x.ErrorCode == nameof(EnumUserOtpCodeErrorCode.OtpExpired)))
                        {
                            ModelState.AddModelError(nameof(request.Otp), _localizer["i18n_OTP_has_expired"]);
                        }
                    }
                }
            }

            return View(request);
        }

        /// <summary>
        /// ResendOtp
        /// </summary>
        /// <returns></returns>
        private async Task ResendOtp(VerifyOtpModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);
            User? user;

            if (request.Type == nameof(Register))
            {
                var userRegister = GetFromTempData(nameof(UserRegisterModel))?.ToString().Deserialize<UserRegisterModel>();
                var (isSuccess, message) = await _userRegisterHandler.SendRegisterOtpAsync(request.PhoneNumber, OtpProviderType.Zalo);
                if (!isSuccess)
                {
                    ModelState.AddModelError(message.Value.Key, message.Value.Value);
                }
            }
            else
            {
                var forgotModel = GetFromTempData(nameof(ForgotModel))?.ToString().Deserialize<ForgotModel>();
                user = await _userManager.FindByEmailAsync(forgotModel?.Email ?? string.Empty);
                if (user == null || !user.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, _localizer["i18n_User_does_not_exist"]);
                }
            }

            //if (user != null)
            //{
            //    var sendResult = await SendOtpAsync(user);
            //    if (!sendResult.IsOK)
            //    {
            //        ModelState.AddModelError(string.Empty, _localizer[sendResult.ErrorMessages.Select(x => x.ErrorCode).FirstOrDefault() ?? string.Empty]);
            //    }

            //    var entry = await _userOtpCache.GetAsync($"{nameof(SendOtpAsync)}.{user.Id}");
            //    request.ExpiredTime = entry?.ExpiredTime;
            //}
        }

        public IActionResult Success(string? returnUrl, string? message = null)
        {
            ViewBag.Message = message;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        public IActionResult ForgotPassword(string? returnUrl)
        {
            var vm = GetFromTempData(nameof(ForgotPasswordModel))?.ToString().Deserialize<ForgotPasswordModel>();
            vm ??= new ForgotPasswordModel();
            vm.ReturnUrl = returnUrl;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (ModelState.IsValid)
            {
                var forgotPasswordModel = GetFromTempData(nameof(ForgotPasswordModel))?.ToString().Deserialize<ForgotPasswordModel>();
                if (forgotPasswordModel == null)
                {
                    ModelState.AddModelError(string.Empty, _localizer["i18n_Data_does_not_exist"]);
                }
                else if (request.VerifyId != forgotPasswordModel.VerifyId)
                {
                    ModelState.AddModelError(string.Empty, _localizer["i18n_Authentication_data_is_incorrect"]);
                }
                else
                {
                    var user = await _userManager.FindByEmailAsync(forgotPasswordModel.Email ?? string.Empty);
                    if (user == null)
                    {
                        ModelState.AddModelError(string.Empty, _localizer["i18n_User_does_not_exist"]);
                    }
                    else
                    {
                        user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, request.Password ?? string.Empty);
                        var result = await _userManager.UpdateAsync(user);

                        if (result.Succeeded)
                        {
                            return await LoginWithoutPassword(user, request.ReturnUrl);
                        }

                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
            }
            return View(request);
        }

        public IActionResult Forgot(string? returnUrl)
        {
            var vm = new ForgotModel
            {
                ReturnUrl = returnUrl
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Forgot(ForgotModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);

            TempData[nameof(ForgotModel)] = request.Serialize();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(request.Email ?? string.Empty);
                if (user == null || (!user.EmailConfirmed && (user.Student == null || user.Teacher == null || user.CSO == null || user.Parent == null)))
                {
                    ModelState.AddModelError(nameof(request.Email), _localizer["i18n_Email_does_not_exist_in_the_system"]);
                    return View(request);
                }

                await _userOtpCache.RemoveAsync($"{nameof(SendOtpAsync)}.{user.Id}");
                var sendResult = await SendOtpAsync(user);
                if (!sendResult.IsOK)
                {
                    ModelState.AddModelError(string.Empty, _localizer[sendResult.ErrorMessages.Select(x => x.ErrorCode).FirstOrDefault() ?? string.Empty]);
                    return View(request);
                }

                return RedirectToAction(nameof(VerifyOtp), new { request.ReturnUrl, type = nameof(Forgot) });
            }

            return View(request);
        }

        public IActionResult SetPassWord(string? returnUrl)
        {
            var vm = new ForgotModel
            {
                ReturnUrl = returnUrl
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassWord(ForgotModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);

            TempData[nameof(ForgotModel)] = request.Serialize();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(request.Email ?? string.Empty);
                if (user == null || (!user.EmailConfirmed && (user.Student == null || user.Teacher == null || user.CSO == null || user.Parent == null)))
                {
                    ModelState.AddModelError(nameof(request.Email), _localizer["i18n_Email_does_not_exist_in_the_system"]);
                    return View(request);
                }

                await _userOtpCache.RemoveAsync($"{nameof(SendOtpAsync)}.{user.Id}");
                var sendResult = await SendOtpAsync(user);
                if (!sendResult.IsOK)
                {
                    ModelState.AddModelError(string.Empty, _localizer[sendResult.ErrorMessages.Select(x => x.ErrorCode).FirstOrDefault() ?? string.Empty]);
                    return View(request);
                }

                return RedirectToAction(nameof(VerifyOtp), new { request.ReturnUrl, type = nameof(Forgot) });
            }

            return View(request);
        }

        /// <summary>
        /// Registration for sample user login
        /// </summary>
        /// <returns></returns>
        //[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None, Duration = 0)]
        public IActionResult Register(string? returnUrl)
        {
            TempData[nameof(VerifyOtp)] = string.Empty;
            var vm = new UserRegisterModel
            {
                ReturnUrl = returnUrl
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserRegisterModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var type = GetFromTempData(nameof(VerifyOtp))?.ToString();
            if (!string.IsNullOrEmpty(type))
            {
                return View(new UserRegisterModel
                {
                    ReturnUrl = request.ReturnUrl,
                    IsShowVerifyOtp = true
                });
            }

            TempData[nameof(UserRegisterModel)] = request.Serialize();
            if (ModelState.IsValid)
            {
                var (isRegisterSuccess, message) = await _userRegisterHandler.TempRegisterUserAsync(request);
                if (isRegisterSuccess)
                {
                    var (isSendOtpSuccess, sendOtpMessage) = await _userRegisterHandler.SendRegisterOtpAsync(request.PhoneNumber);
                    if (!isSendOtpSuccess)
                    {
                        ModelState.AddModelError(sendOtpMessage.Value.Key, sendOtpMessage.Value.Value);
                    }
                    return RedirectToAction(nameof(VerifyOtp), new { request.ReturnUrl, type = nameof(Register), PhoneNumber = request.PhoneNumber });
                }
                else
                {
                    ModelState.AddModelError(message.Value.Key, message.Value.Value);
                }
            }

            return View(request);
        }

        private async Task<MethodResult<bool>> SendOtpAsync(User user)
        {
            var methodResult = new MethodResult<bool>();

            var keyCache = $"{nameof(SendOtpAsync)}.{user.Id}";
            var entry = await _userOtpCache.GetAsync(keyCache);
            if (entry != null)
            {
                methodResult.AddErrorBadRequest("i18n_OTP_waiting_sent_again");
                return methodResult;
            }

            var otpResult = await CreateAndSendMailOtpAsync(user).ConfigureAwait(false);
            if (!otpResult.IsOK)
            {
                return methodResult;
            }

            var timeCache = otpResult?.Result?.ExpiredTime - DateTime.UtcNow;
            if (otpResult?.Result != null && timeCache.HasValue)
            {
                await _userOtpCache.SetAsync(keyCache, otpResult.Result, timeCache.Value);
            }

            return methodResult;
        }

        private async Task<MethodResult<UserOtpCodeModel>> CreateAndSendMailOtpAsync(User user)
        {
            var methodResult = new MethodResult<UserOtpCodeModel>();

            //var otp = await _userManager.GenerateUserTokenAsync(user, DataProtectionTokenProvider.TotpProviderName, DataProtectionTokenProvider.TotpProviderName);
            var otpResult = await _mediator.Send(new CreateUserOtpCommand { UserId = user.Id }).ConfigureAwait(false);
            if (!otpResult.IsOK)
            {
                methodResult.AddErrorBadRequest("i18n_Failed_to_send_OTP");
                return methodResult;
            }

            // Send OTP via email
            var sendResult = await SendMailOtpAsync(user, otpResult?.Result?.OtpCode);
            if (!sendResult.IsOK)
            {
                methodResult.AddErrorBadRequest("i18n_Failed_to_send_OTP");
                return methodResult;
            }

            methodResult.Result = otpResult?.Result;
            return methodResult;
        }

        private async Task<MethodResult<bool>> SendMailOtpAsync(User user, string? otp)
        {
            var param = new
            {
                OtpCode = otp,
                OtpValidTime = string.Format(CultureInfo.InvariantCulture, SenderSettings.OtpValidMinute, _appSetting!.Otp!.StepTime)
            };
            var subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendOtpSubjectFullName, user.FullName);
            var sendResult = await _mediator.Send(new SendOtpCommand { Email = user.Email, Subject = subject, Params = param, Template = EnumSenderTemplate.SendOtp }).ConfigureAwait(false);

            return sendResult;
        }

        private async Task<MethodResult<bool>> VerifyOtpAsync(User user, string? otp)
        {
            //var verify = await _userManager.VerifyUserTokenAsync(user, DataProtectionTokenProvider.TotpProviderName, DataProtectionTokenProvider.TotpProviderName, otp ?? string.Empty);

            return await _mediator.Send(new ConfirmUserOtpCommand { UserId = user.Id, Otp = otp }).ConfigureAwait(false);
        }

        /// <summary>
        /// Impersonation
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Impersonation(string? returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            var clientSecret = context?.Parameters[RequestHeaderSetting.ImpersonationClientSecret]?.ToString();
            if (context != null && !string.IsNullOrEmpty(clientSecret) && context.Client.ClientSecrets.Any(x => x.Value == clientSecret.ToSha256()))
            {
                var userId = context.Parameters[RequestHeaderSetting.UserId]?.ToString();
                var user = await _userManager.FindByIdAsync(userId ?? string.Empty);
                return await LoginWithoutPassword(user, returnUrl);
            }
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        /// <summary>
        /// Entry point into the login workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl ?? string.Empty);
            if (!string.IsNullOrEmpty(vm.UiLocales))
            {
                _languageContext.AcceptLanguage = vm.UiLocales;
            }
            HttpContext.SetCookie(Settings.RequestHeader.AcceptLanguage, vm.UiLocales);
            HttpContext.SetCookie(Settings.RequestHeader.OSName, vm.OSName);
            HttpContext.SetCookie(Settings.RequestHeader.DeviceId, vm.DeviceId);
            HttpContext.SetCookie(Settings.RequestHeader.DeviceName, vm.DeviceName);

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return RedirectToAction("Challenge", "External", new { scheme = vm.ExternalLoginScheme, returnUrl });
            }

            return View(vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Login(LoginInputModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var vm = await BuildLoginViewModelAsync(model);
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            if (ModelState.IsValid)
            {
                var user = await _signInManager.UserManager.FindByNameAsync(model.Username ?? string.Empty);
                if (user is not null && await ValidateLogin(user))
                {
                    var userLogin = await _signInManager.PasswordSignInAsync(user, model.Password ?? string.Empty, model.RememberLogin, true);
                    if (userLogin.Succeeded)
                    {
                        await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id.ToString(), user.UserName, clientId: context?.Client.ClientId));

                        // only set explicit expiration here if user chooses "remember me".
                        // otherwise we rely upon expiration configured in cookie middleware.
                        AuthenticationProperties? props = null;
                        if (AccountOptions.AllowRememberLogin && model.RememberLogin)
                        {
                            props = new AuthenticationProperties
                            {
                                // Check cookies có lưu lại thông tin sau khi đăng nhập
                                IsPersistent = true,
                                ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
                            };
                        }
                        ;

                        // issue authentication cookie with subject ID and username
                        var isuser = new IdentityServerUser(user.Id.ToString())
                        {
                            DisplayName = user.UserName
                        };

                        await HttpContext.SignInAsync(isuser, props).ConfigureAwait(false);

                        if (context != null)
                        {
                            if (context.IsNativeClient())
                            {
                                Thread.Sleep(1300);

                                // The client is native, so this change in how to
                                // return the response is for better UX for the end user.
                                //return this.LoadingPage("Redirect", model.ReturnUrl ?? string.Empty);
                            }

                            // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                            return Redirect(model.ReturnUrl ?? string.Empty);
                        }

                        // request for a local page
                        if (Url.IsLocalUrl(model.ReturnUrl))
                        {
                            return Redirect(model.ReturnUrl);
                        }
                        else if (string.IsNullOrEmpty(model.ReturnUrl))
                        {
                            return Redirect("~/");
                        }
                        else
                        {
                            _logger.LogWarning("Invalid return URL");
                            // user might have clicked on a malicious link - should be logged
                        }
                    }
                    else if (userLogin.IsLockedOut)
                    {
                        ModelState.AddModelError(string.Empty, _localizer["i18n_account_locked"]);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, _localizer["ERROR_CODE.UserNameAndPasswordIncorrect"]);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _localizer["ERROR_CODE.UserNameAndPasswordIncorrect"]);
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, _localizer["i18n_Invalid_Credentials"], clientId: context?.Client.ClientId));
            }

            return View(vm);
        }

        private async Task<IActionResult> LoginWithoutPassword(string phoneNumber, string? returnUrl)
        {
            var user = await _userRepository.DbContext.Set<User>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);
            return await LoginWithoutPassword(user, returnUrl);
        }

        private async Task<IActionResult> LoginWithoutPassword(User? user, string? returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (user is not null && await ValidateLogin(user))
            {
                var props = new AuthenticationProperties
                {
                    // Check cookies có lưu lại thông tin sau khi đăng nhập
                    IsPersistent = false,
                };
                await _signInManager.SignInAsync(user, props);

                await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id.ToString(), user.UserName, clientId: context?.Client.ClientId));

                // issue authentication cookie with subject ID and username
                var isuser = new IdentityServerUser(user.Id.ToString())
                {
                    DisplayName = user.UserName
                };

                await HttpContext.SignInAsync(isuser, props).ConfigureAwait(false);

                if (context != null && context.IsNativeClient())
                {
                    Thread.Sleep(1300);

                    //return this.LoadingPage("Redirect", returnUrl ?? string.Empty);
                }

                // request for a local page
                if (Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
            }

            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await BuildLogoutViewModelAsync(logoutId);
            return await Logout(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId ?? string.Empty);

            if (User?.Identity?.IsAuthenticated == true)
            {
                await UserSession.RemoveSessionIdCookieAsync();

                await _signInManager.SignOutAsync(); //signout Identity

                // delete local authentication cookie
                await HttpContext.SignOutAsync();

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                var url = Url.Action(nameof(Logout), new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return Redirect(vm.PostLogoutRedirectUri);
            //return View("LoggedOut", vm);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginConfirmation), new { returnUrl });

            AuthenticationProperties properties;
            if (provider == LoginProvider.Zalo)
            {
                properties = new AuthenticationProperties
                {
                    RedirectUri = redirectUrl,
                };
                return Challenge(properties, LoginProvider.Zalo);
            }

            properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginConfirmation(string? returnUrl = null)
        {
            returnUrl ??= string.Empty;

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                try
                {
                    var newUser = new User
                    {
                        Id = Guid.NewGuid(),
                        UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                        EmailConfirmed = true,
                        FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName),
                        Birthday = info.Principal.FindFirstValue(ClaimTypes.DateOfBirth)?.ConvertDateTimeFormat("MM/dd/yyyy"),
                        LastName = info.Principal.FindFirstValue(ClaimTypes.Surname),
                        SecurityStamp = Guid.NewGuid().ToString()
                    };

                    _userRepository.DbContext.Add(newUser);
                    await _userRepository.DbContext.SaveChangesAsync();
                    var addLoginResult = await _userManager.AddLoginAsync(newUser, info);
                    if (!addLoginResult.Succeeded)
                    {
                        return RedirectToAction(nameof(Login), new { returnUrl });
                    }
                }
                catch (Exception ex)
                {
                    return RedirectToAction(nameof(Login), new { returnUrl });
                }
            }
            else
            {
                var haveFilledInfo = HaveFilledRequiredInfo(user);
                if (haveFilledInfo)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return Redirect(returnUrl ?? "~/");
                }
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (signInResult.Succeeded)
            {
                var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);
                var birthday = info.Principal.FindFirstValue(ClaimTypes.DateOfBirth)?.ConvertDateTimeFormat("MM/dd/yyyy");
                var gender = info.Principal.FindFirstValue(ClaimTypes.Gender);
                var phoneNumber = info.Principal.FindFirstValue(ClaimTypes.MobilePhone);
                var externalLogin = new ExternalLoginModel
                {
                    Email = email,
                    IsEmailReadonly = !string.IsNullOrEmpty(email),
                    PhoneNumber = phoneNumber,
                    FirstName = firstName,
                    LastName = lastName,
                    DayBirthday = birthday?.Day,
                    MonthBirthday = birthday?.Month,
                    YearBirthday = birthday?.Year,
                    Provider = info.LoginProvider,
                    ReturnUrl = returnUrl
                };

                TempData[nameof(ExternalLoginModel)] = externalLogin.Serialize();
                return View(nameof(ExternalLoginConfirmation), externalLogin);
            }
            else
            {
                ModelState.AddModelError(string.Empty, _localizer["i18n_External_login_failed"]);
            }
            return Redirect(returnUrl ?? "~/");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginModel request, string? returnUrl = null)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var isExistedPhoneNumber = await _userRepository.DbContext.Set<User>().AnyAsync(u => u.PhoneNumber == request.PhoneNumber);
            if (isExistedPhoneNumber)
            {
                ModelState.AddModelError(string.Empty, _localizer[nameof(EnumAuthUserErrorCode.DuplicatePhoneNumber)]);
                return View(request);
            }

            var user = await _userManager.FindByEmailAsync(request.Email ?? string.Empty);

            if (user != null)
            {
                user.Birthday = request.Birthday;
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.PhoneNumber = request.PhoneNumber;
                user.Gender = request.Gender;
                await _userManager.UpdateAsync(user);
                await _userManager.AddToRoleAsync(user, EnumRoleRegister.Student.ToString());
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Redirect(request.ReturnUrl ?? returnUrl ?? "/");
            }
            return View(request);
        }

        private async Task<bool> ValidateLogin(User? user)
        {
            if (user != null)
            {
                var platformCodes = await _platformRepository.Queryable
                    .Include(x => x.UserPlatforms)
                    .Where(x => x.UserPlatforms.Any(n => n.UserId == user.Id))
                    .Select(x => x.Code)
                    .ToListAsync();

                if (platformCodes != null && platformCodes.Count > 0 && !platformCodes.Contains(EnumPlatformCode.LMS))
                {
                    ModelState.AddModelError(string.Empty, _localizer[nameof(EnumAuthUserErrorCode.UserIsNotOnAnyPlatform)]);
                }
                else if (user.Status == EnumUserStatus.Inactive)
                {
                    ModelState.AddModelError(string.Empty, _localizer[nameof(EnumAuthUserErrorCode.AccountHasBeenLocked)]);
                }
                else if (user.Status == EnumUserStatus.Disable)
                {
                    ModelState.AddModelError(string.Empty, _localizer[nameof(EnumAuthUserErrorCode.AccountHasBeenCutOff)]);
                }
                else
                {
                    return true;
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, _localizer[nameof(EnumSystemErrorCode.DataNotExist)]);
            }

            return false;
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Error()
        {
            return View();
        }

        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/

        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

            var vm = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                UiLocales = context?.UiLocales,
                OSName = context?.Parameters[Settings.RequestHeader.OSName],
                DeviceId = context?.Parameters[Settings.RequestHeader.DeviceId],
                DeviceName = context?.Parameters[Settings.RequestHeader.DeviceName],
                ReferralCode = context?.Parameters[RequestHeaderSetting.ReferralCode],
                IsRegister = bool.TryParse(context?.Parameters[RequestHeaderSetting.IsRegister], out var isRegister) && isRegister,
            };

            if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                vm.EnableLocalLogin = local;

                if (!local)
                {
                    vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context!.IdP } };
                }

                return vm;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null)
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName ?? x.Name,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.Client.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            vm.AllowRememberLogin = AccountOptions.AllowRememberLogin;
            vm.EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin;
            vm.ExternalProviders = providers.ToArray();
            return vm;
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl ?? string.Empty);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity?.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri ?? string.Empty,
                ClientName = (string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName) ?? string.Empty,
                SignOutIframeUrl = logout?.SignOutIFrameUrl ?? string.Empty,
                LogoutId = logoutId
            };

            if (User?.Identity?.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }

        private bool HaveFilledRequiredInfo(User user)
        {
            if (user.PhoneNumber == null
                || user.Birthday == null
                || user.FirstName == null
                || user.LastName == null
                || user.Gender == null)
            {
                return false;
            }

            return true;
        }
    }
}
