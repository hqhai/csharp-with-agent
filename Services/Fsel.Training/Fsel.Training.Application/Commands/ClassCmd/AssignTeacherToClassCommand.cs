// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.CommandModels.Classes;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class AssignTeacherToClassCommand : AssignTeacherToClassCommandModel, IRequest<MethodResult<ClassModel>>
    {
    }

    public class AssignTeacherToClassCommandHandler : IRequestHandler<AssignTeacherToClassCommand, MethodResult<ClassModel>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public AssignTeacherToClassCommandHandler(IClassRepository classRepository
            , IUserService userService
            , IMapper mapper)
        {
            _classRepository = classRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<ClassModel>> Handle(AssignTeacherToClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();
            var @class = await _classRepository.GetByIdAsync(request.Id);
            if (@class == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(@class));
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
            @class.Id = request.Id;
            @class.TeacherId = request.TeacherId;

            await _classRepository.ExecuteTransactionAsync(async () =>
            {
                @class.TeacherApprovalStatus = Shared.Enums.EnumTeacherApprovalStatus.Pending;
                _classRepository.Update(@class);
                await _classRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status200OK;
                methodResult.Result = _mapper.Map<ClassModel>(@class);
                return methodResult;
            });
            return methodResult;
        }
    }
}
