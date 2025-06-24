// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Identity.Domain.Models.EntityModels;
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

        public UpdateStudentByAdminCommandHandler(UserManager<User> userManager,
            IMapper mapper,
            ITrainingService trainingService,
            IOrderService orderService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _trainingService = trainingService;
            _orderService = orderService;
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

            #region Validate PhoneNumber

            if (request.PhoneNumber != null && user.UserName == user.PhoneNumber)
            {
                var checkUserName = await _userManager.Users.AnyAsync(x => x.Id != user.Id && x.UserName == request.PhoneNumber, cancellationToken);
                if (checkUserName)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.PhoneNumber));
                    return methodResult;
                }
            }
            //if (!string.IsNullOrEmpty(request.Email))
            //{
            //    var checkEmail = await _userManager.Users.AnyAsync(x => x.Id != user.Id && x.Email == request.Email, cancellationToken);
            //    if (checkEmail)
            //    {
            //        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(request.Email), request.Email);
            //        return methodResult;
            //    }
            //}

            #endregion Validate PhoneNumber

            #region Validate User

            var student = user.Student;
            _mapper.Map(request, user);
            student = _mapper.Map(request, user.Student);

            if (!user.IsValid())
            {
                methodResult.AddErrorBadRequest(user.ErrorMessages);
                return methodResult;
            }
            var method = await Validate(user, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }

            if (student != null)
            {
                student.ParentEmail = string.IsNullOrEmpty(request.Parent?.Email) ? null : request.Parent?.Email;
                student.ParentPhoneNumber = string.IsNullOrEmpty(request.Parent?.PhoneNumber) ? null : request.Parent?.PhoneNumber;
            }

            #endregion Validate User

            #region Save Parent

            if (request.Parent != null)
            {
                request.Parent.FullName = !string.IsNullOrEmpty(request.Parent.FullName) ? request.Parent.FullName : "N/A";
                var userParent = student?.ParentStudents.FirstOrDefault()?.Parent?.User;
                var userResult = await SaveParent(request, student, userParent, cancellationToken);
                if (!userResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(userResult.ErrorMessages);
                    return methodResult;
                }
            }

            #endregion Save Parent

            #region validate and Send OTP

            //var isCheckEmail = !string.IsNullOrEmpty(request.Email) && user.Email != request.Email;
            //if (isCheckEmail)
            //{
            //    user.EmailConfirmed = false;
            //    var userOtpCode = await _mediator.Send(new SaveUserOtpCommand { Id = user.Id }, cancellationToken);
            //    var param = new SendOtpTemplateModel
            //    {
            //        OtpCode = userOtpCode.Result,
            //        AccessLink = string.Format(CultureInfo.InvariantCulture, _appSetting!.ConstantUrl!.ConfirmOtpUrl!, userOtpCode.Result, user.Id),
            //        OtpValidTime = string.Format(CultureInfo.InvariantCulture, SenderSettings.OtpValidDay, _appSetting!.Otp!.StepDayWithAdmin)
            //    };
            //    var subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendOtpSubjectFullName, user.FullName);
            //    var sendResult = await _mediator.Send(new SenderCommand { Email = user.Email, Subject = subject, Params = param, Template = EnumSenderTemplate.SendOtpAndLink }, cancellationToken).ConfigureAwait(false);
            //    if (!sendResult.IsOK)
            //    {
            //        methodResult.AddErrorBadRequest(sendResult?.ErrorMessages);
            //        return methodResult;
            //    }
            //}

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
            var classStudent = await _trainingService.GetClassToStudentId(student?.Id ?? default);
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
                _mapper.Map(request.Parent, user);
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
    }
}
