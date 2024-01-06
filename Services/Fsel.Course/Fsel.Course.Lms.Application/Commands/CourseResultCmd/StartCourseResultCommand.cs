// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CourseResultCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class StartCourseResultCommand : IRequest<MethodResult<CourseResultModel>>
    {
        public Guid CourseResultId { get; set; }
    }

    public class StartCourseResultCommandHandler : IRequestHandler<StartCourseResultCommand, MethodResult<CourseResultModel>>
    {
        private readonly IMapper _mapper;
        private readonly ICourseResultRepository _courseResultRepository;

        public StartCourseResultCommandHandler(IMapper mapper
            , ICourseResultRepository courseResultRepository)
        {
            _mapper = mapper;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<CourseResultModel>> Handle(StartCourseResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseResultModel> methodResult = new MethodResult<CourseResultModel>();

            #region Validation

            var courseResult = await _courseResultRepository.GetByIdAsync(request.CourseResultId);
            if (courseResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseResult));
                return methodResult;
            }

            #endregion Validation

            if (courseResult.Status == EnumResultStatus.New)
            {
                courseResult.Status = EnumResultStatus.Process;
                _courseResultRepository.Update(courseResult);
                await _courseResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<CourseResultModel>(courseResult);
            return methodResult;
        }
    }
}
