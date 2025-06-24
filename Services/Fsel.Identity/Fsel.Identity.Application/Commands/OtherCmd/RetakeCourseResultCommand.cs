// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.OtherCmd
{
    using System.Linq.Dynamic.Core;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Commands.AuthCmd;
    using Fsel.Identity.Application.Commands.UserOtpCodeCmd;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.LmsCourseService.CommandModels;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.OrderService.CommandModels;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Shared.Models.ShareModels.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Net.Http.Headers;

    public class RetakeCourseResultCommand : IRequest<MethodResult<string>>
    {
        public string? Email { get; set; }
        public string? OtpCode { get; set; }
        public string? EventCode { get; set; }
    }

    public class RetakeCourseResultCommandHandler : IRequestHandler<RetakeCourseResultCommand, MethodResult<string>>
    {
        private readonly UserManager<User> _userManager;
        private readonly AppSetting _appSetting;
        private readonly ICompetitionEventsRepository _competitionEventsRepository;
        private readonly IOrderService _orderService;
        private readonly IUserCourseSettingRepository _userCourseSettingRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStudentCompetitionEventsRepository _studentCompetitionEventsRepository;
        private readonly IEventRegistrationRepository _eventRegistrationRepository;
        private readonly ISystemService _systemService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public RetakeCourseResultCommandHandler(UserManager<User> userManager, AppSetting appSetting, ICompetitionEventsRepository competitionEventsRepository, IOrderService orderService, IUserCourseSettingRepository userCourseSettingRepository, IStudentRepository studentRepository, ILmsCourseService lmsCourseService, MediatR.IMediator mediator, IHttpContextAccessor httpContextAccessor, IStudentCompetitionEventsRepository studentCompetitionEventsRepository, ISystemService systemService, IEventRegistrationRepository eventRegistrationRepository, IMapper mapper)
        {
            _userManager = userManager;
            _appSetting = appSetting;
            _competitionEventsRepository = competitionEventsRepository;
            _orderService = orderService;
            _userCourseSettingRepository = userCourseSettingRepository;
            _studentRepository = studentRepository;
            _lmsCourseService = lmsCourseService;
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
            _studentCompetitionEventsRepository = studentCompetitionEventsRepository;
            _systemService = systemService;
            _eventRegistrationRepository = eventRegistrationRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<string>> Handle(RetakeCourseResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<string> methodResult = new MethodResult<string>();
            methodResult.Result = _appSetting.ResourceContent?.LmsWebsiteUrl;
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

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }
            var method = await _mediator.Send(new ConfirmOtpCommand { Otp = request.OtpCode, Email = request.Email }, cancellationToken);
            if (!method.IsOK || method.Result == null)
            {
                methodResult.AddError(method.ErrorMessages);
                return methodResult;
            }

            var tokenResult = await _mediator.Send(new GenerateTokenCommand { Id = user.Id }, cancellationToken);
            if (tokenResult.Result?.AccessToken != null && _httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Authorization] = "Bearer " + tokenResult.Result?.AccessToken;
            }
            var student = await _studentRepository.Queryable.Include(x => x.User).FirstOrDefaultAsync(x => x.UserId == user.Id, cancellationToken);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            if (!student.CourseLevel.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student.CourseLevel));
                return methodResult;
            }

            var userCourseSetting = await _userCourseSettingRepository.Queryable.FirstOrDefaultAsync(x => x.CourseLevel == student.CourseLevel && x.UserId == user.Id && x.Type == EnumUserCourseType.ResetAndLearnAgain, cancellationToken);
            var userCourseSettingModel = _mapper.Map<UserCourseSettingModel>(userCourseSetting);
            if (!userCourseSettingModel.HasRemainingAttempts())
            {
                methodResult.AddErrorBadRequest(nameof(EnumUserCourseSettingErrorCode.CurrentLevelHasNoRetakes), nameof(userCourseSetting));
                return methodResult;
            }

            #region đoạn này sai tại Phúc Xo p.StudentCompetitionEvents.Where(x => x.StudentId == student.Id)

            var competitionEvent = await _competitionEventsRepository.Queryable.Include(p => p.StudentCompetitionEvents.Where(x => x.StudentId == student.Id)).FirstOrDefaultAsync(x => x.EventCode == request.EventCode, cancellationToken);

            #endregion đoạn này sai tại Phúc Xo p.StudentCompetitionEvents.Where(x => x.StudentId == student.Id)

            if (competitionEvent == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(competitionEvent));
                return methodResult;
            }

            if (competitionEvent.StudentCompetitionEvents.Any(p => p.StudentId == student.Id))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(competitionEvent));
                return methodResult;
            }

            if (competitionEvent.EventContent == null || !competitionEvent.EventContent.EndDate.HasValue || competitionEvent.EventContent.EndDate.Value.Date < DateTime.UtcNow.Date)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(competitionEvent));
                return methodResult;
            }

            var courseResult = await _lmsCourseService.RetakeCourseAsync(new RetakeCourseResultCommandModel { CourseLevel = student.CourseLevel.Value });
            if (!courseResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallCourseServiceError));
                return methodResult;
            }

            await _studentCompetitionEventsRepository.ExecuteTransactionAsync(async () =>
            {
                _studentCompetitionEventsRepository.Add(new StudentCompetitionEvent()
                {
                    StudentId = student.Id,
                    CompetitionEventId = competitionEvent.Id
                });
                await _studentCompetitionEventsRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                return methodResult;
            });

            await _orderService.CreateOrderForUserLeaderBoard(new CreateOrderForUserFromLeaderBoardCommandModel()
            {
                UserId = user.Id,
                Month = competitionEvent.EventContent?.PaymentMonth,
                ExpiredDate = competitionEvent.EventContent?.PaymentDate,
                FullName = student.User?.FullName,
                Email = student.User?.Email,
                PaymentMethod = EnumPaymentMethodStatus.BankTransfer,
                PackageId = default,
                EventId = default
            });

            var eventRegistrations = await _eventRegistrationRepository.Queryable.FirstOrDefaultAsync(p => p.Email == request.Email && p.CompetitionEventId == competitionEvent.Id, cancellationToken);
            if (eventRegistrations != null)
            {
                var model = _mapper.Map<RegisterStudentForEventCommandModel>(eventRegistrations);
                model.EventCode = competitionEvent.EventCode;
                await AddToGoogleSheet(model);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task AddToGoogleSheet(RegisterStudentForEventCommandModel request)
        {
            await _systemService.RegisterStudentForEvent(request);
        }
    }
}
