// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.CampusCmd
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class AddCourseIdForStudentsCampusCommand : AddCourseIdForStudentsCampusCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class AddCourseIdForStudentsCampusCommandHandler : IRequestHandler<AddCourseIdForStudentsCampusCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;

        public AddCourseIdForStudentsCampusCommandHandler(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<bool>> Handle(AddCourseIdForStudentsCampusCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            var students = await _studentRepository.Queryable.WhereBulkContains(request.StudentIds, p => p.Id).ToListAsync(cancellationToken);
            students.ForEach(p =>
            {
                p.CourseId = request.CourseId;
                p.CourseLevel = request.CourseLevel;
                p.ClassId = request.ClassId;
                p.ProgramId = request.ProgramId;
                p.LevelId = request.LevelId;
                p.SubjectId = request.SubjectId;
            });

            await _studentRepository.ExecuteTransactionAsync(async () =>
            {
                await _studentRepository.BulkMergeAsync(students);
                await _studentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
