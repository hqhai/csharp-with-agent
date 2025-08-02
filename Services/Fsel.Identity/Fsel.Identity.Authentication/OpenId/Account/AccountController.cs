// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Security.Claims;
using System.Text.Json;
using System.Transactions;
using Fsel.Common.Constants;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Common.Helpers;
using Fsel.Identity.Application.Handlers.Implementations;
using Fsel.Identity.Application.Handlers.Interfaces;
using Fsel.Identity.Authentication.Auth;
using Fsel.Identity.Authentication.OpenId.Base;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Enums.ErrorCodes;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.CommandModels.OpenId;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.Shared.Helpers;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using static IdentityServer4.IdentityServerConstants;

namespace Fsel.Identity.Authentication.OpenId.Account
{
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
        private readonly ILogger<AccountController> _logger;
        private readonly IPlatformRepository _platformRepository;
        private readonly IUserRegisterHandler _userRegisterHandler;
        private readonly IForgotPasswordHandler _forgotPasswordHandler;
        private readonly IOtpDataCollector _otpDataCollector;
        private readonly IUserRepository _userRepository;
        private readonly Core.Base.AuthContext _languageContext;
        private readonly IStringLocalizer _localizer;

        public AccountController(
            IUserSession userSession,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            Core.Base.Managers.SignInManager<User> signInManager,
            Core.Base.Managers.UserManager<User> userManager,
            ILogger<AccountController> logger,
            IUserRepository userRepository,
            Core.Base.AuthContext languageContext,
            IStringLocalizer localizer,
            IPlatformRepository platformRepository,
            IUserRegisterHandler userRegisterHandler,
            IForgotPasswordHandler forgotPasswordHandler,
            IOtpDataCollector otpDataCollector)
        {
            UserSession = userSession;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _userRepository = userRepository;
            _languageContext = languageContext;
            _localizer = localizer;
            _platformRepository = platformRepository;
            _userRegisterHandler = userRegisterHandler;
            _forgotPasswordHandler = forgotPasswordHandler;
            _otpDataCollector = otpDataCollector;
        }

