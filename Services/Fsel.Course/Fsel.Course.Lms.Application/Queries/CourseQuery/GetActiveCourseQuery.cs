// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CourseQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetActiveCourseQuery : IRequest<MethodResult<IList<Guid>>>
    {
        public IList<Guid>? StudentIds { get; set; }
    }

    public class GetActiveCourseQueryHandler : IRequestHandler<GetActiveCourseQuery, MethodResult<IList<Guid>>>
    {
        private readonly ICourseResultRepository _courseResultRepository;


        public GetActiveCourseQueryHandler(ICourseResultRepository courseResultRepository)
        {
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<IList<Guid>>> Handle(GetActiveCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<Guid>> methodResult = new MethodResult<IList<Guid>>();

            if (request == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request), request);
                return methodResult;
            }

            if (request.StudentIds == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.StudentIds), request.StudentIds);
                return methodResult;
            }

            var result = _courseResultRepository.Queryable.Where(x => request.StudentIds.Contains(x.StudentId) && x.WorkingStatus == EnumWorkingStatus.Active).Select(x => x.Id).ToList();
            methodResult.Result = result;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
