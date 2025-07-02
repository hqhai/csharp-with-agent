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
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Commands.UserCmd;
    using Fsel.Identity.Application.Queries.CompetitionEventsQuery;
    using Fsel.Identity.Application.Services;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Application.Services.SenderService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.GoogleSheets;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
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
        private readonly UserManager<User> _userManager;
        private readonly IPlatformRepository _platformRepository;
        private readonly IMediator _mediator;
        private const string DefaultPassword = "Fsel@2024";
        private readonly ISystemService _systemService;

        public FormRegisterStudentForEventCommandHandler(IEventRegistrationRepository eventRegistrationRepository, ICompetitionEventsRepository competitionEventsRepository, IMapper mapper, ISenderService senderService, AppSetting appSetting, UserManager<User> userManager,
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

            var competitionEvent = await _competitionEventsRepository.Queryable.FirstOrDefaultAsync(p => p.EventCode == request.EventCode, cancellationToken);

            if (competitionEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            if (!string.IsNullOrEmpty(request.Email) && !request.Email.IsValidEmail())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }
            if (!string.IsNullOrEmpty(request.PhoneNumber) && !request.PhoneNumber.IsValidPhoneNumber())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }
            if (!string.IsNullOrEmpty(request.ParentEmail) && !request.ParentEmail.IsValidEmail())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }
            if (!string.IsNullOrEmpty(request.ParentPhoneNumber) && !request.ParentPhoneNumber.IsValidPhoneNumber())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }
            if (!string.IsNullOrEmpty(request.TeacherPhoneNumber) && !request.TeacherPhoneNumber.IsValidPhoneNumber())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat));
                return methodResult;
            }

            CompetitionEvent? @event = null;

            if (competitionEvent.EventContent != null && competitionEvent.EventContent.IsParentEvent.HasValue && competitionEvent.EventContent.IsParentEvent.Value && request.DistrictId.HasValue)
            {
                var childEvents = await _mediator.Send(new GetChildEventsByParentIdQuery() { Id = competitionEvent.Id }, cancellationToken);
                var childEvent = childEvents.Result?.FirstOrDefault(p => p.LocationId == request.DistrictId);
                if (childEvent != null)
                {
                    @event = childEvent;
                }
                else
                {
                    @event = competitionEvent;
                }
            }
            else
            {
                @event = competitionEvent;
            }

            if (competitionEvent.EventContent != null && competitionEvent.EventContent.Actions != null && competitionEvent.EventContent.Actions.Any(p => p == EnumSchoolEventRuleAction.RegisterAndCreateUser))
            {
                await RegisterAndCreateUser(request, competitionEvent, @event, methodResult, cancellationToken);
            }
            else
            {
                await Register(request, @event, null, true, methodResult, cancellationToken);
            }

            return methodResult;
        }

        private async Task<MethodResult<bool>> RegisterAndCreateUser(RegisterStudentForEventCommandModel request, CompetitionEvent? parentEvent, CompetitionEvent competitionEvent, MethodResult<bool> methodResult, CancellationToken cancellationToken)
        {
            var queryByEmail = _userManager.Users.Where(x => x.Email == request.Email);
            var queryByUserName = _userManager.Users.Where(x => x.UserName == request.Email);

            var user = await queryByEmail.Union(queryByUserName).FirstOrDefaultAsync(cancellationToken);

            if (user != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Email), request.Email);
                return methodResult;
            }

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
                EmailConfirmed = true,
                PhoneNumber = request.PhoneNumber,
                Birthday = request.BirthDay,
                PhoneNumberConfirmed = false,
                Student = new Student()
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

            var password = request.Password ?? DefaultPassword;

            var passwordValidator = new Microsoft.AspNetCore.Identity.PasswordValidator<User>();
            var validPassword = await passwordValidator.ValidateAsync(_userManager, user, password);
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

            identityStudentResult = await _userManager.CreateAsync(user, password);
            if (!identityStudentResult.Succeeded)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate));
                return methodResult;
            }
            await _userManager.AddToRoleAsync(user, EnumRole.Student.ToString());

            var updateCode = await _mediator.Send(new UpdateCodeStudentCommand { UserId = user.Id, Gender = EnumGender.Male, Birthday = user.Birthday, SchoolId = request.SchoolId, ProvinceId = request.ProvinceId, DistrictId = request.DistrictId }, cancellationToken);
            if (!updateCode.IsOK)
            {
                methodResult.AddErrorBadRequest(updateCode.ErrorMessages);
                return methodResult;
            }

            await Register(request, competitionEvent, user.Student.Id, false, methodResult, cancellationToken);

            await _studentCompetitionEventsRepository.ExecuteTransactionAsync(async () =>
            {
                var studentCompetitionEvent = _studentCompetitionEventsRepository.Add(new StudentCompetitionEvent { StudentId = user.Student.Id, CompetitionEventId = competitionEvent.Id });
                await _studentCompetitionEventsRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            ActionConfig? template = null;
            if (parentEvent != null)
            {
                template = parentEvent.EventContent?.ActionConfigs?.FirstOrDefault(p => p.MailRegister.HasValue);
            }
            else
            {
                template = competitionEvent.EventContent?.ActionConfigs?.FirstOrDefault(p => p.MailRegister.HasValue);
            }

            if (template != null && template.MailRegister.HasValue && !string.IsNullOrEmpty(template.SubjectMailRegister))
            {
                await SendMailInfoUser(request, password, template.MailRegister.Value, template.SubjectMailRegister);
            }
            return methodResult;
        }

        private async Task<MethodResult<bool>> Register(RegisterStudentForEventCommandModel request, CompetitionEvent competitionEvent, Guid? studentId, bool isSendMail, MethodResult<bool> methodResult, CancellationToken cancellationToken)
        {
            var eventRegistration = await _eventRegistrationRepository.Queryable.FirstOrDefaultAsync(p => p.Email == request.Email && p.CompetitionEventId == competitionEvent.Id, cancellationToken);

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

            if (!eventRegistration.IsValid())
            {
                methodResult.AddError(eventRegistration.ErrorMessages);
                return methodResult;
            }

            var template = competitionEvent.EventContent?.ActionConfigs?.FirstOrDefault(p => p.MailRegister.HasValue);

            await _eventRegistrationRepository.ExecuteTransactionAsync(async () =>
            {
                await _eventRegistrationRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                if (isSendMail)
                {
                    await SendMailRegisterEvent(request, competitionEvent, EnumSenderTemplate.MailRegisterEvent, Subject, CultureInfo.InvariantCulture).ConfigureAwait(false);
                }

                return methodResult;
            });
            return methodResult;
        }

        private async Task SendMailInfoUser(RegisterStudentForEventCommandModel request, string password, EnumSenderTemplate senderTemplate, string subject)
        {
            await _senderService.SendEmailAsync(new SendEmailByTemplateCommandModel
            {
                ToEmails = new List<string>() { request.Email ?? string.Empty },
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

        private async Task SendMailRegisterEvent(RegisterStudentForEventCommandModel request, CompetitionEvent competitionEvent, EnumSenderTemplate senderTemplate, string subject, CultureInfo cultureInfo)
        {
            var param = new ParamSendMailEvent
            {
                FullName = request.FirstName + " " + request.LastName,
                LinkLMS = _appSetting.ResourceContent?.LmsWebsiteUrl,
                StartDateEvent = competitionEvent.EventContent?.StartDate?.ToString("dd/MM", cultureInfo),
                EndDateEvent = competitionEvent.EventContent?.EndDate?.ToString("dd/MM/yyyy", cultureInfo),
                StartDateAward = competitionEvent.EventContent?.AwardStartDate?.ToString("dd/MM", cultureInfo),
                EndDateAward = competitionEvent.EventContent?.AwardEndDate?.ToString("dd/MM/yyyy", cultureInfo),
                Date = competitionEvent.EventContent?.StartDate?.ToString("dd/MM/yyyy", cultureInfo),
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                BirthDay = request.BirthDay.ToString("dd/MM/yyyy", cultureInfo),
                School = request.School,
                SchoolStudentCode = request.SchoolStudentCode,
                SchoolGrade = request.SchoolGrade,
                SchoolClass = request.SchoolClass,
                LinkLeaderBoard = competitionEvent.EventContent?.LinkLeaderBoard
            };

            await _senderService.SendEmailAsync(new SendEmailByTemplateCommandModel
            {
                ToEmails = new List<string>() { request.Email ?? string.Empty },
                Template = senderTemplate,
                Subject = subject,
                Params = param,
            });
        }

        private async Task SendMail(RegisterStudentForEventCommandModel request, CompetitionEvent competitionEvent, EnumSenderTemplate senderTemplate, string subject, CultureInfo cultureInfo)
        {
            var param = new ParamSendMailEvent
            {
                FullName = request.FirstName + " " + request.LastName,
                LinkLMS = _appSetting.ResourceContent?.LmsWebsiteUrl,
                StartDateEvent = competitionEvent.EventContent?.StartDate?.ToString("dd/MM", cultureInfo),
                EndDateEvent = competitionEvent.EventContent?.EndDate?.ToString("dd/MM/yyyy", cultureInfo),
                StartDateAward = competitionEvent.EventContent?.AwardStartDate?.ToString("dd/MM", cultureInfo),
                EndDateAward = competitionEvent.EventContent?.AwardEndDate?.ToString("dd/MM/yyyy", cultureInfo),
                Date = competitionEvent.EventContent?.StartDate?.ToString("dd/MM/yyyy", cultureInfo),
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                BirthDay = request.BirthDay.ToString("dd/MM/yyyy", cultureInfo),
                School = request.School,
                SchoolStudentCode = request.SchoolStudentCode,
                SchoolGrade = request.SchoolGrade,
                SchoolClass = request.SchoolClass,
                LinkLeaderBoard = competitionEvent.EventContent?.LinkLeaderBoard
            };

            await _senderService.SendEmailAsync(new SendEmailByTemplateCommandModel
            {
                ToEmails = new List<string>() { request.Email ?? string.Empty },
                Template = senderTemplate,
                Subject = subject,
                Params = param,
            });
        }
    }
}
