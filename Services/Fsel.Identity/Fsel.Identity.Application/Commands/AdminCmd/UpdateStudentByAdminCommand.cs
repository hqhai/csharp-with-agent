// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using System.Globalization;
    using System.Text;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Application.Commands.AuthCmd;
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
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using OtpNet;

    public class UpdateStudentByAdminCommand : UpdateStudentByAdminCommandModel, IRequest<MethodResult<UserModel>>
    {
    }

    public class UpdateStudentByAdminCommandHandler : IRequestHandler<UpdateStudentByAdminCommand, MethodResult<UserModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly AppSetting _appSetting;
        private readonly IMediator _mediator;
        private readonly IUserOtpCodeRepository _userOtpCodeRepository;
        private readonly IHumanRepository _humanRepository;

        public UpdateStudentByAdminCommandHandler(UserManager<User> userManager,
            IMapper mapper,
            AppSetting appSetting,
            IMediator mediator,
            IUserOtpCodeRepository userOtpCodeRepository,
            IHumanRepository humanRepository)
        {
            _userManager = userManager;
            _mapper = mapper;
            _appSetting = appSetting;
            _mediator = mediator;
            _userOtpCodeRepository = userOtpCodeRepository;
            _humanRepository = humanRepository;
        }

        public async Task<MethodResult<UserModel>> Handle(UpdateStudentByAdminCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserModel>();
            var userView = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Student)
                                                   .ThenInclude(x => x!.ParentStudents)
                                                   .FirstOrDefaultAsync(x => x.Human != null && x.Human.Student != null && x.Human.Student.Id == request.Id, cancellationToken);
            if (userView == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist));
                return methodResult;
            }
            var student = userView.Human!.Student!;
            if (request.Parent != null)
            {
                if (student.ParentStudents == null || student.ParentStudents.Count == 0)
                {
                    if (string.IsNullOrEmpty(request.Parent.FullName))
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumParentErrorCode.ParentFullNameNotNull));
                        return methodResult;
                    }

                    Human newHuman = _mapper.Map<Human>(request.Parent);
                    newHuman.Parent = _mapper.Map<Parent>(request.Parent);
                    newHuman.Parent.ParentStudents.Add(new ParentStudent
                    {
                        Student = student
                    });
                    _humanRepository.Add(newHuman);
                    await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    userView = await _userManager.Users.Include(x => x.Human)
                                               .ThenInclude(x => x!.Student)
                                               .ThenInclude(x => x!.ParentStudents)
                                               .ThenInclude(x => x.Parent)
                                               .ThenInclude(x => x!.Human)
                                               .FirstOrDefaultAsync(x => x.Human != null && x.Human.Student != null && x.Human.Student.Id == request.Id, cancellationToken);
                    var human = userView?.Human?.Student?.ParentStudents.FirstOrDefault()?.Parent?.Human;
                    if (human != null)
                    {
                        _mapper.Map(request.Parent, human.Parent);
                        _mapper.Map(request.Parent, human);
                        _humanRepository.Update(human);
                        await _humanRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            if (userView != null)
            {
                var isCheckEmail = userView.Email != request.Email;
                var isCheckPhone = userView.PhoneNumber != request.PhoneNumber;
                if (isCheckEmail || isCheckPhone)
                {
                    userView.EmailConfirmed = false;
                    userView.UserName = isCheckEmail ? request.Email : isCheckPhone ? request.PhoneNumber : userView.UserName;

                    var userOtpCode = await _userOtpCodeRepository.Queryable.FirstOrDefaultAsync(x => x.UserId == userView.Id && x.Status == EnumStatusUser.New && !x.IsDeleted, cancellationToken);
                    if (userOtpCode == null)
                    {
                        var randomSecure = new RandomSecureHelper();
                        var totp = new Totp(Encoding.UTF8.GetBytes(randomSecure.Secretstrings()));
                        var otp = totp.ComputeTotp();

                        userOtpCode = new UserOtpCode
                        {
                            UserId = userView.Id,
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
                        AccessLink = "",
                        OtpValidTime = string.Format(CultureInfo.InvariantCulture, SenderSettings.OtpValidDay, _appSetting!.Otp!.StepDayWithAdmin)
                    };
                    var subject = string.Format(CultureInfo.InvariantCulture, SenderSettings.SendOtpSubjectFullName, userView.FullName);
                    var sendResult = new MethodResult<bool>();
                    if (isCheckEmail)
                    {
                        sendResult = await _mediator.Send(new SenderCommand { Email = request.Email, Subject = subject, Params = param, Template = EnumSenderTemplate.SendOtp }, cancellationToken).ConfigureAwait(false);
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
                _mapper.Map(request, userView.Human?.Student);
                _mapper.Map(request, userView);
                _mapper.Map(request, userView.Human);
                await _userManager.UpdateAsync(userView);
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<UserModel>(userView);
            return methodResult;
        }
    }
}
