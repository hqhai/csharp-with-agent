// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.LandingPages
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Commands.UserCmd;
    using Fsel.Identity.Application.Services;
    using Fsel.Identity.Application.Services.InteractionService;
    using Fsel.Identity.Application.Services.InteractionService.Models;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class RegisterStudentForEventCommand : RegisterStudentForEventCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class RegisterStudentForEventCommandHandler : IRequestHandler<RegisterStudentForEventCommand, MethodResult<bool>>
    {
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IStudentRankingEventsRepository _studentRankingEventsRepository;
        private readonly UserManager<User> _userManager;
        private readonly IPlatformRepository _platformRepository;
        private const string DefaultPassword = "Fsel@2024";
        private readonly ISystemService _systemService;
        private readonly ISenderService _senderService;
        private readonly IOrderService _orderService;
        private readonly IMediator _mediator;
        private readonly IInteractionService _interactionService;

        public RegisterStudentForEventCommandHandler(ICompetitionEventsRepository competitionEventsRepository, IStudentRankingEventsRepository studentRankingEventsRepository, UserManager<User> userManager, IPlatformRepository platformRepository, ISystemService systemService, ISenderService senderService, IOrderService orderService, MediatR.IMediator mediator, IInteractionService interactionService)
        {
            _competitionEventsRepository = competitionEventsRepository;
            _studentRankingEventsRepository = studentRankingEventsRepository;
            _userManager = userManager;
            _platformRepository = platformRepository;
            _systemService = systemService;
            _senderService = senderService;
            _orderService = orderService;
            _mediator = mediator;
            _interactionService = interactionService;
        }

        public async Task<MethodResult<bool>> Handle(RegisterStudentForEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var @event = await _competitionEventsRepository.Queryable.FirstOrDefaultAsync(p => p.EventCode.ToLower() == request.EventCode.ToLower(), cancellationToken);
            if (@event == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var user = await _userManager.Users.Include(p => p.Human).ThenInclude(p => p.Student).FirstOrDefaultAsync(p => p.UserName.ToLower() == request.Email.ToLower() || p.Email.ToLower() == request.Email.ToLower(), cancellationToken);
            if (user == null)
            {
                await CreateUser(request, @event, user, methodResult, cancellationToken);

                await _senderService.SendEmailAsync(new SendEmailByTemplateCommandModel
                {
                    ToEmails = new List<string>() { request.Email ?? string.Empty },
                    Template = EnumSenderTemplate.SignUpEventSuccess,
                    Subject = "",
                    Params = request,
                });

                await _systemService.RegisterStudentForEvent(new RegisterStudentForEventCommandModel()
                {
                    EventCode = @event.EventCode,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    BirthDay = request.BirthDay,
                    Province = request.Province,
                    District = request.District,
                    School = request.School,
                    SchoolGrade = request.SchoolGrade,
                    SchoolClass = request.SchoolClass,
                    SchoolStudentCode = request.SchoolStudentCode,
                });
            }
            else
            {
                var currentDate = DateTime.UtcNow;

                var studentEvents = await _studentRankingEventsRepository.Queryable.Include(p => p.CompetitionEvents).Where(p => p.StudentId == user.Human.Student.Id).ToListAsync(cancellationToken);

                if (studentEvents.Any(p => p.CompetitionEvents != null && p.CompetitionEvents.EventContent != null && p.CompetitionEvents.EventContent.StartDate.HasValue && p.CompetitionEvents.EventContent.EndDate.HasValue && p.CompetitionEvents.EventContent.StartDate.Value.Date <= currentDate.Date && p.CompetitionEvents.EventContent.EndDate.Value.Date >= currentDate.Date))
                {
                    await _senderService.SendEmailAsync(new SendEmailByTemplateCommandModel
                    {
                        ToEmails = new List<string>() { request.Email ?? string.Empty },
                        Template = EnumSenderTemplate.SignUpEventSuccess,
                        Subject = "",
                        Params = request,
                    });
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
                    await _senderService.SendEmailAsync(new SendEmailByTemplateCommandModel
                    {
                        ToEmails = new List<string>() { request.Email ?? string.Empty },
                        Template = EnumSenderTemplate.SignUpEventSuccess,
                        Subject = "",
                        Params = request,
                    });
                    return methodResult;
                }
            }
            return methodResult;
        }

        private async Task<MethodResult<bool>> CreateUser(RegisterStudentForEventCommandModel request, CompetitionEvents @event, User? user, MethodResult<bool> methodResult, CancellationToken cancellationToken)
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
                FullName = request.FirstName + " " + request.LastName,
                Email = request.Email,
                EmailConfirmed = true,
                PhoneNumber = request.PhoneNumber,
                PhoneNumberConfirmed = false,
                Human = new Human()
                {
                    FullName = request.FirstName + " " + request.LastName,
                    Birthday = request.BirthDay,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    Student = new Student()
                    {
                        Occupation = "Student",
                        CourseLevel = EnumCourseLevel.A1,
                        CreatedByParent = false
                    }
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

            await _studentRankingEventsRepository.ExecuteTransactionAsync(async () =>
            {
                identityStudentResult = await _userManager.CreateAsync(user, DefaultPassword);
                if (!identityStudentResult.Succeeded)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.UserFailToCreate));
                    return methodResult;
                }
                await _userManager.AddToRoleAsync(user, EnumRole.Student.ToString());

                var updateCode = await _mediator.Send(new UpdateCodeStudentCommand { UserId = user.Id, Gender = EnumGender.Male, Birthday = user.Human.Birthday }, cancellationToken);
                if (!updateCode.IsOK)
                {
                    methodResult.AddErrorBadRequest(updateCode.ErrorMessages);
                    return methodResult;
                }

                var createSurveyResult = await _interactionService.CreateSurvey(new CreateCustomerSurveyCommandModel
                {
                    Email = user.Email,
                    UserId = user.Id,
                    Answers = new List<CreateSurveyCommandModel>
                {
                    new CreateSurveyCommandModel
                    {
                        Id = Guid.Parse("492D8BB9-CDBE-42E7-AA16-35A1915C3621"),
                        Answer = new { Id = 1,Content = "Google",Image = "gmail-icon.svg"},
                    }
                }
                });

                _studentRankingEventsRepository.Add(new StudentRankingEvents()
                {
                    StudentId = user.Human.Student.Id,
                    CompetitionRankingId = @event.Id
                });
                await _studentRankingEventsRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                return methodResult;
            });
            return methodResult;
        }
    }
}
