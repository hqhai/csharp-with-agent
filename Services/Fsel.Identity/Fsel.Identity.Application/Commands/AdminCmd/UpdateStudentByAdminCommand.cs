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

            var user = await _userManager.Users.Include(x => x.Human)
                                                 .ThenInclude(x => x!.Student)
                                                 .ThenInclude(x => x!.ParentStudents)
                                                 .FirstOrDefaultAsync(x => x.Human != null && x.Human.Student != null && x.Human.Student.Id == request.Id, cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }

            #region Update User

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

            #endregion Update User

            #region Update Parent

            var userResult = await SaveParent(request, student, cancellationToken);
            if (!userResult.IsOK)
            {
                methodResult.AddErrorBadRequest(userResult.ErrorMessages);
                return methodResult;
            }
            if (userResult.Result != null)
            {
                user = userResult.Result;
            }
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(user));
                return methodResult;
            }

            #endregion Update Parent

            #region validate Send OTP

            var isCheckEmail = user.Email != request.Email;
            var isCheckPhone = user.PhoneNumber != request.PhoneNumber;
            if (isCheckEmail || isCheckPhone)
            {
                user.EmailConfirmed = false;
                var userOtpCode = await _userOtpCodeRepository.Queryable.FirstOrDefaultAsync(x => x.UserId == user.Id && x.Status == EnumOtpCodeStatus.New && !x.IsDeleted, cancellationToken);
                if (userOtpCode == null)
                {
                    var randomSecure = new RandomSecureHelper();
                    var totp = new Totp(Encoding.UTF8.GetBytes(randomSecure.Secretstrings()));
                    var otp = totp.ComputeTotp();

                    userOtpCode = new UserOtpCode
                    {
                        UserId = user.Id,
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
                var subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendOtpSubjectFullName, user.FullName);
                var sendResult = new MethodResult<bool>();
                if (isCheckEmail)
                {
                    sendResult = await _mediator.Send(new SenderCommand { Email = request.Email, Subject = subject, Params = param, Template = EnumSenderTemplate.SendOtpAndLink }, cancellationToken).ConfigureAwait(false);
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

            #endregion validate Send OTP

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
            var parentStudent = student?.ParentStudents?.FirstOrDefault();
            _mapper.Map(user.Human, userModel.Human);
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
            if (request.Parent != null)
            {
                if (student.ParentStudents == null || student.ParentStudents.Count == 0)
                {
                    Human human = _mapper.Map<Human>(request.Parent);
                    human.Parent = _mapper.Map<Parent>(request.Parent);
                    human.Parent.ParentStudents.Add(new ParentStudent
                    {
                        Student = student
                    });
                    var method = await Validate(human, cancellationToken);
                    if (!method.IsOK)
                    {
                        methodResult.AddErrorBadRequest(method.ErrorMessages);
                        return methodResult;
                    }
                    _humanRepository.Add(human);
                    await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var user = await _userManager.Users.Include(x => x.Human)
                                                 .ThenInclude(x => x!.Student)
                                                 .ThenInclude(x => x!.ParentStudents)
                                                 .ThenInclude(x => x.Parent)
                                                 .ThenInclude(x => x!.Human)
                                                 .FirstOrDefaultAsync(x => x.Human != null && x.Human.Student != null && x.Human.Student.Id == request.Id, cancellationToken);
                    var human = user?.Human?.Student?.ParentStudents.FirstOrDefault()?.Parent?.Human;

                    if (human != null)
                    {
                        _mapper.Map(request.Parent, human);
                        _mapper.Map(request.Parent, human.Parent);
                        var method = await Validate(human, cancellationToken);
                        if (!method.IsOK)
                        {
                            methodResult.AddErrorBadRequest(method.ErrorMessages);
                            return methodResult;
                        }
                        _humanRepository.Update(human);
                        await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    }
                    methodResult.Result = user;
                }
            }

            return methodResult;
        }

        public async Task<VoidMethodResult> Validate(Human? human, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(human);
            VoidMethodResult methodResult = new VoidMethodResult();
            var humanOther = await _humanRepository.Queryable.Where(x => x.Email == human.Email && x.Id != human.Id).FirstOrDefaultAsync(cancellationToken);
            if (humanOther != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicateEmail), nameof(human.Email));
                return methodResult;
            }

            humanOther = await _humanRepository.Queryable.Where(x => x.PhoneNumber == human.PhoneNumber && x.Id != human.Id).FirstOrDefaultAsync(cancellationToken);
            if (humanOther != null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumAuthUserErrorCode.DuplicatePhoneNumber), nameof(human.PhoneNumber));
                return methodResult;
            }
            if (!human.IsValid())
            {
                methodResult.AddErrorBadRequest(human.ErrorMessages);
                return methodResult;
            }
            if (human.Parent != null)
            {
                if (!human.Parent.IsValid())
                {
                    methodResult.AddErrorBadRequest(human.Parent.ErrorMessages);
                    return methodResult;
                }
            }
            if (human.Student != null)
            {
                if (!human.Student.IsValid())
                {
                    methodResult.AddErrorBadRequest(human.Student.ErrorMessages);
                    return methodResult;
                }
            }
            return methodResult;
        }
    }
}
