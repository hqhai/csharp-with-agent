// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Application.Services.UserServices;
using Fsel.Course.Application.Services.UserServices.Models;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Courses;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Commands.CourseCmd
{
    public class UpdateCourseCommand : UpdateCourseCommandModel, IRequest<MethodResult<CourseModel>>
    {
    }

    public class UpdateCourseTestCommandHandler : IRequestHandler<UpdateCourseCommand, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly CourseHelper _courseHelper;
        private readonly IUserService _userService;

        public UpdateCourseTestCommandHandler(ICourseRepository courseRepository
            , IMapper mapper
            , CourseHelper courseHelper
            , IUserService userService)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _courseHelper = courseHelper;
            _userService = userService;
        }

        public async Task<MethodResult<CourseModel>> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseModel> methodResult = new MethodResult<CourseModel>();

            #region Validation

            var course = _courseRepository.Queryable.Where(e => e.Id == request.Id).Include(e => e.CourseUnitMockTests).Include(e => e.CourseTeachers).FirstOrDefault();
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var method = await _courseHelper.Validate(course, request);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            if (course.Status != EnumCourseStatus.New)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseNotInNewState), nameof(course.Status), course.Status);
                return methodResult;
            }
            _mapper.Map(request, course);
            var teachers = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = course.CourseTeachers.Select(x => x.TeacherId).ToList() });
            if (!teachers.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(teachers), course.CourseTeachers.Select(x => x.TeacherId).ToList());
                return methodResult;
            }

            #endregion Validation

            await _courseRepository.ExecuteTransactionAsync(async () =>
            {
                course = _courseRepository.Update(course);

                await _courseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<CourseModel>(course);
                return methodResult;
            });

            return methodResult;
        }
    }
}
