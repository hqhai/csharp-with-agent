// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Models.CommandModels.Lessons;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Fsel.Course.Application.Commands.AutoGradeSettingCmd
{
    public class CreateAutoGradeSettingCmd : CreateLessonCommandModel, IRequest<MethodResult<bool>>
    {
    }

    public class CreateAutoGradeSettingCmdHandler : IRequestHandler<CreateAutoGradeSettingCmd, MethodResult<bool>>
    {
        private readonly IMapper _mapper;
        public CreateAutoGradeSettingCmdHandler(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<MethodResult<bool>> Handle(CreateAutoGradeSettingCmd request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();

            AutoGradeSetting autoGradeSetting = new AutoGradeSetting();

            autoGradeSetting = _mapper.Map<AutoGradeSetting>(request);

            //await _autoGradeSettingRepository.ExecuteTransactionAsync(async () =>
            //{
            //    autoGradeSetting = _autoGradeSettingRepository.Add(autoGradeSetting);
            //    await _autoGradeSettingRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);


            //    methodResult.StatusCode = StatusCodes.Status201Created;
            //    methodResult.Result = _mapper.Map<bool>(autoGradeSetting);
            //    return methodResult;
            //});

            return methodResult;
        }
    }
}
