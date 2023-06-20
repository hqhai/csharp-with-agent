// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.CourseTimeConfigCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models;
    using Fsel.System.Domain.Models.CommandModels.CourseTimeConfigs;
    using global::System;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SetMonthToClassCommand : SetMonthToClassCommandModel, IRequest<MethodResult<CourseTimeConfigModel>>
    {
    }

    public class SetMonthToClassCommandHandler : IRequestHandler<SetMonthToClassCommand, MethodResult<CourseTimeConfigModel>>
    {
        private readonly ICourseTimeConfigRepository _courseTimeConfigRepository;
        private readonly IMapper _mapper;

        public SetMonthToClassCommandHandler(ICourseTimeConfigRepository courseTimeConfigRepository, IMapper mapper)
        {
            _courseTimeConfigRepository = courseTimeConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<CourseTimeConfigModel>> Handle(SetMonthToClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseTimeConfigModel> methodResult = new MethodResult<CourseTimeConfigModel>();

            #region Validation

            var courseTimeConfig = await _courseTimeConfigRepository.GetIncludeByIdAsync(request.Id);
            if (courseTimeConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseTimeConfigErrorCode.CourseTimeConfigNotEmpty), nameof(request.Id), request.Id);
                return methodResult;
            }

            #endregion Validation

            _mapper.Map(request, courseTimeConfig);
            await _courseTimeConfigRepository.ExecuteTransactionAsync(async () =>
            {
                courseTimeConfig = _courseTimeConfigRepository.Update(courseTimeConfig);
                await _courseTimeConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CourseTimeConfigModel>(courseTimeConfig);
                return methodResult;
            });

            return methodResult;
        }
    }
}
