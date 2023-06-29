// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.CommandModels.Classes;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class AssignTeacherToClass : AssignTeacherToClassModel, IRequest<MethodResult<ClassModel>>
    {
    }

    public class UpdateClassLiveStatusCommandHandler : IRequestHandler<AssignTeacherToClass, MethodResult<ClassModel>>
    {
        private readonly IClassRepository _classRepository;
        private readonly IMapper _mapper;

        public UpdateClassLiveStatusCommandHandler(IClassRepository classRepository, IMapper mapper)
        {
            _classRepository = classRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<ClassModel>> Handle(AssignTeacherToClass request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ClassModel> methodResult = new MethodResult<ClassModel>();
            var @class = await _classRepository.GetByIdAsync(request.Id);
            if (@class == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.ClassesNotExits));
                return methodResult;
            }
            if (@class.TeacherApprovalStatus == Shared.Enums.EnumTeacherApprovalStatus.Approved)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassErrorCode.TeacherApproveStatusIsApprove));
                return methodResult;
            }
            _mapper.Map(request, @class);
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
