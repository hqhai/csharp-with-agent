// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.VideoTimeCodeResultQuery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetVideoTimeCodeTestQuery : IRequest<MethodResult<IList<VideoTimeCodeModel>>>
    {
        public Guid LessonResultId { get; set; }
        public EnumTimeCodeType Type { get; set; }
    }

    public class GetVideoTimeCodeTestQueryHandler : IRequestHandler<GetVideoTimeCodeTestQuery, MethodResult<IList<VideoTimeCodeModel>>>
    {
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IMapper _mapper;

        public GetVideoTimeCodeTestQueryHandler(
            IVideoTimeCodeRepository videoTimeCodeRepository,
            IVideoResultRepository videoResultRepository,
            IMapper mapper)
        {
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _videoResultRepository = videoResultRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<VideoTimeCodeModel>>> Handle(GetVideoTimeCodeTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<VideoTimeCodeModel>> methodResult = new MethodResult<IList<VideoTimeCodeModel>>();
            if (request.Type == EnumTimeCodeType.Standalone)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(EnumTimeCodeType.Standalone));
                return methodResult;
            }
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == request.LessonResultId, cancellationToken);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }

            var videoTimeCodes = await _videoTimeCodeRepository.Queryable.Include(x => x.TimeCodeExercises).ThenInclude(x => x.Exercise)
                .Where(x => x.VideoId == videoResult.VideoId && x.TimeCodeType == request.Type)
                .OrderBy(x => x.DisplayTime)
                .ToListAsync(cancellationToken: cancellationToken);
            if (videoTimeCodes == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCodes));
                return methodResult;
            }
            methodResult.Result = _mapper.Map<IList<VideoTimeCodeModel>>(videoTimeCodes);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}