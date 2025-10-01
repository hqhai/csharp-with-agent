// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CurriculumCmd
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DeleteStudentsFromCurriculumCommand : IRequest<MethodResult<bool>>
    {
        public Guid CurriculumId { get; set; }
        public IList<Guid>? StudentIds { get; set; }
    }

    public class DeleteStudentsFromCurriculumCommandHandler : IRequestHandler<DeleteStudentsFromCurriculumCommand, MethodResult<bool>>
    {
        private readonly ICurriculumRepository _curriculumRepository;
        private readonly ICurriculumStudentRepository _curriculumStudentRepository;
        private readonly IUserService _userService;
        private readonly ICourseRepository _courseRepository;
        private readonly ITrainingService _trainingService;
        private readonly IMediator _mediator;

        public DeleteStudentsFromCurriculumCommandHandler(ICurriculumRepository curriculumRepository, ICurriculumStudentRepository curriculumStudentRepository, IUserService userService, ICourseRepository courseRepository, ITrainingService trainingService, IMediator mediator)
        {
            _curriculumRepository = curriculumRepository;
            _curriculumStudentRepository = curriculumStudentRepository;
            _userService = userService;
            _courseRepository = courseRepository;
            _trainingService = trainingService;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(DeleteStudentsFromCurriculumCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.StudentIds == null || !request.StudentIds.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.StudentIds));
                return methodResult;
            }

            request.StudentIds = request.StudentIds.Distinct().ToList();

            var studentResults = await _userService.GetStudentsByStudentIdsAsync(request.StudentIds);
            var students = studentResults.Content?.Result;

            if (students == null || !students.Any() || students.Count != request.StudentIds.Count)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(students));
                return methodResult;
            }

            var query = await (from cs in _curriculumStudentRepository.Queryable.WhereBulkContains(request.StudentIds, p => p.StudentId)
                               join cu in _curriculumRepository.Queryable on cs.CurriculumId equals cu.Id
                               join cc in _courseRepository.Queryable on cu.CourseCloneId equals cc.Id
                               select new
                               {
                                   CurriculumStudent = cs,
                                   Curriculum = cu,
                                   Course = cc,
                               }).ToListAsync(cancellationToken);

            var currentCurriculums = query.Where(p => p.Curriculum.Id == request.CurriculumId);

            var curriculum = currentCurriculums.FirstOrDefault();

            if (curriculum == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(curriculum));
                return methodResult;
            }

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);

            var updateExpiredDateModels = new List<UpdateExpiredDateForStudentsCampusCommandModel>();

            var updateCourseIdModels = new List<DeleteClassStudentsFromCurriculumCommandModel>();

            students.ForEach(p =>
            {
                if (p.CourseId.HasValue)
                {
                    if (p.CourseId.Value == curriculum.Curriculum.CourseId)
                    {
                        var curriculumStudent = currentCurriculums.FirstOrDefault(x => x.CurriculumStudent.StudentId == p.Id);
                        if (curriculumStudent != null)
                        {
                            var otherCurriculumStudent = query.Where(x => x.CurriculumStudent.Id != curriculumStudent.CurriculumStudent.Id && x.CurriculumStudent.StudentId == curriculumStudent.CurriculumStudent.StudentId).OrderByDescending(x => x.Curriculum.EndDate).FirstOrDefault();

                            updateExpiredDateModels.Add(new UpdateExpiredDateForStudentsCampusCommandModel()
                            {
                                StudentId = curriculumStudent.CurriculumStudent.StudentId,
                                ExpiredDate = otherCurriculumStudent != null ? otherCurriculumStudent.Curriculum.EndDate : null,
                            });

                            updateCourseIdModels.Add(new DeleteClassStudentsFromCurriculumCommandModel()
                            {
                                StudentId = p.Id,
                                NewCourseId = otherCurriculumStudent != null ? otherCurriculumStudent.Curriculum.CourseId : null,
                                CourseCode = otherCurriculumStudent != null ? otherCurriculumStudent.Course.Code : null,
                                CourseLevel = otherCurriculumStudent != null ? otherCurriculumStudent.Course.CourseLevel : null,
                                IsUpdateStudent = true,
                            });
                        }
                    }
                    else
                    {
                        updateCourseIdModels.Add(new DeleteClassStudentsFromCurriculumCommandModel()
                        {
                            StudentId = p.Id,
                            IsUpdateStudent = false
                        });
                    }
                }
            });

            await _curriculumRepository.ExecuteTransactionAsync(async () =>
            {
                await _curriculumStudentRepository.DeleteListAsync(currentCurriculums.Select(p => p.CurriculumStudent).ToList());
                await _curriculumRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                var deleteClassesResults = await _trainingService.DeleteClassStudents(new DeleteClassStudentsFromCurriculumCommandModels()
                {
                    OldCourseId = curriculum.Curriculum.CourseCloneId,
                    Students = updateCourseIdModels
                });

                if (!deleteClassesResults.IsSuccessStatusCode)
                {
                    methodResult.AddError(deleteClassesResults.Error);
                    return methodResult;
                }

                var updateExpiredDateResult = await _userService.UpdateExpiredDateForStudentsCampus(new UpdateExpiredDateForStudentsCampusCommandModels() { Students = updateExpiredDateModels });

                if (!updateExpiredDateResult.IsSuccessStatusCode)
                {
                    methodResult.AddError(updateExpiredDateResult.Error);
                    return methodResult;
                }

                var userIds = students.Select(p => p.Human).Where(p => p.UserId.HasValue).Select(p => p.UserId ?? default).ToList();

                var result = await _mediator.Send(new DeleteDataLearningOfStudentsCommand()
                {
                    CourseId = curriculum.Curriculum.CourseCloneId,
                    UserIds = userIds
                });

                if (!result.IsOK)
                {
                    methodResult.AddError(updateExpiredDateResult.Error);
                    return methodResult;
                }

                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
