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
        private readonly IHumanRepository _humanRepository;
        private readonly UserManager<User> _userManager;

        public UpdateStudentInfoEventCommandHandler(IStudentRepository studentRepository,
            AuthContext authContext,
            IHumanRepository humanRepository,
            UserManager<User> userManager)
        {
            _studentRepository = studentRepository;
            _authContext = authContext;
            _humanRepository = humanRepository;
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
            var userEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userEmail != null && userEmail.Id != _authContext.CurrentUserId)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Email), request.Email);
                return methodResult;
            }

            await UpdateUserAsync(request, cancellationToken);
            var human = await UpdateHumanAsync(request, cancellationToken);
            if (human != null)
            {
                await UpdateStudentAsync(request, human.Id, cancellationToken);
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = true;
            return methodResult;
        }

        private async Task UpdateUserAsync(UpdateStudentInfoEventCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == _authContext.CurrentUserId, cancellationToken);
            if (user == null)
            {
                return;
            }
            user.Email = request.Email;
            user.EmailConfirmed = true;
            user.NormalizedEmail = request.Email?.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
            await _userManager.UpdateAsync(user);
        }

        private async Task<Human?> UpdateHumanAsync(UpdateStudentInfoEventCommand request, CancellationToken cancellationToken)
        {
            var human = await _humanRepository.Queryable.FirstOrDefaultAsync(x => x.UserId == _authContext.CurrentUserId, cancellationToken);
            if (human == null)
            {
                return human;
            }
            human.Email = request.Email;
            human.Birthday = request.Birthday;
            _humanRepository.Update(human);
            await _humanRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return human;
        }

        private async Task UpdateStudentAsync(UpdateStudentInfoEventCommand request, Guid humanId, CancellationToken cancellationToken)
        {
            var student = await _studentRepository.Queryable.FirstOrDefaultAsync(x => x.HumanId == humanId, cancellationToken);
            if (student == null)
            {
                return;
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
            _studentRepository.Update(student);
            await _studentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
