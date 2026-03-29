// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.LandingPages
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Application.Commands.UserCmd;
    using Fsel.Identity.Application.Queries.CompetitionEventsQuery;
    using Fsel.Identity.Application.Services.SenderService;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class FormRegisterStudentForEventCommand : RegisterStudentForEventCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class FormRegisterStudentForEventCommandHandler : IRequestHandler<FormRegisterStudentForEventCommand, MethodResult<bool>>
    {
        private readonly IEventRegistrationRepository _eventRegistrationRepository;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly IMapper _mapper;
        private readonly ISenderService _senderService;
        private readonly AppSetting _appSetting;
        private const string Subject = "Thông tin đăng kí tham gia sự kiện";
        private readonly Microsoft.AspNetCore.Identity.UserManager<User> _userManager;
        private readonly IPlatformRepository _platformRepository;
        private readonly IMediator _mediator;
        private readonly ISystemService _systemService;

        public FormRegisterStudentForEventCommandHandler(IEventRegistrationRepository eventRegistrationRepository, ICompetitionEventsRepository competitionEventsRepository, IMapper mapper, ISenderService senderService, AppSetting appSetting, Microsoft.AspNetCore.Identity.UserManager<User> userManager,
            IPlatformRepository platformRepository, MediatR.IMediator mediator,
            IStudentCompetitionEventsRepository studentCompetitionEventsRepository,
            ISystemService systemService)
        {
            _eventRegistrationRepository = eventRegistrationRepository;
            _competitionEventsRepository = competitionEventsRepository;
            _mapper = mapper;
            _senderService = senderService;
            _appSetting = appSetting;
            _userManager = userManager;
            _platformRepository = platformRepository;
            _mediator = mediator;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _systemService = systemService;
        }

        public async Task<MethodResult<bool>> Handle(FormRegisterStudentForEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            // Bước 1: Tìm kiếm sự kiện thi đấu theo mã sự kiện
            var competitionEvent = await _competitionEventsRepository.Queryable
                .FirstOrDefaultAsync(p => p.EventCode == request.EventCode, cancellationToken);

            if (competitionEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            // Bước 2: Validate các trường dữ liệu đầu vào (email, số điện thoại)
            if (!ValidateInput(request, methodResult))
            {
                return methodResult;
            }

            // Bước 3: Xác định sự kiện cha hoặc con dựa trên district
            var actualEvent = await DetermineActualEventAsync(competitionEvent, request, cancellationToken);

            // Bước 4: Kiểm tra loại đăng ký và xử lý tương ứng
            if (competitionEvent.EventContent?.Actions?.Any(p => p == EnumSchoolEventRuleAction.RegisterAndCreateUser) == true)
            {
                // Đăng ký kèm tạo tài khoản người dùng mới
                await RegisterAndCreateUserAsync(request, competitionEvent, actualEvent, methodResult, cancellationToken);
            }
            else
            {
                // Chỉ đăng ký tham gia sự kiện (không tạo tài khoản)
                await RegisterOnlyAsync(request, actualEvent, methodResult, cancellationToken);
            }

            return methodResult;
        }

        /// <summary>
        /// Bước 2: Validate các trường dữ liệu đầu vào
        /// </summary>
        private bool ValidateInput(RegisterStudentForEventCommandModel request, MethodResult<bool> methodResult)
        {
            // Validate email học sinh
            if (!string.IsNullOrEmpty(request.Email) && !request.Email.IsValidEmail())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return false;
            }

            // Validate số điện thoại học sinh
            if (!string.IsNullOrEmpty(request.PhoneNumber) && !request.PhoneNumber.IsValidPhoneNumber())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Bước 3: Xác định sự kiện thực tế (sự kiện cha hoặc sự kiện con theo district)
        /// </summary>
        private async Task<CompetitionEvent> DetermineActualEventAsync(
            CompetitionEvent parentEvent,
            RegisterStudentForEventCommandModel request,
            CancellationToken cancellationToken)
        {
            // Nếu là sự kiện cha và có districtId thì tìm sự kiện con theo location
            if (parentEvent.EventContent?.IsParentEvent == true && request.DistrictId.HasValue)
            {
                var childEvents = await _mediator.Send(new GetChildEventsByParentIdQuery { Id = parentEvent.Id }, cancellationToken);
                var childEvent = childEvents.Result?.FirstOrDefault(p => p.LocationId == request.DistrictId);

                return childEvent ?? parentEvent;
            }

            return parentEvent;
        }

        /// <summary>
        /// Bước 4a: Đăng ký kèm tạo tài khoản người dùng mới
        /// </summary>
        private async Task<MethodResult<bool>> RegisterAndCreateUserAsync(
            RegisterStudentForEventCommandModel request,
            CompetitionEvent? parentEvent,
            CompetitionEvent competitionEvent,
            MethodResult<bool> methodResult,
            CancellationToken cancellationToken)
        {
            // Bước 4a.1: Kiểm tra user đã tồn tại chưa
            var existingUser = await CheckUserExistsAsync(request.Email, cancellationToken);
            if (existingUser != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Email), request.Email);
                return methodResult;
            }

            // Bước 4a.2: Lấy thông tin platform LMS
            var platform = await _platformRepository.GetPlatformAsync(EnumPlatformCode.LMS, cancellationToken);
            if (platform == null)
            {
                methodResult.AddErrorBadRequest("Platform null");
                return methodResult;
            }

            // Bước 4a.3: Tạo mới user và student
            var user = CreateUserAndStudentAsync(request, platform);

            // Bước 4a.4: Validate password (sẽ tự tạo mới nếu password null)
            var (isPasswordValid, passwordToUse) = await ValidatePasswordAsync(user, request.Password, methodResult);
            if (!isPasswordValid)
            {
                return methodResult;
            }

            // Bước 4a.5: Validate model trước khi tạo
            if (!user.IsValid())
            {
                methodResult.AddError(user.ErrorMessages);
                return methodResult;
            }

            // Bước 4a.6: Tạo user trong Identity
            var identityResult = await _userManager.CreateAsync(user, passwordToUse);
            if (!identityResult.Succeeded)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate));
                return methodResult;
            }

            // Bước 4a.7: Gán role Student cho user
            await _userManager.AddToRoleAsync(user, EnumRole.Student.ToString());

            // Bước 4a.8: Cập nhật mã học sinh
            var updateCodeResult = await UpdateStudentCodeAsync(user, request, cancellationToken);
            if (!updateCodeResult.IsOK)
            {
                methodResult.AddErrorBadRequest(updateCodeResult.ErrorMessages);
                return methodResult;
            }

            // Bước 4a.9: Đăng ký tham gia sự kiện
            await RegisterOnlyAsync(request, competitionEvent, user.Student.Id, false, methodResult, cancellationToken);

            // Bước 4a.10: Lưu vào bảng StudentCompetitionEvent
            await AddStudentCompetitionEventAsync(user.Student.Id, competitionEvent.Id, methodResult, cancellationToken);

            // Bước 4a.11: Gửi email thông tin đăng nhập (nếu có cấu hình)
            await SendLoginInfoEmailIfConfiguredAsync(request, parentEvent ?? competitionEvent, passwordToUse, methodResult);

            return methodResult;
        }

        /// <summary>
        /// Bước 4a.1: Kiểm tra user đã tồn tại chưa
        /// </summary>
        private async Task<User?> CheckUserExistsAsync(string? email, CancellationToken cancellationToken)
        {
            var queryByEmail = _userManager.Users.Where(x => x.Email == email);
            var queryByUserName = _userManager.Users.Where(x => x.UserName == email);

            return await queryByEmail.Union(queryByUserName).FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Bước 4a.3: Tạo mới user và student entity
        /// </summary>
        private User CreateUserAndStudentAsync(RegisterStudentForEventCommandModel request, Platform platform)
        {
            return new User
            {
                UserName = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                EmailConfirmed = true,
                PhoneNumber = request.PhoneNumber,
                Birthday = request.BirthDay,
                PhoneNumberConfirmed = false,
                Student = new Student
                {
                    Occupation = "Student",
                    CourseLevel = EnumCourseLevel.A1,
                    CreatedByParent = false,
                    SchoolId = request.SchoolId,
                    School = request.School,
                    SchoolClass = request.SchoolClass,
                    SchoolGrade = request.SchoolGrade,
                    ProvinceId = request.ProvinceId,
                    DistrictId = request.DistrictId,
                    ParentEmail = request.ParentEmail,
                    ParentPhoneNumber = request.ParentPhoneNumber,
                    SchoolFaculty = request.SchoolFaculty,
                },
                UserPlatforms = new List<UserPlatform>
                {
                    new UserPlatform { PlatformId = platform.Id }
                },
                UserSettings = new List<UserSetting>
                {
                    new UserSetting(true)
                }
            };
        }

        /// <summary>
        /// Bước 4a.4: Validate password theo chính sách Identity
        /// Nếu password là null, sẽ tự động tạo mật khẩu mới
        /// </summary>
        private async Task<(bool isValid, string password)> ValidatePasswordAsync(User user, string? password, MethodResult<bool> methodResult)
        {
            var passwordToValidate = password ?? GeneratePassword();
            var passwordValidator = new PasswordValidator<User>();
            var validPassword = await passwordValidator.ValidateAsync(_userManager, user, passwordToValidate);

            if (!validPassword.Succeeded)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.PasswordIsNotValid));
                return (false, passwordToValidate);
            }

            return (true, passwordToValidate);
        }

        /// <summary>
        /// Bước 4a.8: Cập nhật mã học sinh
        /// </summary>
        private async Task<MethodResult<UserModel>> UpdateStudentCodeAsync(User user, RegisterStudentForEventCommandModel request, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new UpdateCodeStudentCommand
            {
                UserId = user.Id,
                Gender = EnumGender.Male,
                Birthday = user.Birthday,
                SchoolId = request.SchoolId,
                ProvinceId = request.ProvinceId,
                DistrictId = request.DistrictId
            }, cancellationToken);
        }

        /// <summary>
        /// Bước 4a.10: Lưu vào bảng StudentCompetitionEvent
        /// </summary>
        private async Task AddStudentCompetitionEventAsync(Guid studentId, Guid eventId, MethodResult<bool> methodResult, CancellationToken cancellationToken)
        {
            await _studentCompetitionEventsRepository.ExecuteTransactionAsync(async () =>
            {
                var studentCompetitionEvent = _studentCompetitionEventsRepository.Add(
                    new StudentCompetitionEvent { StudentId = studentId, CompetitionEventId = eventId });
                await _studentCompetitionEventsRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });
        }

        /// <summary>
        /// Bước 4a.11: Gửi email thông tin đăng nhập nếu được cấu hình
        /// </summary>
        private async Task SendLoginInfoEmailIfConfiguredAsync(
            RegisterStudentForEventCommandModel request,
            CompetitionEvent competitionEvent,
            string password,
            MethodResult<bool> methodResult)
        {
            var template = competitionEvent.EventContent?.ActionConfigs?.FirstOrDefault(p => p.MailRegister.HasValue);

            if (template != null && template.MailRegister.HasValue && !string.IsNullOrEmpty(template.SubjectMailRegister))
            {
                await SendMailInfoUserAsync(request, password, template.MailRegister.Value, template.SubjectMailRegister);
            }
        }

        /// <summary>
        /// Bước 4b: Đăng ký tham gia sự kiện (không tạo tài khoản)
        /// </summary>
        private async Task<MethodResult<bool>> RegisterOnlyAsync(
            RegisterStudentForEventCommandModel request,
            CompetitionEvent competitionEvent,
            MethodResult<bool> methodResult,
            CancellationToken cancellationToken)
        {
            return await RegisterOnlyAsync(request, competitionEvent, null, true, methodResult, cancellationToken);
        }

        /// <summary>
        /// Bước 4b: Đăng ký tham gia sự kiện (nội bộ - có thể có studentId)
        /// </summary>
        private async Task<MethodResult<bool>> RegisterOnlyAsync(
            RegisterStudentForEventCommandModel request,
            CompetitionEvent competitionEvent,
            Guid? studentId,
            bool isSendMail,
            MethodResult<bool> methodResult,
            CancellationToken cancellationToken)
        {
            // Bước 4b.1: Kiểm tra đã đăng ký sự kiện này chưa
            var eventRegistration = await _eventRegistrationRepository.Queryable
                .FirstOrDefaultAsync(p => p.Email == request.Email && p.CompetitionEventId == competitionEvent.Id, cancellationToken);

            // Bước 4b.2: Tạo mới hoặc cập nhật đăng ký
            if (eventRegistration == null)
            {
                eventRegistration = _mapper.Map<EventRegistration>(request);
                eventRegistration.CompetitionEventId = competitionEvent.Id;
                eventRegistration.StudentId = studentId;
                eventRegistration.Status = EnumEventRegistrationStatus.Active;
                eventRegistration = _eventRegistrationRepository.Add(eventRegistration);
            }
            else
            {
                _mapper.Map(request, eventRegistration);
                eventRegistration = _eventRegistrationRepository.Update(eventRegistration);
            }

            // Bước 4b.3: Validate dữ liệu đăng ký
            if (!eventRegistration.IsValid())
            {
                methodResult.AddError(eventRegistration.ErrorMessages);
                return methodResult;
            }

            // Bước 4b.4: Lưu và gửi email xác nhận
            await _eventRegistrationRepository.ExecuteTransactionAsync(async () =>
            {
                await _eventRegistrationRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;

                if (isSendMail)
                {
                    await SendMailRegisterEventAsync(request, competitionEvent);
                }

                return methodResult;
            });

            return methodResult;
        }

        /// <summary>
        /// Gửi email thông tin đăng nhập cho học sinh mới
        /// </summary>
        private async Task SendMailInfoUserAsync(RegisterStudentForEventCommandModel request, string password, EnumSenderTemplate senderTemplate, string subject)
        {
            await _senderService.SendEmailAsync(new SendEmailByTemplateCommandModel
            {
                ToEmails = new List<string> { request.Email ?? string.Empty },
                Template = senderTemplate,
                Subject = subject,
                Params = new
                {
                    UserName = request.Email,
                    Password = password,
                    FullName = request.FirstName + " " + request.LastName,
                    Class = request.SchoolClass,
                    School = request.School
                },
            });
        }

        /// <summary>
        /// Gửi email xác nhận đăng ký sự kiện
        /// </summary>
        private async Task SendMailRegisterEventAsync(RegisterStudentForEventCommandModel request, CompetitionEvent competitionEvent)
        {
            var param = BuildMailEventParams(request, competitionEvent);

            await _senderService.SendEmailAsync(new SendEmailByTemplateCommandModel
            {
                ToEmails = new List<string> { request.Email ?? string.Empty },
                Template = EnumSenderTemplate.MailRegisterEvent,
                Subject = Subject,
                Params = param,
            });
        }

        /// <summary>
        /// Xây dựng tham số cho email sự kiện
        /// </summary>
        private ParamSendMailEvent BuildMailEventParams(RegisterStudentForEventCommandModel request, CompetitionEvent competitionEvent)
        {
            return new ParamSendMailEvent
            {
                FullName = request.FirstName + " " + request.LastName,
                LinkLMS = _appSetting.ResourceContent?.LmsWebsiteUrl,
                StartDateEvent = competitionEvent.EventContent?.StartDate?.ToString("dd/MM", CultureInfo.InvariantCulture),
                EndDateEvent = competitionEvent.EventContent?.EndDate?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                StartDateAward = competitionEvent.EventContent?.AwardStartDate?.ToString("dd/MM", CultureInfo.InvariantCulture),
                EndDateAward = competitionEvent.EventContent?.AwardEndDate?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                Date = competitionEvent.EventContent?.StartDate?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                BirthDay = request.BirthDay.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                School = request.School,
                SchoolStudentCode = request.SchoolStudentCode,
                SchoolGrade = request.SchoolGrade,
                SchoolClass = request.SchoolClass,
                LinkLeaderBoard = competitionEvent.EventContent?.LinkLeaderBoard
            };
        }

        /// <summary>
        /// Tạo mật khẩu theo quy tắc:
        /// - 8 ký tự
        /// - Ký tự đặc biệt: '@'
        /// - Số: '23456789'
        /// - Tối thiểu 1 ký tự in Hoa (loại O, I, L, W, V)
        /// - Tối thiểu 1 ký tự in thường (loại O, I, L, W, V)
        /// </summary>
        private string GeneratePassword()
        {
            const string uppercaseChars = "ABCDEFGHJKMNPQRSTUXYZ"; // Loại O, I, L, W, V
            const string lowercaseChars = "abcdefghjkmnpqrstuxyz"; // Loại O, I, L, W, V
            const string numberChars = "23456789";
            const string specialChar = "@";

            var random = new Random();

            // Lấy ngẫu nhiên 1 ký tự từ mỗi loại bắt buộc
            var upperChar = uppercaseChars[random.Next(uppercaseChars.Length)];
            var lowerChar = lowercaseChars[random.Next(lowercaseChars.Length)];
            var numberChar = numberChars[random.Next(numberChars.Length)];
            var special = specialChar[0];

            // Còn lại 4 ký tự, lấy ngẫu nhiên từ tất cả các loại
            const string allChars = "ABCDEFGHJKMNPQRSTUXYZabcdefghjkmnpqrstuxyz23456789@";
            var remaining = new char[4];
            for (int i = 0; i < 4; i++)
            {
                remaining[i] = allChars[random.Next(allChars.Length)];
            }

            // Ghép tất cả lại và shuffle
            var allChars2 = new[] { upperChar, lowerChar, numberChar, special }
                .Concat(remaining)
                .ToArray();

            // Shuffle để random vị trí
            for (int i = allChars2.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (allChars2[i], allChars2[j]) = (allChars2[j], allChars2[i]);
            }

            return new string(allChars2);
        }
    }
}
