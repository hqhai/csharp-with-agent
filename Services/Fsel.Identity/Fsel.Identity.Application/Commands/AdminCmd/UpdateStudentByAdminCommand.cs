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
    using Fsel.Identity.Application.Commands.UserOtpCodeCmd;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
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
        private readonly IHumanRepository _humanRepository;

        public UpdateStudentByAdminCommandHandler(UserManager<User> userManager,
            IMapper mapper,
            ITrainingService trainingService,
            IOrderService orderService,
            AppSetting appSetting,
            IMediator mediator,
            IHumanRepository humanRepository)
        {
            _userManager = userManager;
            _mapper = mapper;
            _trainingService = trainingService;
            _orderService = orderService;
            _appSetting = appSetting;
            _mediator = mediator;
            _humanRepository = humanRepository;
        }

        public async Task<MethodResult<StudentModel>> Handle(UpdateStudentByAdminCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentModel>();

            var user = await _userManager.Users.Include(x => x.Human)
                                                         .ThenInclude(x => x!.Student)
                                                         .ThenInclude(x => x!.ParentStudents)
                                                         .ThenInclude(x => x.Parent)
                                                         .ThenInclude(x => x!.Human)
                                                         .FirstOrDefaultAsync(x => x.Human != null && x.Human.Student != null && x.Human.Student.Id == request.Id, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }
            var isCheckEmail = !string.IsNullOrEmpty(request.PhoneNumber) && user.Email != request.Email;
            var isCheckPhone = !string.IsNullOrEmpty(request.PhoneNumber) && user.PhoneNumber != request.PhoneNumber;

            #region Validate User

            var student = user.Human?.Student;
            _mapper.Map(request, user);
            _mapper.Map(request, user.Human);
            student = _mapper.Map(request, user.Human?.Student);
            if (!user.IsValid())
            {
                methodResult.AddErrorBadRequest(user.ErrorMessages);
                return methodResult;
            }
            var method = await Validate(user.Human, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }

            #endregion Validate User

            #region Save Parent

            var humanParent = student?.ParentStudents.FirstOrDefault()?.Parent?.Human;
            var userResult = await SaveParent(request, student, humanParent, cancellationToken);
            if (!userResult.IsOK)
            {
                methodResult.AddErrorBadRequest(userResult.ErrorMessages);
                return methodResult;
            }

            #endregion Save Parent

            #region validate and Send OTP

            if (isCheckEmail || isCheckPhone)
            {
                user.EmailConfirmed = false;
                var userOtpCode = await _mediator.Send(new SaveUserOtpCodeCommand { Id = user.Id }, cancellationToken);

                var param = new SendOtpTemplateModel
                {
                    OtpCode = userOtpCode.Result,
                    AccessLink = string.Format(CultureInfo.InvariantCulture, _appSetting!.ConstantUrl!.ConfirmOtpUrl!, userOtpCode.Result),
                    OtpValidTime = string.Format(CultureInfo.InvariantCulture, SenderSettings.OtpValidDay, _appSetting!.Otp!.StepDayWithAdmin)
                };
                var subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendOtpSubjectFullName, user.FullName);
                var sendResult = new MethodResult<bool>();
                if (isCheckEmail)
                {
                    sendResult = await _mediator.Send(new SenderCommand { Email = user.Email, Subject = subject, Params = param, Template = EnumSenderTemplate.SendOtpAndLink }, cancellationToken).ConfigureAwait(false);
                }
                else if (isCheckPhone)
                {
                    sendResult = await _mediator.Send(new SenderCommand { Email = user.Email, Subject = subject, Params = param, Template = EnumSenderTemplate.SendOtpAndLink }, cancellationToken).ConfigureAwait(false);
                }

                if (!sendResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(sendResult?.ErrorMessages);
                    return methodResult;
                }
            }

            #endregion validate and Send OTP

            await _userManager.UpdateAsync(user).ConfigureAwait(false);
            _humanRepository.Update(user.Human ?? new Human());
            await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = await GetUser(user, student);
            return methodResult;
        }

        private async Task<StudentModel> GetUser(User user, Student? student)
        {
            var userModel = _mapper.Map<StudentModel>(user);
            _mapper.Map(user.Human, userModel.Human);
            _mapper.Map(student, userModel);
            var parentStudent = student?.ParentStudents?.FirstOrDefault();
            if (parentStudent != null && parentStudent.Parent != null)
            {
                userModel.Parent = _mapper.Map<ParentProfileModel>(parentStudent.Parent.Human);
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

        public async Task<VoidMethodResult> SaveParent(UpdateStudentByAdminCommand request, Student? student, Human? human, CancellationToken cancellationToken)
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
                human = _mapper.Map<Human>(request.Parent);
                human.Parent = _mapper.Map<Parent>(request.Parent);
                human.Parent.ParentStudents.Add(new ParentStudent { Student = student });
                var validation = await ValidateAndHandleErrors(human, methodResult, cancellationToken);
                if (!validation)
                {
                    return methodResult;
                }
                _humanRepository.Add(human);
            }
            else if (human != null)
            {
                _mapper.Map(request.Parent, human);
                _mapper.Map(request.Parent, human.Parent);
                var validation = await ValidateAndHandleErrors(human, methodResult, cancellationToken);
                if (!validation)
                {
                    return methodResult;
                }
                _humanRepository.Update(human);
            }
            await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            return methodResult;
        }

        private async Task<bool> ValidateAndHandleErrors(Human human, VoidMethodResult methodResult, CancellationToken cancellationToken)
        {
            var method = await Validate(human, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return false;
            }
            return true;
        }

        public async Task<VoidMethodResult> Validate(Human? human, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(human);
            var methodResult = new VoidMethodResult();
            if (!string.IsNullOrEmpty(human.Email))
            {
                var emailCheck = await CheckDuplicateAsync(human.Email, human.Id, nameof(EnumAuthUserErrorCode.DuplicateEmail), nameof(human.Email), cancellationToken);
                if (emailCheck != null)
                {
                    return emailCheck;
                }
            }
            if (!string.IsNullOrEmpty(human.PhoneNumber))
            {
                var phoneNumberCheck = await CheckDuplicateAsync(human.PhoneNumber, human.Id, nameof(EnumAuthUserErrorCode.DuplicatePhoneNumber), nameof(human.PhoneNumber), cancellationToken);
                if (phoneNumberCheck != null)
                {
                    return phoneNumberCheck;
                }
            }

            if (!human.IsValid())
            {
                methodResult.AddErrorBadRequest(human.ErrorMessages);
            }

            if (human.Parent != null && !human.Parent.IsValid())
            {
                methodResult.AddErrorBadRequest(human.Parent.ErrorMessages);
            }

            if (human.Student != null && !human.Student.IsValid())
            {
                methodResult.AddErrorBadRequest(human.Student.ErrorMessages);
            }

            return methodResult;
        }

        private async Task<VoidMethodResult?> CheckDuplicateAsync(string? fieldValue, Guid? currentId, string errorCode, string fieldName, CancellationToken cancellationToken)
        {
            var humanOther = await _humanRepository.Queryable
                .Where(x => (x.Email == fieldValue || x.PhoneNumber == fieldValue) && x.Id != currentId)
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
