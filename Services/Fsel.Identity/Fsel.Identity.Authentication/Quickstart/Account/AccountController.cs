// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using AutoMapper;
using Fsel.Identity.Application.Commands.SenderCmd;
using Fsel.Identity.Application.Commands.UserOtpCmd;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Infrastructure.ValueSettings;
using Fsel.Identity.Authentication.Quickstart.Base;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

//using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using System.Transactions;
using Fsel.Identity.Domain.Models.CommandModels.Quickstarts;
using Fsel.Shared.Constants;
using Fsel.Shared.Enums;
using Fsel.Identity.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;
using Fsel.Core.Base.Managers;
using Microsoft.Net.Http.Headers;
using Fsel.Common.Constants;
using PhoneNumbers;

namespace Fsel.Identity.Authentication.Quickstart.Account
{
    /// <summary>
    /// This sample controller implements a typical login/logout/provision workflow for local and external accounts.
    /// The login service encapsulates the interactions with the user data store. This data store is in-memory only and cannot be used for production!
    /// The interaction service provides a way for the UI to communicate with identityserver for validation and context retrieval
    /// </summary>
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : BaseController
    {
        //private readonly TestUserStore _users;
        protected IUserSession UserSession { get; private set; }

        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly AppSetting _appSetting;
        private readonly ILogger<AccountController> _logger;
        private readonly IParentRepository _parentRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IUserOtpRepository _userOtpRepository;
        private readonly IUserRepository _userRepository;
        private readonly Core.Base.AuthContext _languageContext;

        public AccountController(
            IUserSession userSession,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IMediator mediator,
            IMapper mapper,
            AppSetting appSetting,
            ILogger<AccountController> logger,
            IParentRepository parentRepository,
            IStudentRepository studentRepository,
            IUserOtpRepository userOtpRepository,
            IUserRepository userRepository,
            Core.Base.AuthContext languageContext)
        {
            // if the TestUserStore is not in DI, then we'll just use the global users collection
            // this is where you would plug in your own custom identity management library (e.g. ASP.NET Identity)
            //_users = users ?? new TestUserStore(TestUsers.Users);

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
        }

        /// <summary>
        /// Verify otp for sample user login
        /// </summary>
        /// <returns></returns>
        public IActionResult VerifyOtp(string? returnUrl, string? type)
        {
            return View(new VerifyOtpModel
            {
                Type = type,
                ReturnUrl = returnUrl,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpModel? request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (ModelState.IsValid)
            {
                //var type = GetFromTempData(nameof(VerifyOtp))?.ToString();
                var userRegisterModel = GetFromTempData(nameof(UserRegisterModel))?.ToString().Deserialize<UserRegisterModel>();
                var forgotModel = GetFromTempData(nameof(ForgotModel))?.ToString().Deserialize<ForgotModel>();

                var email = userRegisterModel?.Email ?? forgotModel?.Email;

                var user = await _userManager.FindByEmailAsync(email ?? string.Empty);
                if (request.Type == nameof(Register))
                {
                    if (userRegisterModel == null)
                    {
                        ModelState.AddModelError(string.Empty, "Data does not exist");
                    }
                    else if (string.IsNullOrEmpty(userRegisterModel.Password))
                    {
                        ModelState.AddModelError(nameof(userRegisterModel.Password), "Password cannot be empty");
                    }
                    else if (user == null)
                    {
                        ModelState.AddModelError(string.Empty, "User does not exist");
                    }
                    else if (user.EmailConfirmed)
                    {
                        ModelState.AddModelError(string.Empty, "User has been confirmed");
                    }
                    else
                    {
                        var verify = await VerifyOtpAsync(user, request.Otp);
                        if (verify)
                        {
                            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                            var result = await _userManager.ConfirmEmailAsync(user, token);

                            _mapper.Map(userRegisterModel, user);
                            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, userRegisterModel.Password);
                            user = await _userRepository.GenerateUserDataAsync(user, EnumRoleRegister.Student);
                            result = await _userManager.UpdateAsync(user);

                            if (result.Succeeded)
                            {
                                ViewBag.Success = "User successfuly added!!";

                                return RedirectToAction(nameof(Success), new
                                {
                                    request.ReturnUrl,
                                    message = "Congratulations, your account has been successfully created."
                                });
                            }

                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(nameof(request.Otp), "OTP Invalid");
                        }
                    }
                }
                else if (request.Type == nameof(Forgot))
                {
                    if (forgotModel == null)
                    {
                        ModelState.AddModelError(string.Empty, "Data does not exist");
                    }
                    else if (user == null || !user.EmailConfirmed)
                    {
                        ModelState.AddModelError(string.Empty, "User does not exist");
                    }
                    else
                    {
                        var verify = await VerifyOtpAsync(user, request.Otp);
                        if (verify)
                        {
                            TempData[nameof(ForgotPasswordModel)] = new ForgotPasswordModel
                            {
                                VerifyId = Guid.NewGuid(),
                                Email = email,
                                ReturnUrl = request.ReturnUrl,
                            }.Serialize();
                            return RedirectToAction(nameof(ForgotPassword), new { request.ReturnUrl });
                        }
                        else
                        {
                            ModelState.AddModelError(nameof(request.Otp), "OTP Invalid");
                        }
                    }
                }
            }

            return View(request);
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
            ViewBag.ReturnUrl = returnUrl;
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
                    ModelState.AddModelError(string.Empty, "Data does not exist");
                }
                else if (request.VerifyId != forgotPasswordModel.VerifyId)
                {
                    ModelState.AddModelError(string.Empty, "Authentication data is incorrect");
                }
                else
                {
                    var user = await _userManager.FindByEmailAsync(forgotPasswordModel.Email ?? string.Empty);
                    if (user == null)
                    {
                        ModelState.AddModelError(string.Empty, "User does not exist");
                    }
                    else
                    {
                        user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, request.Password ?? string.Empty);
                        var result = await _userManager.UpdateAsync(user);

                        if (result.Succeeded)
                        {
                            return RedirectToAction(nameof(Success), new
                            {
                                request.ReturnUrl,
                                message = "Congratulations, your account password has been successfully changed."
                            });
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
                if (user == null || !user.EmailConfirmed)
                {
                    ModelState.AddModelError(nameof(request.Email), "Email does not exist in the system");
                    return View(request);
                }

                var sendResult = await SendOtpAsync(user);
                if (!sendResult.IsOK)
                {
                    ModelState.AddModelError(nameof(request.Email), "Failed to send OTP");
                    return View(request);
                }

                return RedirectToAction(nameof(VerifyOtp), new { request.ReturnUrl, type = nameof(Forgot) });
            }

            return View(request);
        }

        /// <summary>
        /// ResendOtp
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ResendOtp(string? returnUrl, string? type)
        {
            User? user;

            if (type == nameof(Register))
            {
                var userRegister = GetFromTempData(nameof(UserRegisterModel))?.ToString().Deserialize<UserRegisterModel>();
                user = await _userManager.FindByEmailAsync(userRegister?.Email ?? string.Empty);
                if (user == null)
                {
                    return RedirectToAction(nameof(VerifyOtp), new { returnUrl, type });
                }
            }
            else
            {
                var forgotModel = GetFromTempData(nameof(ForgotModel))?.ToString().Deserialize<ForgotModel>();
                user = await _userManager.FindByEmailAsync(forgotModel?.Email ?? string.Empty);
                if (user == null || !user.EmailConfirmed)
                {
                    return RedirectToAction(nameof(VerifyOtp), new { returnUrl, type });
                }
            }
            var sendResult = await SendOtpAsync(user);
            if (!sendResult.IsOK)
            {
                ModelState.AddModelError(nameof(user.Email), "Failed to send OTP");
            }

            return RedirectToAction(nameof(VerifyOtp), new { returnUrl, type });
        }

        /// <summary>
        /// Registration for sample user login
        /// </summary>
        /// <returns></returns>
        public IActionResult Register(string? returnUrl)
        {
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

            TempData[nameof(UserRegisterModel)] = request.Serialize();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(request.Email ?? string.Empty);
                if (user != null && user.EmailConfirmed)
                {
                    ModelState.AddModelError(nameof(request.Email), "Email duplicated!");
                    return View(request);
                }

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    if (user == null)
                    {
                        user = _mapper.Map<User>(request);
                        user.UserName = request.Email;
                        var result = await _userManager.CreateAsync(user, request.Password ?? string.Empty);
                        result = await _userManager.AddToRoleAsync(user, EnumRoleRegister.Student.ToString());

                        if (!result.Succeeded)
                        {
                            scope.Dispose();
                            result.Errors.ForEach(x =>
                            {
                                ModelState.AddModelError(x.Code, x.Description);
                            });
                            return View(request);
                        }
                    }

                    var sendResult = await SendOtpAsync(user);
                    if (!sendResult.IsOK)
                    {
                        scope.Dispose();
                        ModelState.AddModelError(nameof(request.Email), "Failed to send OTP");
                        return View(request);
                    }

                    scope.Complete();

                    return RedirectToAction(nameof(VerifyOtp), new { request.ReturnUrl, type = nameof(Register) });
                }

                //var context = await _interaction.GetAuthorizationContextAsync(request.ReturnUrl);
            }

            return View(request);
        }

