// Copyright (c) Atlantic. All rights reserved.

using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Lms.Application.Commands.CourseResultCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
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
        private readonly AuthContext _authContext;
        private readonly ICourseResultRepository _courseResultRepository;

        public StartCourseResultCommandHandler(IMapper mapper
            , AuthContext authContext
            , ICourseResultRepository courseResultRepository)
        {
            _mapper = mapper;
            _authContext = authContext;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<CourseResultModel>> Handle(StartCourseResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseResultModel> methodResult = new MethodResult<CourseResultModel>();

            var courseResult = await _courseResultRepository.Queryable.Include(x => x.Course).FirstOrDefaultAsync(x => x.Id == request.CourseResultId, cancellationToken);
            if (courseResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseResult));
                return methodResult;
            }

            if (courseResult.Course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseResult.Course));
                return methodResult;
            }
            if (courseResult.Status == EnumResultStatus.New)
            {
                courseResult.ProcessDate = DateTime.UtcNow;
                courseResult.Status = EnumResultStatus.Process;
                await _courseResultRepository.BulkUpdateList(new List<CourseResult> { courseResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId };
                });
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<CourseResultModel>(courseResult);
            return methodResult;
        }
    }
}
