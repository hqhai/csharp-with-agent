// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ActionResults;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Base;
using Fsel.Course.Domain.Enums;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.UnitResults;
using Fsel.Course.Lms.Application.Services.UserServices;
using Fsel.Course.Lms.Application.Services.UserServices.Models;
using Fsel.Shared.Enums;
using MediatR;

namespace Fsel.Course.Lms.Application.Commands.UnitResultCmd
{
    public class OpenNextUnitForExtendCmd : OpenNextUnitForExtendUserCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class OpenNextUnitForExtendCmdHandler : IRequestHandler<OpenNextUnitForExtendCmd, MethodResult<bool>>
    {
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;

        public OpenNextUnitForExtendCmdHandler(IUnitResultRepository unitResultRepository, AuthContext authContext, IUserService userService)
        {
            _unitResultRepository = unitResultRepository;
            _authContext = authContext;
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(OpenNextUnitForExtendCmd request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            var studentRegistration = await _userService.GetStudentTrialRegistration();
            var studentRegistrationResult = studentRegistration?.Content?.Result;

            if (studentRegistrationResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            if (studentRegistrationResult.Status != EnumTrialRegistrationStatus.Payment)
            {
                StudentTrialRegistrationModel model = new StudentTrialRegistrationModel()
                {
                    UserId = _authContext.CurrentUserId,
                    Status = EnumTrialRegistrationStatus.Payment
                };
                await UpdateTrialRegistrationStatus(model);
            }

            if (studentRegistrationResult.Status != EnumTrialRegistrationStatus.Finished)
            {
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            var studentId = studentResult.Content?.Result?.Id;

            var unitResult = _unitResultRepository.Queryable.Where(x => x.StudentId == studentId && x.Status == EnumResultStatus.Done);
            if (unitResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.UnitId), request.StudentId, nameof(request.CourseId));
                return methodResult;
            }

            await _unitResultRepository.ExecuteTransactionAsync(async () =>
            {
                _unitResultRepository.UpdateList(unitResult);
                await _unitResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }

        private async Task UpdateTrialRegistrationStatus(StudentTrialRegistrationModel model)
        {
            await _userService.UpdateTrialRegistrationStatusAsync(model);
        }
    }
}
