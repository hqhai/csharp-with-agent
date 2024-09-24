// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.CourseTargetConfigCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Domain.Enums;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.CourseTargetConfigs;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateCourseTargetConfigCommand : UpdateCourseTargetConfigCommandModel, IRequest<MethodResult<CourseTargetConfigModel>>
    {
    }

    public class UpdateCourseTargetConfigCommandHandler : IRequestHandler<UpdateCourseTargetConfigCommand, MethodResult<CourseTargetConfigModel>>
    {
        private readonly ICourseTargetConfigRepository _courseTargetConfigRepository;
        private readonly IMapper _mapper;

        public UpdateCourseTargetConfigCommandHandler(ICourseTargetConfigRepository courseTargetConfigRepository,
                                                      IMapper mapper)
        {
            _courseTargetConfigRepository = courseTargetConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<CourseTargetConfigModel>> Handle(UpdateCourseTargetConfigCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseTargetConfigModel> methodResult = new MethodResult<CourseTargetConfigModel>();

            #region validate

            if (string.IsNullOrEmpty(request.Title))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseTargetConfigErrorCode.TitleNotNull), nameof(request.Title), nameof(request.Title));
                return methodResult;
            }

            if (request.LessonNumberPerWeek <= 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseTargetConfigErrorCode.NonNegativeLessonNumberPerWeek), nameof(request.LessonNumberPerWeek), nameof(request.LessonNumberPerWeek));
                return methodResult;
            }

            if (request.MaxHoursPerLesson <= 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseTargetConfigErrorCode.NonNegativeMaxHoursPerLesson), nameof(request.MaxHoursPerLesson), nameof(request.MaxHoursPerLesson));
                return methodResult;
            }

            if (await _courseTargetConfigRepository.Queryable.AnyAsync(x => x.Id != request.Id && x.CourseType == request.CourseType && x.LessonNumberPerWeek == request.LessonNumberPerWeek && x.MaxHoursPerLesson == request.MaxHoursPerLesson, cancellationToken))
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseTargetConfigErrorCode.CourseTagetAlreadyExist), nameof(request), nameof(request));
                return methodResult;
            }

            var courseTargetConfig = await _courseTargetConfigRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (courseTargetConfig == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Id), nameof(request.Id));
                return methodResult;
            }

            #endregion

            await _courseTargetConfigRepository.ExecuteTransactionAsync(async () =>
            {
                var command = _mapper.Map(request, courseTargetConfig);
                _courseTargetConfigRepository.Update(command);
                await _courseTargetConfigRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

                methodResult.Result = _mapper.Map<CourseTargetConfigModel>(command);
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }
    }
}
