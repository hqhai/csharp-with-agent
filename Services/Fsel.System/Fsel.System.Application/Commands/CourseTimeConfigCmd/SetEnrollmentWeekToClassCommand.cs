// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.CourseTimeConfigCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.CommandModels.CourseTimeConfigs;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SetEnrollmentWeekToClassCommand : SetEnrollmentWeekToClassCommandModel, IRequest<MethodResult<IList<CourseTimeConfigModel>>>
    {
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

            var courseTimeConfigs = await _courseTimeConfigRepository.Queryable.ToListAsync(cancellationToken);

            courseTimeConfigs.ForEach(x => { x.EnrollmentWeek = request.EnrollmentWeek; });

            #endregion Validation

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
