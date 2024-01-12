// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.CommandModels.Lessons;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Commands.AiGradeSettingCmd
{
    public class CreateAiGradeSettingCmd : CreateLessonCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateAutoGradeSettingCmdHandler : IRequestHandler<CreateAiGradeSettingCmd, MethodResult<bool>>
    {
        private readonly IMapper _mapper;
        private readonly IAiGradeSettingRepository _aiGradeSettingRepository;
        public CreateAutoGradeSettingCmdHandler(IMapper mapper, IAiGradeSettingRepository aiGradeSettingRepository)
        {
            _mapper = mapper;
            _aiGradeSettingRepository = aiGradeSettingRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateAiGradeSettingCmd request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            AiGradeSetting autoGradeSetting = new AiGradeSetting();

            autoGradeSetting = _mapper.Map<AiGradeSetting>(request);

            await _aiGradeSettingRepository.ExecuteTransactionAsync(async () =>
            {
                autoGradeSetting = _aiGradeSettingRepository.Add(autoGradeSetting);
                await _aiGradeSettingRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);


                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<bool>(autoGradeSetting);
                return methodResult;
            });

            return methodResult;
        }
    }
}
