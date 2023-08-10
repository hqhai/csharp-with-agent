// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using System.Globalization;
    using System.Text;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Application.Commands.AuthCmd;
    using Fsel.Identity.Application.Services.OrderService;
    using Fsel.Identity.Application.Services.TrainingService;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Infrastructure.ValueSettings;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.SenderTemplates;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
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
            var userView = await GetUserViewAsync(request.Id, cancellationToken);
            if (userView == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(userView));
                return methodResult;
            }
            var student = userView.Human?.Student;
            var humanParent = userView.Human?.Student?.ParentStudents.FirstOrDefault()?.Parent?.Human;
            if (student != null)
            {
                if (request.Parent != null && string.IsNullOrEmpty(request.Parent.FullName))
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Parent.FullName));
                    return methodResult;
                }
                await SaveParent(request, student, humanParent, cancellationToken);
            }
            await UpdateUser(request, userView, cancellationToken);

            var method = await SendOTP(request, userView, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            var userModel = await GetStudentModel(userView);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = userModel;
            return methodResult;
        }

        private async Task<User?> GetUserViewAsync(Guid studentId, CancellationToken cancellationToken)
        {
            return await _userManager.Users.Include(x => x.Human)
                .ThenInclude(x => x!.Student)
                .ThenInclude(x => x!.ParentStudents)
                .ThenInclude(x => x.Parent)
                .ThenInclude(x => x!.Human)
                .FirstOrDefaultAsync(x => x.Human != null && x.Human.Student != null && x.Human.Student.Id == studentId, cancellationToken);
        }

        private async Task SaveParent(UpdateStudentByAdminCommand request, Student student, Human? human, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(student);
            if (human == null)
            {
                human = _mapper.Map<Human>(request.Parent);
                human.Parent = _mapper.Map<Parent>(request.Parent);
                human.Parent.ParentStudents.Add(new ParentStudent
                {
                    Student = student
                });
                _humanRepository.Add(human);
            }
            else
            {
                if (request.Parent != null)
                {
                    _mapper.Map(request.Parent, human);
                    _mapper.Map(request.Parent, human.Parent);
                    _humanRepository.Update(human);
                }
                else
                {
                    await _humanRepository.DeleteAsync(human);
                }
            }

            await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task<StudentModel> GetStudentModel(User? user)
        {
            StudentModel studentModel = new StudentModel();
            if (user != null)
            {
                var student = user.Human?.Student;
                var parentStudent = student?.ParentStudents?.FirstOrDefault();
                _mapper.Map(user, studentModel);
                _mapper.Map(user.Human, studentModel.Human);
                _mapper.Map(student, studentModel);

                if (parentStudent != null && parentStudent.Parent != null)
                {
                    studentModel.Parent = _mapper.Map<ParentProfileModel>(parentStudent.Parent.Human);
                    studentModel.Parent.Occupation = parentStudent.Parent.Occupation;
                }
                var classStudent = await _trainingService.GetClassByStudentId(student?.Id ?? default);
                var @class = classStudent?.Content?.Result;
                if (@class != null)
                {
                    studentModel.CodeClass = classStudent?.Content?.Result?.Code;
                    var package = await _orderService.GetPackages();
                    if (package.IsSuccessStatusCode)
                    {
                        studentModel.Membership = package.Content?.Result?.FirstOrDefault(p => p.Id == @class.PackageId)?.Code;
                    }
                }
            }
            return studentModel;
        }

        public async Task UpdateUser(UpdateStudentByAdminCommand request, User? user, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (user != null)
            {
                _mapper.Map(request, user);
                await _userManager.UpdateAsync(user);

                _mapper.Map(request, user.Human);
                _mapper.Map(request, user.Human?.Student);
                _humanRepository.Update(user.Human ?? new Human());
                await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<VoidMethodResult> SendOTP(UpdateStudentByAdminCommand request, User? user, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            VoidMethodResult methodResult = new VoidMethodResult();
            if (user != null)
            {
                var isCheckEmail = user.Email != request.Email;
                var isCheckPhone = user.PhoneNumber != request.PhoneNumber;
                if (isCheckEmail || isCheckPhone)
                {
                    user.EmailConfirmed = false;
                    user.UserName = isCheckEmail ? request.Email : isCheckPhone ? request.PhoneNumber : user.UserName;
                    var userOtpCode = await _userOtpCodeRepository.Queryable.FirstOrDefaultAsync(x => x.UserId == user.Id && x.Status == EnumStatusUser.New && !x.IsDeleted, cancellationToken);
                    if (userOtpCode == null)
                    {
                        var randomSecure = new RandomSecureHelper();
                        var totp = new Totp(Encoding.UTF8.GetBytes(randomSecure.Secretstrings()));
                        var otp = totp.ComputeTotp();

                        userOtpCode = new UserOtpCode
                        {
                            UserId = user.Id,
                            OTPCode = otp,
                            Status = EnumStatusUser.New,
                            ExpiredTime = DateTime.Now.AddDays(_appSetting!.Otp!.StepDayWithAdmin)
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
            }

            return methodResult;
        }
    }
}
