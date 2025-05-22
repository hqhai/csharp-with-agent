// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStudentByCourseLevelCommand : IRequest<MethodResult<bool>>
    {
        public Guid Id { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseLevel? BaseCourseLevel { get; set; }
    }

    public class UpdateStudentByCourseLevelCommandHandler : IRequestHandler<UpdateStudentByCourseLevelCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;

        public UpdateStudentByCourseLevelCommandHandler(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<MethodResult<bool>> Handle(UpdateStudentByCourseLevelCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var student = await _studentRepository.Queryable.FirstOrDefaultAsync(x => x.UserId == request.Id, cancellationToken: cancellationToken);

            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            student.CourseLevel = request.CourseLevel;
            if (request.BaseCourseLevel.HasValue)
            {
                student.BaseCourseLevel = request.BaseCourseLevel.Value;
            }

            _studentRepository.Update(student, false, x => x.CreatedDate, x => x.CreatedByParent, x => x.CreatedFullName, x => x.CreatedUserId
            , x => x.DeletedDate, x => x.DeletedFullName, x => x.DeletedUserId, x => x.IsDeleted, x => x.PackageId, x => x.Occupation, x => x.School,
            x => x.NumberOfShield, x => x.NumberOfToken, x => x.NumberOfTokenExchanged, x => x.NumberOfTokenReceived, x => x.ParentPhoneNumber,
            x => x.ClassId, x => x.UserId, x => x.BeginnerGuideStr, x => x.DistrictId, x => x.ProvinceId, x => x.SchoolClass, x => x.SchoolFaculty,
            x => x.SchoolGrade, x => x.SchoolId, x => x.ExpiredDate, x => x.ParentEmail);
            await _studentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }
    }
}
