// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.StudentCmd.StudentEventCmd
{
    using System.ComponentModel.DataAnnotations;
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using static Fsel.Shared.Constants.ValueSettings;

    public class UpdateStudentInfoEventCommand : IRequest<MethodResult<bool>>
    {
        [Required]
        public string? Email { get; set; }

        public DateTime Birthday { get; set; }

        [Required]
        public string? ParentPhoneNumber { get; set; }

        public string? ParentEmail { get; set; }
    }

    public class UpdateStudentInfoEventCommandHandler : IRequestHandler<UpdateStudentInfoEventCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly AuthContext _authContext;
        private readonly UserManager<User> _userManager;

        public UpdateStudentInfoEventCommandHandler(IStudentRepository studentRepository,
            AuthContext authContext,
            UserManager<User> userManager)
        {
            _studentRepository = studentRepository;
            _authContext = authContext;
            _userManager = userManager;
        }

        public async Task<MethodResult<bool>> Handle(UpdateStudentInfoEventCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            if (string.IsNullOrEmpty(request.Email))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Email), request.Email);
                return methodResult;
            }
            if (!request.Email.IsValidEmail())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.Email), request.Email);
                return methodResult;
            }

            if (string.IsNullOrEmpty(request.ParentPhoneNumber))
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.ParentPhoneNumber), request.ParentPhoneNumber);
                return methodResult;
            }
            if (!request.ParentPhoneNumber.IsValidPhoneNumber())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.ParentPhoneNumber), request.ParentPhoneNumber);
                return methodResult;
            }

            if (!string.IsNullOrEmpty(request.ParentEmail) && !request.ParentEmail.IsValidEmail())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.ParentEmail), request.ParentEmail);
                return methodResult;
            }
            var userEmail = await _userManager.Users.FirstOrDefaultAsync(x => x.Id != _authContext.CurrentUserId && x.Email == request.Email && x.EmailConfirmed, cancellationToken);
            if (userEmail != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Email), request.Email);
                return methodResult;
            }

            var method = await UpdateUserAsync(request, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }

            var studentMethod = await UpdateStudentAsync(request, _authContext.CurrentUserId, cancellationToken);
            if (!studentMethod.IsOK)
            {
                methodResult.AddErrorBadRequest(studentMethod.ErrorMessages);
                return methodResult;
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }

        private async Task<VoidMethodResult> UpdateUserAsync(UpdateStudentInfoEventCommand request, CancellationToken cancellationToken)
        {
            var methodResult = new VoidMethodResult();
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user), _authContext.CurrentUserId);
                return methodResult;
            }
            user.Email = request.Email;
            user.Birthday = request.Birthday;
            user.EmailConfirmed = true;
            user.NormalizedEmail = request.Email?.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
            if (!user.IsValid())
            {
                methodResult.AddErrorBadRequest(user.ErrorMessages);
                return methodResult;
            }
            await _userManager.UpdateAsync(user);

            return methodResult;
        }

        private async Task<VoidMethodResult> UpdateStudentAsync(UpdateStudentInfoEventCommand request, Guid userId, CancellationToken cancellationToken)
        {
            var methodResult = new VoidMethodResult();
            var student = await _studentRepository.Queryable.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student), userId);
                return methodResult;
            }
            int age = Shared.Helpers.DateTimeHelper.GetYearOld(request.Birthday);
            if (age <= AgeMilestone.ChildrenAge)
            {
                student.CourseLevel = EnumCourseLevel.A2;
            }
            else if (age >= AgeMilestone.StudentAge)
            {
                student.CourseLevel = EnumCourseLevel.B1;
            }
            student.ParentEmail = request.ParentEmail;
            student.ParentPhoneNumber = request.ParentPhoneNumber;
            if (!student.IsValid())
            {
                methodResult.AddErrorBadRequest(student.ErrorMessages);
                return methodResult;
            }
            _studentRepository.Update(student);
            await _studentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await AddParentAsync(request, student.Id, cancellationToken);

            return methodResult;
        }

        private async Task AddParentAsync(UpdateStudentInfoEventCommand request, Guid studentId, CancellationToken cancellationToken)
        {
            var user = new User()
            {
                FirstName = "N/A",
                LastName = "N/A",
                PhoneNumber = request.ParentPhoneNumber,
                Email = request.ParentEmail,
                Parent = new Parent()
                {
                    ParentStudents = new List<ParentStudent>()
                    {
                        new ParentStudent()
                        {
                            StudentId = studentId
                        }
                    }
                }
            };
            if (user.IsValid())
            {
                await _userManager.CreateAsync(user);
            }
        }
    }
}
