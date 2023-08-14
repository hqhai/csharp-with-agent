// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.CommandModels.Classes;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class AssignTeacherToClassCommand : AssignTeacherToClassCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class AssignTeacherToClassCommandHandler : IRequestHandler<AssignTeacherToClassCommand, MethodResult<bool>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IUserService _userService;

        public AssignTeacherToClassCommandHandler(IClassRepository classRepository, IUserService userService)
        {
            _classRepository = classRepository;
            _userService = userService;
        }

        public async Task<MethodResult<bool>> Handle(AssignTeacherToClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var classes = await _classRepository.GetByIdAsync(request.ClassId);
            if (classes == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(classes));
                return methodResult;
            }
            var teacherResult = await _userService.GetTeacherByIdAsync(request.TeacherId);
            if (!teacherResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(teacherResult));
                return methodResult;
            }

            var teacher = teacherResult?.Content?.Result;
            if (teacher == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(teacher));
                return methodResult;
            }
            classes.TeacherId = request.TeacherId;
            classes.TeacherApprovalStatus = EnumTeacherApprovalStatus.Pending;
            await _classRepository.ExecuteTransactionAsync(async () =>
            {
                _classRepository.Update(classes);
                await _classRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = true;
                return methodResult;
            });
            return methodResult;
        }
    }
}
