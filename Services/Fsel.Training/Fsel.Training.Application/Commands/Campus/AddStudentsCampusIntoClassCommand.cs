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
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class AddStudentsCampusIntoClassCommand : AddStudentsCampusIntoClassCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class AddStudentsCampusIntoClassCommandHandler : IRequestHandler<AddStudentsCampusIntoClassCommand, MethodResult<bool>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IClassStudentRepository _classStudentRepository;
        private readonly ISystemService _systemService;
        private readonly IUserService _userService;
        private readonly IMediator _mediator;
        private readonly ICourseService _courseService;

        public AddStudentsCampusIntoClassCommandHandler(IClassRepository classRepository, IClassStudentRepository classStudentRepository, ISystemService systemService, IUserService userService, IMediator mediator, ICourseService courseService)
        {
            _classRepository = classRepository;
            _classStudentRepository = classStudentRepository;
            _systemService = systemService;
            _userService = userService;
            _mediator = mediator;
            _courseService = courseService;
        }

        public async Task<MethodResult<bool>> Handle(AddStudentsCampusIntoClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var courseResult = await _courseService.GetCourseByIdAsync(request.CourseId);
            if (!courseResult.IsSuccessStatusCode)
            {
                methodResult.AddError(courseResult.Error);
                return methodResult;
            }
            var course = courseResult.Content?.Result;
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            var @class = await _classRepository.Queryable.FirstOrDefaultAsync(p => p.CourseId == request.CourseId, cancellationToken);

            var classStudentEntities = await _classStudentRepository.Queryable.WhereBulkContains(request.StudentIds, p => p.StudentId).ToListAsync(cancellationToken);

            var classStudents = new List<ClassStudent>();

            var updateExpiredDateForStudentsModels = new List<UpdateExpiredDateForStudentsCampusCommandModel>();

            if (@class == null)
            {
                var codeResult = await _mediator.Send(new GetNewClassCodeQuery { Code = course.Code, CourseLevel = course.CourseLevel }, cancellationToken).ConfigureAwait(false);
                var code = codeResult.Result;

                request.StudentIds.ForEach(p =>
                {
                    if (!classStudentEntities.Any(x => x.StudentId == p))
                    {
                        classStudents.Add(new ClassStudent() { StudentId = p, IsActive = true });
                        updateExpiredDateForStudentsModels.Add(new UpdateExpiredDateForStudentsCampusCommandModel()
                        {
                            StudentId = p,
                            ExpiredDate = request.ExpiredDate,
                        });
                    }
                });

                await _classStudentRepository.ExecuteTransactionAsync(async () =>
                {
                    var newClass = new Class
                    {
                        Code = code,
                        Name = code,
                        CourseId = request.CourseId,
                        Status = EnumClassStatus.Active,
                        PackageId = default,
                        ClassStudents = classStudents
                    };
                    @class = _classRepository.Add(newClass);
                    await _classRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    methodResult.StatusCode = StatusCodes.Status200OK;
                    methodResult.Result = true;
                    return methodResult;
                });
            }
            else
            {
                request.StudentIds.ForEach(p =>
                {
                    if (!classStudentEntities.Any(x => x.StudentId == p))
                    {
                        classStudents.Add(new ClassStudent() { StudentId = p, ClassId = @class.Id, IsActive = true });
                        updateExpiredDateForStudentsModels.Add(new UpdateExpiredDateForStudentsCampusCommandModel()
                        {
                            StudentId = p,
                            ExpiredDate = request.ExpiredDate,
                        });
                    }
                });

                await _classStudentRepository.ExecuteTransactionAsync(async () =>
                {
                    await _classStudentRepository.BulkMergeAsync(classStudents);
                    await _classStudentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    methodResult.StatusCode = StatusCodes.Status200OK;
                    methodResult.Result = true;
                    return methodResult;
                });
            }

            if (@class == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(@class));
                return methodResult;
            }

            var updateStudentResult = await _userService.AddCourseIdForStudentsCampus(new AddCourseIdForStudentsCampusCommandModel()
            {
                ClassId = @class.Id,
                CourseId = request.CourseId,
                CourseLevel = course.CourseLevel,
                ProgramId = request.ProgramId,
                SubjectId = request.SubjectId,
                LevelId = request.LevelId,
                StudentIds = classStudents.Select(p => p.StudentId).ToList()
            });
            if (!updateStudentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(updateStudentResult.Error);
                return methodResult;
            }

            var updateExpiredDateResult = await _userService.UpdateExpiredDateForStudentsCampus(new UpdateExpiredDateForStudentsCampusCommandModels()
            {
                Students = updateExpiredDateForStudentsModels
            });

            if (!updateExpiredDateResult.IsSuccessStatusCode)
            {
                methodResult.AddError(updateExpiredDateResult.Error);
                return methodResult;
            }

            return methodResult;
        }
    }
}
