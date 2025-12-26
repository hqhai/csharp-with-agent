// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.Campus
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using Fsel.Training.Application.Queries.ClassQuery;
    using Fsel.Training.Application.Services.CourseServices;
    using Fsel.Training.Application.Services.SystemServices;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.IRepositories;
    using LinqKit;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class DeleteClassStudentsFromCurriculumCommand : DeleteClassStudentsFromCurriculumCommandModels, IRequest<MethodResult<bool>>
    {
    }

    public class DeleteClassStudentsFromCurriculumCommandHandler : IRequestHandler<DeleteClassStudentsFromCurriculumCommand, MethodResult<bool>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IClassStudentRepository _classStudentRepository;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;
        private readonly IMediator _mediator;
        private readonly ICourseService _courseService;

        public DeleteClassStudentsFromCurriculumCommandHandler(IClassRepository classRepository, IClassStudentRepository classStudentRepository, ISystemService systemService, IUserService userService, IMediator mediator, ICourseService courseService)
        {
            _classRepository = classRepository;
            _classStudentRepository = classStudentRepository;
            _systemService = systemService;
            _userService = userService;
            _mediator = mediator;
            _courseService = courseService;
        }

        public async Task<MethodResult<bool>> Handle(DeleteClassStudentsFromCurriculumCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.Students == null || !request.Students.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            var studentIds = request.Students.Select(p => p.StudentId).ToList();

            var newCourseIds = request.Students.Where(p => p.NewCourseId.HasValue).Select(p => p.NewCourseId ?? default).GroupBy(p => p).ToList();

            var courseIds = newCourseIds.Select(p => p.Key).ToList();

            var @currentClass = await _classRepository.Queryable.FirstOrDefaultAsync(p => p.CourseId == request.OldCourseId, cancellationToken);

            if (@currentClass != null)
            {
                await DeleteClassStudents(studentIds, @currentClass.Id, cancellationToken);
            }

            await UpdateClassStudents(studentIds, cancellationToken);

            await CreateClasses(courseIds, request.Students, cancellationToken);

            var classEntities = await _classRepository.Queryable.Include(p => p.ClassStudents).WhereBulkContains(courseIds, p => p.CourseId).ToListAsync(cancellationToken);

            var updateCourseIdOfStudents = new List<UpdateCourseIdOfStudentsCommandModel>();

            request.Students.Where(p => p.IsUpdateStudent).ForEach(p =>
            {
                if (p.NewCourseId.HasValue)
                {
                    var eClass = classEntities.FirstOrDefault(x => x.CourseId == p.NewCourseId.Value);
                    if (eClass != null)
                    {
                        var eClassStudent = eClass.ClassStudents.FirstOrDefault(n => n.StudentId == p.StudentId);
                        if (eClassStudent != null)
                        {
                            eClassStudent.IsActive = true;
                        }
                        else
                        {
                            eClass.ClassStudents.Add(new ClassStudent()
                            {
                                StudentId = p.StudentId,
                                IsActive = true
                            });
                        }

                        updateCourseIdOfStudents.Add(new UpdateCourseIdOfStudentsCommandModel()
                        {
                            StudentId = p.StudentId,
                            ClassId = eClass.Id,
                            CourseId = p.NewCourseId,
                            CourseLevel = p.CourseLevel,
                        });
                    }
                }
                else
                {
                    updateCourseIdOfStudents.Add(new UpdateCourseIdOfStudentsCommandModel()
                    {
                        StudentId = p.StudentId
                    });
                }
            });

            await UpdateClasses(classEntities, cancellationToken);

            var updateCourseIdResult = await _userService.UpdateCourseIdForStudentsCampus(new UpdateCourseIdOfStudentsCommandModels()
            {
                Students = updateCourseIdOfStudents
            });

            return methodResult;
        }

        private async Task UpdateClasses(List<Class>? classes, CancellationToken cancellationToken)
        {
            if (classes != null && classes.Any())
            {
                _classRepository.UpdateList(classes);
                await _classStudentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task UpdateClassStudents(List<Guid> studentIds, CancellationToken cancellationToken)
        {
            var classStudents = await _classStudentRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId).ToListAsync(cancellationToken);
            if (classStudents.Any())
            {
                classStudents.ForEach(p => p.IsActive = false);
                _classStudentRepository.UpdateList(classStudents);
                await _classStudentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task DeleteClassStudents(List<Guid>? studentIds, Guid classId, CancellationToken cancellationToken)
        {
            var deleteClassStudents = await _classStudentRepository.Queryable.WhereBulkContains(studentIds, p => p.StudentId).Where(p => p.ClassId == classId).ToListAsync(cancellationToken);

            if (deleteClassStudents.Any())
            {
                await _classStudentRepository.DeleteListAsync(deleteClassStudents);
                await _classStudentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task CreateClasses(List<Guid>? courseIds, IList<DeleteClassStudentsFromCurriculumCommandModel>? students, CancellationToken cancellationToken)
        {
            var classEntities = await _classRepository.Queryable.WhereBulkContains(courseIds, p => p.CourseId).ToListAsync(cancellationToken);

            var @classes = new List<Class>();

            if (courseIds != null)
            {
                courseIds.ForEach(async x =>
                {
                    var @class = classEntities.FirstOrDefault(p => p.CourseId == x);
                    var courseInfo = students?.FirstOrDefault(p => p.NewCourseId == x);
                    if (@class == null && courseInfo != null)
                    {
                        var codeResult = await _mediator.Send(new GetNewClassCodeQuery { Code = courseInfo?.CourseCode, CourseLevel = courseInfo?.CourseLevel }, cancellationToken).ConfigureAwait(false);
                        var code = codeResult.Result;
                        classes.Add(new Class
                        {
                            Code = code,
                            Name = code,
                            CourseId = x,
                            Status = EnumClassStatus.Active,
                            PackageId = default
                        });
                    }
                });
                if (@classes.Any())
                {
                    await _classRepository.AddList(@classes);
                    await _classRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
