// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Application.Services.UserServices;
using Fsel.Course.Application.Services.UserServices.Models;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Courses.V1i1;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Infrastructure.Common.CourseHelpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Commands.CourseCmd
{
    public class CreateCourseCommand : UpdateCourseCommandModel, IRequest<MethodResult<CourseModel>>
    {
    }

    public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly ILevelRepository _levelRepository;
        private readonly ICategoryRepository _categoryRepository;

        public CreateCourseCommandHandler(ICourseRepository courseRepository,
                                          IMapper mapper,
                                          IUserService userService,
                                          ILevelRepository levelRepository,
                                          ICategoryRepository categoryRepository)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _userService = userService;
            _levelRepository = levelRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<MethodResult<CourseModel>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseModel> methodResult = new MethodResult<CourseModel>();

            var validate = await Validate(request, cancellationToken);
            if (!validate.IsOK)
            {
                methodResult.AddErrorBadRequest(validate.ErrorMessages);
                return methodResult;
            }

            var course = CourseFactory.Create(request).Build(version: 0, originalId: Guid.NewGuid());
            if (!course.IsValid())
            {
                methodResult.AddErrorBadRequest(course.ErrorMessages);
                return methodResult;
            }

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

        private async Task<VoidMethodResult> Validate(UpdateCourseCommandModel request, CancellationToken cancellationToken)
        {
            VoidMethodResult methodResult = new VoidMethodResult();

            if (request.Modules == null || !request.Modules.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseModulesNotNull), nameof(request.Modules), request.Modules);
                return methodResult;
            }

            var teachers = await _userService.GetTeacherByIdsAsync(new GetTeacherByIdsQueryModel { Ids = request.CourseTeachers?.Select(x => x.TeacherId).ToList() });
            if (!teachers.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(teachers), request.CourseTeachers?.Select(x => x.TeacherId).ToList());
                return methodResult;
            }

            var checkCode = await _courseRepository.Queryable.AsNoTracking().AnyAsync(x => !string.IsNullOrEmpty(request.Code) && !string.IsNullOrEmpty(x.Code) && x.Code.Trim() == request.Code.Trim(), cancellationToken).ConfigureAwait(false);
            if (checkCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(checkCode), request.Code);
                return methodResult;
            }

            var checkLevel = await _levelRepository.Queryable.AsNoTracking().AnyAsync(x => x.Id == request.LevelId, cancellationToken);
            if (!checkLevel)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(checkLevel), request.LevelId);
                return methodResult;
            }

            var checkProgram = await _categoryRepository.Queryable.AsNoTracking().AnyAsync(x => x.Id == request.ProgramId, cancellationToken);
            if (!checkProgram)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(checkProgram), request.ProgramId);
                return methodResult;
            }

            return methodResult;
        }
    }
}
