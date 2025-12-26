// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using System.Globalization;
    using System.Threading;
    using System.Transactions;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Commands.AuthCmd;
    using Fsel.Identity.Application.Commands.UserCmd;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.LmsCourseService.CommandModels;
    using Fsel.Identity.Application.Services.LmsCourseService.Model;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.CommandModels;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Admins;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Net.Http.Headers;
    using PhoneNumbers;
    using static IdentityServer4.Models.IdentityResources;

    public class CreateUserStudentToAdminCommand : CreateUserStudentToAdminCommandModel, IRequest<MethodResult<UserModel>>
    {
        public bool IsTrialRegistration { get; set; }
        public Guid? PackageId { get; set; }
        public DateTime? ExpireDate { get; set; }
        public EnumPaymentRevenueType RevenueType { get; set; }
    }

    public class CreateUserStudentToAdminCommandHandler : IRequestHandler<CreateUserStudentToAdminCommand, MethodResult<UserModel>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly Microsoft.AspNetCore.Identity.UserManager<User> _userManager;
        private readonly IOrderService _orderService;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly IPlatformRepository _platformRepository;
        private readonly IHostEnvironment _environment;
        private const string DefaultPassword = "Admin@123";
        private const string RoleStudent = nameof(Student);
        private const int TotalUserDateNow = 2100;
        private const int MinAgeYoung = 14;
        private const int MaxAgeChildren = 13;
        private static string GenderNam = "nam";
        private static string GenderNu = "nữ";

        public CreateUserStudentToAdminCommandHandler(IHttpContextAccessor httpContextAccessor,
            IMediator mediator,
            IMapper mapper,
            AuthContext authContext,
            Microsoft.AspNetCore.Identity.UserManager<User> userManager,
            IOrderService orderService,
            ILmsCourseService lmsCourseService,
            IPlatformRepository platformRepository,
            IHostEnvironment environment)
        {
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
            _mapper = mapper;
            _authContext = authContext;
            _userManager = userManager;
            _orderService = orderService;
            _lmsCourseService = lmsCourseService;
            _platformRepository = platformRepository;
            _environment = environment;
        }

        public async Task<MethodResult<UserModel>> Handle(CreateUserStudentToAdminCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserModel>();

            #region Validate environment

            //if (_environment.IsProduction())
            //{
            //    methodResult.AddError(StatusCodes.Status401Unauthorized, "Not Have Access Production");
            //    return methodResult;
            //}

            //var countUser = await _userManager.Users.Where(x => x.CreatedUserId == _authContext.CurrentUserId && x.CreatedDate.Date == DateTime.UtcNow.Date).CountAsync(cancellationToken);
            //if (countUser > TotalUserDateNow)
            //{
            //    methodResult.AddErrorBadRequest("exceeding 50 Users created");
            //    return methodResult;
            //}

            #endregion Validate environment

            var courseResult = await HandleCourse(request);
            if (!courseResult.IsOK)
            {
                methodResult.AddError(courseResult.ErrorMessages);
                return methodResult;
            }
            var course = courseResult.Result;
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            #region Validate Email And Phone

            if (string.IsNullOrEmpty(request.Email))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Email));
                return methodResult;
            }
            if (!request.Email.IsValidEmail())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.Email));
                return methodResult;
            }
            if (!string.IsNullOrEmpty(request.PhoneNumber) && !request.PhoneNumber.IsValidPhoneNumber())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.PhoneNumber));
                return methodResult;
            }

            #endregion Validate Email And Phone

            if (request.PackageId.HasValue)
            {
                var packageResults = await _orderService.GetPackages();
                if (!packageResults.IsSuccessStatusCode)
                {
                    methodResult.AddError(packageResults.Error);
                    return methodResult;
                }
                var package = packageResults.Content?.Result?.FirstOrDefault(x => x.Id == request.PackageId);
                if (package == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(package), request.PackageId);
                    return methodResult;
                }
            }
            foreach (IPasswordValidator<User> passwordValidator in _userManager.PasswordValidators)
            {
                var resultData = await passwordValidator.ValidateAsync(_userManager, new User(), request.Password);
                if (!resultData.Succeeded)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.Password));
                    return methodResult;
                }
            }
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(user));
                return methodResult;
            }
            //using (var scope = new TransactionScope(TransactionScopeOption.Required,
            //    new TransactionOptions
            //    {
            //        IsolationLevel = IsolationLevel.ReadCommitted
            //    },
            //    TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    user = new User();
                    user.UserName = request.Email;
                    user.Email = request.Email;
                    user.FirstName = request.FullName.ParseFullName().FirstName;
                    user.LastName = request.FullName.ParseFullName().LastName;
                    user.EmailConfirmed = true;
                    user.Gender = GetEnumGender(request.Gender);
                    user.PhoneNumber = request.PhoneNumber;
                    user.Student = new Student
                    {
                        Occupation = RoleStudent,
                        SchoolId = !string.IsNullOrEmpty(request.SchoolId) && Guid.TryParse(request.SchoolId, out Guid schoolIdData) ? schoolIdData : null,
                        School = request.School,
                        CourseId = !string.IsNullOrEmpty(request.CourseId) && Guid.TryParse(request.CourseId, out Guid courseId) ? courseId : null,
                    };
                    if (string.IsNullOrEmpty(request.DateOfBirth))
                    {
                        user.Birthday = GetBirthdayToCourseLevel(course.CourseLevel);
                    }
                    else if (DateTime.TryParse(request.DateOfBirth, out DateTime dateOfBirth))
                    {
                        user.Birthday = dateOfBirth;
                    }

                    user.UserSettings = new List<UserSetting>()
                    {
                        new UserSetting(true)
                    };
                    await UpdatePlatformToUserAsync(user, cancellationToken);
                    if (!user.IsValid())
                    {
                        methodResult.AddErrorBadRequest(user.ErrorMessages);
                        return methodResult;
                    }

                    var result = await _userManager.CreateAsync(user, request.Password ?? DefaultPassword);
                    if (!result.Succeeded)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate));
                        return methodResult;
                    }
                    await _userManager.AddToRoleAsync(user, nameof(Student));
                    //scope.Complete();
                }
                catch
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.SendAuthErorr));
                    //scope.Dispose();
                }
            }
            if (!methodResult.IsOK || user == null)
            {
                return methodResult;
            }
            var tokenResult = await _mediator.Send(new GenerateTokenCommand { Id = user.Id }, cancellationToken);
            if (tokenResult.Result?.AccessToken != null && _httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Authorization] = "Bearer " + tokenResult.Result?.AccessToken;
            }

            var updateCode = await _mediator.Send(new UpdateCodeStudentCommand
            {
                UserId = user.Id,
                Gender = GetEnumGender(request.Gender),
                Birthday = user?.Birthday,
                SchoolName = request.School,
                SchoolId = !string.IsNullOrEmpty(request.SchoolId) && Guid.TryParse(request.SchoolId, out Guid schoolId) ? schoolId : null,
            }, cancellationToken);
            if (!updateCode.IsOK)
            {
                methodResult.AddErrorBadRequest(updateCode.ErrorMessages);
                return methodResult;
            }
            var student = user?.Student;

            await _lmsCourseService.SavePlacementTestDoneAsync(new SavePlacementTestDoneCommandModel { CourseLevel = GetCourseLevel(course.CourseLevel), StudentId = student?.Id ?? default });
            var orderResult = await SaveOrderAsync(user, course, request);
            if (!orderResult.IsOK)
            {
                methodResult.AddErrorBadRequest(orderResult.ErrorMessages);
                return methodResult;
            }

            methodResult.Result = _mapper.Map<UserModel>(user);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static EnumCourseLevel GetCourseLevel(EnumCourseLevel courseLevel)
        {
            if (courseLevel.GetEnumCourseType() == EnumCourseType.Ielts)
            {
                courseLevel = courseLevel.GetLevelAcaToLevelIELTS() ?? default;
            }
            return courseLevel;
        }

        private async Task<MethodResult<CourseModel>> HandleCourse(CreateUserStudentToAdminCommand request)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CourseModel>();
            CourseModel? course = default;
            if (!string.IsNullOrEmpty(request.CourseId) && Guid.TryParse(request.CourseId, out Guid courseId))
            {
                var courseResult = await _lmsCourseService.GetCourseByIdAsync(courseId);
                if (!courseResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallCourseServiceError));
                    return methodResult;
                }
                course = courseResult?.Content?.Result;
            }
            if (!string.IsNullOrEmpty(request.CourseLevel) && Enum.TryParse(request.CourseLevel, out EnumCourseLevel coureLevel) && course == null)
            {
                var courseResult = await _lmsCourseService.GetCourseByLevelAsync(coureLevel);
                if (!courseResult.IsSuccessStatusCode)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallCourseServiceError));
                    return methodResult;
                }
                course = courseResult?.Content?.Result;
            }
            methodResult.Result = course;
            return methodResult;
        }

        private async Task UpdatePlatformToUserAsync(User user, CancellationToken cancellationToken)
        {
            var platform = await _platformRepository.GetPlatformAsync(EnumPlatformCode.LMS, cancellationToken);
            if (platform != null)
            {
                user.UserPlatforms.Add(new UserPlatform
                {
                    PlatformId = platform.Id
                });
            }
        }

        private async Task<VoidMethodResult> SaveOrderAsync(User user, CourseModel course, CreateUserStudentToAdminCommand request)
        {
            var methodResult = new VoidMethodResult();
            var packagesResult = await _orderService.GetPackages();
            if (!packagesResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallOrderServiceError));
                return methodResult;
            }
            var packages = packagesResult.Content?.Result?.OrderBy(x => x.Price);
            var package = packages?.FirstOrDefault(p => request.PackageId.HasValue && p.Id == request.PackageId);
            if (package == null)
            {
                package = packages?.FirstOrDefault(p => p.Code == EnumPackageCode.BASIC.ToString());
            }
            if (package == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var createOrderResult = await _orderService.CreateOrderForStudentAsync(new CreateOrderByUserIdCommandModel
            {
                UserId = user.Id,
                CourseLevel = course.CourseLevel,
                Address = "35 Lac Trung",
                PaymentMethod = EnumPaymentMethodStatus.BankTransfer,
                CourseId = course.Id,
                PackageId = package.Id,
                IsTrialRegistration = request.IsTrialRegistration,
                ExpireDate = request.ExpireDate,
                RevenueType = request.RevenueType,
                IsSendEmail = bool.TryParse(request.IsSendMail, out bool isSendMail) && isSendMail,
            });
            if (!createOrderResult.IsSuccessStatusCode)
            {
                methodResult.AddError(createOrderResult.Error);
                return methodResult;
            }
            var order = createOrderResult.Content?.Result;
            if (order == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static EnumGender GetEnumGender(string? gender)
        {
            var enumGender = EnumGender.Male;
            if (!string.IsNullOrEmpty(gender) && !Enum.TryParse(gender, out enumGender))
            {
                gender = gender.ToLower(CultureInfo.CurrentCulture);
                if (gender.Contains(GenderNu, StringComparison.CurrentCulture))
                {
                    enumGender = EnumGender.Female;
                }
                else if (gender.Contains(GenderNam, StringComparison.CurrentCulture))
                {
                    enumGender = EnumGender.Male;
                }
                else
                {
                    enumGender = EnumGender.Other;
                }
            }
            return enumGender;
        }

        private static DateTime GetBirthdayToCourseLevel(EnumCourseLevel courseLevel)
        {
            int yearOld = default;
            switch (courseLevel)
            {
                case EnumCourseLevel.MS1:
                case EnumCourseLevel.MS2:
                case EnumCourseLevel.MS3:
                case EnumCourseLevel.C1:
                case EnumCourseLevel.B2:
                    yearOld = MinAgeYoung;
                    break;

                default:
                    yearOld = MaxAgeChildren;
                    break;
            }
            return new DateTime(DateTime.UtcNow.AddYears(-yearOld).Year, 1, 1);
        }
    }
}
