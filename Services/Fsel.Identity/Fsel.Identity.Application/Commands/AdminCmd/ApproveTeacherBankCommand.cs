// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class ApproveTeacherBankCommand : IRequest<MethodResult<UserModel>>
    {
        public Guid Id { get; set; }
        public bool Status { get; set; }
    }

    public class RequestUpdateTeacherBankCommandHandler : IRequestHandler<ApproveTeacherBankCommand, MethodResult<UserModel>>
    {
        private readonly UserManager<User> _userManager;
        private readonly ITeacherBankAccountRepository _teacherBankAccountRepository;
        private readonly IMapper _mapper;

        public RequestUpdateTeacherBankCommandHandler(UserManager<User> userManager,
            ITeacherBankAccountRepository teacherBankAccountRepository,
            IMapper mapper)
        {
            _userManager = userManager;
            _teacherBankAccountRepository = teacherBankAccountRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<UserModel>> Handle(ApproveTeacherBankCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<UserModel>();
            var user = await _userManager.Users.Include(x => x.Human)
                                                   .ThenInclude(x => x!.Teacher)
                                                   .ThenInclude(x => x!.TeacherBankAccounts).FirstOrDefaultAsync(x => x.Id == request.Id.ToString(), cancellationToken);
            if (user == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUserErrorCode.UserNotExist), nameof(request.Id), request.Id);
                return methodResult;
            }
            var sl = user.Human?.Teacher?.TeacherBankAccounts?.Count;
            TeacherBankAccount? teacherBankAccount = null;
            if (request.Status && sl == 2)
            {
                teacherBankAccount = user.Human?.Teacher?.TeacherBankAccounts?.FirstOrDefault(x => x.Status == EnumStatusBank.Approve);
                if (teacherBankAccount == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumTeacherErrorCode.TeacherBankAccountNotExistsApprove));
                    return methodResult;
                }
                user.Human?.Teacher?.TeacherBankAccounts?.Remove(teacherBankAccount);
                var teacherBankAccountNew = user.Human?.Teacher?.TeacherBankAccounts?.FirstOrDefault(x => x.Status == EnumStatusBank.New);
                teacherBankAccountNew!.Status = EnumStatusBank.Approve;
                _teacherBankAccountRepository.Update(teacherBankAccountNew);
                await _teacherBankAccountRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                teacherBankAccount = user.Human?.Teacher?.TeacherBankAccounts?.FirstOrDefault(x => x.Status == EnumStatusBank.New);
                if (teacherBankAccount == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumTeacherErrorCode.TeacherBankAccountNotExistsApprove));
                    return methodResult;
                }
                user.Human?.Teacher?.TeacherBankAccounts?.Remove(teacherBankAccount);
            }

            await _userManager.UpdateAsync(user);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<UserModel>(user);
            return methodResult;
        }
    }
}
