// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserCmd
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Models;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.SystemService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Users;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateCodeStudentCommand : UpdateCodeStudentCommandModel, IRequest<MethodResult<UserModel>>
    {
    }

    public class UpdateCodeStudentCommandHandler : IRequestHandler<UpdateCodeStudentCommand, MethodResult<UserModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly AuthContext _authContext;
        private readonly IStudentRepository _studentRepository;
        private readonly ISystemService _systemService;
        private readonly IMapper _mapper;

        public UpdateCodeStudentCommandHandler(UserManager<User> userManager,
            AuthContext authContext,
            IStudentRepository studentRepository,
            ISystemService systemService,
            IMapper mapper)
        {
            _userManager = userManager;
            _authContext = authContext;
            _studentRepository = studentRepository;
            _systemService = systemService;
            _mapper = mapper;
        }

        public async Task<MethodResult<UserModel>> Handle(UpdateCodeStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserModel>();
            var user = await _userManager.Users.Include(x => x!.Student).FirstOrDefaultAsync(x => x.Id == (request.UserId ?? _authContext.CurrentUserId), cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }
            if (user == null || user.Student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user.Student));
                return methodResult;
            }
            if (request.Birthday == null && request.YearBirthday == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Birthday));
                return methodResult;
            }

            if (request.Birthday == null && request.YearBirthday != null)
            {
                request.Birthday = new DateTime(request.YearBirthday.Value, 1, 1);
            }
            user.Code = await GeneratorCodeAsync(request);
            int age = DateTimeHelper.GetYearOld(request.Birthday);
            if (age <= 13)
            {
                user.Student.CourseLevel = EnumCourseLevel.A2;
            }
            else if (age >= 14)
            {
                user.Student.CourseLevel = EnumCourseLevel.B1;
            }

            user.Student.ProvinceId = request.ProvinceId;
            user.Student.DistrictId = request.DistrictId;
            user.Student.SchoolId = request.SchoolId;
            if (request.SchoolId.HasValue)
            {
                var schoolResults = await _systemService.GetSchoolByIds(new List<Guid> { request.SchoolId.Value });
                if (schoolResults.IsSuccessStatusCode)
                {
                    user.Student.School = schoolResults.Content?.Result?.FirstOrDefault()?.Name;
                }
            }
            else if (!string.IsNullOrEmpty(request.SchoolName))
            {
                var schoolResult = await _systemService.ExecuteListSchoolQueryAsync
                (
                    new BaseQueryModel
                    {
                        Filters = new List<GenericFilterModel>()
                        {
                            new GenericFilterModel
                            {
                                Property = "LocationName",
                                Operator = Common.Enums.EnumFilterOperator.Like,
                                Value = request.SchoolName
                            }
                        }
                    }
                );
                if (!schoolResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(schoolResult.Error);
                    return methodResult;
                }
                var school = schoolResult.Content?.Result?.FirstOrDefault();
                if (school != null)
                {
                    user.Student.School = school.Name;
                    user.Student.SchoolId = school.Id;
                }
                else
                {
                    user.Student.School = request.SchoolName;
                }
            }

            _mapper.Map(request, user);
            if (!user.IsValid())
            {
                methodResult.AddErrorBadRequest(user.ErrorMessages);
                return methodResult;
            }

            if (!user.Student.IsValid())
            {
                methodResult.AddErrorBadRequest(user.Student.ErrorMessages);
                return methodResult;
            }
            await _userManager.UpdateAsync(user);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }

        private async Task<string> GeneratorCodeAsync(UpdateCodeStudentCommand request)
        {
            var stt = _studentRepository.GetNextSequenceValue<int>(SqlSettings.Sequence.UserSequence);
            var currentDate = DateTime.UtcNow;
            var weekNumber = (currentDate.DayOfYear - 1) / 7 + 1;
            var lastDigitOfYear = currentDate.Year % 10;
            var lastOfBirthDay = request.Birthday!.Value.Year % 100;
            var number = request.Gender == EnumGender.Male ? 0 : request.Gender == EnumGender.Female ? 1 : 2;
            var code = $"HN_{weekNumber}{lastDigitOfYear}{number}{lastOfBirthDay}{stt:D3}";
            if (await _studentRepository.Queryable.Include(x => x.User).AnyAsync(x => x.User != null && x.User.Code == code))
            {
                code = $"HN_{weekNumber}{lastDigitOfYear}{number}{2}{lastOfBirthDay}{stt:D3}";
            }
            return code;
        }
    }
}
