// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.VideoTimeCodeResultQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetVideoTimeCodeReportQuery : IRequest<MethodResult<TestResultReportModel>>
    {
        public Guid VideoTimeCodeResultId { get; set; }
    }

    public class GetVideoTimeCodeReportQueryHandler : IRequestHandler<GetVideoTimeCodeReportQuery, MethodResult<TestResultReportModel>>
    {
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IMapper _mapper;

        public GetVideoTimeCodeReportQueryHandler(IVideoTimeCodeResultRepository videoTimeCodeResultRepository, IMapper mapper)
        {
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<TestResultReportModel>> Handle(GetVideoTimeCodeReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<TestResultReportModel> methodResult = new MethodResult<TestResultReportModel>();

            var videoTimeCodeResult = await _videoTimeCodeResultRepository.Queryable
                .Include(x => x.VideoTimeCode)
                .Where(x => x.Id == request.VideoTimeCodeResultId)
                .FirstOrDefaultAsync(cancellationToken);
            if (videoTimeCodeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCodeResult));
                return methodResult;
            }
            var videoTimeCodeResultDto = _mapper.Map<TestResultReportModel>(videoTimeCodeResult);
            if (videoTimeCodeResultDto != null)
            {
                videoTimeCodeResultDto.WorkingTime = videoTimeCodeResult.WorkingTime;
                videoTimeCodeResultDto.Score = videoTimeCodeResult.CorrectCount;
                videoTimeCodeResultDto.HighestStreak = videoTimeCodeResult.HighestStreak;
            }

            methodResult.Result = videoTimeCodeResultDto;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
