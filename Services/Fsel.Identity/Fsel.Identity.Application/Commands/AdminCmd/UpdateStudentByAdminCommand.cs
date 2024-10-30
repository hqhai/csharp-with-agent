// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using System.Globalization;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Commands.AuthCmd;
    using Fsel.Identity.Application.Commands.UserOtpCmd;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.SenderTemplates;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateStudentByAdminCommand : UpdateStudentByAdminCommandModel, IRequest<MethodResult<StudentModel>>
    {
    }

    public class UpdateStudentByAdminCommandHandler : IRequestHandler<UpdateStudentByAdminCommand, MethodResult<StudentModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly ITrainingService _trainingService;
        private readonly IOrderService _orderService;
        private readonly AppSetting _appSetting;
        private readonly IMediator _mediator;

        public UpdateStudentByAdminCommandHandler(UserManager<User> userManager,
            IMapper mapper,
            ITrainingService trainingService,
            IOrderService orderService,
            AppSetting appSetting,
            IMediator mediator)
        {
            _userManager = userManager;
            _mapper = mapper;
            _trainingService = trainingService;
            _orderService = orderService;
            _appSetting = appSetting;
            _mediator = mediator;
        }

        public async Task<MethodResult<StudentModel>> Handle(UpdateStudentByAdminCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentModel>();

            var user = await _userManager.Users.Include(x => x!.Student)
                                            .ThenInclude(x => x!.ParentStudents)
                                            .ThenInclude(x => x.Parent)
                                            .FirstOrDefaultAsync(x => x.Student != null && x.Student.Id == request.Id, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }
            var isCheckEmail = !string.IsNullOrEmpty(request.Email) && user.Email != request.Email;

            #region Validate User

            var student = user.Student;
            _mapper.Map(request, user);
            student = _mapper.Map(request, user.Student);
            if (!user.IsValid())
            {
                methodResult.AddErrorBadRequest(user.ErrorMessages);
                return methodResult;
            }

            #endregion Validate User

            #region Save Parent

            var userPrarent = student?.ParentStudents.FirstOrDefault()?.Parent?.User;
            var userResult = await SaveParent(request, student, userPrarent, cancellationToken);
            if (!userResult.IsOK)
            {
                methodResult.AddErrorBadRequest(userResult.ErrorMessages);
                return methodResult;
            }

            #endregion Save Parent

            #region validate and Send OTP

            if (isCheckEmail)
            {
                user.EmailConfirmed = false;
                var userOtpCode = await _mediator.Send(new SaveUserOtpCommand { Id = user.Id }, cancellationToken);
                var param = new SendOtpTemplateModel
                {
                    OtpCode = userOtpCode.Result,
                    AccessLink = string.Format(CultureInfo.InvariantCulture, _appSetting!.ConstantUrl!.ConfirmOtpUrl!, userOtpCode.Result),
                    OtpValidTime = string.Format(CultureInfo.InvariantCulture, SenderSettings.OtpValidDay, _appSetting!.Otp!.StepDayWithAdmin)
                };
                var subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendOtpSubjectFullName, user.FullName);
                var sendResult = await _mediator.Send(new SenderCommand { Email = user.Email, Subject = subject, Params = param, Template = EnumSenderTemplate.SendOtpAndLink }, cancellationToken).ConfigureAwait(false);
                if (!sendResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(sendResult?.ErrorMessages);
                    return methodResult;
                }
            }

            #endregion validate and Send OTP

            await _userManager.UpdateAsync(user).ConfigureAwait(false);
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = await GetUser(user, student);
            return methodResult;
        }

        private async Task<StudentModel> GetUser(User user, Student? student)
        {
            var userModel = _mapper.Map<StudentModel>(user);
            _mapper.Map(student, userModel);
            var parentStudent = student?.ParentStudents?.FirstOrDefault();
            if (parentStudent != null && parentStudent.Parent != null)
            {
                userModel.Parent = _mapper.Map<ParentProfileModel>(parentStudent.Parent.User);
                userModel.Parent.Occupation = parentStudent.Parent.Occupation;
            }
            var classStudent = await _trainingService.GetClassByStudentId(student?.Id ?? default);
            var @class = classStudent?.Content?.Result;
            if (@class != null)
            {
                userModel.CodeClass = @class.Code;
            }
            var package = await _orderService.GetPackages();
            if (package.IsSuccessStatusCode)
            {
                userModel.Membership = package.Content?.Result?.FirstOrDefault(p => p.Id == student?.PackageId)?.Code;
            }
            return userModel;
        }

        public async Task<VoidMethodResult> SaveParent(UpdateStudentByAdminCommand request, Student? student, User? user, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(student);
            var methodResult = new VoidMethodResult();
            if (request.Parent == null)
            {
                return methodResult;
            }
            if (student.ParentStudents == null || !student.ParentStudents.Any())
            {
                user = _mapper.Map<User>(request.Parent);
                user.Parent = _mapper.Map<Parent>(request.Parent);
                user.Parent.ParentStudents.Add(new ParentStudent { Student = student });
                var validation = await ValidateAndHandleErrors(user, methodResult, cancellationToken);
                if (!validation)
                {
                    return methodResult;
                }
                await _userManager.CreateAsync(user, "Admin@123");
            }
            else if (user != null)
            {
                _mapper.Map(request.Parent, user.Parent);
                var validation = await ValidateAndHandleErrors(user, methodResult, cancellationToken);
                if (!validation)
                {
                    return methodResult;
                }
                await _userManager.UpdateAsync(user);
            }
            return methodResult;
        }

        private async Task<bool> ValidateAndHandleErrors(User user, VoidMethodResult methodResult, CancellationToken cancellationToken)
        {
            var method = await Validate(user, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return false;
            }
            return true;
        }

        public async Task<VoidMethodResult> Validate(User user, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(user);
            var methodResult = new VoidMethodResult();
            if (!string.IsNullOrEmpty(user.Email))
            {
                var emailCheck = await CheckDuplicateAsync(user.Email, user.Id, nameof(EnumAuthUserErrorCode.DuplicateEmail), nameof(user.Email), cancellationToken);
                if (emailCheck != null)
                {
                    return emailCheck;
                }
            }
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                var phoneNumberCheck = await CheckDuplicateAsync(user.PhoneNumber, user.Id, nameof(EnumAuthUserErrorCode.DuplicatePhoneNumber), nameof(user.PhoneNumber), cancellationToken);
                if (phoneNumberCheck != null)
                {
                    return phoneNumberCheck;
                }
            }

            if (!user.IsValid())
            {
                methodResult.AddErrorBadRequest(user.ErrorMessages);
            }

            if (user.Parent != null && !user.Parent.IsValid())
            {
                methodResult.AddErrorBadRequest(user.Parent.ErrorMessages);
            }

            if (user.Student != null && !user.Student.IsValid())
            {
                methodResult.AddErrorBadRequest(user.Student.ErrorMessages);
            }

            return methodResult;
        }

        private async Task<VoidMethodResult?> CheckDuplicateAsync(string? fieldValue, Guid? currentId, string errorCode, string fieldName, CancellationToken cancellationToken)
        {
            var humanOther = await _userManager.Users.Where(x => (x.Email == fieldValue || x.PhoneNumber == fieldValue) && x.Id != currentId)
                .FirstOrDefaultAsync(cancellationToken);

            if (humanOther != null)
            {
                var methodResult = new VoidMethodResult();
                methodResult.AddErrorBadRequest(errorCode, fieldName);
                return methodResult;
            }

            return null;
        }
    }
}
