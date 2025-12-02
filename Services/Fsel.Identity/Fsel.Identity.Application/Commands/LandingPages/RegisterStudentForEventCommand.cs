// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.LandingPages
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Commands.UserCmd;
    using Fsel.Identity.Application.Services;
    using Fsel.Identity.Application.Commands.UserOtpCodeCmd;
    using Fsel.Identity.Application.Services.InteractionService;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SenderService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class ParamSendMailEvent
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? SchoolGrade { get; set; }
        public string? SchoolClass { get; set; }
        public string? SchoolStudentCode { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? BirthDay { get; set; }
        public string? PhoneNumber { get; set; }
        public string? School { get; set; }
        public string? StartDateEvent { get; set; }
        public string? EndDateEvent { get; set; }
        public string? StartDateAward { get; set; }
        public string? EndDateAward { get; set; }
        public string? Date { get; set; }
        public string? LinkLMS { get; set; }
        public string? LinkLeaderBoard { get; set; }
        public string? LinkLuckyStar { get; set; }
        public string? LinkResetProgress { get; set; }
    };

    public class RegisterStudentForEventCommand : RegisterStudentForEventCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class RegisterStudentForEventCommandHandler : IRequestHandler<RegisterStudentForEventCommand, MethodResult<bool>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IEventRegistrationRepository _eventRegistrationRepository;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly UserManager<User> _userManager;
        private readonly IPlatformRepository _platformRepository;
        private const string DefaultPassword = "Fsel@2024";
        private readonly ISystemService _systemService;
        private readonly ISenderService _senderService;
        private readonly IOrderService _orderService;
        private readonly IMediator _mediator;
        private readonly IInteractionService _interactionService;
        private readonly AppSetting _appSetting;
        private readonly IMapper _mapper;
        private readonly IUserCourseSettingRepository _userCourseSettingRepository;
        private const string CreateAccountWithEventSuccess = "Thông tin tài khoản tham gia sự kiện";
        private const string WasInAnotherEvent = "Thông báo tài khoản không đủ điều kiện tham gia sự kiện";
        private const string LearnedOnThePlatform = "Thông báo đặt lại dữ liệu khóa học để tham gia sự kiện";
        private const string SignUpEventSuccess = "Thông tin đăng kí tham gia sự kiện";

        public RegisterStudentForEventCommandHandler(ICompetitionEventsRepository competitionEventsRepository, IStudentCompetitionEventsRepository studentCompetitionEventsRepository, UserManager<User> userManager, IPlatformRepository platformRepository, ISystemService systemService, ISenderService senderService, IOrderService orderService, MediatR.IMediator mediator, IInteractionService interactionService, AppSetting appSetting, IMapper mapper, IUserCourseSettingRepository userCourseSettingRepository, IEventRegistrationRepository eventRegistrationRepository)
        {
            _competitionEventsRepository = competitionEventsRepository;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _userManager = userManager;
            _platformRepository = platformRepository;
            _systemService = systemService;
            _senderService = senderService;
            _orderService = orderService;
            _mediator = mediator;
            _interactionService = interactionService;
            _appSetting = appSetting;
            _mapper = mapper;
            _userCourseSettingRepository = userCourseSettingRepository;
            _eventRegistrationRepository = eventRegistrationRepository;
        }

        public async Task<MethodResult<bool>> Handle(RegisterStudentForEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var @event = await _competitionEventsRepository.Queryable.FirstOrDefaultAsync(p => p.EventCode == request.EventCode, cancellationToken);
            if (@event == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var param = new ParamSendMailEvent
            {
                FullName = request.FirstName + " " + request.LastName,
                Email = request.Email,
                Password = DefaultPassword,
                StartDateEvent = @event.EventContent?.StartDate?.ToString("dd-MM-yyy", CultureInfo.CurrentCulture),
                EndDateEvent = @event.EventContent?.EndDate?.ToString("dd-MM-yyy", CultureInfo.CurrentCulture),
                StartDateAward = @event.EventContent?.AwardStartDate?.ToString("dd-MM-yyy", CultureInfo.CurrentCulture),
                EndDateAward = @event.EventContent?.AwardEndDate?.ToString("dd-MM-yyy", CultureInfo.CurrentCulture),
                LinkLMS = _appSetting.ResourceContent?.LmsWebsiteUrl,
                LinkLeaderBoard = @event.EventContent?.LinkLeaderBoard,
                LinkLuckyStar = @event.EventContent?.LinkLuckyStar,
            };

            var queryByUserName = _userManager.Users
                .Include(p => p.Student)
                .Where(p => p.UserName == request.Email);

            var queryByEmail = _userManager.Users
                .Include(p => p.Student)
                .Where(p => p.Email == request.Email);

            var user = await queryByUserName
                .Union(queryByEmail)
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null || user.Student == null)
            {
                await CreateUser(request, @event, user, methodResult, cancellationToken);

                await SendMail(request.Email ?? string.Empty, param, EnumSenderTemplate.CreateAccountWithEventSuccess, CreateAccountWithEventSuccess);

                await AddToGoogleSheet(request);
            }
            else
            {
                var currentDate = DateTime.UtcNow;

                var studentEvents = await _studentCompetitionEventsRepository.Queryable.Include(p => p.CompetitionEvents).Where(p => p.StudentId == user.Student.Id).ToListAsync(cancellationToken);

                if (studentEvents.Any(p => p.CompetitionEvents != null && p.CompetitionEvents.EventContent != null && p.CompetitionEvents.EventContent.StartDate.HasValue && p.CompetitionEvents.EventContent.EndDate.HasValue && p.CompetitionEvents.EventContent.StartDate.Value.Date <= currentDate.Date && p.CompetitionEvents.EventContent.EndDate.Value.Date >= currentDate.Date))
                {
                    await SendMail(request.Email ?? string.Empty, param, EnumSenderTemplate.WasInAnotherEvent, WasInAnotherEvent);
                    await UpdateEventRegistration(user.Email ?? string.Empty, @event.Id, user!.Student!.Id, methodResult, cancellationToken);
                    return methodResult;
                }

                var orderResults = await _orderService.GetOrdersByUserId(new GetOrdersByUserIdQueryModel()
                {
                    UserId = user.Id,
                    Status = EnumOrderStatus.Payment
                });
                var orders = orderResults.Content?.Result;

                if (orders != null && orders.Count > 0)
                {
                    var userOtpCode = await _mediator.Send(new SaveUserOtpCodeCommand { Id = user.Id, ExpiredTime = @event.EventContent?.EndDate?.Date }, cancellationToken);
                    param.LinkResetProgress = string.Format(CultureInfo.InvariantCulture, _appSetting.ConstantUrl?.LinkResetProgress ?? string.Empty, user.Email, userOtpCode.Result, @event.EventCode);
                    // sai tại Phuc Xo
                    var userCourseSetting = await _userCourseSettingRepository.Queryable.FirstOrDefaultAsync(x => x.CourseLevel == user.Student.CourseLevel && x.UserId == user.Id && x.Type == EnumUserCourseType.ResetAndLearnAgain, cancellationToken);
                    var userCourseSettingModel = _mapper.Map<UserCourseSettingModel>(userCourseSetting);
                    if (!userCourseSettingModel.HasRemainingAttempts())
                    {
                        await SendMail(request.Email ?? string.Empty, param, EnumSenderTemplate.WasInAnotherEvent, WasInAnotherEvent);
                    }
                    // sai tại Phuc Xo
                    else
                    {
                        await SendMail(request.Email ?? string.Empty, param, EnumSenderTemplate.LearnedOnThePlatform, LearnedOnThePlatform);
                    }
                    await UpdateEventRegistration(user.Email ?? string.Empty, @event.Id, user!.Student!.Id, methodResult, cancellationToken);
                    return methodResult;
                }

                await _studentCompetitionEventsRepository.ExecuteTransactionAsync(async () =>
                {
                    _studentCompetitionEventsRepository.Add(new StudentCompetitionEvent()
                    {
                        StudentId = user.Student.Id,
                        CompetitionEventId = @event.Id
                    });
                    await _studentCompetitionEventsRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    return methodResult;
                });

                await SendMail(request.Email ?? string.Empty, param, EnumSenderTemplate.SignUpEventSuccess, SignUpEventSuccess);
                await AddToGoogleSheet(request);

                await UpdateEventRegistration(user.Email ?? string.Empty, @event.Id, user!.Student!.Id, methodResult, cancellationToken);

                return methodResult;
            }
            return methodResult;
        }

        private async Task UpdateEventRegistration(string email, Guid competitionEventId, Guid studentId, MethodResult<bool> methodResult, CancellationToken cancellationToken)
        {
            var eventRegistration = await _eventRegistrationRepository.Queryable.FirstOrDefaultAsync(p => p.Email == email && p.CompetitionEventId == competitionEventId, cancellationToken);
            if (eventRegistration != null)
            {
                eventRegistration.StudentId = studentId;
                await _eventRegistrationRepository.ExecuteTransactionAsync(async () =>
                {
                    _eventRegistrationRepository.Update(eventRegistration);
                    await _eventRegistrationRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    return methodResult;
                });
            }
        }

        private async Task AddToGoogleSheet(RegisterStudentForEventCommandModel request)
        {
            await _systemService.RegisterStudentForEvent(request);
        }

        private async Task SendMail(string email, ParamSendMailEvent param, EnumSenderTemplate senderTemplate, string subject)
        {
            await _senderService.SendEmailAsync(new SendEmailByTemplateCommandModel
            {
                ToEmails = new List<string>() { email },
                Template = senderTemplate,
                Subject = subject,
                Params = param,
            });
        }

        private async Task<MethodResult<bool>> CreateUser(RegisterStudentForEventCommandModel request, CompetitionEvent @event, User? user, MethodResult<bool> methodResult, CancellationToken cancellationToken)
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
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Birthday = request.BirthDay,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                Student = new Student()
                {
                    Occupation = "Student",
                    CourseLevel = EnumCourseLevel.A1,
                    CreatedByParent = false,
                    School = request.School,
                    SchoolId = request.SchoolId,
                    SchoolClass = request.SchoolClass,
                    SchoolGrade = request.SchoolGrade,
                    ParentPhoneNumber = request.ParentPhoneNumber,
                    ParentEmail = request.ParentEmail,
                    SchoolFaculty = request.SchoolFaculty,
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

            await _studentCompetitionEventsRepository.ExecuteTransactionAsync(async () =>
            {
                identityStudentResult = await _userManager.CreateAsync(user, DefaultPassword);
                if (!identityStudentResult.Succeeded)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate));
                    return methodResult;
                }
                await _userManager.AddToRoleAsync(user, EnumRole.Student.ToString());

                var updateCode = await _mediator.Send(new UpdateCodeStudentCommand { UserId = user.Id, Gender = EnumGender.Male, Birthday = user.Birthday }, cancellationToken);
                if (!updateCode.IsOK)
                {
                    methodResult.AddErrorBadRequest(updateCode.ErrorMessages);
                    return methodResult;
                }

                //var createSurveyResult = await _interactionService.CreateSurvey(new CreateCustomerSurveyCommandModel
                //{
                //    Email = user.Email,
                //    UserId = user.Id,
                //    Answers = new List<CreateSurveyCommandModel>
                //    {
                //        new CreateSurveyCommandModel
                //        {
                //            Id = Guid.Parse("492D8BB9-CDBE-42E7-AA16-35A1915C3621"),
                //            Answer = new { Id = 1,Content = "Google",Image = "gmail-icon.svg"},
                //        }
                //    }
                //});

                _studentCompetitionEventsRepository.Add(new StudentCompetitionEvent()
                {
                    StudentId = user.Student.Id,
                    CompetitionEventId = @event.Id
                });
                await _studentCompetitionEventsRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                return methodResult;
            });

            await UpdateEventRegistration(user.Email ?? string.Empty, @event.Id, user.Student.Id, methodResult, cancellationToken);

            return methodResult;
        }
    }
}
