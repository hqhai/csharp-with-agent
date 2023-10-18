// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.UserCmd
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Users;
    using Fsel.Identity.Domain.Models.EntityModels;
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
        private readonly IMapper _mapper;

        public UpdateCodeStudentCommandHandler(UserManager<User> userManager, AuthContext authContext, IStudentRepository studentRepository,
            IMapper mapper)
        {
            _userManager = userManager;
            _authContext = authContext;
            _studentRepository = studentRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<UserModel>> Handle(UpdateCodeStudentCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserModel>();
            var user = await _userManager.Users.Include(x => x.Human).ThenInclude(x => x!.Student).FirstOrDefaultAsync(x => x.Id == (request.UserId ?? _authContext.CurrentUserId), cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
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

            var stt = await _studentRepository.Queryable.CountAsync(cancellationToken);
            var currentDate = DateTime.UtcNow;
            var weekNumber = (currentDate.DayOfYear - 1) / 7 + 1;
            var lastDigitOfYear = currentDate.Year % 10;
            var lastOfBirthDay = request.Birthday!.Value.Year % 100;
            var number = request.Gender == EnumGender.Male ? 0 : request.Gender == EnumGender.Female ? 1 : 2;
            var code = $"HN_{weekNumber}{lastDigitOfYear}{number}{lastOfBirthDay}{stt:000}";
            if (await _studentRepository.Queryable.Include(x => x.Human).AnyAsync(x => x!.Human!.Code == code, cancellationToken))
            {
                code = $"HN_{weekNumber}{lastDigitOfYear}{number}{2}{lastOfBirthDay}{stt:000}";
            }
            user.Human!.Code = code;
            int age = DateTimeHelper.GetYearOld(request.Birthday);
            if (age <= 13)
            {
                user.Human!.Student!.CourseLevel = EnumCourseLevel.A2;
            }
            else if (age >= 14)
            {
                user.Human!.Student!.CourseLevel = EnumCourseLevel.B1;
            }

            _mapper.Map(request, user.Human);
            await _userManager.UpdateAsync(user);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }
    }
}
