// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using System.Globalization;
    using System.Threading;
    using System.Transactions;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
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
    using Microsoft.Extensions.Hosting;
    using Microsoft.Net.Http.Headers;

    public class CreateUserStudentToAdminCommand : CreateUserStudentToAdminCommandModel, IRequest<MethodResult<UserModel>>
    {
        public bool IsTrialRegistration { get; set; }
        public Guid? PackageId { get; set; }
        public Guid CourseId { get; set; }
        public DateTime? ExpireDate { get; set; }
        public EnumPaymentRevenueType PaymentRevenueType { get; set; }
    }

    public class CreateUserStudentToAdminCommandHandler : IRequestHandler<CreateUserStudentToAdminCommand, MethodResult<UserModel>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly UserManager<User> _userManager;
        private readonly IOrderService _orderService;
        private readonly IHumanRepository _humanRepository;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly IPlatformRepository _platformRepository;
        private readonly IHostEnvironment _environment;

        private const string DefaultPassword = "Admin@123";
        private const string RoleStudent = nameof(Student);
        private const int TotalUserDateNow = 2100;
        private const int MinAgeYoung = 14;
        private const int MaxAgeChildren = 13;

        public CreateUserStudentToAdminCommandHandler(IHttpContextAccessor httpContextAccessor, IMediator mediator, IMapper mapper, AuthContext authContext, UserManager<User> userManager, IOrderService orderService, IHumanRepository humanRepository, ILmsCourseService lmsCourseService, IPlatformRepository platformRepository, IHostEnvironment environment = null)
        {
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
            _mapper = mapper;
            _authContext = authContext;
            _userManager = userManager;
            _orderService = orderService;
            _humanRepository = humanRepository;
            _lmsCourseService = lmsCourseService;
            _platformRepository = platformRepository;
            _environment = environment;
        }

        public async Task<MethodResult<UserModel>> Handle(CreateUserStudentToAdminCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserModel>();
            //if (_environment.IsProduction())
            //{
            //    methodResult.AddError(StatusCodes.Status401Unauthorized, "Not Have Access Production");
            //    return methodResult;
            //}
            if (string.IsNullOrEmpty(request.Email))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Email));
                return methodResult;
            }
            //var countUser = await _userManager.Users.Where(x => x.CreatedUserId == _authContext.CurrentUserId && x.CreatedDate.Date == DateTime.UtcNow.Date).CountAsync(cancellationToken);
            //if (countUser > TotalUserDateNow)
            //{
            //    methodResult.AddErrorBadRequest("exceeding 50 Users created");
            //    return methodResult;
            //}

            #region Get Course

            var courseResult = await _lmsCourseService.GetCourseByIdAsync(request.CourseId);
            if (!courseResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallCourseServiceError));
                return methodResult;
            }
            var course = courseResult?.Content?.Result;
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            #endregion Get Course

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(user));
                return methodResult;
            }
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    user = new User();
                    user.UserName = request.Email;
                    user.Email = request.Email;
                    user.FullName = request.FullName ?? request.Email;
                    user.EmailConfirmed = true;
                    user.PhoneNumber = request.PhoneNumber;
                    user.UserSettings = new List<UserSetting>()
                        {
                            new UserSetting(true)
                        };
                    user = CreateHumanToUser(user, request, course.CourseLevel);
                    await UpdatePlatformToUserAsync(user, cancellationToken);

                    if (!user.IsValid())
                    {
                        methodResult.AddErrorBadRequest(user.ErrorMessages);
                        return methodResult;
                    }

                    var result = await _userManager.CreateAsync(user, DefaultPassword);
                    if (!result.Succeeded)
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate));
                        return methodResult;
                    }
                    await _userManager.AddToRoleAsync(user, nameof(Student));
                    scope.Complete();
                }
                catch
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.SendAuthErorr));
                    scope.Dispose();
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
            EnumGender gender = EnumGender.Male;
            if (!Enum.TryParse(request.Gender, out gender))
            {
                gender = EnumGender.Male;
            }
            var updateCode = await _mediator.Send(new UpdateCodeStudentCommand
            {
                UserId = user.Id,
                Gender = gender,
                Birthday = user.Human?.Birthday,
                SchoolName = request.School,
                SchoolId = request.SchoolId
            }, cancellationToken);
            if (!updateCode.IsOK)
            {
                methodResult.AddErrorBadRequest(updateCode.ErrorMessages);
                return methodResult;
            }
            var student = user.Human?.Student;

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
                PaymentRevenueType = request.PaymentRevenueType
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

        private static User CreateHumanToUser(User user, CreateUserStudentToAdminCommand request, EnumCourseLevel level)
        {
            EnumGender gender = EnumGender.Male;
            if (!Enum.TryParse(request.Gender, out gender))
            {
                gender = EnumGender.Male;
            }
            Human human = new();
            human.Email = user.Email;
            human.FullName = user.FullName;
            human.Gender = gender;
            if (string.IsNullOrEmpty(request.DateOfBirth))
            {
                human.Birthday = GetBirthdayToCourseLevel(level);
            }
            else
            {
                DateTime.TryParse(request.DateOfBirth, new CultureInfo("vi-VN"), DateTimeStyles.None, out DateTime dateOfBirth);
                human.Birthday = dateOfBirth;
            }
            human.PhoneNumber = request.PhoneNumber;
            human.UserId = user.Id;
            human.Student = new Student
            {
                HumanId = human.Id,
                Occupation = RoleStudent,
                SchoolId = request.SchoolId,
                School = request.School,
                CourseId = request.CourseId,
            };
            user.Human = human;
            return user;
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