        private async Task<MethodResult<bool>> SendOtpAsync(User user)
        {
            //var otp = await _userManager.GenerateUserTokenAsync(user, DataProtectionTokenProvider.TotpProviderName, DataProtectionTokenProvider.TotpProviderName);

            var otpResult = await _mediator.Send(new CreateUserOtpCommand { UserId = user.Id }).ConfigureAwait(false);
            var otp = otpResult?.Result;

            var param = new
            {
                OtpCode = otp,
                OtpValidTime = string.Format(CultureInfo.InvariantCulture, SenderSettings.OtpValidMinute, _appSetting!.Otp!.StepTime)
            };
            var subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendOtpSubjectFullName, user.FullName);
            var sendResult = await _mediator.Send(new SendOtpCommand { Email = user.Email, Subject = subject, Params = param, Template = EnumSenderTemplate.SendOtp }).ConfigureAwait(false);

            return sendResult;
        }

        private async Task<bool> VerifyOtpAsync(User user, string? otp)
        {
            //var verify = await _userManager.VerifyUserTokenAsync(user, DataProtectionTokenProvider.TotpProviderName, DataProtectionTokenProvider.TotpProviderName, otp ?? string.Empty);

            var otpResult = await _mediator.Send(new ConfirmUserOtpCommand { UserId = user.Id, Otp = otp }).ConfigureAwait(false);
            return otpResult?.Result ?? false;
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
            HttpContext.SetCookie(Settings.RequestHeader.AcceptLanguage, _languageContext.CurrentCountryInfo?.CultureCode);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string? provider)
        {
            ArgumentNullException.ThrowIfNull(model);

            if (!string.IsNullOrEmpty(provider))
            {
                var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { model.ReturnUrl });
                var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
                return Challenge(properties, provider);
            }

            _logger.LogWarning("Start Login");
            _logger.LogWarning("Model: " + model.Serialize);
            _logger.LogWarning("ReturnUrl: " + model.ReturnUrl);
            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            //// the user clicked the "cancel" button
            //if (button != "login")
            //{
            //    if (context != null)
            //    {
            //        // if the user cancels, send a result back into IdentityServer as if they
            //        // denied the consent (even if this client does not require consent).
            //        // this will send back an access denied OIDC error response to the client.
            //        await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

            //        // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
            //        if (context.IsNativeClient())
            //        {
            //            // The client is native, so this change in how to
            //            // return the response is for better UX for the end user.
            //            return this.LoadingPage("Redirect", model.ReturnUrl ?? string.Empty);
            //        }

            //        return Redirect(model.ReturnUrl ?? string.Empty);
            //    }
            //    else
            //    {
            //        // since we don't have a valid context, then we just go back to the home page
            //        return Redirect("~/");
            //    }
            //}

            _logger.LogWarning("ModelState.IsValid: " + ModelState.IsValid);

            if (ModelState.IsValid)
            {
                var user = await _signInManager.UserManager.FindByNameAsync(model.Username ?? string.Empty);

                if (user is not null)
                {
                    var userLogin = await _signInManager.CheckPasswordSignInAsync(user, model.Password ?? string.Empty, true);

                    // validate username/password against in-memory store
                    if (userLogin.Succeeded)
                    {
                        _logger.LogWarning("UserLogin.Succeeded: " + userLogin.Succeeded);

                        await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id.ToString(), user.UserName, clientId: context?.Client.ClientId));

                        // only set explicit expiration here if user chooses "remember me".
                        // otherwise we rely upon expiration configured in cookie middleware.
                        AuthenticationProperties? props = null;
                        if (AccountOptions.AllowRememberLogin && model.RememberLogin)
                        {
                            props = new AuthenticationProperties
                            {
                                IsPersistent = true,
                                ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
                            };
                        };

                        // issue authentication cookie with subject ID and username
                        var isuser = new IdentityServerUser(user.Id.ToString())
                        {
                            DisplayName = user.UserName
                        };

                        await HttpContext.SignInAsync(isuser, props);

                        if (context != null)
                        {
                            _logger.LogWarning("UserLogin.Succeeded: " + 1);
                            if (context.IsNativeClient())
                            {
                                _logger.LogWarning("UserLogin.Succeeded: " + 2);

                                // The client is native, so this change in how to
                                // return the response is for better UX for the end user.
                                return this.LoadingPage("Redirect", model.ReturnUrl ?? string.Empty);
                            }

                            // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                            return Redirect(model.ReturnUrl ?? string.Empty);
                        }

                        // request for a local page
                        if (Url.IsLocalUrl(model.ReturnUrl))
                        {
                            _logger.LogWarning("UserLogin.Succeeded: " + 3);
                            return Redirect(model.ReturnUrl);
                        }
                        else if (string.IsNullOrEmpty(model.ReturnUrl))
                        {
                            return Redirect("~/");
                        }
                        else
                        {
                            // user might have clicked on a malicious link - should be logged
                            throw new Exception("invalid return URL");
                        }
                    }
                }

                await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials", clientId: context?.Client.ClientId));
                ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
            }

            // something went wrong, show form with error
            var vm = await BuildLoginViewModelAsync(model);

            return View(vm);
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

            //if (vm.ShowLogoutPrompt == false)
            //{
            //    // if the request for logout was properly authenticated from IdentityServer, then
            //    // we don't need to show the prompt and can just log the user out directly.
            //    return await Logout(vm);
            //}

            //return View(vm);
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
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity?.IsAuthenticated == true)
            {
                //await HttpContext.SignOutAsync(IdentityServerConstants.DefaultCheckSessionCookieName);
                //await HttpContext.SignOutAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme);
                //await HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);
                //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                //await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
                //await HttpContext.SignOutAsync(Settings.OpenId);

                //foreach (var cookie in Request.Cookies.Keys)
                //{
                //    Response.Cookies.Delete(cookie);
                //}

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null)
        {
            returnUrl ??= string.Empty;

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login), new { returnUrl });
            }
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (signInResult.Succeeded)
            {
                return Redirect(returnUrl);
            }
            if (signInResult.IsLockedOut)
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
                var externalLogin = new ExternalLoginModel
                {
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    DayBirthday = birthday?.Day,
                    MonthBirthday = birthday?.Month,
                    YearBirthday = birthday?.Year,
                    Provider = info.LoginProvider,
                };

                TempData[nameof(ExternalLoginModel)] = externalLogin.Serialize();
                return View(nameof(ExternalLoginConfirmation), externalLogin);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginModel request)
        {
            ArgumentNullException.ThrowIfNull(request);
            var externalLogin = GetFromTempData(nameof(ExternalLoginModel))?.ToString().Deserialize<ExternalLoginModel>();

            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null || externalLogin == null)
            {
                return View(nameof(Error));
            }

            request.Email = externalLogin.Email;
            var user = await _userManager.FindByEmailAsync(request.Email ?? string.Empty);
            Microsoft.AspNetCore.Identity.IdentityResult result;

            if (user != null)
            {
                result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return Redirect(request.ReturnUrl ?? string.Empty);
                }
            }
            else
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    user = new User
                    {
                        Email = request.Email,
                        UserName = request.Email,
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        Gender = request.Gender,
                        Birthday = request.Birthday,
                        EmailConfirmed = true,
                    };

                    user = await _userRepository.GenerateUserDataAsync(user, EnumRoleRegister.Student);
                    result = await _userManager.CreateAsync(user);
                    result = await _userManager.AddToRoleAsync(user, EnumRoleRegister.Student.ToString());

                    if (result.Succeeded)
                    {
                        result = await _userManager.AddLoginAsync(user, info);
                        if (result.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);

                            scope.Complete();
                            return Redirect(request.ReturnUrl ?? string.Empty);
                        }
                    }

                    scope.Dispose();
                }
            }

            foreach (var error in result.Errors)
            {
                ModelState.TryAddModelError(error.Code, error.Description);
            }

            return View(nameof(ExternalLoginConfirmation), request);
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

            if (User?.Identity.IsAuthenticated != true)
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
    }
}
