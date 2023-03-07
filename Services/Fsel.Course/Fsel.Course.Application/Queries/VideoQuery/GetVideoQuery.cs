using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Helpers;
using Fsel.Course.Common.Models.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Application.Queries.VideoQuery
{
    public class GetVideoQuery : IRequest<MethodResult<VideoModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetVideoQueryHandler : IRequestHandler<GetVideoQuery, MethodResult<VideoModel>>
    {
        private readonly IMapper _mapper;
        private readonly IVideoRepository _videoRepository;

        public GetVideoQueryHandler(IMapper mapper, IVideoRepository videoRepository)
        {
            _mapper = mapper;
            _videoRepository = videoRepository;
        }

        public async Task<MethodResult<VideoModel>> Handle(GetVideoQuery request, CancellationToken cancellationToken)
        {
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();

            var video = await _videoRepository.GetByIdAsync(request.Id);

            if (video == null)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                methodResult.AddErrorMessage(
                    nameof(EnumVideoErrorCode.VD01V),
                    new[] { MethodHelper.GenerateErrorResult(nameof(request.Id), request.Id) });
                return methodResult;
            }

            methodResult.Result = _mapper.Map<VideoModel>(video);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
