// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.V1i1.LessonQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.V1i1;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLessonReportQuery : IRequest<MethodResult<LessonReportModel>>
    {
        public Guid LessonResultId { get; set; }
    }

    public class GetLessonReportQueryHandler : IRequestHandler<GetLessonReportQuery, MethodResult<LessonReportModel>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly VideoConverter _videoConverter;

        public GetLessonReportQueryHandler(ILessonResultRepository lessonResultRepository
            , IVideoResultRepository videoResultRepository
            , VideoConverter videoConverter)
        {
            _lessonResultRepository = lessonResultRepository;
            _videoResultRepository = videoResultRepository;
            _videoConverter = videoConverter;
        }

        public async Task<MethodResult<LessonReportModel>> Handle(GetLessonReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<LessonReportModel>();
            var lessonResult = await _lessonResultRepository.GetByIdAsync(request.LessonResultId);
            if (lessonResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(lessonResult));
                return methodResult;
            }
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == request.LessonResultId, cancellationToken);
            if (videoResult == null)
            {
                return methodResult;
            }
            methodResult.Result = await _videoConverter.GetLessonReport(videoResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
