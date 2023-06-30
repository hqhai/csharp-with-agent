// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Commands.ClassLiveWorkFlowCmd
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Training.Application.Services.UserServices;
    using Fsel.Training.Domain.Enums;
    using Fsel.Training.Domain.Enums.ErrorCodes;
    using Fsel.Training.Domain.IRepositories;
    using Fsel.Training.Domain.Models.CommandModels.ClassLiveWorkFlows;
    using Fsel.Training.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateClassLiveWorkFlowCommand : UpdateClassLiveWorkFlowCommandModel, IRequest<MethodResult<ClassLiveWorkFlowModel>>
    {
    }

    public class UpdateClassLiveWorkFlowCommandHandler : IRequestHandler<UpdateClassLiveWorkFlowCommand, MethodResult<ClassLiveWorkFlowModel>>
    {
        private readonly IClassLiveWorkFlowRepository _classLiveWorkFlowRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UpdateClassLiveWorkFlowCommandHandler(IClassLiveWorkFlowRepository classLiveWorkFlowRepository,
            AuthContext authContext,
            IUserService userService,
            IMapper mapper)
        {
            _classLiveWorkFlowRepository = classLiveWorkFlowRepository;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<ClassLiveWorkFlowModel>> Handle(UpdateClassLiveWorkFlowCommand request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<ClassLiveWorkFlowModel>();
            ArgumentNullException.ThrowIfNull(request);

            var teacherResult = await _userService.GetTeacherByUserIdAsync(_authContext.CurrentUserId);
            if (!teacherResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.TeacherNotExits));
                return methodResult;
            }
            var teacher = teacherResult.Content?.Result;

            var classLiveWorkFlow = await _classLiveWorkFlowRepository.Queryable.FirstOrDefaultAsync(x => x.TeacherId == teacher!.Id && x.Id == request.Id, cancellationToken);
            if (classLiveWorkFlow == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumClassLiveWorkFlowErrorCode.ClassLiveWorkFlowNotExits));
                return methodResult;
            }
            if (request.IsActice)
            {
                classLiveWorkFlow.Status = EnumWorkFlowStatus.Planed;
            }
            else
            {
                classLiveWorkFlow.Status = EnumWorkFlowStatus.SubstitutionRequest;
                classLiveWorkFlow.TeacherId = default;
            }

            await _classLiveWorkFlowRepository.ExecuteTransactionAsync(async () =>
            {
                classLiveWorkFlow = _classLiveWorkFlowRepository.Update(classLiveWorkFlow);
                await _classLiveWorkFlowRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<ClassLiveWorkFlowModel>(classLiveWorkFlow);
                return methodResult;
            });
            return methodResult;
        }
    }
}
