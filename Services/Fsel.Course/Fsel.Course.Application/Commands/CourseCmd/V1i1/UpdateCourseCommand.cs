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
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Commands.CourseCmd.V1i1
{
    public class UpdateCourseCommand : UpdateCourseCommandModel, IRequest<MethodResult<CourseModel>>
    {
    }

    public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly CourseHelper _courseHelper;
        private readonly IUserService _userService;

        public UpdateCourseCommandHandler(ICourseRepository courseRepository
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
            var methodResult = new MethodResult<CourseModel>();
            var course = await _courseRepository.Queryable.Include(e => e.CourseUnitMockTests)
                                            .Include(e => e.CourseTeachers)
                                            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course), request.Id);
                return methodResult;
            }
            _mapper.Map(request, course);
            if (!course.IsValid())
            {
                methodResult.AddErrorBadRequest(course.ErrorMessages);
                return methodResult;
            }

            #region Validation

            var method = await _courseHelper.ValidateV1i1(course, request);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }

            var teachers = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = course.CourseTeachers.Select(x => x.TeacherId).ToList() });
            if (!teachers.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(teachers), course.CourseTeachers.Select(x => x.TeacherId).ToList());
                return methodResult;
            }

            #endregion Validation

            course.CourseUnitMockTests.ForEach(x =>
            {
                var query = course.CourseUnitMockTests.OrderBy(n => n.DisplayOrder);
                if (x.UnitId != null)
                {
                    x.Number = query.Where(n => n.UnitId != null).ToList().IndexOf(x) + 1;
                }
                else if (x.MockTestId != null)
                {
                    x.Number = query.Where(n => n.MockTestId != null).ToList().IndexOf(x) + 1;
                }
            });

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
