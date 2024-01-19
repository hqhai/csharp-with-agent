// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.AiGradeSetting;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Commands.AiGradeSettingCmd
{
    public class CreateMockTestAISettingCmd : MockTestAiSettingModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateAutoGradeSettingCmdHandler : IRequestHandler<CreateMockTestAISettingCmd, MethodResult<bool>>
    {
        private readonly IMapper _mapper;
        private readonly IMockTestAISettingRepository _aiGradeSettingRepository;
        public CreateAutoGradeSettingCmdHandler(IMapper mapper, IMockTestAISettingRepository aiGradeSettingRepository)
        {
            _mapper = mapper;
            _aiGradeSettingRepository = aiGradeSettingRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateMockTestAISettingCmd request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            List<MockTestAISetting> autoGradeSetting = new List<MockTestAISetting>();

            autoGradeSetting = _mapper.Map<List<MockTestAISetting>>(request.MockTestAiSettingModels);

            await _aiGradeSettingRepository.ExecuteTransactionAsync(async () =>
            {
                await _aiGradeSettingRepository.AddList(autoGradeSetting);
                await _aiGradeSettingRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }
    }
}
