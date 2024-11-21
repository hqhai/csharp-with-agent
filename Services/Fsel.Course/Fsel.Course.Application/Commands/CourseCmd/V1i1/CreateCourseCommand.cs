// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Application.Services.UserServices;
using Fsel.Course.Application.Services.UserServices.Models;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Courses;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using EntityCourse = Fsel.Course.Domain.Entities.Course;

namespace Fsel.Course.Application.Commands.CourseCmd.V1i1
{
    public class CreateCourseCommand : UpdateCourseCommandModel, IRequest<MethodResult<CourseModel>>
    {
    }

    public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly CourseHelper _courseHelper;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public CreateCourseCommandHandler(ICourseRepository courseRepository
            , CourseHelper courseHelper
            , IMapper mapper
            , IUserService userService)
        {
            _courseRepository = courseRepository;
            _courseHelper = courseHelper;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<MethodResult<CourseModel>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<CourseModel>();

            #region Validation

            var course = _mapper.Map<EntityCourse>(request);
            if (!course.IsValid())
            {
                methodResult.AddErrorBadRequest(course.ErrorMessages);
                return methodResult;
            }

            var teachers = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = course.CourseTeachers.Select(x => x.TeacherId).ToList() });
            if (!teachers.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(teachers), course.CourseTeachers.Select(x => x.TeacherId).ToList());
                return methodResult;
            }
            var method = await _courseHelper.ValidateV1i1(course, request);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }

            #endregion Validation

            course.CourseUnitMockTests.ForEach(x =>
            {
                var query = course.CourseUnitMockTests.OrderBy(n => n.DisplayOrder);
                if (x.UnitId.HasValue)
                {
                    x.Number = query.Where(n => n.UnitId.HasValue).ToList().IndexOf(x) + 1;
                }
                else if (x.MockTestId.HasValue)
                {
                    x.Number = query.Where(n => n.MockTestId.HasValue).ToList().IndexOf(x) + 1;
                }
            });

            await _courseRepository.ExecuteTransactionAsync(async () =>
            {
                course = _courseRepository.Add(course);
                await _courseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CourseModel>(course);
                return methodResult;
            });

            return methodResult;
        }
    }
}
