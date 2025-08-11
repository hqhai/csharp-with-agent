// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base.Interfaces;
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
using Courses = Fsel.Course.Domain.Entities.Course;

namespace Fsel.Course.Application.Commands.CourseCmd
{
    public class UpdateCourseCommand : UpdateCourseCommandModel, IRequest<MethodResult<CourseModel>>
    {
    }

    public class UpdateCourseTestCommandHandler : IRequestHandler<UpdateCourseCommand, MethodResult<CourseModel>>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IVersionEntityUpdater<Courses> _versionEntityUpdater;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILevelRepository _levelRepository;

        public UpdateCourseTestCommandHandler(ICourseRepository courseRepository,
                                              IMapper mapper,
                                              IUserService userService,
                                              IVersionEntityUpdater<Courses> versionEntityUpdater,
                                              ICategoryRepository categoryRepository,
                                              ILevelRepository levelRepository)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _userService = userService;
            _versionEntityUpdater = versionEntityUpdater;
            _categoryRepository = categoryRepository;
            _levelRepository = levelRepository;
        }

        public async Task<MethodResult<CourseModel>> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseModel> methodResult = new MethodResult<CourseModel>();

            var course = await _courseRepository.Queryable
                                                .Where(x => x.Id == request.Id)
                                                .Include(e => e.CourseModules)
                                                .Include(e => e.CourseTeachers)
                                                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var validate = await Validate(request, course.OriginalId, cancellationToken);
            if (!validate.IsOK)
            {
                methodResult.AddErrorBadRequest(validate.ErrorMessages);
                return methodResult;
            }

            var newVersionCourse = CourseFactory.Create(request).Build();
            if (!newVersionCourse.IsValid())
            {
                methodResult.AddErrorBadRequest(newVersionCourse.ErrorMessages);
                return methodResult;
            }

            await _versionEntityUpdater.UpdateEntity(course, newVersionCourse,
                async (_, entity) => true,
                async (oldEntity, newEntity) =>
                {
                    oldEntity.Name = newEntity.Name;
                    oldEntity.Code = newEntity.Code;
                    oldEntity.InstructionContent = newEntity.InstructionContent;
                    oldEntity.Status = newEntity.Status;
                    oldEntity.UnitCount = newEntity.UnitCount;
                    oldEntity.TestCount = newEntity.TestCount;
                    oldEntity.LevelId = newEntity.LevelId;
                    oldEntity.ProgramId = newEntity.ProgramId;

                    CourseModuleHandler(course, newVersionCourse, newEntity, oldEntity);
                    CourseTeacherHandler(course, newVersionCourse, newEntity, oldEntity);

                    await Task.Yield();
                }
            );

            methodResult.Result = _mapper.Map<CourseModel>(course);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static void CourseModuleHandler(Courses course, Courses newVersionCourse, Courses newEntity, Courses oldEntity)
        {
            var removedModules = course.CourseModules
                                               .ExceptBy(newVersionCourse.CourseModules.Select(x => $"{x.OriginalId}-{x.CourseConfigType}"), u => $"{u.OriginalId}-{u.CourseConfigType}")
                                               .ToList();
            if (removedModules.Any())
            {
                removedModules.ForEach(module =>
                {
                    course.CourseModules.Remove(module);
                });
            }

            foreach (var module in newEntity.CourseModules)
            {
                var existingModule = course.CourseModules.FirstOrDefault(m => m.OriginalId == module.OriginalId && m.CourseConfigType == module.CourseConfigType);
                if (existingModule != null)
                {
                    existingModule.Percent = module.Percent;
                    existingModule.OpenOrder = module.OpenOrder;
                    existingModule.DisplayOrder = module.DisplayOrder;
                    existingModule.DisplayNumber = module.DisplayNumber;
                    existingModule.OriginalId = module.OriginalId;
                }
                else
                {
                    oldEntity.CourseModules.Add(module);
                }
            }
        }

        private static void CourseTeacherHandler(Courses course, Courses newVersionCourse, Courses newEntity, Courses oldEntity)
        {
            var removedTeachers = course.CourseTeachers
                                               .ExceptBy(newVersionCourse.CourseTeachers.Select(x => x.TeacherId), u => u.TeacherId)
                                               .ToList();
            if (removedTeachers.Any())
            {
                removedTeachers.ForEach(teacher =>
                {
                    course.CourseTeachers.Remove(teacher);
                });
            }

            foreach (var teacher in newEntity.CourseTeachers)
            {
                var existingTeacher = course.CourseTeachers.FirstOrDefault(m => m.TeacherId == teacher.TeacherId);
                if (existingTeacher != null)
                {
                    existingTeacher.AvatarPath = teacher.AvatarPath;
                    existingTeacher.Nationality = teacher.Nationality;
                    existingTeacher.Deggree = teacher.Deggree;
                    existingTeacher.Experience = teacher.Experience;
                    existingTeacher.Strength = teacher.Strength;
                }
                else
                {
                    oldEntity.CourseTeachers.Add(teacher);
                }
            }
        }

        private async Task<VoidMethodResult> Validate(UpdateCourseCommandModel request, Guid originId, CancellationToken cancellationToken)
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

            var checkCode = await _courseRepository.Queryable.AsNoTracking()
                                                   .AnyAsync(x => x.Id != request.Id && x.OriginalId != originId && !string.IsNullOrEmpty(request.Code) && !string.IsNullOrEmpty(x.Code) && x.Code.Trim() == request.Code.Trim(), cancellationToken)
                                                   .ConfigureAwait(false);
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
