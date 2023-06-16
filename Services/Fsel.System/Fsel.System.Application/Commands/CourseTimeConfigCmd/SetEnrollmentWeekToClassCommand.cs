// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.CourseTimeConfigCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.Enums.ErrorCodes;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models;
    using Fsel.System.Domain.Models.CommandModels.CourseTimeConfigs;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SetEnrollmentWeekToClassCommand : SetEnrollmentWeekToClassCommandModel, IRequest<MethodResult<IList<CourseTimeConfigModel>>>
    {
        public IList<Guid>? Ids { get; set; }
    }

    public class SetEnrollmentWeekToClassCommandHandler : IRequestHandler<SetEnrollmentWeekToClassCommand, MethodResult<IList<CourseTimeConfigModel>>>
    {
        private readonly ICourseTimeConfigRepository _courseTimeConfigRepository;
        private readonly IMapper _mapper;

        public SetEnrollmentWeekToClassCommandHandler(ICourseTimeConfigRepository courseTimeConfigRepository, IMapper mapper)
        {
            _courseTimeConfigRepository = courseTimeConfigRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CourseTimeConfigModel>>> Handle(SetEnrollmentWeekToClassCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<CourseTimeConfigModel>>();

            #region Validation

            var courseTimeConfigs = await _courseTimeConfigRepository.GetByIdsAsync(request.Ids!);
            if (courseTimeConfigs == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseTimeConfigErrorCode.CourseTimeConfigNotEmpty), nameof(request.Id), request.Id);
                return methodResult;
            }
            _mapper.Map(request, courseTimeConfigs);
            await _courseTimeConfigRepository.ExecuteTransactionAsync(async () =>
            {
                _courseTimeConfigRepository.UpdateList(courseTimeConfigs);
                await _courseTimeConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                methodResult.StatusCode = StatusCodes.Status201Created;
                methodResult.Result = _mapper.Map<IList<CourseTimeConfigModel>>(courseTimeConfigs);
                return methodResult;
            });

            return methodResult;
        }
    }
}
