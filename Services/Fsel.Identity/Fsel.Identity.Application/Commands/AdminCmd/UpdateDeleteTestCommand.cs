// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Identity.Application.Services.LmsCourseService;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class UpdateDeleteTestCommand : IRequest<MethodResult<bool>>
    {
        public Guid StudentId { get; set; }
    }

    public class UpdateDeleteTestCommandHandler : IRequestHandler<UpdateDeleteTestCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ILmsCourseService _lmsCourseService;

        public UpdateDeleteTestCommandHandler(IStudentRepository studentRepository,
            ILmsCourseService lmsCourseService)
        {
            _studentRepository = studentRepository;
            _lmsCourseService = lmsCourseService;
        }

        public async Task<MethodResult<bool>> Handle(UpdateDeleteTestCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var student = await _studentRepository.Queryable.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == request.StudentId, cancellationToken);
            if (student == null || student.User == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student), request.StudentId);
                return methodResult;
            }
            int age = DateTimeHelper.GetYearOld(student.User.Birthday);
            student.BaseCourseLevel = null;
            student.CourseId = null;
            student.ClassId = null;
            if (age <= 13)
            {
                student.CourseLevel = EnumCourseLevel.A2;
            }
            else if (age >= 14)
            {
                student.CourseLevel = EnumCourseLevel.B1;
            }
            await _lmsCourseService.DeletePTAndCourseAsync(request.StudentId).ConfigureAwait(false);

            _studentRepository.Update(student);
            await _studentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            methodResult.Result = true;
            return methodResult;
        }
    }
}