        /// <summary>
        /// Verify otp for sample user login
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> VerifyOtp(string? identity, string? returnUrl, string? type, string? otpInfo)
        {
            var otpSessionInfo = otpInfo?.DecodeUrlBase64ToObject<OtpSessionInfo>();
            otpSessionInfo ??= await _otpDataCollector.GetOtpSessionInfo(identity, type);
            if (otpSessionInfo != null)
            {
                var verifyOtpModel = new VerifyOtpModel
                {
                    Type = type,
                    Identity = identity,
                    ReturnUrl = returnUrl,
                };

                SetDataForViewByOtpSessionIfo(otpSessionInfo, verifyOtpModel);
                return View(verifyOtpModel);
            }

            return View(new VerifyOtpModel
            {
                Type = type,
                Identity = identity,
                ReturnUrl = returnUrl,
                ExpiredTime = DateTime.UtcNow.Add(OtpSetting.GapSendDuration)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpModel? request, string? button)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!string.IsNullOrEmpty(button))
            {
                var otpSessionInfo = await ResendOtp(request);
                if (otpSessionInfo != null)
                {
                    SetDataForViewByOtpSessionIfo(otpSessionInfo, request);
                }
                else
                {
                    request.ExpiredTime = DateTime.UtcNow.Add(OtpSetting.GapSendDuration);
                }
            }
            else if (ModelState.IsValid)
            {
                if (request.Type == nameof(Register))
                {
                    if (string.IsNullOrEmpty(request.Otp))
                    {
                        ModelState.AddModelError(string.Empty, _localizer["i18n_OTP_cannot_be_empty"]);
                    }
                    var (isSuccessfully, otpSessionInfo) = await _userRegisterHandler.VerifyUserAsync(request.Identity ?? string.Empty, request.Otp);
                    if (isSuccessfully)
                    {
                        await _userRegisterHandler.CreateUserAsync(request.Identity, request.Otp);
                        return await LoginWithoutPassword(request.Identity, request.ReturnUrl);
                    }
                    else
                    {
                        if (otpSessionInfo.OtpExpired)
                        {
                            ViewData["OtpExpired"] = true;
                            ModelState.AddModelError(string.Empty, $"Otp expired");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, $"Invalid otp");
                        }
                        SetDataForViewByOtpSessionIfo(otpSessionInfo, request);
                    }
                }
                else if (request.Type == nameof(Forgot))
                {
                    var identity = request?.Identity;

                    if (!request.Identity.IsValidEmail() && !request.Identity.IsValidPhoneNumber())
                    {
                        ModelState.AddModelError(nameof(request.Identity), _localizer["i18n_error_email_or_phone_number"]);
                        return View(request);
                    }
                    var (isSuccessfully, otpSessionInfo) = await _forgotPasswordHandler.VerifyOtpAsync(request.Identity, request.Otp);
                    if (isSuccessfully)
                    {
                        var token = await _forgotPasswordHandler.GetResetPasswordToken(identity);
                        if (string.IsNullOrEmpty(token))
                        {
                            ModelState.AddModelError(string.Empty, _localizer["i18n_error_generate_token"]);
                        }
                        else
                        {
                            return RedirectToAction(nameof(ResetPassword), new { Identity = identity, Token = token, ReturnUrl = request.ReturnUrl });
                        }
                    }
                    else
                    {
                        if (otpSessionInfo.OtpExpired)
                        {
                            ViewData["OtpExpired"] = true;
                            ModelState.AddModelError(string.Empty, $"Otp expired");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, $"Invalid otp");
                        }
                        SetDataForViewByOtpSessionIfo(otpSessionInfo, request);
                    }
                }
            }

            return View(request);
        }

        private async Task<OtpSessionInfo> ResendOtp(VerifyOtpModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Identity);

            var otpProvider = OtpProviderType.Zalo;

            if (Equals(request.OtpProvider, OtpProviderType.Sms.ToString()))
            {
                otpProvider = OtpProviderType.Sms;
            }
            else if (Equals(request.OtpProvider, OtpProviderType.Email.ToString()))
            {
                otpProvider = OtpProviderType.Email;
            }

            if (request.Type == nameof(Register))
            {
                var userRegister = GetFromTempData(nameof(UserRegisterModel))?.ToString().Deserialize<UserRegisterModel>();
                var (isSuccess, otpSessionInfo) = await _userRegisterHandler.SendRegisterOtpAsync(request.Identity, otpProvider);
                return otpSessionInfo;
            }
            else
            {
                var forgotModel = GetFromTempData(nameof(ForgotModel))?.ToString().Deserialize<ForgotModel>();
                var (isSuccess, otpSessionInfo) = await _forgotPasswordHandler.SendOtpAsync(request.Identity, otpProvider);
                return otpSessionInfo;
            }
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
        public async Task<IActionResult> Forgot([FromForm] ForgotModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (ModelState.IsValid)
            {
                if (!request.Identity.IsValidEmail() && !request.Identity.IsValidPhoneNumber())
                {
                    ModelState.AddModelError(nameof(request.Identity), _localizer["i18n_error_email_or_phone_number"]);
                    return View(request);
                }

                var user = await _userRepository.GetUserByIdentity(request.Identity);

                if (user == null)
                {
                    ModelState.AddModelError(nameof(request.Identity), _localizer["i18n_Email_does_not_exist_in_the_system"]);
                    return View(request);
                }

                var result = await _forgotPasswordHandler.SendOtpAsync(request.Identity, request.Identity.IsValidEmail() ? OtpProviderType.Email : OtpProviderType.Zalo);

                if (!result.Item1)
                {
                    var otpInfo = result.Item2.EncodeObjectToUrlBase64();
                    return RedirectToAction(nameof(VerifyOtp), new { request.ReturnUrl, type = nameof(Forgot), Identity = request.Identity, otpInfo });
                }
                return RedirectToAction(nameof(VerifyOtp), new { request.ReturnUrl, type = nameof(Forgot), Identity = request.Identity });
            }

            return View(request);
        }

        public IActionResult ResetPassword(string token, string identity, string? returnUrl)
        {
            var viewModel = new ForgotPasswordModel
            {
                ReturnUrl = returnUrl,
                Token = token,
                Identity = identity,
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ForgotPasswordModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (ModelState.IsValid)
            {
                if (request == null)
                {
                    ModelState.AddModelError(string.Empty, _localizer["i18n_Data_does_not_exist"]);
                }
                else if (string.IsNullOrEmpty(request.Token))
                {
                    ModelState.AddModelError(string.Empty, _localizer["i18n_Authentication_data_is_incorrect"]);
                }
                else
                {
                    var user = await _userRepository.GetUserByIdentity(request.Identity);
                    if (user == null)
                    {
                        ModelState.AddModelError(string.Empty, _localizer["i18n_User_does_not_exist"]);
                    }
                    else
                    {
                        var resetPassResult = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);
                        if (resetPassResult.Succeeded)
                        {
                            return RedirectToAction(nameof(Login));
                        }
                        foreach (var error in resetPassResult.Errors)
                        {
                            ModelState.TryAddModelError(error.Code, error.Description);
                        }
                    }
                }
            }
            return View(request);
        }

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
                var isRegisterSuccess = await _userRegisterHandler.TempRegisterUserAsync(request);
                if (isRegisterSuccess)
                {
                    var (isSendOtpSuccess, sendOtpMessage) = await _userRegisterHandler.SendRegisterOtpAsync(request.PhoneNumber);
                    return RedirectToAction(nameof(VerifyOtp), new { request.ReturnUrl, type = nameof(Register), Identity = request.PhoneNumber });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Đã tồn tại tài khoản");
                }
            }

            return View(request);
        }

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
        }

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
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user != null && !await ValidateLogin(user))
            {
                return RedirectToAction(nameof(Login), new { returnUrl });
            }
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (signInResult.Succeeded)
            {
                return Redirect(returnUrl);
            }
            else if (signInResult.IsLockedOut)
            {
                return RedirectToAction(nameof(Forgot), new { returnUrl });
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
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

                user = await _userManager.FindByEmailAsync(email ?? string.Empty);
                if (user != null)
                {
                    if (!string.IsNullOrEmpty(user.PhoneNumber))
                    {
                        var result = await _userManager.AddLoginAsync(user, info);
                        if (result.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: true);
                            return Redirect(returnUrl);
                        }
                    }

                    externalLogin.FirstName = firstName;
                    externalLogin.LastName = lastName;
                    externalLogin.DayBirthday = birthday?.Day;
                    externalLogin.MonthBirthday = birthday?.Month;
                    externalLogin.YearBirthday = birthday?.Year;
                }

                TempData[nameof(ExternalLoginModel)] = externalLogin.Serialize();
                return View(nameof(ExternalLoginConfirmation), externalLogin);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginModel request, string? returnUrl = null)
        {
            ArgumentNullException.ThrowIfNull(request);
            var externalLogin = GetFromTempData(nameof(ExternalLoginModel))?.ToString().Deserialize<ExternalLoginModel>();

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null || externalLogin == null)
            {
                return View(request);
            }

            if (!string.IsNullOrEmpty(externalLogin.Email))
            {
                request.Email = externalLogin.Email;
            }

            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var user = await _userManager.FindByEmailAsync(request.Email ?? string.Empty);
            if (user != null)
            {
                var result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (!roles.Contains(EnumRole.Student.ToString()))
                    {
                        user = await _userRepository.GenerateUserDataAsync(user, EnumRoleRegister.Student);
                        result = await _userManager.UpdateAsync(user);
                        result = await _userManager.AddToRoleAsync(user, EnumRoleRegister.Student.ToString());
                    }
                    if (!await ValidateLogin(user))
                    {
                        return View(request);
                    }
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return Redirect(request.ReturnUrl ?? returnUrl ?? string.Empty);
                }
            }
            else
            {
                return await _userRepository.DbContext.Database.CreateExecutionStrategy().ExecuteAsync<IActionResult>(async () =>
                {
                    using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                    user = new User
                    {
                        Email = request.Email,
                        PhoneNumber = request.PhoneNumber,
                        UserName = request.Email,
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        Gender = request.Gender,
                        Birthday = request.Birthday,
                        EmailConfirmed = true,
                    };

                    user = await _userRepository.GenerateUserDataAsync(user, EnumRoleRegister.Student);
                    var result = await _userManager.CreateAsync(user);
                    result = await _userManager.AddToRoleAsync(user, EnumRoleRegister.Student.ToString());

                    if (result.Succeeded)
                    {
                        result = await _userManager.AddLoginAsync(user, info);
                        if (result.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);

                            scope.Complete();
                            return Redirect(request.ReturnUrl ?? returnUrl ?? string.Empty);
                        }
                    }
                    scope.Dispose();

                    foreach (var error in result.Errors)
                    {
                        ModelState.TryAddModelError(error.Code, error.Description);
                    }
                    return View(request);
                });
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


        private void SetDataForViewByOtpSessionIfo(OtpSessionInfo otpSessionInfo, VerifyOtpModel verifyOtpModel)
        {
            if (otpSessionInfo != null)
            {
                if (otpSessionInfo.IsOtpBlocked)
                {
                    ViewData["IsOtpBlocked"] = true;
                    verifyOtpModel.ExpiredTime = DateTime.UtcNow.Add(otpSessionInfo.WaitTimeDuration.Value);
                    return;
                }
                else if (otpSessionInfo.CanSendDirectly)
                {
                    verifyOtpModel.ExpiredTime = default(DateTime);
                    return;
                }
                else
                {
                    verifyOtpModel.ExpiredTime = DateTime.UtcNow.Add(otpSessionInfo.SendInfo.WaitTimeDuration.Value);
                }
            }
        }
    }
}
