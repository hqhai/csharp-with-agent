// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.CourseTimeConfigCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.CourseTimeConfigs;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

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

            await _courseTimeConfigRepository.ExecuteTransactionAsync(async () =>
            {
                var courseTimeConfig = await _courseTimeConfigRepository.Queryable.FirstOrDefaultAsync(x => x.CourseId == request.CourseId, cancellationToken);
                if (courseTimeConfig != null)
                {
                    _mapper.Map(request, courseTimeConfig);
                    courseTimeConfig = _courseTimeConfigRepository.Update(courseTimeConfig);
                }
                else if (courseTimeConfig == null)
                {
                    courseTimeConfig = _mapper.Map<CourseTimeConfig>(request);
                    if (!courseTimeConfig.IsValid())
                    {
                        methodResult.AddErrorBadRequest(courseTimeConfig.ErrorMessages);
                        return methodResult;
                    }
                    var lastTimeWeek = await _courseTimeConfigRepository.Queryable.FirstOrDefaultAsync(cancellationToken);

                    courseTimeConfig.EnrollmentWeek = lastTimeWeek?.EnrollmentWeek ?? 0;
                    courseTimeConfig = _courseTimeConfigRepository.Add(courseTimeConfig);
                }
                await _courseTimeConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<CourseTimeConfigModel>(courseTimeConfig);
                return methodResult;
            });

            return methodResult;
        }
    }
}
