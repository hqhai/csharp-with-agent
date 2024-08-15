// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Commands.OtherCmd
{
    using System.Linq.Dynamic.Core;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Commands.AuthCmd;
    using Fsel.Identity.Application.Commands.StudentRankingEvents;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Application.Services.LmsCourseService.CommandModels;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Net.Http.Headers;

    public class ToolRetakeCourseResultCommand : IRequest<MethodResult<bool>>
    {
        public string? Email { get; set; }
        public string? EventCode { get; set; }
    }

    public class ToolRetakeCourseResultCommandHandler : IRequestHandler<ToolRetakeCourseResultCommand, MethodResult<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserCourseSettingRepository _userCourseSettingRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ILmsCourseService _lmsCourseService;
        private readonly MediatR.IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ToolRetakeCourseResultCommandHandler(UserManager<User> userManager, IUserCourseSettingRepository userCourseSettingRepository, IStudentRepository studentRepository, ILmsCourseService lmsCourseService, MediatR.IMediator mediator, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _userCourseSettingRepository = userCourseSettingRepository;
            _studentRepository = studentRepository;
            _lmsCourseService = lmsCourseService;
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MethodResult<bool>> Handle(ToolRetakeCourseResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
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

            var tokenResult = await _mediator.Send(new GenerateTokenCommand { Id = user.Id }, cancellationToken);
            if (tokenResult.Result?.AccessToken != null && _httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Authorization] = "Bearer " + tokenResult.Result?.AccessToken;
            }
            var student = await _studentRepository.Queryable.Include(x => x.Human).FirstOrDefaultAsync(x => x.Human != null && x.Human.UserId == user.Id, cancellationToken);
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
            var checkLuckySpinResult = await _mediator.Send(new CheckStudentLuckySpinCmd(), cancellationToken);
            if (!checkLuckySpinResult.IsOK)
            {
                methodResult.AddErrorBadRequest(checkLuckySpinResult.ErrorMessages);
                return methodResult;
            }

            if (checkLuckySpinResult.Result != null && checkLuckySpinResult.Result.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(checkLuckySpinResult));
                return methodResult;
            }
            var userCourseSetting = _userCourseSettingRepository.Queryable.FirstOrDefault(x => x.CourseLevel == student.CourseLevel && x.UserId == user.Id && x.Type == EnumUserCourseType.ResetAndLearnAgain);
            if (userCourseSetting != null && userCourseSetting.Value <= 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUserCourseSettingErrorCode.CurrentLevelHasNoRetakes), nameof(checkLuckySpinResult));
                return methodResult;
            }
            var courseResult = await _lmsCourseService.RetakeCourseAsync(new RetakeCourseResultCommand { CourseLevel = student.CourseLevel.Value });
            if (!courseResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallCourseServiceError));
                return methodResult;
            }
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
