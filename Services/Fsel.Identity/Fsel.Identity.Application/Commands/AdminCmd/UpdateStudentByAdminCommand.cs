// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using System.Globalization;
    using System.Text;
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Application.Commands.AuthCmd;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
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
    using OtpNet;

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
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly IHumanRepository _humanRepository;

        public UpdateStudentByAdminCommandHandler(UserManager<User> userManager,
            IMapper mapper,
            ITrainingService trainingService,
            IOrderService orderService,
            AppSetting appSetting,
            IMediator mediator,
            IUserOtpCodeRepository userOtpCodeRepository,
            IHumanRepository humanRepository)
        {
            _userManager = userManager;
            _mapper = mapper;
            _trainingService = trainingService;
            _orderService = orderService;
            _appSetting = appSetting;
            _mediator = mediator;
            _userOtpCodeRepository = userOtpCodeRepository;
            _humanRepository = humanRepository;
        }

        public async Task<MethodResult<StudentModel>> Handle(UpdateStudentByAdminCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<StudentModel>();

            #region Validate Email and PhoneNumber

            var userView = await _userManager.Users.Include(x => x.Human)
                                                 .ThenInclude(x => x!.Student)
                                                 .ThenInclude(x => x!.ParentStudents)
                                                 .FirstOrDefaultAsync(x => x.Human != null && x.Human.Student != null && x.Human.Student.Id == request.Id, cancellationToken);
            if (userView == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(userView));
                return methodResult;
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == request.Email && x.Id != userView.Id, cancellationToken: cancellationToken);
            if (user != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicateEmail), nameof(request.Email), request.Email);
                return methodResult;
            }
            user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber && x.Id != userView.Id, cancellationToken: cancellationToken);
            if (user != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicatePhoneNumber), nameof(request.PhoneNumber), request.PhoneNumber);
                return methodResult;
            }

            #endregion Validate Email and PhoneNumber

            var student = userView.Human?.Student;

            #region Update Parent

            var userResult = await SaveParent(request, student, cancellationToken);
            if (userResult.Result != null)
            {
                userView = userResult.Result;
            }
            if (userView == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(userView));
                return methodResult;
            }

            #endregion Update Parent

            #region validate Send OTP

            var isCheckEmail = userView.Email != request.Email;
            var isCheckPhone = userView.PhoneNumber != request.PhoneNumber;
            if (isCheckEmail || isCheckPhone)
            {
                userView.EmailConfirmed = false;
                userView.UserName = isCheckEmail ? request.Email : isCheckPhone ? request.PhoneNumber : userView.UserName;

                var userOtpCode = await _userOtpCodeRepository.Queryable.FirstOrDefaultAsync(x => x.UserId == userView.Id && x.Status == EnumOtpCodeStatus.New && !x.IsDeleted, cancellationToken);
                if (userOtpCode == null)
                {
                    var randomSecure = new RandomSecureHelper();
                    var totp = new Totp(Encoding.UTF8.GetBytes(randomSecure.Secretstrings()));
                    var otp = totp.ComputeTotp();

                    userOtpCode = new UserOtpCode
                    {
                        UserId = userView.Id,
                        OTPCode = otp,
                        Status = EnumOtpCodeStatus.New,
                        ExpiredTime = DateTime.UtcNow.AddDays(_appSetting!.Otp!.StepDayWithAdmin)
                    };
                    _userOtpCodeRepository.Add(userOtpCode);
                    await _userOtpCodeRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }

                var param = new SendOtpTemplateModel
                {
                    OtpCode = userOtpCode.OTPCode,
                    AccessLink = string.Format(CultureInfo.InvariantCulture, _appSetting!.ConstantUrl!.ConfirmOtpUrl!, userOtpCode.OTPCode),
                    OtpValidTime = string.Format(CultureInfo.InvariantCulture, SenderSettings.OtpValidDay, _appSetting!.Otp!.StepDayWithAdmin)
                };
                var subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendOtpSubjectFullName, userView.FullName);
                var sendResult = new MethodResult<bool>();
                if (isCheckEmail)
                {
                    sendResult = await _mediator.Send(new SenderCommand { Email = request.Email, Subject = subject, Params = param, Template = EnumSenderTemplate.SendOtpAndLink }, cancellationToken).ConfigureAwait(false);
                }
                else if (isCheckPhone)
                {
                    sendResult = await _mediator.Send(new SenderCommand { Email = userView.Email, Subject = subject, Params = param, Template = EnumSenderTemplate.SendOtpAndLink }, cancellationToken).ConfigureAwait(false);
                }

                if (!sendResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(sendResult?.ErrorMessages);
                    return methodResult;
                }
            }

            #endregion validate Send OTP

            #region Update User

            _mapper.Map(request, userView);
            _mapper.Map(request, userView.Human);
            student = _mapper.Map(request, userView.Human?.Student);
            var method = ValidateStudent(userView);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            await _userManager.UpdateAsync(userView).ConfigureAwait(false);
            _humanRepository.Update(userView.Human ?? new Human());
            await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            #endregion Update User

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = await GetUser(userView, student);
            return methodResult;
        }

        private async Task<StudentModel> GetUser(User userView, Student? student)
        {
            var userModel = _mapper.Map<StudentModel>(userView);
            var parentStudent = student?.ParentStudents?.FirstOrDefault();
            _mapper.Map(userView.Human, userModel.Human);
            _mapper.Map(student, userModel);
            if (parentStudent != null && parentStudent.Parent != null)
            {
                userModel.Parent = _mapper.Map<ParentProfileModel>(parentStudent.Parent.Human);
                userModel.Parent.Occupation = parentStudent.Parent.Occupation;
            }
            var classStudent = await _trainingService.GetClassByStudentId(student?.Id ?? default);
            var @class = classStudent?.Content?.Result;
            if (@class != null)
            {
                userModel.CodeClass = classStudent?.Content?.Result?.Code;
                var package = await _orderService.GetPackages();
                if (package.IsSuccessStatusCode)
                {
                    userModel.Membership = package.Content?.Result?.FirstOrDefault(p => p.Id == @class.PackageId)?.Code;
                }
            }
            return userModel;
        }

        public async Task<MethodResult<User>> SaveParent(UpdateStudentByAdminCommand request, Student? student, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(student);
            var methodResult = new MethodResult<User>();
            if (student.ParentStudents == null || student.ParentStudents.Count == 0)
            {
                Human newHuman = _mapper.Map<Human>(request.Parent);
                newHuman.Parent = _mapper.Map<Parent>(request.Parent);
                newHuman.Parent.ParentStudents.Add(new ParentStudent
                {
                    Student = student
                });
                var method = ValidateParent(newHuman);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                _humanRepository.Add(newHuman);
                await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var userView = await _userManager.Users.Include(x => x.Human)
                                             .ThenInclude(x => x!.Student)
                                             .ThenInclude(x => x!.ParentStudents)
                                             .ThenInclude(x => x.Parent)
                                             .ThenInclude(x => x!.Human)
                                             .FirstOrDefaultAsync(x => x.Human != null && x.Human.Student != null && x.Human.Student.Id == request.Id, cancellationToken);
                var human = userView?.Human?.Student?.ParentStudents.FirstOrDefault()?.Parent?.Human;
                if (human != null)
                {
                    _mapper.Map(request.Parent, human);
                    _mapper.Map(request.Parent, human.Parent);
                    var method = ValidateParent(human);
                    if (!method.IsOK)
                    {
                        methodResult.AddErrorBadRequest(method.ErrorMessages);
                        return methodResult;
                    }
                    _humanRepository.Update(human);
                    await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                methodResult.Result = userView;
            }
            return methodResult;
        }

        public VoidMethodResult ValidateParent(Human human)
        {
            ArgumentNullException.ThrowIfNull(human);
            VoidMethodResult methodResult = new VoidMethodResult();
            if (!human.IsValid())
            {
                methodResult.AddErrorBadRequest(human.ErrorMessages);
                return methodResult;
            }
            if (!human.Parent!.IsValid())
            {
                methodResult.AddErrorBadRequest(human.Parent!.ErrorMessages);
                return methodResult;
            }
            return methodResult;
        }

        public VoidMethodResult ValidateStudent(User user)
        {
            ArgumentNullException.ThrowIfNull(user);
            VoidMethodResult methodResult = new VoidMethodResult();
            if (!user.IsValid())
            {
                methodResult.AddErrorBadRequest(user.ErrorMessages);
                return methodResult;
            }
            if (!user.Human!.IsValid())
            {
                methodResult.AddErrorBadRequest(user.Human!.ErrorMessages);
                return methodResult;
            }
            if (!user.Human!.Student!.IsValid())
            {
                methodResult.AddErrorBadRequest(user.Human!.Student!.ErrorMessages);
                return methodResult;
            }
            return methodResult;
        }
    }
}
